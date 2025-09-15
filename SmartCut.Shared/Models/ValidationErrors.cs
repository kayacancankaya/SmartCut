using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models
{
    public class ValidationErrors
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Width { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
        public string Height { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string StockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public bool HasErrors => !string.IsNullOrWhiteSpace(Name)
            || !string.IsNullOrWhiteSpace(Description)
            || !string.IsNullOrWhiteSpace(Width)
            || !string.IsNullOrWhiteSpace(Length)
            || !string.IsNullOrWhiteSpace(Height)
            || !string.IsNullOrWhiteSpace(StockCode)
            || !string.IsNullOrWhiteSpace(StockName)
            || !string.IsNullOrWhiteSpace(CustomerCode)
            || !string.IsNullOrWhiteSpace(CustomerName)
            || !string.IsNullOrWhiteSpace(Line)
            || !string.IsNullOrWhiteSpace(Quantity)
            || !string.IsNullOrWhiteSpace(InvoiceNumber)
    }
}
