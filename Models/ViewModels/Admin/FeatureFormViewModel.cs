using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class FeatureFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Özellik Adi")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Grup")]
    public string GroupName { get; set; } = string.Empty;

    [Display(Name = "Seçenekler")]
    public string? OptionsContent { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
