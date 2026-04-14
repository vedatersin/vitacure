using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface ICacheObservabilityService
{
    void RecordLookup(bool hit);
    void RecordWrite();
    void RecordTagEviction(string tag);
    CacheMetricsViewModel GetSnapshot();
}
