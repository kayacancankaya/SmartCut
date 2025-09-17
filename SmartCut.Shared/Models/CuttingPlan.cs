using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models
{
    public class CuttingPlan
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        public string Explanation { get; set; } = string.Empty;

        public List<CutEntry> CutEntries { get; set; } = new();
        [Required]
        public float ScrapVolume { get; set; }
        [Required]
        public float PercentFulfilled { get; set; }
    }

}
