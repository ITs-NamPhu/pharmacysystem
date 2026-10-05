using PharmacyManagement.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Middlewares;
using PharmacyManagement.DTOs.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PharmacyManagement.share;

using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Services.Implements;
using PharmacyManagement.Services.BatchSelection;
using PharmacyManagement.Services.Notifications;

using PharmacyManagement.Repositories.Interfaces;
using PharmacyManagement.Repositories.Implements;

using FluentValidation;
using PharmacyManagement.Validators.BusinessRule;
using PharmacyManagement.Validators.FluentValidation.User;
using PharmacyManagement.Validators.PermissionHandle;

using PharmacyManagement.Handlers;

using Microsoft.AspNetCore.Authorization;

using Hangfire;

using StackExchange.Redis;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace PharmacyManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMemoryCache();

            var redisConnection = builder.Configuration["RedisConnection"] ?? "localhost:6379";

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "PharmacySys_";
            });

            // Multiplexer dùng cho IdempotencyMiddleware (SET NX ... ).
            // AbortOnConnectFail=false: không ném lỗi khi Redis chưa sẵn sàng, thử kết nối lại nền.
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnection, opts => opts.AbortOnConnectFail = false));
            builder.Services.AddScoped(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

            builder.Services.AddRateLimiter(rateLimit =>
            {
                rateLimit.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                rateLimit.AddPolicy("Limit_Per_IP", httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetTokenBucketLimiter(ipAddress, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        TokensPerPeriod = 1
                    });
                });

                rateLimit.AddConcurrencyLimiter("Limit_Concurrent_Requests", options =>
                {
                    options.PermitLimit = 5; // Số lượng request đồng thời tối đa
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 4; // Số lượng request chờ tối đa
                });
            });

            // Add services to the container.
            builder.Services.AddDbContext<PharmacySystemDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PharSystemConnection")));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JwtConfig"));

            var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:5000" };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", policy =>
                {
                    policy.WithOrigins(corsOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings =
                    builder.Configuration
                    .GetSection("JwtConfig")
                    .Get<JwtSetting>();

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,

                        ValidateAudience = true,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,

                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                        ClockSkew = TimeSpan.Zero
                    };


                options.Events = new JwtBearerEvents
                {
                    // Case: token không hợp lệ
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = new ApiResponse<object>
                            {
                                EC = -998,
                                StatusCode = 401,
                                EM = "Access token is invalid.",
                                DT = null
                            };

                            return context.Response.WriteAsJsonAsync(response);
                        }

                        return Task.CompletedTask;
                    },
                    // Case: token hết hạn
                    OnAuthenticationFailed = context =>
                    {
                        if (!context.Response.HasStarted)
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.ContentType = "application/json";

                                var response = new ApiResponse<object>
                                {
                                    EC = -999,
                                    StatusCode = 401,
                                    EM = "Access token has expired.",
                                    DT = null
                                };

                                return context.Response.WriteAsJsonAsync(response);
                            }
                        }

                        return Task.CompletedTask;
                    },

                    OnForbidden = context =>
                    {
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";

                            var response = new ApiResponse<object>
                            {
                                StatusCode = 403,
                                EM = "You do not have permission to access this resource.",
                                DT = null,
                                EC = -1
                            };

                            return context.Response.WriteAsJsonAsync(response);
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();


            // Repository
            builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
            builder.Services.AddScoped<ICustomerTypeRepository, CustomerTypeRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IUnitRepository, UnitRepository>();
            builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
            builder.Services.AddScoped<IMedicineCategoryRepository, MedicineCategoryRepository>();
            builder.Services.AddScoped<IBranchRepository, BranchRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
            builder.Services.AddScoped<IUnitConversionRepository, UnitConversionRepository>();
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            builder.Services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
            builder.Services.AddScoped<IStockTakeRepository, StockTakeRepository>();
            builder.Services.AddScoped<IStockAdjustmentRepository, StockAdjustmentRepository>();
            builder.Services.AddScoped<IDestroyReceiptRepository, DestroyReceiptRepository>();
            builder.Services.AddScoped<ICustomerDebtSummaryRepository, CustomerDebtSummaryRepository>();
            builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();

            // Service
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<ICustomerTypeService, CustomerTypeService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IUnitService, UnitService>();
            builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
            builder.Services.AddScoped<IMedicineCategoryService, MedicineCategoryService>();
            builder.Services.AddScoped<IBranchService, BranchService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IMedicineService, MedicineService>();
            builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IStockTakeService, StockTakeService>();
            builder.Services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
            builder.Services.AddScoped<IDestroyReceiptService, DestroyReceiptService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IDebtSummaryService, DebtSummaryService>();
            builder.Services.AddScoped<DebtSummaryService>();
            builder.Services.AddScoped<IReceiptService, ReceiptService>();
            builder.Services.AddScoped<IReconciliationService, ReconciliationService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IAiSearchService, AiSearchService>();
            // Batch selection
            builder.Services.AddScoped<FefoBatchSelectionStrategy>();
            builder.Services.AddScoped<ManualBatchSelectionStrategy>();
            builder.Services.AddScoped<BatchSelectionStrategyFactory>();

            // Authorization
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            // Validators
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
            builder.Services.AddScoped<UserBusinessValidator>();
            builder.Services.AddScoped<CustomerTypeBusinessValidator>();
            builder.Services.AddScoped<RoleBusinessValidator>();
            builder.Services.AddScoped<UnitBusinessValidator>();
            builder.Services.AddScoped<ManufacturerBusinessValidator>();
            builder.Services.AddScoped<MedicineCategoryBusinessValidator>();
            builder.Services.AddScoped<BranchBusinessValidator>();
            builder.Services.AddScoped<CustomerBusinessValidator>();
            builder.Services.AddScoped<SupplierBusinessValidator>();
            builder.Services.AddScoped<WarehouseBusinessValidator>();
            builder.Services.AddScoped<MedicineBusinessValidator>();
            builder.Services.AddScoped<UnitConversionBusinessValidator>();
            builder.Services.AddScoped<InvoiceBusinessValidator>();
            builder.Services.AddScoped<InvoiceItemBusinessValidator>();
            builder.Services.AddScoped<GoodsReceiptBusinessValidator>();
            builder.Services.AddScoped<GoodsReceiptItemUpdateHandler>();
            builder.Services.AddScoped<StockTakeBusinessValidator>();
            builder.Services.AddScoped<StockAdjustmentBusinessValidator>();
            builder.Services.AddScoped<DestroyReceiptBusinessValidator>();

            builder.Services.AddControllers();

            // Hangfire
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("PharSystemConnection")));
            builder.Services.AddHangfireServer();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseMiddleware<ExceptionMiddleware>();
            if (!app.Environment.IsEnvironment("Docker"))
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("ReactPolicy");

            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseMiddleware<IdempotencyMiddleware>();
            app.UseAuthorization();

            app.MapControllers();

            app.UseHangfireDashboard("/hangfire");

            if (app.Environment.IsEnvironment("Docker"))
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PharmacySystemDbContext>();
                db.Database.Migrate();

                // khai báo Hangfire trước khi đăng ký RecurringJob.
                // Background server của Hangfire cài schema lúc khởi động - trước khi DB được tạo - nên cần tự cài lại.
                var connectionString = builder.Configuration.GetConnectionString("PharSystemConnection");
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                var assembly = typeof(Hangfire.SqlServer.SqlServerStorage).Assembly;
                using var stream = assembly.GetManifestResourceStream("Hangfire.SqlServer.Install.sql");
                using var reader = new System.IO.StreamReader(stream);
                cmd.CommandText = reader.ReadToEnd();
                cmd.CommandTimeout = 300;
                cmd.ExecuteNonQuery();
            }

            // Job chạy vào lúc 00:01 sáng ngày mùng 1 hàng tháng
            RecurringJob.AddOrUpdate<DebtSummaryService>(
                "monthly-debt-closing",
                service => service.ProcessMonthlyClosingAsync(
                    DateTime.Now.AddMonths(-1).Year,
                    DateTime.Now.AddMonths(-1).Month),
                "1 0 1 * *",
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                }
                );

            // Job đối soát chạy lúc 2h sáng mỗi ngày:
            // so sánh Invoice.PaidAmount với SUM(ReceiptDetail.AmountApplied)
            // để phát hiện sớm mọi sai lệch do lỗi logic code
            RecurringJob.AddOrUpdate<IReconciliationService>(
                "daily-reconciliation",
                service => service.ReconcileAsync(),
                "0 2 * * *",
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                }
                );

            app.Run();
        }
    }
}
