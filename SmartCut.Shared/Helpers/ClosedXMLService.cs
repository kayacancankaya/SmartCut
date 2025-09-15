using ClosedXML.Excel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using SmartCut.Shared.Interfaces;
using SmartCut.Shared.Models;

namespace SmartCut.Shared.Helpers
{
    public class ClosedXMLService : IExcelService
    {
        private readonly ILogger<ClosedXMLService> _logger;
        public ClosedXMLService(ILogger<ClosedXMLService> logger)
        {
            _logger = logger;
        }
        public async Task<List<OrderDTO>> ImportOrdersAsync(IBrowserFile file)
        {
            try
            {

                var data = new List<List<string>>();

                using var stream = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(stream);

                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.First();
                stream.Position = 0;

                List<OrderDTO> orders = new List<OrderDTO>();
                foreach (var row in ws.RowsUsed())
                {
                    // Skip header row
                    if (row.RowNumber() == 1) continue;
                    var orderDTO = new OrderDTO();
                    foreach (var cell in row.CellsUsed())
                    {
                        
                        switch (cell.Address.ColumnNumber)
                        {
                            case 1:
                                orderDTO.InvoiceNumber = cell.GetString();
                                break;
                            case 2:
                                orderDTO.Line = (int)cell.GetDouble();
                                break;
                            case 3:
                                orderDTO.Width = (float)cell.GetDouble();
                                break;
                            case 4:
                                orderDTO.Length = (float)cell.GetDouble();
                                break;
                            case 5:
                                orderDTO.Height = (float)cell.GetDouble();
                                break;
                            case 6:
                                orderDTO.Quantity = (float)cell.GetDouble();
                                break;
                            case 7:
                                orderDTO.StockCode = cell.GetString();
                                break;
                            case 8:
                                orderDTO.StockName = cell.GetString();
                                break;
                            case 9:
                                orderDTO.CustomerCode = cell.GetString();
                                break;
                            case 10:
                                orderDTO.CustomerName = cell.GetString();
                                break;
                            case 11:
                                orderDTO.Description = cell.GetString();
                                break;
                        }
                    }
                    orders.Add(orderDTO);
                }
                return orders ?? new List<OrderDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing orders from Excel");
                return new List<OrderDTO>();
            }
        }
        public async Task<List<BlockDTO>> ImportBlocksAsync(IBrowserFile file)
        {
            try
            {
                var data = new List<List<string>>();

                using var stream = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(stream);

                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.First();
                stream.Position = 0;

                List<BlockDTO> blocks = new List<BlockDTO>();
                foreach (var row in ws.RowsUsed())
                {
                    // Skip header row
                    if (row.RowNumber() == 1) continue;
                    var block = new BlockDTO();
                    foreach (var cell in row.CellsUsed())
                    {
                        
                        switch (cell.Address.ColumnNumber)
                        {
                            case 1:
                                block.Name = cell.GetString() ?? string.Empty;
                                break;
                            case 2:
                                block.Width = (float)cell.GetDouble();
                                break;
                            case 3:
                                block.Length = (float)cell.GetDouble();
                                break;
                            case 4:
                                block.Height = (float)cell.GetDouble();
                                break;
                            case 5:
                                block.Material = cell.GetString() ?? string.Empty;
                                break;
                            case 6:
                                block.Description = cell.GetString() ?? string.Empty;
                                break;
                        }
                    }
                    blocks.Add(block);
                }
                return blocks ?? new List<BlockDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing blocks from Excel");
                return new List<BlockDTO>();
            }
        }
    }
}
