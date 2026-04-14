namespace vitacure.Models.ViewModels.Admin;

public class AdminBreadcrumbItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsActive { get; set; }
}

public class AdminPageHeroViewModel
{
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconClass { get; set; } = "fa-solid fa-grid-2";
    public IReadOnlyList<AdminBreadcrumbItemViewModel> Breadcrumbs { get; set; } = Array.Empty<AdminBreadcrumbItemViewModel>();
    public IReadOnlyList<AdminHeroStatViewModel> Stats { get; set; } = Array.Empty<AdminHeroStatViewModel>();
    public string? AsideTitle { get; set; }
    public string? AsideText { get; set; }
}

public class AdminFilterBarViewModel
{
    public string ActionUrl { get; set; } = string.Empty;
    public string SearchName { get; set; } = "q";
    public string SearchPlaceholder { get; set; } = string.Empty;
    public string? SearchValue { get; set; }
    public string SummaryLabel { get; set; } = string.Empty;
    public IReadOnlyList<AdminFilterSelectViewModel> Selects { get; set; } = Array.Empty<AdminFilterSelectViewModel>();
    public string? PrimaryActionLabel { get; set; }
    public string? PrimaryActionUrl { get; set; }
}

public class AdminFilterSelectViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public IReadOnlyList<AdminFilterOptionViewModel> Options { get; set; } = Array.Empty<AdminFilterOptionViewModel>();
}

public class AdminFilterOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class AdminHeroStatViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? ToneClass { get; set; }
}
