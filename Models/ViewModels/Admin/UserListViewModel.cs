namespace vitacure.Models.ViewModels.Admin;

public class UserListViewModel
{
    public string? SearchTerm { get; set; }
    public string AccountTypeFilter { get; set; } = "all";
    public string StatusFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int CustomerCount { get; set; }
    public int BackOfficeCount { get; set; }
    public IReadOnlyList<UserListItemViewModel> Users { get; set; } = Array.Empty<UserListItemViewModel>();
}

public class UserListItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccountTypeLabel { get; set; } = string.Empty;
    public string RoleSummary { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
