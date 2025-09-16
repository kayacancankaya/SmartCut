using SmartCut.Shared.Models;
namespace SmartCut.Shared.Interfaces
{
    public interface IDataService
    {
        Task<IEnumerable<Block>?> GetBlocksAsync(int pageNumber, int pageSize, string name, string description, string material);
        Task<bool> CreateBlockAsync(Block block);
        Task<int> CheckIfBlockExistsAsync(string blockName);
        Task<bool> CreateOrderAsync(OrderDTO orderDTO);
        Task<bool> ImportOrdersAsync(List<OrderDTO> orders);
        Task<bool> ImportBlocksAsync(List<BlockDTO> blocks);
    }
}
