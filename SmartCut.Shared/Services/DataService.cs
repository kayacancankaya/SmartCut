using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCut.Shared.Data;
using SmartCut.Shared.Interfaces;
using SmartCut.Shared.Models;
using System.Collections.Generic;
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
        public async Task<bool> CreateOrderAsync(OrderDTO orderDTO)
        {
            try
            {
                float quantity = 0f;
                int invoiceItemCount = 0;
                //check if order exists
                var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.InvoiceNumber == orderDTO.InvoiceNumber);
                if (existingOrder != null)
                {
                    quantity = await _context.Orders.FirstOrDefaultAsync(o => o.InvoiceNumber == orderDTO.InvoiceNumber).ContinueWith(t => t.Result.TotalQuantity);
                }
                    
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
                        CreatedAt = DateTime.Now,
                        CreatedBy = string.Empty,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = string.Empty,
                    };
                    orders.Add(order);
                }
                await _context.Orders.AddRangeAsync(orders);
                await _context.SaveChangesAsync();

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> ImportBlocksAsync(List<BlockDTO> blockDTOs)
        {
            try
            {
                if (blockDTOs == null) return false;
                if (blockDTOs.Count <= 0) return false;
                List<Block> blocks = new();

                foreach (var dto in blockDTOs)
                {
                    var block = new Block
                    {
                        Name = dto.Name ?? string.Empty,
                        Normalized_Name = dto.Name.ToUpperInvariant(),
                        Width = dto.Width,
                        Length = dto.Length,
                        Height = dto.Height,
                        Description = dto.Description ?? string.Empty,
                        Normalized_Description = dto.Description?.ToUpperInvariant() ?? string.Empty,
                        Material = dto.Material ?? string.Empty,
                        Normalized_Material = dto.Material?.ToUpperInvariant() ?? string.Empty,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    blocks.Add(block);
                }

                await _context.Blocks.AddRangeAsync(blocks);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> ImportOrdersAsync(List<OrderDTO> orderDTOs)
        {
            try
            {
                if (orderDTOs == null) return false;
                if (orderDTOs.Count <= 0) return false;
                bool result = await InsertOrdersAsync(orderDTOs);
                if (!result)
                {
                    await ImportOrdersCallBack(orderDTOs);
                    return false;
                }
                result = await InsertOrderLinesAsync(orderDTOs);
                if (!result)
                {
                    await ImportOrdersCallBack(orderDTOs);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message.ToString());
                return false;
            }
        }
        private async Task<bool> InsertOrdersAsync(List<OrderDTO> orderDTOs)
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
                        CreatedAt = DateTime.Now,
                        CreatedBy = string.Empty,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = string.Empty,
                    };
                    orders.Add(order);
                }
                await _context.Orders.AddRangeAsync(orders);
                await _context.SaveChangesAsync();

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
        private async Task<bool> InsertOrderLinesAsync(List<OrderDTO> orderDTOs)
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
                        //CreatedAt = DateTime.Now,
                        //CreatedBy = string.Empty,
                        //UpdatedAt = DateTime.Now,
                        //UpdatedBy = string.Empty,

                    };
                    orderLines.Add(orderLine);
                }
                await _context.OrderLines.AddRangeAsync(orderLines);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
        private async Task ImportOrdersCallBack(List<OrderDTO> orderDTOs)
        {
            try
            {
                IEnumerable<string> distinctInvoices = orderDTOs.Select(o => o.InvoiceNumber).Distinct().ToList();
                List<Order> orders = new();
                List<OrderLine> orderLines = new();
                foreach (var invoice in distinctInvoices)
                {
                    var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.InvoiceNumber == invoice);
                    var existingOrderLines = await _context.OrderLines.Where(i => i.InvoiceNumber == invoice).ToListAsync();
                    if (existingOrder != null)
                        orders.Add(existingOrder);
                    if (existingOrderLines != null)
                    {
                        if(existingOrderLines.Count > 0)
                            orderLines.AddRange(existingOrderLines);
                    }
                }
                if (orderLines.Count > 0)
                    _context.OrderLines.RemoveRange(orderLines);
                if (orders.Count > 0)
                    _context.Orders.RemoveRange(orders);

                return; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return;
            }
        }
    }
}
