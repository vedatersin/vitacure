using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminTagService
{
    Task<TagListViewModel> GetTagsAsync(CancellationToken cancellationToken = default);
    Task<TagFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default);
    Task<TagFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(TagFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TagFormViewModel model, CancellationToken cancellationToken = default);
}
