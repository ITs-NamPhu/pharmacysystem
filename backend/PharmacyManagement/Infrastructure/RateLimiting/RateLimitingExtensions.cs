using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmacyManagement.share;
using StackExchange.Redis;
using RedisRateLimiting;

namespace PharmacyManagement.Infrastructure.RateLimiting
{
    /// <summary>Tên các policy rate limiting dùng trong [EnableRateLimiting].</summary>
    public static class RateLimitPolicies
    {
        public const string Auth = "Policy_Auth";
        public const string Refresh = "Policy_Refresh";
        public const string Expensive = "Policy_Expensive";
        public const string Dashboard = "Policy_Dashboard";
        public const string Read = "Policy_Read";
        public const string Write = "Policy_Write";
    }

    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddAppRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration,
            IConnectionMultiplexer redisConnection
            )
        {
            var settings = configuration
                .GetSection("RateLimiting")
                .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

            services.AddSingleton(settings);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString();
                    }

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new ApiResponse<object>
                        {
                            EC = StatusCodes.Status429TooManyRequests,
                            StatusCode = StatusCodes.Status429TooManyRequests,
                            EM = "Too many requests. Please try again later.",
                            DT = null
                        },
                        cancellationToken: token);
                };

                // T0 - Xác thực: chặt nhất, partition theo IP.
                options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
                    PerIp(httpContext, settings.Auth, redisConnection));

                options.AddPolicy(RateLimitPolicies.Refresh, httpContext =>
                    PerIp(httpContext, settings.Refresh, redisConnection));

                // T1 - Truy vấn nặng: partition theo user (fallback IP).
                options.AddPolicy(RateLimitPolicies.Expensive, httpContext =>
                    PerKey(httpContext, settings.Expensive, redisConnection));

                // T1 - Dashboard: sliding window.
                options.AddPolicy(RateLimitPolicies.Dashboard, httpContext =>
                    PerKeySlidingWindow(httpContext, settings.Dashboard, redisConnection));

                // T4 - Fallback cho mọi endpoint chưa gắn policy:
                // phân nhánh theo HTTP method (GET/HEAD/OPTIONS = đọc, còn lại = ghi).
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var rule = IsRead(httpContext.Request.Method)
                        ? settings.Read
                        : settings.Write;

                    return PerKey(httpContext, rule, redisConnection);
                });
            });

            return services;
        }

        private static RateLimitPartition<string> PerIp(HttpContext httpContext, RateLimitRule rule, IConnectionMultiplexer redisConnection)
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RedisRateLimitPartition.GetTokenBucketRateLimiter($"ip:{ip}", _ => ToRedisTokenBucket(rule, redisConnection));
        }

        private static RateLimitPartition<string> PerKey(HttpContext httpContext, RateLimitRule rule, IConnectionMultiplexer redisConnection)
        {
            var key = GetPartitionKey(httpContext);
            return RedisRateLimitPartition.GetTokenBucketRateLimiter(key, _ => ToRedisTokenBucket(rule, redisConnection));
        }

        private static RateLimitPartition<string> PerKeySlidingWindow(
            HttpContext httpContext,
            SlidingWindowRule rule,
            IConnectionMultiplexer redisConnection)
        {
            var key = GetPartitionKey(httpContext);
            return RedisRateLimitPartition.GetSlidingWindowRateLimiter(key, _ => new RedisSlidingWindowRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => redisConnection,
                PermitLimit = rule.PermitLimit,
                Window = TimeSpan.FromSeconds(rule.WindowSeconds),
            });
        }

        /// <summary>User đã xác thực -> user id; ngược lại -> IP.</summary>
        private static string GetPartitionKey(HttpContext httpContext)
        {
            var userId = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }

            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ip}";
        }

        private static bool IsRead(string method) =>
            HttpMethods.IsGet(method)
            || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method);

        private static RedisTokenBucketRateLimiterOptions ToRedisTokenBucket(RateLimitRule rule, IConnectionMultiplexer redis) => new()
        {
            ConnectionMultiplexerFactory = () => redis,
            TokenLimit = rule.TokenLimit,
            ReplenishmentPeriod = TimeSpan.FromSeconds(rule.RefillPeriodSeconds),
            TokensPerPeriod = rule.RefillTokens,
        };
    }
}
