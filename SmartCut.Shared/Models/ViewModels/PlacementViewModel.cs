using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models.ViewModels
{
    public class PlacementViewModel
    {
        public long OrderLineId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
        public float H { get; set; }
        public float L { get; set; }
        public string Color { get; set; } = "#007bff"; 
    }
}
