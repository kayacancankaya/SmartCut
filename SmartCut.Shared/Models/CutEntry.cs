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
        [ForeignKey("OrderLine")]
        public long OrderLineId { get; set; }
        [Required]
        public int QuantityFulfilled { get; set; }

        public ICollection<Position> Positions { get; set; } = new List<Position>();
        public Dimension? Dimensions { get; set; } 
    }
}
