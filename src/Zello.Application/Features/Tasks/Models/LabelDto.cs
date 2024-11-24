using System.ComponentModel.DataAnnotations;

namespace Zello.Application.Features.Tasks.Models;

public class LabelDto {
    public LabelDto() {
        Name = string.Empty;
        Color = string.Empty;
    }

    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$",
        ErrorMessage = "Color must be a valid hex color code (e.g., #FF0000)")]
    public string Color { get; set; }
}
