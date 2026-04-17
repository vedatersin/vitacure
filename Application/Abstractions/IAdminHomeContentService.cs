using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminHomeContentService
{
    Task<HomeContentFormViewModel> GetModelAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(HomeContentFormViewModel model, CancellationToken cancellationToken = default);
}
