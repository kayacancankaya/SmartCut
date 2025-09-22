using SmartCut.Shared.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models.ViewModels
{
    public class CalculationViewModel
    {
        public int Id { get; set; }
        public int OrderLineId { get; set; }
        public int QuantityFulfilled { get; set; }
        public OrderDTO Order { get; set; } = new();
        public List<Position> Positions { get; set; } = new();
        public Dimension? Dimension { get; set; } = new();
        public Block? Block { get; set; } = new();
        public bool ShowPositions { get; set; } = false;
        public bool ShowOrders { get; set; } = false;
    }
}
