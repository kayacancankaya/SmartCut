using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using MySqlConnector;
using SmartCut.Shared.Data;
using SmartCut.Shared.Interfaces;
using SmartCut.Shared.Models;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace SmartCut.Shared.Services
{
    public class DataService : IDataService
    {
        private readonly ILogger<DataService> _logger;
        private readonly ApplicationDbContext _context;
        public DataService(ILogger<DataService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<bool> CreateBlockAsync(Block block)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> ImportOrdersAsync(List<OrderDTO> orderDTOs)
        {
            try
            {
                if (orderDTOs == null) return false;
                if (orderDTOs.Count <= 0) return false;
                List<Order> orders = MapOrderDTOToOrder(orderDTOs);
                if (orders == null) return false;
                if (orders.Count <= 0) return false;
                var result = await BulkUpsertOrdersAsync(orders);
                if (!result) return false;
                List<OrderLine> orderLines = MapOrderDTOToOrderLine(orderDTOs);
                result = await BulkInsertOrderLinesAsync(orderLines);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message.ToString());
                return false;
            }
        }
        private List<Order> MapOrderDTOToOrder(List<OrderDTO> orderDTOs)
        {
            try
            {

                IEnumerable<string> distinctInvoices = orderDTOs.Select(o => o.InvoiceNumber).Distinct().ToList();
                string customerCode = string.Empty;
                string customerName = string.Empty;
                float quantity = 0f;
                int invoiceItemCount = 0;

                List<Order> orders = new();
                foreach (var invoice in distinctInvoices)
                {
                    customerCode = orderDTOs.Where(o => o.InvoiceNumber == invoice).Select(o => o.CustomerCode).FirstOrDefault() ?? string.Empty;
                    customerName = orderDTOs.Where(o => o.InvoiceNumber == invoice).Select(o => o.CustomerName).FirstOrDefault() ?? string.Empty;
                    quantity = orderDTOs.Where(o => o.InvoiceNumber == invoice).Sum(o => o.Quantity);
                    invoiceItemCount = orderDTOs.Where(o => o.InvoiceNumber == invoice).Count();
                    var order = new Order
                    {
                        InvoiceNumber = invoice,
                        CustomerCode = customerCode,
                        CustomerName = customerName,
                        Normalized_CustomerName = customerName.ToUpperInvariant(),
                        Date = DateTime.Now,
                        DueDate = DateTime.Now.AddMonths(2),
                        TotalQuantity = quantity,
                        InvoiceItemCount = (short)invoiceItemCount,

                    };
                    orders.Add(order);
                }
                return orders;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<Order>();
            }
        }
        private List<OrderLine> MapOrderDTOToOrderLine(List<OrderDTO> orderDTOs)
        {
            try
            {
                List<OrderLine> orderLines = new();
                foreach (var order in orderDTOs)
                {
                    var orderLine = new OrderLine
                    {
                        InvoiceNumber = order.InvoiceNumber,
                        Line = order.Line,
                        StockCode = order.StockCode ?? string.Empty,
                        StockName = order.StockName ?? string.Empty,
                        Normalized_StockName = order.StockName?.ToUpperInvariant() ?? string.Empty,
                        CustomerCode = order.CustomerCode ?? string.Empty,
                        CustomerName = order.CustomerName ?? string.Empty,
                        Normalized_CustomerName = order.CustomerName?.ToUpperInvariant(),
                        Description = order.Description ?? string.Empty,
                        Quantity = order.Quantity,
                        DueDate = DateTime.Now.AddMonths(2),
                        Date = DateTime.Now,
                        Width = order.Width,
                        Length = order.Length,
                        Height = order.Height,

                    };
                    orderLines.Add(orderLine);
                }
                return orderLines;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<OrderLine>();
            }
        }
        private async Task<bool> BulkUpsertOrdersAsync(List<Order> orders)
        {
            try
            {
                var values = string.Join(", ", orders.Select((o, i) =>
                                                        $"(@InvoiceNumber{i}, @CustomerCode{i}, @CustomerName{i}, @NormalizedCustomerName{i}, @Date{i}, @DueDate{i}, @TotalQuantity{i}, @InvoiceItemCount{i})"));

                string sql = $@"
                    MERGE INTO Orders AS Target
                    USING (VALUES {values}) 
                           AS Source (InvoiceNumber, CustomerCode, CustomerName, Normalized_CustomerName, Date, DueDate, TotalQuantity, InvoiceItemCount)
                    ON Target.InvoiceNumber = Source.InvoiceNumber
                    WHEN MATCHED THEN
                        UPDATE SET 
                            CustomerCode = Source.CustomerCode,
                            CustomerName = Source.CustomerName,
                            Normalized_CustomerName = Source.Normalized_CustomerName,
                            Date = Source.Date,
                            DueDate = Source.DueDate,
                            TotalQuantity = Source.TotalQuantity,
                            InvoiceItemCount = Source.InvoiceItemCount
                    WHEN NOT MATCHED THEN
                        INSERT (InvoiceNumber, CustomerCode, CustomerName, Normalized_CustomerName, Date, DueDate, TotalQuantity, InvoiceItemCount)
                        VALUES (Source.InvoiceNumber, Source.CustomerCode, Source.CustomerName, Source.Normalized_CustomerName, Source.Date, Source.DueDate, Source.TotalQuantity, Source.InvoiceItemCount);";
                var parameters = orders.SelectMany((o, i) => new[]
                {
                    new MySqlParameter($"@InvoiceNumber{i}", o.InvoiceNumber),
                    new MySqlParameter($"@CustomerCode{i}", o.CustomerCode ?? (object)DBNull.Value),
                    new MySqlParameter($"@CustomerName{i}", o.CustomerName ?? (object)DBNull.Value),
                    new MySqlParameter($"@NormalizedCustomerName{i}", o.Normalized_CustomerName ?? (object)DBNull.Value),
                    new MySqlParameter($"@Date{i}", o.Date),
                    new MySqlParameter($"@DueDate{i}", o.DueDate),
                    new MySqlParameter($"@TotalQuantity{i}", o.TotalQuantity),
                    new MySqlParameter($"@InvoiceItemCount{i}", o.InvoiceItemCount),
                }).ToArray();

                int rowsAff = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                if (rowsAff <= 0) return false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
        private async Task<bool> BulkInsertOrderLinesAsync(List<OrderLine> orderLines)
        {
            try
            {
                if (orderLines == null) return false;
                if (orderLines.Count <= 0) return false;
                var values = orderLines.Select((o, i) =>
                                                        $"(@InvoiceNumber{i},@Line{i}, @CustomerCode{i}, @CustomerName{i}, @Normalized_CustomerName{i}, @StockCode{i}, @StockName{i}, @Normalized_StockName{i}, " +
                                                        $"@Date{i}, @DueDate{i}, @Width{i}, @Length{i},@Height{i},@Quantity{i},@Description{i})");
                string sql = $@"
                    MERGE INTO OrderItems AS Target
                    USING (VALUES {values}) 
                           AS Source (InvoiceNumber,Line, CustomerCode, CustomerName, Normalized_CustomerName, StockCode, StockName, Normalized_StockName,Date,DueDate, Width, Length,Height,Quantity,Description)
                    ON Target.InvoiceNumber = Source.InvoiceNumber and Target.Line=Source.Line
                    WHEN MATCHED THEN
                        UPDATE SET 
                            CustomerCode = Source.CustomerCode,
                            CustomerName = Source.CustomerName,
                            Normalized_CustomerName = Source.Normalized_CustomerName,
                            Date = Source.Date,
                            DueDate = Source.DueDate,
                            StockCode = Source.StockCode,
                            StockName = Source.StockName,
                            Normalized_StockName = Source.Normalized_StockName,
                            Width = Source.Width,   
                            Length = Source.Length,
                            Height = Source.Height,
                            Quantity = Source.Quantity,
                            Description = Source.Description
                    WHEN NOT MATCHED THEN
                        INSERT (InvoiceNumber,Line, CustomerCode, CustomerName, Normalized_CustomerName,StockCode, StockName, Normalized_StockName, Date, DueDate, Width,Length,Height,Quantity,Description)
                        VALUES (Source.InvoiceNumber, Source.CustomerCode, Source.CustomerName, Source.Normalized_CustomerName,Source.StockCode, Source.StockName, Source.Normalized_StockName, Source.Date, Source.DueDate,Source.Width,Source.Length,Source.Height,Source.Quantity,Source.Description );";

                var parameters = orderLines.SelectMany((o, i) => new[]
                {
                    new MySqlParameter($"@InvoiceNumber{i}", o.InvoiceNumber),
                    new MySqlParameter($"@CustomerCode{i}", o.CustomerCode ?? (object)DBNull.Value),
                    new MySqlParameter($"@CustomerName{i}", o.CustomerName ?? (object)DBNull.Value),
                    new MySqlParameter($"@Normalized_CustomerName{i}", o.Normalized_CustomerName ?? (object)DBNull.Value),
                    new MySqlParameter($"@StockCode{i}", o.StockCode ?? (object)DBNull.Value),
                    new MySqlParameter($"@StockName{i}", o.StockName ?? (object)DBNull.Value),
                    new MySqlParameter($"@Normalized_StockName{i}", o.Normalized_StockName ?? (object)DBNull.Value),
                    new MySqlParameter($"@Date{i}", o.Date),
                    new MySqlParameter($"@DueDate{i}", o.DueDate),
                    new MySqlParameter($"@Width{i}", o.Width),
                    new MySqlParameter($"@Length{i}", o.Length),
                    new MySqlParameter($"@Height{i}", o.Height),
                    new MySqlParameter($"@Quantity{i}", o.Quantity),
                    new MySqlParameter($"@Description{i}", o.Description),

                }).ToArray();

                int rowsAff = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                if (rowsAff <= 0) return false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
    }
}
