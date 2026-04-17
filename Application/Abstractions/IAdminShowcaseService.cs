using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminShowcaseService
{
    Task<ShowcaseListViewModel> GetShowcasesAsync(CancellationToken cancellationToken = default);
    Task<ShowcaseFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default);
    Task<ShowcaseFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ShowcaseFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ShowcaseFormViewModel model, CancellationToken cancellationToken = default);
}
