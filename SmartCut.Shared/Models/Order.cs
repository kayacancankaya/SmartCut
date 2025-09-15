using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartCut.Shared.Models
{
    public class Order
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderId { get; set; }
        [Required]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        public int BranchCode { get; set; } = 0;
        [Required]
        public char BillType { get; set; } = 'I';
        [Required]
        [MaxLength(450)]
        public string InvoiceNumber { get; set; } = string.Empty;
        [Required]
        [MaxLength(450)]
        public string CustomerCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string Status { get; set; } = "Producing";

        [MaxLength(450)]
        public string? CustomerName { get; set; } = string.Empty;
        [MaxLength(450)]
        public string? Normalized_CustomerName { get; set; } = string.Empty;
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime? DueDate { get; set; } = DateTime.Now.AddMonths(2);
        public float GrossAmount { get; set; } = 0;
        public float SalesDiscount { get; set; } = 0;
        public float VAT { get; set; } = 0;
        public string? Description { get; set; } = string.Empty;
        public string? Normalized_Description { get; set; } = string.Empty;
        [Required]
        [MaxLength(3)]
        public string? CurrencyType { get; set; } = string.Empty;
        public float CurrencyAmount { get; set; }

        [MaxLength(100)]
        public string? WarehouseReceiptNumber { get; set; }
        public char? Code1 { get; set; }
        public char? Code2 { get; set; }
        public char? Code3 { get; set; }
        public char? Code4 { get; set; }
        public char? Code5 { get; set; }
        public short? PaymentDay { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime? PaymentDate { get; set; } =DateTime.MinValue;
        public char? VATIncludedFlag { get; set; }             // KDV_DAHILMI - char(1) NULL
        public short? InvoiceItemCount { get; set; }           // faturadaki kalemler - smallint NULL
        public float TotalQuantity { get; set; } = 0;
        public float TotalAmount { get; set; } = 0;

        [MaxLength(100)]
        public string? PlanCode { get; set; }                      // PLA_KODU - varchar(8) NULL   
        public bool IsActive { get; set; } = true;
        public string? Reserved1 { get; set; }
        public string? Reserved2 { get; set; }
        public string? Normalized_Reserved1 { get; set; }
        public string? Normalized_Reserved2 { get; set; }
        public float FReserved3 { get; set; }
        public float FReserved4 { get; set; }
        public float FReserved5 { get; set; }
        public char? CReserved6 { get; set; }
        public byte? BReserved7 { get; set; }
        public short? IReserved8 { get; set; }
        public int? LReserved9 { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? DReserved10 { get; set; } = DateTime.MinValue;

        [MaxLength(100)]
        public string? ProjectCode { get; set; }
        [MaxLength(100)]
        public string? ConditionCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? PriceDate { get; set; } = DateTime.MinValue;
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? ConditionDate { get; set; } = DateTime.MinValue;
        public byte? ExportType { get; set; }
        [MaxLength(450)]
        public string? CustomsNumber { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? CustomsDate { get; set; } = DateTime.MinValue;
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? ExecutionDate { get; set; } = DateTime.MinValue;
        [MaxLength(450)]
        public string? ExportReferenceNumber { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        [MaxLength(450)]
        public string UpdatedBy { get; set; } = string.Empty;
        public short? IncomingBranchCode { get; set; }
        public short? OutgoingBranchCode { get; set; }
        public char ApprovalType { get; set; } = 'A';
        public int ApprovalNumber { get; set; }
        public short BusinessCode { get; set; }

        [MaxLength(8)]
        public string? PaymentCode { get; set; }
        public float CostUnit { get; set; }
        public short? PaymentDueDay { get; set; }
        [MaxLength(450)]
        public string? JobCode { get; set; }
        [MaxLength(450)]
        public string? GovernmentInvoiceNumber { get; set; }
        public int? ElectronicDocument { get; set; }
        [MaxLength(450)]
        public string? ExternalAppId { get; set; }
        [MaxLength(450)]
        public string? ExternalReferenceId { get; set; }
        public int? HalfFat { get; set; }
        public float InvoiceAltM3 { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? DovBazTar { get; set; } = DateTime.MinValue;
        public float OTVTevTutar { get; set; }
        public short? TopGirDepo { get; set; }
        public char? BForm { get; set; }
        public int? WithholdingRefund { get; set; }
        public char? NotInvoice { get; set; }
        public int? Accommodation { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}