using System.Security.Claims;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace PharmacyManagement.Middlewares
{
    /// <summary>
    /// Middleware idempotency dùng Redis (TTL 24 giờ).
    /// Với các request mutation (POST/PUT/DELETE/PATCH) - trừ endpoint miễn trừ:
    ///  - Thiếu/rỗng header "Idempotency-Key" -> từ chối ngay 400 Bad Request.
    ///  - Có key: SET NX để "claim" quyền xử lý đúng 1 lần (chặn double-submit đồng thời).
    ///  - Request đã xử lý thành công trước đó -> replay đúng response cũ.
    ///  - Request thất bại -> xóa key để cho phép retry.
    ///  - Khi Redis lỗi -> bỏ qua bảo vệ (fail-open), vẫn xử lý request bình thường.
    /// </summary>
    public class IdempotencyMiddleware
    {
        private static readonly string[] MutatingMethods = { "POST", "PUT", "DELETE", "PATCH" };
        private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);

        // Các endpoint đặc biệt không bắt buộc khóa (đăng nhập/refresh/logout, Hangfire dashboard).
        private static readonly string[] ExemptPathPrefixes = { "/api/auth", "/hangfire" };

        private static bool IsExempt(PathString path)
            => ExemptPathPrefixes.Any(prefix => path.StartsWithSegments(prefix));

        private readonly RequestDelegate _next;
        private readonly ILogger<IdempotencyMiddleware> _logger;

        public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IDatabase redis)
        {
            var method = context.Request.Method.ToUpperInvariant();

            // 1. Không phải request thay đổi dữ liệu -> cho đi tiếp
            if (!MutatingMethods.Contains(method))
            {
                await _next(context);
                return;
            }

            // 2. Endpoint đặc biệt được miễn khóa (đăng nhập/refresh/logout, Hangfire dashboard)
            if (IsExempt(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // 3. Chặn ngay từ ngoài: thiếu hoặc rỗng Idempotency-Key -> 400 Bad Request
            if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idemHeader)
                || string.IsNullOrWhiteSpace(idemHeader.ToString()))
            {
                await WriteJsonResponseAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    "IDEM001",
                    "Missing Idempotency-Key header. An idempotency key is required for this request.");
                return;
            }

            var redisKey = BuildRedisKey(context, idemHeader.ToString().Trim());
            var useIdempotency = true;

            try
            {
                var claimed = await redis.StringSetAsync(
                    redisKey,
                    JsonSerializer.Serialize(new { status = "Processing" }),
                    IdempotencyTtl,
                    When.NotExists);

                if (!claimed)
                {
                    var existing = await redis.StringGetAsync(redisKey);
                    if (!existing.IsNullOrEmpty)
                    {
                        using var doc = JsonDocument.Parse(existing.ToString());
                        var status = doc.RootElement.GetProperty("status").GetString();
                        if (status == "Processing")
                        {
                            await WriteJsonResponseAsync(
                                context, 409, "IDEM002",
                                "Request is already being processed (duplicate Idempotency-Key).");
                            return;
                        }

                        if (status == "Completed")
                        {
                            await ReplayCompletedAsync(context, doc.RootElement);
                            return;
                        }
                    }

                    // Key đã hết hạn giữa chừng -> thử claim lại một lần cuối
                    var reClaimed = await redis.StringSetAsync(
                        redisKey,
                        JsonSerializer.Serialize(new { status = "Processing" }),
                        IdempotencyTtl,
                        When.NotExists);
                    if (!reClaimed)
                    {
                        await WriteJsonResponseAsync(
                            context, 409, "IDEM003",
                            "Request is already being processed (duplicate Idempotency-Key).");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Fail-open: Redis lỗi thì không chặn nghiệp vụ
                _logger.LogWarning(ex, "Idempotency protection skipped due to Redis failure.");
                useIdempotency = false;
            }

            await ExecuteAndCaptureAsync(context, redis, redisKey, useIdempotency);
        }

        private static string BuildRedisKey(HttpContext context, string clientKey)
        {
            var branchId = context.Request.Headers.TryGetValue("X-Branch-Id", out var branchValues)
                ? branchValues.ToString()
                : "0";
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            var method = context.Request.Method.ToUpperInvariant();
            return $"Idem:{branchId}:{userId}:{method}:{context.Request.Path}:{clientKey}";
        }

        private async Task ExecuteAndCaptureAsync(HttpContext context, IDatabase redis, string redisKey, bool useIdempotency)
        {
            var originalBody = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await _next(context);

                buffer.Position = 0;
                var body = await new StreamReader(buffer, Encoding.UTF8).ReadToEndAsync();

                if (useIdempotency)
                {
                    var statusCode = context.Response.StatusCode;
                    try
                    {
                        if (statusCode is >= 200 and < 300)
                        {
                            var payload = JsonSerializer.Serialize(new
                            {
                                status = "Completed",
                                statusCode,
                                contentType = context.Response.ContentType,
                                body
                            });
                            await redis.StringSetAsync(redisKey, payload, IdempotencyTtl);
                        }
                        else
                        {
                            await redis.KeyDeleteAsync(redisKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to persist idempotency result.");
                    }
                }
            }
            catch
            {
                if (useIdempotency)
                {
                    try { await redis.KeyDeleteAsync(redisKey); }
                    catch (Exception delEx)
                    {
                        _logger.LogWarning(delEx, "Failed to clean up idempotency key after exception.");
                    }
                }
                throw;
            }
            finally
            {
                context.Response.Body = originalBody;
                if (buffer.Length > 0)
                {
                    buffer.Position = 0;
                    await buffer.CopyToAsync(originalBody);
                }
            }
        }

        private static async Task ReplayCompletedAsync(HttpContext context, JsonElement root)
        {
            var statusCode = root.TryGetProperty("statusCode", out var sc) ? sc.GetInt32() : 200;
            var contentType = root.TryGetProperty("contentType", out var ct)
                ? ct.GetString()
                : "application/json";
            var body = root.TryGetProperty("body", out var b) ? b.GetString() : null;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = string.IsNullOrEmpty(contentType)
                ? "application/json"
                : contentType;

            if (!string.IsNullOrEmpty(body))
            {
                await context.Response.WriteAsync(body);
            }
        }

        private static async Task WriteJsonResponseAsync(HttpContext context, int statusCode, string errorCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode,
                errorCode,
                ec = -1,
                em = message,
                dt = (object?)null
            });
        }
    }
}