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

        public bool HasErrors => !string.IsNullOrWhiteSpace(Name)
            || !string.IsNullOrWhiteSpace(Description)
            || !string.IsNullOrWhiteSpace(Width)
            || !string.IsNullOrWhiteSpace(Length)
            || !string.IsNullOrWhiteSpace(Height);
    }
}
