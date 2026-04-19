using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class BrandFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Marka Adi")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Aciklama")]
    public string? Description { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
