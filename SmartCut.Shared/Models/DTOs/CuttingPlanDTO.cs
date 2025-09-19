using System.ComponentModel.DataAnnotations;

namespace SmartCut.Shared.Models.DTOs
{
    public class CuttingPlanDTO
    {
        public long Id { get; set; }
        public int Status { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public List<CutEntryDTO> CutEntries { get; set; } = new();
        public float ScrapVolume { get; set; }
        [Required]
        public float PercentFulfilled { get; set; }
        public bool ShowDetails { get; set; } = false;
    }
}
