using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models.DTOs
{
    public class BlockDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string? Material { get; set; } = string.Empty;
    }
}
