using System;
using System.Threading.RateLimiting;
using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Cozulmus rate limit degerlerinden algoritmaya uygun partition limiter'i olusturur.
/// </summary>
// islevi: Policy'nin HTTP ve metadata kararlarindan bagimsiz olarak limiter seceneklerini kurar.
// sistemdeki gorevi: Algoritma bazli limiter olusturma ayrintilarini RateLimitPolicy disinda tek yerde toplar.
internal static class RateLimitPartitionFactory
{
    // Algoritma belirtecine gore uygun partition limiter'ini olusturur.
    public static RateLimitPartition<string> Create(
        string algorithm,
        string key,
        int permitLimit,
        int windowSeconds,
        int queueLimit,
        int segmentsPerWindow,
        int tokensPerPeriod)
    {
        return algorithm switch
        {
            RateLimitAlgorithm.SlidingWindow => SlidingWindow(
                key,
                permitLimit,
                windowSeconds,
                queueLimit,
                segmentsPerWindow),
            RateLimitAlgorithm.TokenBucket => TokenBucket(
                key,
                permitLimit,
                windowSeconds,
                queueLimit,
                tokensPerPeriod),
            RateLimitAlgorithm.Concurrency => Concurrency(key, permitLimit, queueLimit),
            _ => FixedWindow(key, permitLimit, windowSeconds, queueLimit)
        };
    }

    // Sabit pencere limiter'ini kurar.
    private static RateLimitPartition<string> FixedWindow(
        string key,
        int permitLimit,
        int windowSeconds,
        int queueLimit)
    {
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueLimit = queueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
    }

    // Kayan pencere limiter'ini kurar.
    private static RateLimitPartition<string> SlidingWindow(
        string key,
        int permitLimit,
        int windowSeconds,
        int queueLimit,
        int segmentsPerWindow)
    {
        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            SegmentsPerWindow = segmentsPerWindow,
            QueueLimit = queueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
    }

    // Token kova limiter'ini kurar.
    private static RateLimitPartition<string> TokenBucket(
        string key,
        int tokenLimit,
        int periodSeconds,
        int queueLimit,
        int tokensPerPeriod)
    {
        return RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = tokenLimit,
            TokensPerPeriod = tokensPerPeriod,
            ReplenishmentPeriod = TimeSpan.FromSeconds(periodSeconds),
            QueueLimit = queueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
    }

    // Es zamanlilik limiter'ini kurar; pencere kullanmaz.
    private static RateLimitPartition<string> Concurrency(string key, int permitLimit, int queueLimit)
    {
        return RateLimitPartition.GetConcurrencyLimiter(key, _ => new ConcurrencyLimiterOptions
        {
            PermitLimit = permitLimit,
            QueueLimit = queueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }
}
