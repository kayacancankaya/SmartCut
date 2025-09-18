using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models
{
    public class OrderLineDTO
    {
        public int Id { get; set; } // optional
        public int Width { get; set; }   // W (cm)
        public int Length { get; set; }  // L (cm)
        public int Height { get; set; }  // H (cm)
        public int Quantity { get; set; }
    }
}
