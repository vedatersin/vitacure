using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminFeatureService
{
    Task<FeatureListViewModel> GetFeaturesAsync(CancellationToken cancellationToken = default);
    Task<FeatureFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default);
    Task<FeatureFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(FeatureFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(FeatureFormViewModel model, CancellationToken cancellationToken = default);
}
