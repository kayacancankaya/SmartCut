using SmartCut.Shared.Models;
using SmartCut.Shared.Models.DTOs;
namespace SmartCut.Shared.Interfaces
{
    public interface IDataService
    {
        Task<IEnumerable<OrderLine>?> GetOrdersAsync(int pageNumber, int pageSize, string invoiceNumber, int line, string stockCode, string stockName, string customerCode, string customerName, string description);
        Task<IEnumerable<Block>?> GetBlocksAsync(int pageNumber, int pageSize, string name, string description, string material);
        Task<IEnumerable<Block>?> GetAllBlocksAsync();
        Task<List<CuttingPlanDTO>?> GetCalculationsAsync(int pageNumber, int pageSize, int status, float percentFulfilled, string InvoiceNumber, int line, string stockCode, string stockName, string customerCode, string customerName);
        Task<bool> CreateBlockAsync(Block block);
        Task<int> CheckIfBlockExistsAsync(string blockName);
        Task<bool> CreateOrderAsync(OrderDTO orderDTO);
        Task<bool> ImportOrdersAsync(List<OrderDTO> orders);
        Task<bool> ImportBlocksAsync(List<BlockDTO> blocks);
        Task<CuttingPlanDTO?> CalculateCuttingPlanAsync(CalculationDTO dTO);
    }
}
