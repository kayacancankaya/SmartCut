using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Helpers
{

    public sealed class FreeBox
    {
        // Origin of this free space in the block (mm)
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        // Dimensions of this free space (mm)
        public float W { get; set; }
        public float H { get; set; }
        public float L { get; set; }

        // Optional helper
        public double Volume => (double)W * H * L;
    }
}
