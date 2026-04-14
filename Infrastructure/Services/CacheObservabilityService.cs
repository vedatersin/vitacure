using System.Collections.Concurrent;
using System.Threading;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class CacheObservabilityService : ICacheObservabilityService
{
    private readonly ConcurrentQueue<string> _recentEvictedTags = new();
    private int _totalLookups;
    private int _hitCount;
    private int _missCount;
    private int _writeCount;
    private int _evictionCount;

    public void RecordLookup(bool hit)
    {
        Interlocked.Increment(ref _totalLookups);

        if (hit)
        {
            Interlocked.Increment(ref _hitCount);
            return;
        }

        Interlocked.Increment(ref _missCount);
    }

    public void RecordWrite()
    {
        Interlocked.Increment(ref _writeCount);
    }

    public void RecordTagEviction(string tag)
    {
        Interlocked.Increment(ref _evictionCount);
        _recentEvictedTags.Enqueue(tag);

        while (_recentEvictedTags.Count > 5 && _recentEvictedTags.TryDequeue(out _))
        {
        }
    }

    public CacheMetricsViewModel GetSnapshot()
    {
        var totalLookups = Volatile.Read(ref _totalLookups);
        var hitCount = Volatile.Read(ref _hitCount);
        var missCount = Volatile.Read(ref _missCount);
        var writeCount = Volatile.Read(ref _writeCount);
        var evictionCount = Volatile.Read(ref _evictionCount);
        var hitRate = totalLookups == 0
            ? 0
            : (int)Math.Round((double)hitCount / totalLookups * 100, MidpointRounding.AwayFromZero);

        return new CacheMetricsViewModel
        {
            TotalLookups = totalLookups,
            HitCount = hitCount,
            MissCount = missCount,
            WriteCount = writeCount,
            EvictionCount = evictionCount,
            RecentEvictedTags = _recentEvictedTags.Reverse().ToArray(),
            HitRateLabel = $"%{hitRate}",
            StatusLabel = totalLookups == 0 && writeCount == 0
                ? "Soguk"
                : hitCount >= missCount
                    ? "Isindi"
                    : "Dengeleniyor",
            Detail = BuildDetail(totalLookups, hitCount, missCount, writeCount, evictionCount)
        };
    }

    private static string BuildDetail(int totalLookups, int hitCount, int missCount, int writeCount, int evictionCount)
    {
        if (totalLookups == 0 && writeCount == 0 && evictionCount == 0)
        {
            return "Henuz storefront cache trafigi olusmadi.";
        }

        return $"Toplam {totalLookups} okuma, {hitCount} hit, {missCount} miss, {writeCount} yazma ve {evictionCount} invalidation gozlemlendi.";
    }
}
