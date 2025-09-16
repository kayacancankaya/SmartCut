using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Helpers
{
    public sealed class Orientation
    {
        public float W { get; set; }
        public float H { get; set; }
        public float L { get; set; }

        public Orientation() { }

        public Orientation(float w, float h, float l)
        {
            W = w;
            H = h;
            L = l;
        }

        public double Volume => (double)W * H * L;

        public float[] ToArray() => new[] { W, H, L };

        // Utility: generate all 6 axis-aligned rotations for a box (w,h,l)
        public static IEnumerable<Orientation> All(float w, float h, float l)
        {
            yield return new Orientation(w, h, l);
            yield return new Orientation(w, l, h);
            yield return new Orientation(h, w, l);
            yield return new Orientation(h, l, w);
            yield return new Orientation(l, w, h);
            yield return new Orientation(l, h, w);
        }
    }
}
