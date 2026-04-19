using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminCollectionService
{
    Task<CollectionListViewModel> GetCollectionsAsync(CancellationToken cancellationToken = default);
    Task<CollectionFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default);
    Task<CollectionFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CollectionFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(CollectionFormViewModel model, CancellationToken cancellationToken = default);
}
