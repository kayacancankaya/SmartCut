using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models.DTOs
{
    public class CutEntryDTO
    {
        public long Id { get; set; }
        public long OrderLineId { get; set; }
        public int QuantityFulfilled { get; set; }
        public OrderDTO Order { get; set; } = new();
        public List<Position> Positions { get; set; } = new();
        public Dimension? Dimension { get; set; } = new();

    }
}
