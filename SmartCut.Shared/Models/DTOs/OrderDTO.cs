using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models.DTOs
{
    public class OrderDTO
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public int Line { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public float Height { get; set; }
        public float Quantity { get; set; }
        public string? StockCode { get; set; } = string.Empty;
        public string? StockName { get; set; } = string.Empty;
        public string? CustomerCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
    }
}
