using Microsoft.AspNetCore.Http;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminMediaLibraryService
{
    Task<MediaLibraryViewModel> GetLibraryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaAssetListItemViewModel>> GetLatestItemsAsync(int take = 48, CancellationToken cancellationToken = default);
    Task<MediaAssetListItemViewModel> UploadAsync(IFormFile file, string? slug, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(MediaAssetUpdateInputModel model, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
