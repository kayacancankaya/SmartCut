using ClosedXML.Excel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCut.Shared.Interfaces;
using SmartCut.Shared.Models;
using SmartCut.Shared.Resources.Localization;
using SmartCut.Shared.Models.DTOs;

namespace SmartCut.Shared.Helpers
{
    public class ClosedXMLService : IExcelService
    {
        private readonly ILogger<ClosedXMLService> _logger;
        private readonly IConfiguration _configuration;
        public ClosedXMLService(ILogger<ClosedXMLService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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
        public byte[] ExportBlocks(IEnumerable<Block> blocks)
        {
            try
            {

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Blocks");


                var imagePath = Path.Combine(_configuration["BasePath"], "images", "logos", blocks.Select(c => c.CompanyId).FirstOrDefault().ToString(), "brandlogo.png");
                if (File.Exists(imagePath))
                {
                    var img = ws.AddPicture(imagePath)
                                .MoveTo(ws.Cell("A1"))
                                .WithSize(100, 100);
                }

                // 2. Title row (merged + styled)
                ws.Range("A1:G1").Merge().Value = Localizer.Get("Block List");
                ws.Range("A1:G1").Style
                    .Font.SetBold()
                    .Font.SetFontSize(18)
                    .Fill.SetBackgroundColor(XLColor.LightBlue)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // 3. Header row
                ws.Cell(3, 1).Value = "Id";
                ws.Cell(3, 3).Value = "Name";
                ws.Cell(3, 4).Value = "Length";
                ws.Cell(3, 5).Value = "Width";
                ws.Cell(3, 6).Value = "Height";
                ws.Cell(3, 7).Value = "Material";
                ws.Cell(3, 8).Value = "Created At";

                ws.Range("A3:G3").Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.Orange)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // 4. Fill rows from Block objects
                int row = 4;
                foreach (var block in blocks)
                {
                    ws.Cell(row, 1).Value = block.Id;
                    ws.Cell(row, 2).Value = block.Name;
                    ws.Cell(row, 3).Value = block.Length;
                    ws.Cell(row, 4).Value = block.Width;
                    ws.Cell(row, 5).Value = block.Height;
                    ws.Cell(row, 6).Value = block.Material;
                    ws.Cell(row, 7).Value = block.CreatedAt;

                    row++;
                }

                // 5. Make table stylish
                ws.Range(3, 1, row - 1, 8).CreateTable();

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting blocks to Excel");
                return Array.Empty<byte>();
            }
        }

    }
}
