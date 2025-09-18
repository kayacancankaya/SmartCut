using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCut.Shared.Models
{
    public class CutEntry
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public long OrderLineId { get; set; }
        [Required]
        public long CuttingPlanId { get; set; }
        [Required]
        public int QuantityFulfilled { get; set; }

        public List<Position> Positions { get; set; } = new();
        public Dimension? Dimension { get; set; } = new();

        public OrderLine? OrderLine { get; set; }

        [ForeignKey(nameof(CuttingPlanId))]
        public CuttingPlan? CuttingPlan { get; set; }
    }
}
