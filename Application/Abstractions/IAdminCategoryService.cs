using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminCategoryService
{
    Task<CategoryListViewModel> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default);
    Task<CategoryFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default);
}
