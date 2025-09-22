using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models
{
    public class CalculationDTO
    {
        public int BlockId { get; set; }
        public List<int> OrderLineIDs { get; set; } = new List<int>();
    }
}
