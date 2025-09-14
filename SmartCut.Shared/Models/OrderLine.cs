using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Models
{
    public class OrderLine
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [ForeignKey("Order")]
        public long OrderId { get; set; }
        [Required]
        [MaxLength(255)]
        public string? StockCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(450)]
        public string? StockName { get; set; } = string.Empty;
        [Required]
        [MaxLength(450)]
        public string? Normalized_StockName { get; set; } = string.Empty;
        [Required]
        [MaxLength(450)]
        public string? InvoiceNumber { get; set; } 
        [Required]
        public int Line { get; set; }
        [Required]
        public float Quantity { get; set; }
        public float Quantity2 { get; set; }
        public float ConversionRate { get; set; }

        [Required]
        public char? TransactionTypeCode { get; set; } = 'E';
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public float UnitPrice { get; set; } // STHAR_BF
        [Required]
        public float NetPrice { get; set; } // STHAR_NF
        [Required]
        public float Discount { get; set; } // STHAR_IAF
        [Required]
        public float VAT { get; set; } // STHAR_KDV

        [Required]
        public short? WarehouseCode { get; set; } = 0; // STHAR_DEPO
        public string? Description { get; set; } = string.Empty;// STHAR_ACIKLAMA
        public string? Normalized_Description { get; set; } = string.Empty;// STHAR_ACIKLAMA

        public float SalePrice1 { get; set; } // STHAR_SATISK
        public float CostPrice1 { get; set; } // STHAR_MALFISK
        [Required]
        public char? InvoiceType { get; set; } = 'I';
        [Required]
        [MaxLength(3)]

        public string CurrencyType { get; set; } = "USD"; 


        public short? PaymentDay { get; set; } // STHAR_ODEGUN

        [Required]
        [MaxLength(450)]
        public string CustomerCode { get; set; } = string.Empty; // STHAR_CARI
        [Required]
        [MaxLength(450)]
        public string CustomerName { get; set; } = string.Empty; 
        [Required]
        [MaxLength(450)]
        public string Normalized_CustomerName { get; set; } = string.Empty; 
        public int SalespersonCode { get; set; }
        [Required]
        public char ApprovalStatus { get; set; } = 'A';
        [Required]
        public int BranchCode { get; set; } = 0;
        public DateTime? DueDate { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public float Height { get; set; }
        public bool IsActive { get; set; } = true;
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime? CreatedAt { get; set; }
        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(450)]
        public string UpdatedBy { get; set; } = string.Empty;
        public Order? Order { get; set; }
    }
}
