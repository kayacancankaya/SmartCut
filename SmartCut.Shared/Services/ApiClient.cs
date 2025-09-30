using Microsoft.Extensions.Logging;
using SmartCut.Shared.Models;
using SmartCut.Shared.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartCut.Shared.Services
{

    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiClient> _logger;
        public ApiClient(HttpClient http,
                                    ILogger<ApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }
        public async Task<List<OrderDTO>> GetOrdersAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/web/getorders");
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<OrderDTO>>() : new List<OrderDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<OrderDTO>();
            }
        }
        public async Task<IEnumerable<OrderLine>?> GetOrdersAsync(int pageNumber = 1,int pageSize = 10,string invoiceNumber = "",int line = 0,string stockCode = "",
                                                            string stockName = "",string customerCode = "",string customerName = "",string description = "")
        {
            try
            {
                var response = await _http.GetFromJsonAsync<IEnumerable<OrderLine>?>(@$"api/web/getorders?pageNumber={pageNumber}&pageSize={pageSize}&invoiceNumber={invoiceNumber}&line={line}
                                                                                        &stockCode={stockCode}&stockName={stockName}&customerCode={customerCode}&customerName={customerName}&description={description}");
                return response;
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
                var response = await _http.GetFromJsonAsync<IEnumerable<Block>?>($"api/web/getallblocks?companyId={companyId}");
                return response;
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
                var response = await _http.GetFromJsonAsync<IEnumerable<OrderDTO>?>($"api/web/getallorders?companyId={companyId}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<OrderDTO>();
            }
        }
        public async Task<IEnumerable<Block>?> GetBlocksAsync(int pageNumber=1,int pageSize = 10,string name ="",string description = "",string material = "")
        {
            try
            {
                var response = await _http.GetFromJsonAsync<IEnumerable<Block>?>($"api/web/getblocks?pageNumber={pageNumber}&pageSize={pageSize}&name={name}&description={description}&material={material}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<Block>();
            }
        }
        public async Task<CuttingPlanDTO?> GetCuttingPlanByIdAsync(int cuttingId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<CuttingPlanDTO?>($"api/web/getcuttingplanbyid?cuttingId={cuttingId}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new CuttingPlanDTO();
            }
        }
        public async Task<List<CuttingPlanDTO>?> GetCalculationsAsync(int pageNumber = 1, int pageSize = 10, int status = 0, float percentFulfilled = 0, string invoiceNumber = "", int line = 0, string stockCode = "", string stockName = "", string customerCode = "", string customerName = "")
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<CuttingPlanDTO>?>($"api/web/getcalculations?pageNumber={pageNumber}&pageSize={pageSize}&status={status}&percentFulfilled={percentFulfilled}&invoiceNumber={invoiceNumber}&line={line}&stockCode={stockCode}&stockName={stockName}&customerCode={customerCode}&customerName={customerName}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<CuttingPlanDTO>();
            }
        }
        public async Task<bool> CreateOrderAsync(OrderDTO order)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/web/createorder", order);
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
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
                var response = await _http.GetAsync($"api/web/ifblockexists?blockName={blockName}");
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<int>() : -1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return -1;
            }
        }
        public async Task<bool> CreateBlockAsync(Block block)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/web/createblock", block);
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> ImportOrdersAsync(List<OrderDTO> orders)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/web/importorders", orders);
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> ImportBlocksAsync(List<BlockDTO> blocks)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/web/importblocks", blocks);
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return false;
            }
        }
        public async Task<CuttingPlanDTO?> CalculateCuttingPlanAsync(CalculationDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/web/calculatecuttingplan",dto);
                
                if(response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<CuttingPlanDTO>();

                return new CuttingPlanDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new CuttingPlanDTO();
            }
        }
    }
}
