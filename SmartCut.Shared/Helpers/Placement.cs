using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Helpers
{


    public sealed class Placement
    {
        // Order line identifier satisfied by this placement
        public int OrderLineId { get; set; }

        // Position of the placed part within the block (mm), aligned to free-box origin
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        // Final oriented dimensions of the placed part (mm)
        public float W { get; set; }
        public float H { get; set; }
        public float L { get; set; }
        public bool IsPlaced { get; set; } = false;
    }
}
