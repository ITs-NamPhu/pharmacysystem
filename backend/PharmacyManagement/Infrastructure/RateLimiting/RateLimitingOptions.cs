namespace PharmacyManagement.Infrastructure.RateLimiting
{
    /// <summary>
    /// Cấu hình rate limiting, bind từ section "RateLimiting" trong appsettings.json.
    /// </summary>
    public class RateLimitingSettings
    {
        /// <summary>Login: chống brute-force theo IP.</summary>
        public RateLimitRule Auth { get; set; } = new()
        {
            TokenLimit = 10,
            RefillTokens = 10,
            RefillPeriodSeconds = 60,
            QueueLimit = 0
        };

        /// <summary>Refresh token: theo IP.</summary>
        public RateLimitRule Refresh { get; set; } = new()
        {
            TokenLimit = 60,
            RefillTokens = 60,
            RefillPeriodSeconds = 60,
            QueueLimit = 0
        };

        /// <summary>Truy vấn nặng: dashboard (token bucket dùng chung) + AI + heavy GET.</summary>
        public RateLimitRule Expensive { get; set; } = new()
        {
            TokenLimit = 40,
            RefillTokens = 120,
            RefillPeriodSeconds = 60,
            QueueLimit = 20
        };

        public RateLimitRule Read { get; set; } = new()
        {
            TokenLimit = 60,
            RefillTokens = 300,
            RefillPeriodSeconds = 60,
            QueueLimit = 20
        };

        public RateLimitRule Write { get; set; } = new()
        {
            TokenLimit = 20,
            RefillTokens = 60,
            RefillPeriodSeconds = 60,
            QueueLimit = 5
        };

        public RateLimitRule Global { get; set; } = new()
        {
            TokenLimit = 30,
            RefillTokens = 120,
            RefillPeriodSeconds = 60,
            QueueLimit = 10
        };

        /// <summary>Dashboard: sliding window.</summary>
        public SlidingWindowRule Dashboard { get; set; } = new()
        {
            PermitLimit = 120,
            WindowSeconds = 60,
            SegmentsPerWindow = 6,
            QueueLimit = 20
        };
    }

    public class RateLimitRule
    {
        public int TokenLimit { get; set; }

        public int RefillTokens { get; set; }

        public int RefillPeriodSeconds { get; set; }

        public int QueueLimit { get; set; }
    }

    public class SlidingWindowRule
    {
        public int PermitLimit { get; set; }

        public int WindowSeconds { get; set; }

        public int SegmentsPerWindow { get; set; }

        public int QueueLimit { get; set; }
    }
}
