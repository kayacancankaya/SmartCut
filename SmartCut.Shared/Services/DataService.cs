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
            try
            {
                if (block == null) return false;
                block.Normalized_Name = block.Name.ToUpperInvariant();
                block.Normalized_Description = block.Description?.ToUpperInvariant() ?? string.Empty;
                block.Normalized_Material = block.Material?.ToUpperInvariant() ?? string.Empty;
                block.CreatedAt = DateTime.Now;
                block.UpdatedAt = DateTime.Now;
                await _context.Blocks.AddAsync(block);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message.ToString());
                return false;
            }
        }
        public async Task<int> CheckIfBlockExistsAsync(string blockName)
        {
            try
            {
               var existingBlock = await _context.Blocks.FirstOrDefaultAsync(b => b.Name == blockName);
                if (existingBlock != null)
                    return 0;
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return -1;
            }
        }
        public async Task<bool> CreateOrderAsync(OrderDTO orderDTO)
        {
            try
            {
                float quantity = 0f;
                int invoiceItemCount = 0;
                if (orderDTO == null) return false;
                if (string.IsNullOrEmpty(orderDTO.InvoiceNumber)) return false;
                if (orderDTO.Line <= 0) return false;
                if (orderDTO.Quantity <= 0) return false;
               bool lineExists = false;
                //check if order line exists
                var existingOrderLine = await _context.OrderLines.FirstOrDefaultAsync(o => o.InvoiceNumber == orderDTO.InvoiceNumber && o.Line == orderDTO.Line);
                if (existingOrderLine != null)
                {
                    lineExists = true;
                    existingOrderLine.Width = orderDTO.Width;
                    existingOrderLine.Length = orderDTO.Length;
                    existingOrderLine.Height = orderDTO.Height;
                    existingOrderLine.Quantity = orderDTO.Quantity;
                    existingOrderLine.Description = orderDTO.Description ?? string.Empty;
                    existingOrderLine.Normalized_Description = orderDTO.Description?.ToUpperInvariant() ?? string.Empty;
                    existingOrderLine.StockCode = orderDTO.StockCode ?? string.Empty;
                    existingOrderLine.StockName = orderDTO.StockName ?? string.Empty;
                    existingOrderLine.Normalized_StockName = orderDTO.StockName?.ToUpperInvariant() ?? string.Empty;
                    existingOrderLine.CustomerCode = orderDTO.CustomerCode ?? string.Empty;
                    existingOrderLine.CustomerName = orderDTO.CustomerName ?? string.Empty;
                    existingOrderLine.Normalized_CustomerName = orderDTO.CustomerName?.ToUpperInvariant() ?? string.Empty;
                    existingOrderLine.UpdatedAt = DateTime.Now;
                    _context.OrderLines.Update(existingOrderLine);
                    await _context.SaveChangesAsync();
                }
                //check if order exists
                var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.InvoiceNumber == orderDTO.InvoiceNumber);
                if (existingOrder != null)
                {
                    quantity = await _context.OrderLines.Where(o => o.InvoiceNumber == orderDTO.InvoiceNumber).SumAsync(o => o.Quantity);
                    quantity += orderDTO.Quantity;
                    if (!lineExists)
                        quantity += orderDTO.Quantity;
                    invoiceItemCount = await _context.OrderLines.Where(o => o.InvoiceNumber == orderDTO.InvoiceNumber).CountAsync();
                    if (!lineExists)
                        invoiceItemCount += 1;
                    existingOrder.TotalQuantity = quantity;
                    existingOrder.InvoiceItemCount = (short)invoiceItemCount;
                    existingOrder.UpdatedAt = DateTime.Now;
                    _context.Orders.Update(existingOrder);
                    if(!lineExists)
                    {
                        var orderLine = new OrderLine
                        {
                            InvoiceNumber = orderDTO.InvoiceNumber,
                            Line = orderDTO.Line,
                            StockCode = orderDTO.StockCode ?? string.Empty,
                            StockName = orderDTO.StockName ?? string.Empty,
                            Normalized_StockName = orderDTO.StockName?.ToUpperInvariant() ?? string.Empty,
                            CustomerCode = orderDTO.CustomerCode ?? string.Empty,
                            CustomerName = orderDTO.CustomerName ?? string.Empty,
                            Normalized_CustomerName = orderDTO.CustomerName?.ToUpperInvariant(),
                            Description = orderDTO.Description ?? string.Empty,
                            Quantity = orderDTO.Quantity,
                            DueDate = DateTime.Now.AddMonths(2),
                            Date = DateTime.Now,
                            Width = orderDTO.Width,
                            Length = orderDTO.Length,
                            Height = orderDTO.Height,
                        };
                        await _context.OrderLines.AddAsync(orderLine);
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    invoiceItemCount = 1;
                    var order = new Order
                    {
                        InvoiceNumber = orderDTO.InvoiceNumber,
                        CustomerCode = orderDTO.CustomerCode ?? string.Empty,
                        CustomerName = orderDTO.CustomerName ?? string.Empty,
                        Normalized_CustomerName = orderDTO.CustomerName?.ToUpperInvariant() ?? string.Empty,
                        Date = DateTime.Now,
                        DueDate = DateTime.Now.AddMonths(2),
                        TotalQuantity = orderDTO.Quantity,
                        InvoiceItemCount = (short)invoiceItemCount,
                        CreatedAt = DateTime.Now,
                        CreatedBy = string.Empty,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = string.Empty,
                    };
                    
                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync();

                    var orderLine = new OrderLine
                    {
                        InvoiceNumber = orderDTO.InvoiceNumber,
                        Line = orderDTO.Line,
                        StockCode = orderDTO.StockCode ?? string.Empty,
                        StockName = orderDTO.StockName ?? string.Empty,
                        Normalized_StockName = orderDTO.StockName?.ToUpperInvariant() ?? string.Empty,
                        CustomerCode = orderDTO.CustomerCode ?? string.Empty,
                        CustomerName = orderDTO.CustomerName ?? string.Empty,
                        Normalized_CustomerName = orderDTO.CustomerName?.ToUpperInvariant(),
                        Description = orderDTO.Description ?? string.Empty,
                        Quantity = orderDTO.Quantity,
                        DueDate = DateTime.Now.AddMonths(2),
                        Date = DateTime.Now,
                        Width = orderDTO.Width,
                        Length = orderDTO.Length,
                        Height = orderDTO.Height,
                    };

                    await _context.OrderLines.AddAsync(orderLine);
                    await _context.SaveChangesAsync();
                }
                
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
