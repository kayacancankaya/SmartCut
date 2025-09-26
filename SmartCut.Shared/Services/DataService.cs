using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCut.Shared.Data;
using SmartCut.Shared.Helpers;
using SmartCut.Shared.Interfaces;
using SmartCut.Shared.Models;
using SmartCut.Shared.Models.DTOs;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        public async Task<IEnumerable<OrderLine>?> GetOrdersAsync(int pageNumber, int pageSize, string invoiceNumber, int line, string stockCode, string stockName, string customerCode, string customerName, string description)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;
                var query = _context.OrderLines.AsQueryable();
                if (!string.IsNullOrEmpty(invoiceNumber))
                    query = query.Where(o => o.InvoiceNumber.Contains(invoiceNumber));
                if (line > 0)
                    query = query.Where(o => o.Line == line);
                if (!string.IsNullOrEmpty(stockCode))
                    query = query.Where(o => o.StockCode.Contains(stockCode));
                if (!string.IsNullOrEmpty(stockName))
                    query = query.Where(o => o.Normalized_StockName.Contains(stockName.ToUpperInvariant()));
                if (!string.IsNullOrEmpty(customerCode))
                    query = query.Where(o => o.CustomerCode.Contains(customerCode));
                if (!string.IsNullOrEmpty(customerName))
                    query = query.Where(o => o.Normalized_CustomerName.Contains(customerName.ToUpperInvariant()));
                if (description != null)
                    query = query.Where(o => o.Normalized_Description.Contains(description.ToUpperInvariant()));
                return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<OrderLine>();
            }
        }
        public async Task<IEnumerable<Block>?> GetAllBlocksAsync(string companyId)
        {
            try
            {
                return await _context.Blocks.Where(c=>c.CompanyId == companyId).OrderBy(n => n.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<Block>();
            }
        }
        public async Task<IEnumerable<OrderDTO>?> GetAllOrdersAsync(string companyId)
        {
            try
            {
                IEnumerable<OrderLine> order = await _context.OrderLines.Where(c=>c.CompanyId == companyId).OrderBy(n => n.InvoiceNumber).ThenBy(l=>l.Line).ToListAsync();
                List<OrderDTO> orderDTOs = new();
                foreach (var o in order)
                {
                    OrderDTO dto = new OrderDTO()
                    {
                        InvoiceNumber = o.InvoiceNumber,
                        Line = o.Line,
                        StockCode = o.StockCode,
                        StockName = o.StockName,
                        CustomerCode = o.CustomerCode,
                        CustomerName = o.CustomerName,
                        Description = o.Description,
                        Quantity = o.Quantity,
                        Width = o.Width,
                        Length = o.Length,
                        Height = o.Height
                    };
                    orderDTOs.Add(dto);
                }
                return orderDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<OrderDTO>();
            }
        }
        public async Task<IEnumerable<Block>?> GetBlocksAsync(int pageNumber, int pageSize, string name, string description, string material)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;
                var query = _context.Blocks.AsQueryable();
                if (!string.IsNullOrEmpty(name))
                {
                    var normalizedName = name.ToUpperInvariant();
                    query = query.Where(b => b.Normalized_Name.Contains(normalizedName));
                }
                if (!string.IsNullOrEmpty(description))
                {
                    var normalizedDescription = description.ToUpperInvariant();
                    query = query.Where(b => b.Normalized_Description.Contains(normalizedDescription));
                }
                if (!string.IsNullOrEmpty(material))
                {
                    var normalizedMaterial = material.ToUpperInvariant();
                    query = query.Where(b => b.Normalized_Material.Contains(normalizedMaterial));
                }
                return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<Block>();
            }
        }
        public async Task<List<CuttingPlanDTO>?> GetCalculationsAsync(int pageNumber, int pageSize, int status, float percentFulfilled, string InvoiceNumber, int line,string stockCode,string stockName,string customerCode,string customerName)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;
               string rawSQL = "SELECT DISTINCT cp.Id,cp.CompanyId, cp.BlockId, cp.Status, cp.Explanation, cp.ScrapVolume, cp.PercentFulfilled " +
                    "FROM CuttingPlans cp " +
                    "JOIN CutEntries ce ON cp.Id = ce.CuttingPlanId " +
                    "JOIN OrderLines ol ON ce.OrderLineId = ol.Id " +
                    "WHERE 1=1 ";
                if (status > 0)
                    rawSQL += $" AND cp.Status = {status} ";
                if (percentFulfilled > 0)
                    rawSQL += $" AND cp.PercentFulfilled >= {percentFulfilled} ";
                if (!string.IsNullOrEmpty(InvoiceNumber))
                    rawSQL += $" AND ol.InvoiceNumber LIKE '%{InvoiceNumber}%' ";
                if (line > 0)
                    rawSQL += $" AND ol.Line = {line} ";
                if (!string.IsNullOrEmpty(stockCode))
                    rawSQL += $" AND ol.StockCode LIKE '%{stockCode}%' ";
                if (!string.IsNullOrEmpty(stockName))
                {
                    var normalizedStockName = stockName.ToUpperInvariant();
                    rawSQL += $" AND ol.Normalized_StockName LIKE '%{normalizedStockName}%' ";
                }
                if (!string.IsNullOrEmpty(customerCode))
                    rawSQL += $" AND ol.CustomerCode LIKE '%{customerCode}%' ";
                if (!string.IsNullOrEmpty(customerName))
                {
                    var normalizedCustomerName = customerName.ToUpperInvariant();
                    rawSQL += $" AND ol.Normalized_CustomerName LIKE '%{normalizedCustomerName}%' ";
                }
                rawSQL += " ORDER BY cp.Id DESC ";
                rawSQL += $" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize};";
                
                var calculations = await _context.CuttingPlans.FromSqlRaw(rawSQL).ToListAsync();
                
                List<CuttingPlanDTO> calculationDTOs = new();
                foreach (var calc in calculations)
                {
                    CuttingPlanDTO dto = new CuttingPlanDTO()
                    {
                        Id = calc.Id,
                        Status = calc.Status,
                        Explanation = calc.Explanation,
                        ScrapVolume = calc.ScrapVolume,
                        PercentFulfilled = calc.PercentFulfilled,
                        CutEntries = new List<CutEntryDTO>()
                    };
                    var cutEntries = await _context.CutEntries.Where(ce => ce.CuttingPlanId == calc.Id).ToListAsync();
                    if (cutEntries != null)
                    {
                        foreach (var entry in cutEntries)
                        {
                            CutEntryDTO entryDTO = new CutEntryDTO()
                            {
                                Id = entry.Id,
                                OrderLineId = entry.OrderLineId,
                                QuantityFulfilled = entry.QuantityFulfilled,
                                Positions = await _context.Positions.Where(p => p.CutEntryId == entry.Id).ToListAsync(),
                                Dimension = await _context.Dimensions.FirstOrDefaultAsync(d => d.CutEntryId == entry.Id),
                                Order = await _context.OrderLines.Where(ol => ol.Id == entry.OrderLineId).Select(ol => new OrderDTO
                                {
                                    InvoiceNumber = ol.InvoiceNumber,
                                    Line = ol.Line,
                                    StockCode = ol.StockCode,
                                    StockName = ol.StockName,
                                    CustomerCode = ol.CustomerCode,
                                    CustomerName = ol.CustomerName,
                                    Description = ol.Description,
                                    Quantity = ol.Quantity,
                                    Width = ol.Width,
                                    Length = ol.Length,
                                    Height = ol.Height
                                }).FirstOrDefaultAsync() ?? new OrderDTO()
                            };
                            dto.CutEntries.Add(entryDTO);
                        }
                    }

                    calculationDTOs.Add(dto);
                }

                return calculationDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<CuttingPlanDTO>();
            }
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
                _logger.LogError(ex, ex.Message.ToString());
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
                    if (!lineExists)
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
                _logger.LogError(ex, ex.Message.ToString());
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
                _logger.LogError(ex, ex.Message.ToString());
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
                        if (existingOrderLines.Count > 0)
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
        public async Task<CuttingPlanDTO?> CalculateCuttingPlanAsync(CalculationDTO calculationDTO)
        {
            try
            {
                if (calculationDTO.BlockId <= 0)
                    return new CuttingPlanDTO();
                if (calculationDTO.OrderLineIDs == null)
                    return new CuttingPlanDTO();
                if (calculationDTO.OrderLineIDs.Count == 0)
                    return new CuttingPlanDTO();
                Block? block = await _context.Blocks.FirstOrDefaultAsync(b => b.Id == calculationDTO.BlockId);
                if (block == null)
                    return new CuttingPlanDTO();
                double blockVolume = (double)block.Width * (double)block.Height * (double)block.Length;

                List<OrderLine> orderLines = new();
                foreach (var id in calculationDTO.OrderLineIDs)
                {
                    var ol = await _context.OrderLines.FirstOrDefaultAsync(o => o.Id == id);
                    if (ol != null)
                        orderLines.Add(ol);
                    
                }
                double totalOrderVolume = 0d;
                foreach (var line in orderLines)
                    totalOrderVolume += ((double)line.Width * (double)line.Height * (double)line.Length) * line.Quantity;

                // Internal data structures
                var orderLineDims = orderLines.ToDictionary(
                    k => k.Id,
                    v => new float[] { v.Width, v.Height, v.Length }
                );

                // Expand units by quantity with volume for sorting
                int totalUnits = Convert.ToInt32(orderLines.Sum(ol => ol.Quantity));
                var units = new List<UnitRequest>(totalUnits);
                foreach (var line in orderLines)
                {
                    var vol = (double)line.Width * line.Height * line.Length;
                    for (int i = 0; i < line.Quantity; i++)
                    {
                        units.Add(new UnitRequest
                        {
                            OrderLineId = line.Id,
                            W = line.Width,
                            H = line.Height,
                            L = line.Length,
                            Volume = vol
                        });
                    }
                }

                // Sort First-Fit Decreasing by volume
                units.Sort((a, b) => b.Volume.CompareTo(a.Volume));

                // Free space list - start with full block
                var free = new List<FreeBox>
                {
                    new FreeBox
                    {
                        X = 0f, Y = 0f, Z = 0f,
                        W = block.Width,
                        H = block.Height,
                        L = block.Length
                    }
                };

                var placements = new List<Placement>(units.Count);
                int placedCount = 0;

                // Packing loop
                foreach (var unit in units)
                {
                    Placement placement = TryPlaceUnit(unit, free);
                    placements.Add(placement);
                }

                // Aggregate results
                double placedVolume = placements.Where(i=>i.IsPlaced).Sum(p => (double)p.W * p.H * p.L);
                double scrapVolume = Math.Max(0d, blockVolume - placedVolume);
                double percentFulfilled = totalOrderVolume <= 0 ? 0 : Math.Min(100.0, (placedVolume / totalOrderVolume) * 100.0);

                // Group placements by order line
                var planEntries = new List<CutEntry>();
                var planEntriesDTO = new List<CutEntryDTO>();
                var placementsGrouped = placements.Where(i=>i.IsPlaced).GroupBy(p => p.OrderLineId);
                List<Dimension> dimensions = new();
                List<SmartCut.Shared.Models.Position> allPositions = new();
                foreach (var grp in placementsGrouped)
                {
                    int counter = 0;
                    var positions = grp.Select(g => new float[] { g.X, g.Y, g.Z, g.W,g.H,g.L }).ToList();
                    List<SmartCut.Shared.Models.Position> posList = new();
                    foreach (var position in positions) {
                        counter++;
                        SmartCut.Shared.Models.Position position1 = new SmartCut.Shared.Models.Position
                        {
                            OrderLineId = grp.Key,
                            QuantityLine = counter,
                            X = position[0],
                            Y = position[1],
                            Z = position[2],
                            Ex = position[3],
                            Ey = position[4],
                            Ez = position[5]
                        };
                        posList.Add(position1);
                    }
                    float[] dimsOriginal = orderLineDims.TryGetValue(grp.Key, out var d) ? d : new float[] { 0f, 0f, 0f };
                   
                    Dimension dimension = new Dimension
                    {
                        OrderLineId = grp.Key,
                        X = dimsOriginal[0],
                        Y = dimsOriginal[1],
                        Z = dimsOriginal[2]
                    };

                    OrderDTO orderDTO = new OrderDTO
                    {
                        InvoiceNumber = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.InvoiceNumber).FirstOrDefault() ?? string.Empty,
                        Line = orderLines.Where(ol => ol.Id == grp.Key).Select(l=>l.Line).FirstOrDefault(),
                        Width = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Width).FirstOrDefault(),
                        Length = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Length).FirstOrDefault(),
                        Height = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Height).FirstOrDefault(),
                        Quantity = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Quantity).FirstOrDefault(),
                        StockCode = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.StockCode).FirstOrDefault() ?? string.Empty,
                        StockName = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.StockName).FirstOrDefault() ?? string.Empty,
                        CustomerCode = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.CustomerCode).FirstOrDefault() ?? string.Empty,
                        CustomerName = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.CustomerName).FirstOrDefault() ?? string.Empty,
                        Description = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Description).FirstOrDefault() ?? string.Empty,
                    };

                    planEntriesDTO.Add(new CutEntryDTO
                    {
                        OrderLineId = grp.Key,
                        OrderQuantity = orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Quantity).FirstOrDefault(),
                        QuantityFulfilled = positions.Count,
                        Positions = posList,
                        Dimension = dimension,
                        Order = orderDTO,
                        IsFulfilled = positions.Count >= orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Quantity).FirstOrDefault() ? true : false
                    });
                    planEntries.Add(new CutEntry
                    {
                        OrderLineId = grp.Key,
                        QuantityFulfilled = positions.Count,
                        Positions = posList,
                        TotalVolume = (float)orderDTO.Width * orderDTO.Height * orderDTO.Length * orderDTO.1,
                        Dimension = dimension,
                        OrderLine = orderLines.FirstOrDefault(ol => ol.Id == grp.Key),
                        IsFulfilled = positions.Count >= orderLines.Where(ol => ol.Id == grp.Key).Select(l => l.Quantity).FirstOrDefault() ? true : false
                    });

      
                    allPositions.AddRange(posList);
                    if (!dimensions.Any(d => d.OrderLineId == grp.Key))
                        dimensions.Add(dimension);

                }
                List<OrderLine> unfulfilledLines = new();
                foreach (var line in orderLines)
                {
                    if (!placementsGrouped.Any(g => g.Key == line.Id))
                        unfulfilledLines.Add(line);
                }
                foreach (var line in unfulfilledLines)
                {
                    planEntriesDTO.Add(new CutEntryDTO
                    {
                        OrderLineId = line.Id,
                        OrderQuantity = line.Quantity,
                        QuantityFulfilled = 0,
                        TotalVolume = (float)line.Width * line.Height * line.Length * line.Quantity,
                        Positions = new List<SmartCut.Shared.Models.Position>(),
                        Dimension = new Dimension
                        {
                            OrderLineId = line.Id,
                            X = line.Width,
                            Y = line.Height,
                            Z = line.Length
                        },
                        Order = new OrderDTO
                        {
                            InvoiceNumber = line.InvoiceNumber,
                            Line = line.Line,
                            Width = line.Width,
                            Length = line.Length,
                            Height = line.Height,
                            Quantity = line.Quantity,
                            StockCode = line.StockCode ?? string.Empty,
                            StockName = line.StockName ?? string.Empty,
                            CustomerCode = line.CustomerCode ?? string.Empty,
                            CustomerName = line.CustomerName ?? string.Empty,
                            Description = line.Description ?? string.Empty,
                        },
                        IsFulfilled = false
                    });
                    planEntries.Add(new CutEntry
                    {
                        OrderLineId = line.Id,
                        QuantityFulfilled = 0,
                        Positions = new List<SmartCut.Shared.Models.Position>(),
                        TotalVolume = (float)line.Width * line.Height * line.Length * line.Quantity,
                        Dimension = new Dimension
                        {
                            OrderLineId = line.Id,
                            X = line.Width,
                            Y = line.Height,
                            Z = line.Length
                        },
                        OrderLine = line,
                        IsFulfilled = false
                    });
                }
                // Identify shortfalls per order line for explanation
                var shortfalls = new List<string>();
                var requestedById = orderLines.ToDictionary(k => k.Id, v => Convert.ToInt32(v.Quantity));
                var fulfilledById = placementsGrouped.ToDictionary(k => k.Key, v => v.Count());
                foreach (var req in requestedById)
                {
                    fulfilledById.TryGetValue(req.Key, out int got);
                    int missing = Math.Max(0, req.Value - got);
                    if (missing > 0)
                    {
                        var dims = orderLineDims[req.Key];
                        double volPer = (double)dims[0] * dims[1] * dims[2];
                        double volMissing = missing * volPer;
                        shortfalls.Add($"OrderLine {req.Key}: missing {missing} pcs ({volMissing} volume)");
                    }
                }

                string explanation;
                int status;
                if (placedCount == units.Count)
                {
                    status = 3;
                    explanation = "Validated inputs. Using 3D guillotine bin-packing with First-Fit Decreasing by volume and minimal leftover heuristic. Successfully placed all order lines within the block without overlap.";
                }
                else
                {
                    status = 4;
                    explanation =
                        "Validated inputs. Using 3D guillotine bin-packing (FFD by volume) with minimal leftover heuristic. " +
                        "Block capacity or geometry prevented full fulfillment. Partial plan generated. " +
                        (shortfalls.Count > 0 ? "Shortfalls: " + string.Join("; ", shortfalls) : "");
                }

                // Build JSON result object
                var result = new CuttingPlan
                {
                    Status = status,
                    Explanation = explanation,
                    CutEntries = planEntries,
                    ScrapVolume = (float)scrapVolume,
                    PercentFulfilled = (float)percentFulfilled
                };
                await _context.CuttingPlans.AddAsync(result);
                await _context.CutEntries.AddRangeAsync(planEntries);
               
                //make all CutEntryId in positions
                foreach (var pos in allPositions)
                {
                    var entry = planEntries.FirstOrDefault(e => e.OrderLineId == pos.OrderLineId);
                    if (entry != null)
                    {
                        pos.CutEntryId = entry.Id;
                    }
                }
                foreach (var dim in dimensions)
                {
                    var entry = planEntries.FirstOrDefault(e => e.Dimension == dim);
                    if (entry != null)
                    {
                        dim.CutEntryId = entry.Id;
                    }
                }
                await _context.SaveChangesAsync();
               
                CuttingPlanDTO cuttingPlanDTO = new CuttingPlanDTO
                {
                    Id = result.Id,
                    Status = result.Status,
                    Explanation = result.Explanation,
                    ScrapVolume = result.ScrapVolume,
                    PercentFulfilled = result.PercentFulfilled,
                    CutEntries = planEntriesDTO,
                };
                return cuttingPlanDTO; 

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new CuttingPlanDTO();

            }
        }
        Placement TryPlaceUnit(UnitRequest unit, List<FreeBox> freeList)
        {
                Placement placement = new();

                // Candidate best
                int bestIdx = -1;
                SmartCut.Shared.Helpers.Orientation? bestOri = null;
                double bestLeftover = double.MaxValue;
                double bestTieBreak = double.MaxValue;

                // Try each free box and 6 orientations
                for (int i = 0; i < freeList.Count; i++)
                {
                    var f = freeList[i];
                    var orientations = GetOrientations(unit.W, unit.H, unit.L);

                    foreach (var o in orientations)
                    {
                        if (o.W <= f.W && o.H <= f.H && o.L <= f.L)
                        {
                            double leftover = ((double)f.W * f.H * f.L) - ((double)o.W * o.H * o.L);
                            // Tie-break: minimal max slack among axes
                            double slackX = (double)f.W - o.W;
                            double slackY = (double)f.H - o.H;
                            double slackZ = (double)f.L - o.L;
                            double tie = Math.Max(slackX, Math.Max(slackY, slackZ));

                            if (leftover < bestLeftover || (Math.Abs(leftover - bestLeftover) < 1e-3 && tie < bestTieBreak))
                            {
                                bestLeftover = leftover;
                                bestTieBreak = tie;
                                bestIdx = i;
                                bestOri = o;
                            }
                        }
                    }
                }

                if (bestIdx >= 0 && bestOri != null)
                {
                    var f = freeList[bestIdx];
                    var o = bestOri;

                    // Place at origin of selected free space
                    var p = new Placement
                    {
                        OrderLineId = unit.OrderLineId,
                        X = f.X,
                        Y = f.Y,
                        Z = f.Z,
                        W = o.W,
                        H = o.H,
                        L = o.L
                    };

                    // Split the selected free box into 3 non-overlapping boxes (right, top, front)
                    var right = new FreeBox
                    {
                        X = f.X + o.W,
                        Y = f.Y,
                        Z = f.Z,
                        W = f.W - o.W,
                        H = f.H,
                        L = f.L
                    };
                    var top = new FreeBox
                    {
                        X = f.X,
                        Y = f.Y + o.H,
                        Z = f.Z,
                        W = o.W,
                        H = f.H - o.H,
                        L = f.L
                    };
                    var front = new FreeBox
                    {
                        X = f.X,
                        Y = f.Y,
                        Z = f.Z + o.L,
                        W = o.W,
                        H = o.H,
                        L = f.L - o.L
                    };

                    // Replace selected free with splits
                    freeList.RemoveAt(bestIdx);

                    // Add valid non-degenerate boxes
                    AddIfValid(right, freeList);
                    AddIfValid(top, freeList);
                    AddIfValid(front, freeList);

                    // Prune contained/duplicate spaces
                    PruneFreeList(freeList);

                    placement = p;
                    placement.IsPlaced = true;
                    return placement;
                }
                placement = new Placement();
                placement.IsPlaced = false;
                placement.OrderLineId = unit.OrderLineId;
                placement.W = unit.W;
                placement.H = unit.H;
                placement.L = unit.L;
                placement.X = 0f;
                placement.Y = 0f;
                placement.Z = 0f;
                return placement ;
            }
            void AddIfValid(FreeBox box, List<FreeBox> list)
            {
                const float eps = 1e-3f;
                if (box.W > eps && box.H > eps && box.L > eps)
                    list.Add(box);
            }
            void PruneFreeList(List<FreeBox> list)
            {
                // Remove any box fully contained in another
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (i == j) continue;
                        if (Contains(list[j], list[i]))
                        {
                            list.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                }
            }
            bool Contains(FreeBox outer, FreeBox inner)
            {
                return inner.X >= outer.X - 1e-3f &&
                       inner.Y >= outer.Y - 1e-3f &&
                       inner.Z >= outer.Z - 1e-3f &&
                       inner.X + inner.W <= outer.X + outer.W + 1e-3f &&
                       inner.Y + inner.H <= outer.Y + outer.H + 1e-3f &&
                       inner.Z + inner.L <= outer.Z + outer.L + 1e-3f;
            }
            List<SmartCut.Shared.Helpers.Orientation> GetOrientations(float w, float h, float l)
            {
                // 6 axis-aligned rotations
                return new List<SmartCut.Shared.Helpers.Orientation>
                {
                    new SmartCut.Shared.Helpers.Orientation(w,h,l),
                    new SmartCut.Shared.Helpers.Orientation(w,l,h),
                    new SmartCut.Shared.Helpers.Orientation(h,w,l),
                    new SmartCut.Shared.Helpers.Orientation(h,l,w),
                    new SmartCut.Shared.Helpers.Orientation(l,w,h),
                    new SmartCut.Shared.Helpers.Orientation(l,h,w)
                };
            }

  
    }
}