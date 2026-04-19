using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminStorageSettingsService
{
    Task<StorageSettingsFormViewModel> GetModelAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(StorageSettingsFormViewModel model, CancellationToken cancellationToken = default);
}
