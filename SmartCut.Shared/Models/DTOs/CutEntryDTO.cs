namespace SmartCut.Shared.Models.DTOs
{
    public class CutEntryDTO
    {
        public int Id { get; set; }
        public int OrderLineId { get; set; }
        public float OrderQuantity { get; set; }
        public int QuantityFulfilled { get; set; }
        public float TotalVolume { get; set; }
        public OrderDTO Order { get; set; } = new();
        public List<Position> Positions { get; set; } = new();
        public Dimension? Dimension { get; set; } = new();
        public bool IsFulfilled { get; set; } = false;
        public bool ShowPositions { get; set; } = false;
        public bool ShowOrders { get; set; } = false;

    }
}
