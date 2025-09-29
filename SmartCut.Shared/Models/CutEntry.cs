using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCut.Shared.Models
{
    public class CutEntry
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        [Required]
        public int OrderLineId { get; set; }
        [Required]
        public int CuttingPlanId { get; set; }
        [Required]
        public int QuantityFulfilled { get; set; }
        [Required]
        public bool IsFulfilled { get; set; } = false;

        public List<Position> Positions { get; set; } = new();
        public Dimension? Dimension { get; set; } = new();

        public OrderLine? OrderLine { get; set; }

        [ForeignKey(nameof(CuttingPlanId))]
        public CuttingPlan? CuttingPlan { get; set; }
    }
}
