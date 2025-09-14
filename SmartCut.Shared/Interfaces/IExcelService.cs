using Microsoft.AspNetCore.Components.Forms;
using SmartCut.Shared.Models;

namespace SmartCut.Shared.Interfaces
{
    public interface IExcelService 
    {
        Task<List<OrderDTO>> ImportOrdersAsync(IBrowserFile file);
    }
}
