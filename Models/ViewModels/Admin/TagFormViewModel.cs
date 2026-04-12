using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class TagFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Etiket Adı")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;
}
