using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class CacheObservabilityServiceTests
{
    [Fact]
    public void GetSnapshot_Returns_Expected_Counters_And_Status()
    {
        var service = new CacheObservabilityService();

        service.RecordLookup(hit: true);
        service.RecordLookup(hit: true);
        service.RecordLookup(hit: false);
        service.RecordWrite();
        service.RecordTagEviction("product");
        service.RecordTagEviction("category");

        var snapshot = service.GetSnapshot();

        Assert.Equal(3, snapshot.TotalLookups);
        Assert.Equal(2, snapshot.HitCount);
        Assert.Equal(1, snapshot.MissCount);
        Assert.Equal(1, snapshot.WriteCount);
        Assert.Equal(2, snapshot.EvictionCount);
        Assert.Equal("%67", snapshot.HitRateLabel);
        Assert.Equal("Isindi", snapshot.StatusLabel);
        Assert.Equal(new[] { "category", "product" }, snapshot.RecentEvictedTags);
    }
}
