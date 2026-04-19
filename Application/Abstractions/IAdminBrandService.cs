using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminBrandService
{
    Task<BrandListViewModel> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<BrandFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default);
    Task<BrandFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(BrandFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(BrandFormViewModel model, CancellationToken cancellationToken = default);
}
