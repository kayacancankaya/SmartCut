using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Helpers
{
    public sealed class UnitRequest
    {
        // Order line identifier this unit belongs to
        public long OrderLineId { get; set; }

        // Dimensions (mm) after rotation decision during packing loop
        public float W { get; set; }
        public float H { get; set; }
        public float L { get; set; }

        // Precomputed volume for sorting (W*H*L of the original part)
        public double Volume { get; set; }
    }

}
