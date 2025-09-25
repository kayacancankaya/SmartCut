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
                workbook.Properties.Author = "SmartCut";
                workbook.Properties.Title = Localizer.Get("Block List");
                workbook.Properties.Subject = Localizer.Get("Block List");
                workbook.Properties.Created = DateTime.Now;
                workbook.Properties.Company = "Birileri Dis Ticaret Ltd.Sti.";
                var ws = workbook.Worksheets.Add(Localizer.Get("Blocks"));


                //set first row for padding
                ws.Range("A1:I1").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Row(1).Height = 9;

                var imagePath = Path.Combine(_configuration["BasePath"], "images", "logos", blocks.Select(c => c.CompanyId).FirstOrDefault().ToString(), "brandlogo.png");
                if (File.Exists(imagePath))
                {
                    var img = ws.AddPicture(imagePath)
                                .MoveTo(ws.Cell("B2"))
                                .WithSize(100, 100);
                }
                // Title row (merged + styled)
                ws.Row(2).Height = 60;
                ws.Range("B2:H2").Merge().Value = Localizer.Get("Block List");
                ws.Range("B2:H2").Style
                    .Font.SetBold()
                    .Font.SetFontSize(18)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#F7C975"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                //padding row
                ws.Range("A3:I3").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Row(3).Height = 9;
                // Header row
                ws.Cell(4, 2).Value = Localizer.Get("Id");
                ws.Cell(4, 3).Value = Localizer.Get("Name");
                ws.Cell(4, 4).Value = Localizer.Get("Length");
                ws.Cell(4, 5).Value = Localizer.Get("Width");
                ws.Cell(4, 6).Value = Localizer.Get("Height");
                ws.Cell(4, 7).Value = Localizer.Get("Material");
                ws.Cell(4, 8).Value = Localizer.Get("Created At");

                ws.Range("B4:H4").Style
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#3B3D8E"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Fill rows from objects
                int row = 5;
                foreach (var block in blocks)
                {
                    ws.Cell(row, 2).Value = block.Id;
                    ws.Cell(row, 3).Value = block.Name;
                    ws.Cell(row, 4).Value = block.Length;
                    ws.Cell(row, 5).Value = block.Width;
                    ws.Cell(row, 6).Value = block.Height;
                    ws.Cell(row, 7).Value = block.Material;
                    ws.Cell(row, 8).Value = block.CreatedAt;
                    ws.Range("B" + row + ":I" + row).Style.Fill.SetBackgroundColor(row % 2 == 0 ? XLColor.FromHtml("#EFF7F4") : XLColor.FromHtml("FCFEF8"));
                    row++;
                }

                // Make table stylish
                ws.Range(4, 2, row - 1, 7).CreateTable();
                ws.Columns().AdjustToContents();
                ws.Range("B4:H" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                //adjust background color 
                ws.Range("A1:A" + (row + 2)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Range("A"+ row + ":I" + (row + 2)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Range("I1:I" + (row + 2)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Column(1).Width = 1.5;
                ws.Column(9).Width = 1.5;
                // Save to MemoryStream
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
        public byte[] ExportOrders(IEnumerable<OrderDTO> orders)
        {
            try
            {

                using var workbook = new XLWorkbook();
                workbook.Properties.Author = "SmartCut";
                workbook.Properties.Title = Localizer.Get("Order List");
                workbook.Properties.Subject = Localizer.Get("Order List");
                workbook.Properties.Created = DateTime.Now;
                workbook.Properties.Company = "Birileri Dis Ticaret Ltd.Sti.";
                var ws = workbook.Worksheets.Add(Localizer.Get("Orders"));


                //set first row for padding
                ws.Range("A1:M1").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Row(1).Height = 9;

                var imagePath = Path.Combine(_configuration["BasePath"], "images", "logos", orders.Select(c => c.CompanyId).FirstOrDefault().ToString() ?? "0", "brandlogo.png");
                if (File.Exists(imagePath))
                {
                    var img = ws.AddPicture(imagePath)
                                .MoveTo(ws.Cell("B2"))
                                .WithSize(100, 100);
                }
                // Title row (merged + styled)
                ws.Row(2).Height = 60;
                ws.Range("B2:L2").Merge().Value = Localizer.Get("Order List");
                ws.Range("B2:L2").Style
                    .Font.SetBold()
                    .Font.SetFontSize(18)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#F7C975"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                //padding row
                ws.Range("A3:M3").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Row(3).Height = 9;
                // Header row
                ws.Cell(4, 2).Value = Localizer.Get("Invoice No");
                ws.Cell(4, 3).Value = Localizer.Get("Order Line Number");
                ws.Cell(4, 4).Value = Localizer.Get("Length");
                ws.Cell(4, 5).Value = Localizer.Get("Width");
                ws.Cell(4, 6).Value = Localizer.Get("Height");
                ws.Cell(4, 7).Value = Localizer.Get("Quantity");
                ws.Cell(4, 8).Value = Localizer.Get("Product Code");
                ws.Cell(4, 9).Value = Localizer.Get("Product Name");
                ws.Cell(4, 10).Value = Localizer.Get("Customer Code");
                ws.Cell(4, 11).Value = Localizer.Get("Customer Name");
                ws.Cell(4, 12).Value = Localizer.Get("Description");

                ws.Range("B4:L4").Style
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#3B3D8E"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Fill rows from objects
                int row = 5;
                foreach (var order in orders)
                {
                    ws.Cell(row, 2).Value = order.InvoiceNumber;
                    ws.Cell(row, 3).Value = order.Line;
                    ws.Cell(row, 4).Value = order.Length;
                    ws.Cell(row, 5).Value = order.Width;
                    ws.Cell(row, 6).Value = order.Height;
                    ws.Cell(row, 7).Value = order.Quantity;
                    ws.Cell(row, 8).Value = order.StockCode;
                    ws.Cell(row, 9).Value = order.StockName;
                    ws.Cell(row, 10).Value = order.CustomerCode;
                    ws.Cell(row, 11).Value = order.CustomerName;
                    ws.Cell(row, 12).Value = order.Description;
                    ws.Range("B" + row + ":L" + row).Style.Fill.SetBackgroundColor(row % 2 == 0 ? XLColor.FromHtml("#EFF7F4") : XLColor.FromHtml("FCFEF8"));
                    row++;
                }

                // Make table stylish
                ws.Range(4, 2, row - 1, 11).CreateTable();
                ws.Columns().AdjustToContents();
                ws.Range("B4:L" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                //adjust background color 
                ws.Range("A1:A" + (row + 2)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Range("A"+ row + ":M" + (row + 2)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Range("I1:M" + (row + 2)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E9E9F7"));
                ws.Column(1).Width = 1.5;
                ws.Column(9).Width = 1.5;
                // Save to MemoryStream
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
