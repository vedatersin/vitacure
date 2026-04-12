using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminProductService
{
    Task<ProductListViewModel> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default);
    Task<ProductFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);
}
