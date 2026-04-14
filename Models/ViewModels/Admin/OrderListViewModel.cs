namespace vitacure.Models.ViewModels.Admin;

public class AdminOrderListViewModel
{
    public string? SearchTerm { get; set; }
    public string StatusFilter { get; set; } = "all";
    public string VolumeFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public IReadOnlyList<AdminOrderListItemViewModel> Orders { get; set; } = Array.Empty<AdminOrderListItemViewModel>();
}

public class AdminOrderListItemViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
