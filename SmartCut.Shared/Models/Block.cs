using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartCut.Shared.Models
{
    public class Block
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        [Required(
        ErrorMessageResourceType = typeof(Resources.Localization.AppResource),
        ErrorMessageResourceName = "Block Name is Required"
        )]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string Normalized_Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;
        public string? Normalized_Description { get; set; } = string.Empty;

        [Required(
        ErrorMessageResourceType = typeof(Resources.Localization.AppResource),
        ErrorMessageResourceName = "Length is required"
        )]
        public float Length { get; set; }
        [Required(
        ErrorMessageResourceType = typeof(Resources.Localization.AppResource),
        ErrorMessageResourceName = "Width is required"
        )]
        public float Width { get; set; }
        [Required(
        ErrorMessageResourceType = typeof(Resources.Localization.AppResource),
        ErrorMessageResourceName = "Height is required"
        )]
        public float Height { get; set; }
        public string? Material { get; set; } = string.Empty;
        public string? Normalized_Material { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [NotMapped]
        public bool IsSelected { get; set; } = false;
        public Block()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

    }
}
