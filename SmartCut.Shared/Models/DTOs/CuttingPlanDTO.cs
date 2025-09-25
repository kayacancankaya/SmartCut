using System.ComponentModel.DataAnnotations;

namespace SmartCut.Shared.Models.DTOs
{
    public class CuttingPlanDTO
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public Block? Block { get; set; } = new();
        public string Explanation { get; set; } = string.Empty;
        public List<CutEntryDTO> CutEntries { get; set; } = new();
        public List<CutEntryDTO> NotCuttedEntries { get; set; } = new();
        public float TotalVolume { get; set; }
        public float ScrapVolume { get; set; }
        [Required]
        public float PercentFulfilled { get; set; }
        public bool ShowDetails { get; set; } = false;
        public bool ShowNotIncludedDetails { get; set; } = false;
    }
}
