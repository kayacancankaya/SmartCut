using Microsoft.AspNetCore.Components.Forms;
using SmartCut.Shared.Models;
using SmartCut.Shared.Models.DTOs;

namespace SmartCut.Shared.Interfaces
{
    public interface IExcelService 
    {
        Task<List<OrderDTO>> ImportOrdersAsync(IBrowserFile file);
        Task<List<BlockDTO>> ImportBlocksAsync(IBrowserFile file);
        byte[] ExportBlocks(IEnumerable<Block> BlockList);
        byte[] ExportOrders(IEnumerable<OrderDTO> Orders);
    }
}
