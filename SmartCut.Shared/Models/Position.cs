using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SmartCut.Shared.Models
{
    public class Position
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
        public int CutEntryId { get; set; }
        [Required]
        public int QuantityLine { get; set; }
        [Required]
        public float X { get; set; }
        [Required]
        public float Y { get; set; }
        [Required]
        public float Z { get; set; }
        [Required]
        public float Ex { get; set; }
        [Required]
        public float Ey { get; set; }
        [Required]
        public float Ez { get; set; }


    }
}
