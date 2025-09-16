using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCut.Shared.Data;
using SmartCut.Shared.Helpers;
using SmartCut.Shared.Interfaces;
using SmartCut.Shared.Models;
using System.Collections.Generic;
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
        public async Task<IEnumerable<Block>?> GetAllBlocksAsync()
        {
            try
            {
                return await _context.Blocks.OrderBy(n => n.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<Block>();
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
        public async Task<int> CalculateCuttingPlanAsync(CalculationDTO calculationDTO)
        {
            try
            {
                if (calculationDTO.BlockId <= 0)
                    return 1;
                if (calculationDTO.OrderLineIDs == null)
                    return 2;
                if (calculationDTO.OrderLineIDs.Count == 0)
                    return 2;
                Block? block = await _context.Blocks.FirstOrDefaultAsync(b => b.Id == calculationDTO.BlockId);
                if (block == null)
                    return 1;
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
                    var found = TryPlaceUnit(unit, free, out var placement);
                    if (found && placement != null)
                    {
                        placements.Add(placement);
                        placedCount++;
                    }
                }

                // Aggregate results
                double placedVolume = placements.Sum(p => (double)p.W * p.H * p.L);
                double scrapVolume = Math.Max(0d, blockVolume - placedVolume);
                double percentFulfilled = totalOrderVolume <= 0 ? 0 : Math.Min(100.0, (placedVolume / totalOrderVolume) * 100.0);

                // Group placements by order line
                var planEntries = new List<CutEntry>();
                var placementsGrouped = placements.GroupBy(p => p.OrderLineId);
                foreach (var grp in placementsGrouped)
                {
                    var positions = grp.Select(g => new float[] { g.X, g.Y, g.Z }).ToList();
                    List<SmartCut.Shared.Models.Position> posList = new();
                    foreach (var position in positions) {
                        SmartCut.Shared.Models.Position position1 = new SmartCut.Shared.Models.Position
                        {
                            X = position[0],
                            Y = position[1],
                            Z = position[2]
                        };
                        posList.Add(position1);
                    }
                    float[] dimsOriginal = orderLineDims.TryGetValue(grp.Key, out var d) ? d : new float[] { 0f, 0f, 0f };
                    Dimension dimension = new Dimension
                    {
                        X = dimsOriginal[0],
                        Y = dimsOriginal[1],
                        Z = dimsOriginal[2]
                    };
                       
                    
                    
                    planEntries.Add(new CutEntry
                    {
                        OrderLineId = grp.Key,
                        QuantityFulfilled = positions.Count,
                        Positions = posList,
                        Dimensions = dimension
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
                var result = new CuttingPlanSummary
                {
                    Status = status,
                    Explanation = explanation,
                    CuttingPlan = planEntries,
                    ScrapVolume = (float)scrapVolume,
                    PercentFulfilled = (float)percentFulfilled
                };

                // Optional: persist or attach to DTO if needed by caller
                // Example: calculationDTO.ResultJson = JsonSerializer.Serialize(result);
                // If you have a Calculation entity, you could save it here.

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return -1;
            }
        }

        // Local types and helpers for packing algorithm
        bool TryPlaceUnit(UnitRequest unit, List<FreeBox> freeList, out Placement? placement)
            {
                placement = null;

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
                    return true;
                }

                return false;
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

        //    _logger.LogInformation("Block volume: {bv}, total order volume: {ov}", blockVolume, totalOrderVolume);

        //                // Try ILP approach first (if OR-Tools available)
        //                CuttingPlanSummary? summary = null;

        //                bool ortoolsAvailable = false;
        //                try
        //                {
        //                    // detect OR-Tools by trying to load type (keeps compile-time optional)
        //                    var ortoolsType = Type.GetType("Google.OrTools.LinearSolver.Solver, Google.OrTools");
        //                    ortoolsAvailable = ortoolsType != null;
        //                }
        //                catch
        //                {
        //                    ortoolsAvailable = false;
        //                }

        //                if (ortoolsAvailable)
        //                {
        //                    try
        //                    {
        //                        summary = SolveWithILP(block, orderLines);
        //                        _logger.LogInformation("ILP solver succeeded.");
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        _logger.LogWarning(ex, "ILP solver failed, falling back to greedy packing.");
        //                        summary = null;
        //                    }
        //                }

        //                // If ILP wasn't available or failed, fallback to greedy 3D guillotine-like heuristic
        //                if (summary == null)
        //                {
        //                    summary = Greedy3DPacking(block, orderLines);
        //                    summary.Notes = (summary.Notes ?? "") + (ortoolsAvailable ? "ILP failed, used greedy fallback." : "OR-Tools not available, used greedy fallback.");
        //                }

        //                // Log summary
        //                _logger.LogInformation("Cutting Plan Summary: BlocksNeeded={blocks}, WasteVol={waste}, TotalProduced={prod}",
        //                    summary.BlocksNeeded, summary.WasteVolume, summary.ProducedQuantities.Sum(kv => kv.Value));

        //                // TODO: persist summary if needed

        //                return 0; // success
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, ex.Message.ToString());
        //                return -1;
        //            }
        //        }

        //        // ---------------------------
        //        // ILP solver using OR-Tools
        //        // ---------------------------
        //        private CuttingPlanSummary? SolveWithILP(Block block, List<OrderLineDTO> lines)
        //        {
        //            // Note: This method expects Google.OrTools to be referenced.
        //            // If OR-Tools is not installed, this method should not be called.
        //            // The ILP model here is a pattern-based simplification:
        //            // 1) For each candidate layer height (distinct heights of order lines), we compute a 2D packing pattern that yields counts per item for ONE layer.
        //            // 2) We then create integer variables that represent how many of each layer we place in a block (subject to block.Height).
        //            // 3) Using these patterns, we compute how many items a single block can produce depending on combination of layers.
        //            // 4) Finally, we choose the minimum number of blocks to meet demands (this becomes another ILP).
        //            //
        //            // This is a simplification and not full 3D guillotine packing, but it leverages ILP to combine layer patterns optimally.

        //            // Because this method uses OR-Tools types, keep all OR-Tools code inside to avoid compile errors if package missing.
        //#if NET8_0_OR_GREATER
        //            // If you compile against .NET 8 and have Google.OrTools package, the following code will run.
        //#endif

        //            // Attempt to load OR-Tools types via reflection to avoid hard dependency at compile time;
        //            // but the recommended approach is to add the Google.OrTools NuGet and directly use its API.
        //            Type? solverType = Type.GetType("Google.OrTools.LinearSolver.Solver, Google.OrTools");
        //            if (solverType == null)
        //                throw new InvalidOperationException("OR-Tools not found.");

        //            // --- Build layer patterns ---
        //            var uniqueHeights = lines.Select(l => l.Height).Distinct().OrderBy(h => h).ToList();

        //            // For each height, compute a 2D packing using a simple greedy skyline/guillotine in the XY plane for that layer height.
        //            var patterns = new List<LayerPattern>();
        //            foreach (var h in uniqueHeights)
        //            {
        //                var layerPattern = PackLayer2D(Convert.ToInt32(block.Width), Convert.ToInt32(block.Length), h, lines);
        //                if (layerPattern.TotalPlaced > 0)
        //                    patterns.Add(layerPattern);
        //            }

        //            if (patterns.Count == 0)
        //                throw new InvalidOperationException("No feasible layer patterns found (maybe all item heights > block height).");

        //            // Now we attempt to determine how many of each pattern to put into ONE BLOCK (sum patternHeight * count <= block.Height)
        //            // and then how many blocks are needed to satisfy all orders.
        //            // Build ILP using OR-Tools (integer variables).
        //            // We'll build a two-stage ILP: variables layersInBlock[p] = number of that pattern per block.
        //            // Then compute itemsPerBlock[i] = sum_p layersInBlock[p] * pattern.PlacedCounts[i]
        //            // Finally choose blocksCount (integer) and ensure blocksCount * itemsPerBlock[i] >= demand_i minimizing blocksCount.
        //            //
        //            // This is nonlinear if written raw (product blocksCount * itemsPerBlock variables). Instead we use patternBlockCount[p]
        //            // = total number of pattern layers used across ALL blocks (global count), and blocksCount variable.
        //            // Constraints:
        //            //   sum_p patternHeight[p] * patternBlockCount[p] <= blocksCount * block.Height
        //            //   For each item i: sum_p patternBlockCount[p] * pattern.PlacedCounts[i] >= demand[i]
        //            // objective: minimize blocksCount
        //            //
        //            // This is linear (patternBlockCount and blocksCount are integer variables).

        //            // Now create solver and variables via reflection-invoked OR-Tools API for flexibility.
        //            // For readability and maintainability I'd normally reference OR-Tools directly. Below I'll assume OR-Tools is available
        //            // and call it directly (so please add Google.OrTools package); reflection here would make the code unreadable.

        //            // --- Direct usage (recommended) ---
        //#if GOOGLE_ORTOOLS
        //        // If you have Google.OrTools referenced at compile time, use the following:
        //        var solver = Solver.CreateSolver("SCIP"); // or "CBC_MIXED_INTEGER_PROGRAMMING"
        //        if (solver == null)
        //            throw new InvalidOperationException("Could not create OR-Tools solver.");

        //        int pCount = patterns.Count;
        //        int itemCount = lines.Count;

        //        // Variables: patternBlockCount[p] >= 0 integer
        //        var patternBlockCount = new Google.OrTools.LinearSolver.Variable[pCount];
        //        for (int p = 0; p < pCount; ++p)
        //        {
        //            patternBlockCount[p] = solver.MakeIntVar(0.0, double.PositiveInfinity, $"patCount_{p}");
        //        }

        //        // blocksCount integer >= 0
        //        var blocksCount = solver.MakeIntVar(0.0, double.PositiveInfinity, "blocksCount");

        //        // Height constraint: sum_p patternHeight[p] * patternBlockCount[p] <= block.Height * blocksCount
        //        var heightExpr = solver.MakeConstraint(0, double.PositiveInfinity, "heightCap"); // we'll set upper via expression
        //        {
        //            // add LHS - block.Height * blocksCount <= 0
        //            for (int p = 0; p < pCount; ++p)
        //                heightExpr.SetCoefficient(patternBlockCount[p], patterns[p].Height);

        //            heightExpr.SetCoefficient(blocksCount, -block.Height);
        //            heightExpr.SetUpperBound(0.0);
        //        }

        //        // Demand constraints: for each item i: sum_p patternBlockCount[p] * pattern.PlacedCounts[i] >= demand[i]
        //        for (int i = 0; i < itemCount; ++i)
        //        {
        //            var c = solver.MakeConstraint(lines[i].Quantity, double.PositiveInfinity, $"demand_{i}");
        //            for (int p = 0; p < pCount; ++p)
        //            {
        //                int placed = patterns[p].GetPlacedCountForLineIndex(i);
        //                if (placed > 0)
        //                    c.SetCoefficient(patternBlockCount[p], placed);
        //            }
        //        }

        //        // Objective: minimize blocksCount
        //        var objective = solver.Objective();
        //        objective.SetCoefficient(blocksCount, 1);
        //        objective.SetMinimization();

        //        var resultStatus = solver.Solve();
        //        if (resultStatus != Solver.OPTIMAL && resultStatus != Solver.FEASIBLE)
        //            throw new InvalidOperationException($"OR-Tools solver status: {resultStatus}");

        //        // Build summary
        //        var summary = new CuttingPlanSummary();
        //        summary.BlocksNeeded = (int)Math.Ceiling(blocksCount.SolutionValue());
        //        summary.TotalBlockVolume = block.Width * block.Length * block.Height;
        //        summary.TotalOrderVolume = lines.Sum(l => l.Width * l.Length * l.Height * l.Quantity);
        //        summary.ProducedQuantities = new Dictionary<OrderLineDTO, int>();
        //        for (int i = 0; i < itemCount; ++i)
        //        {
        //            double produced = 0;
        //            for (int p = 0; p < pCount; ++p)
        //                produced += patterns[p].GetPlacedCountForLineIndex(i) * patternBlockCount[p].SolutionValue();
        //            summary.ProducedQuantities[lines[i]] = (int)Math.Floor(produced + 1e-6);
        //        }
        //        summary.WasteVolume = summary.BlocksNeeded * summary.TotalBlockVolume - summary.ProducedQuantities.Sum(kv => kv.Key.Width * kv.Key.Length * kv.Key.Height * kv.Value);
        //        return summary;
        //#else
        //            // If you don't have Google.OrTools define GOOGLE_ORTOOLS if you add it. For now throw to signal ILP not available.
        //            throw new InvalidOperationException("Direct OR-Tools compile-time usage not enabled. Build with Google.OrTools or disable ILP branch.");
        //#endif
        //        }

        //        // ---------------------------
        //        // Greedy 3D guillotine-like heuristic
        //        // ---------------------------
        //        private CuttingPlanSummary Greedy3DPacking(Block block, List<OrderLineDTO> lines)
        //        {
        //            // Strategy:
        //            // 1) Allow XY rotations (swap length/width).
        //            // 2) Create layers by selecting an item height (tallest-first).
        //            // 3) For each layer height, pack 2D rectangle packing into block.Width x block.Length using simple shelf algorithm.
        //            // 4) For every time one block is filled in height, count items produced.
        //            // 5) Repeat blocks until demands are met or a max blocks threshold reached.

        //            var remaining = lines.ToDictionary(l => l, l => l.Quantity); // quantities left
        //            var summary = new CuttingPlanSummary();
        //            summary.TotalBlockVolume = block.Width * block.Length * block.Height;
        //            summary.TotalOrderVolume = lines.Sum(l => l.Width * l.Length * l.Height * l.Quantity);

        //            int blocksUsed = 0;
        //            int safetyLimit = 10000; // avoid infinite loops
        //            while (remaining.Values.Any(q => q > 0) && safetyLimit-- > 0)
        //            {
        //                blocksUsed++;
        //                int usedHeight = 0;
        //                var producedThisBlock = lines.ToDictionary(l => l, l => 0);

        //                // Create layers until block height exhausted
        //                while (usedHeight < block.Height)
        //                {
        //                    // choose the best layer height among items that still need production and fit in remaining height
        //                    var candidateHeights = remaining.Where(kv => kv.Value > 0).Select(kv => kv.Key.Height).Distinct()
        //                        .Where(h => h <= (block.Height - usedHeight)).OrderByDescending(h => h).ToList();
        //                    if (!candidateHeights.Any()) break;

        //                    bool layerPlaced = false;
        //                    foreach (var layerH in candidateHeights)
        //                    {
        //                        // try pack layer using shelf (row) packing across block.Length and block.Width
        //                        var layerPacking = PackLayer2D(block.Width, block.Length, layerH, lines, remaining);

        //                        if (layerPacking.TotalPlaced == 0)
        //                            continue;

        //                        // apply placement: reduce remaining quantities
        //                        foreach (var kv in layerPacking.PlacedCountsPerLine)
        //                        {
        //                            if (kv.Value <= 0) continue;
        //                            producedThisBlock[kv.Key] += kv.Value;
        //                            remaining[kv.Key] -= kv.Value;
        //                            if (remaining[kv.Key] < 0) remaining[kv.Key] = 0;
        //                        }

        //                        usedHeight += layerH;
        //                        layerPlaced = true;
        //                        break; // pick one layer at a time (first-fit by tallest)
        //                    }

        //                    if (!layerPlaced) break; // can't place any layer in remaining height
        //                }

        //                // Add producedThisBlock to summary
        //                foreach (var kv in producedThisBlock)
        //                {
        //                    if (!summary.ProducedQuantities.ContainsKey(kv.Key))
        //                        summary.ProducedQuantities[kv.Key] = 0;
        //                    summary.ProducedQuantities[kv.Key] += kv.Value;
        //                }

        //                // safety stop if no items produced in a block (avoid infinite loop)
        //                if (producedThisBlock.All(kv => kv.Value == 0))
        //                {
        //                    summary.Notes = "Greedy packing could not place any new items in the current block - some items may not fit.";
        //                    break;
        //                }
        //            }

        //            summary.BlocksNeeded = blocksUsed;
        //            float producedVolume = summary.ProducedQuantities.Sum(kv => kv.Key.Width * kv.Key.Length * kv.Key.Height * kv.Value);
        //            summary.WasteVolume = summary.BlocksNeeded * summary.TotalBlockVolume - producedVolume;
        //            return summary;
        //        }

        //        // Helper: simple 2D layer packing (greedy shelf algorithm). This variant returns counts assuming unlimited copies (limited by remaining if provided).
        //        private LayerPattern PackLayer2D(int blockW, int blockL, int layerH, List<OrderLineDTO> lines, Dictionary<OrderLineDTO, int>? remaining = null)
        //        {
        //            // We'll create a simple shelf packer: fill rows across length; each shelf height = max item height in shelf (here all equal to layerH),
        //            // we place items left-to-right; we allow rotation in XY plane (swap length/width) to try to fit more.
        //            // lines is the master list; we will attempt placement for items whose Height == layerH (only those make sense for this layer).
        //            var pattern = new LayerPattern(layerH);
        //            // consider only items with matching height
        //            var candidates = lines.Where(l => l.Height == layerH).ToList();
        //            if (!candidates.Any()) return pattern;

        //            // We'll sort candidates by area descending to place big rectangles first
        //            var ordered = candidates.OrderByDescending(c => c.Width * c.Length).ToList();

        //            // shelf packing: maintain current row Y offset (not needed for counts), with remaining length and width
        //            int remainingWidth = blockW;
        //            int remainingLength = blockL;
        //            // We'll simulate using rows along the Length: i.e., form shelves that span blockW and stacking shelves along Length
        //            var shelfRemainingLength = blockL;
        //            int shelfHeight = layerH; // all items in layer have the same height by definition
        //            int posX = 0;
        //            int posY = 0;

        //            // We'll maintain placements counts per item
        //            foreach (var item in ordered)
        //            {
        //                int allowed = remaining == null ? item.Quantity : remaining.GetValueOrDefault(item, item.Quantity);
        //                if (allowed <= 0) continue;

        //                // compute how many of this item fit per shelf cell with rotation attempts
        //                // compute in tile grid: how many fit across width (W) and length (L)
        //                int fitW = Math.Max(0, blockW / item.Width);
        //                int fitL = Math.Max(0, blockL / item.Length);
        //                int fitRotW = Math.Max(0, blockW / item.Length);
        //                int fitRotL = Math.Max(0, blockL / item.Width);

        //                int perBlockIfOnlyThis = Math.Max(fitW * fitL, fitRotW * fitRotL);
        //                if (perBlockIfOnlyThis <= 0) continue;

        //                // But we are packing layer-so per-layer we can fit:
        //                int perLayerFit;
        //                // compute per-layer by dividing area heuristically:
        //                int across = blockW / item.Width;
        //                int down = blockL / item.Length;
        //                int acrossRot = blockW / item.Length;
        //                int downRot = blockL / item.Width;
        //                perLayerFit = Math.Max(across * down, acrossRot * downRot);

        //                if (perLayerFit <= 0) continue;

        //                // Limit by remaining demand
        //                int placeCount = Math.Min(perLayerFit, allowed);

        //                pattern.PlacedCountsPerLine[item] = placeCount;
        //                pattern.TotalPlaced += placeCount;
        //            }

        //            return pattern;
        //        }

        //        // Helper class for layer packing pattern
        //        private class LayerPattern
        //        {
        //            public int Height { get; }
        //            // PlacedCountsPerLine uses object reference equality to OrderLineDTO; ensure same instances passed
        //            public Dictionary<OrderLineDTO, int> PlacedCountsPerLine { get; } = new();

        //            public LayerPattern(int h)
        //            {
        //                Height = h;
        //            }

        //            public int TotalPlaced => PlacedCountsPerLine.Values.Sum();

        //            // Convert to index-based get (for ILP path, if you build mapping)
        //            public int GetPlacedCountForLineIndex(int i)
        //            {
        //                if (i < 0) return 0;
        //                var kv = PlacedCountsPerLine.ElementAtOrDefault(i);
        //                return kv.Equals(default(KeyValuePair<OrderLineDTO, int>)) ? 0 : kv.Value;
        //            }
        //        }


    }
}