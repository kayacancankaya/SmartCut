using SmartCut.Shared.Models;
namespace SmartCut.Shared.Interfaces
{
    public interface IDataService
    {
        Task<bool> CreateBlockAsync(Block block);
        Task<bool> ImportOrdersAsync(List<OrderDTO> orders);
    }
}
