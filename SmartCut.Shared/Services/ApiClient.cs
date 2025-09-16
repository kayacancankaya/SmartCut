using Microsoft.Extensions.Logging;
using SmartCut.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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
        public async Task<IEnumerable<Block>?> GetBlocksAsync(int pageNumber=1,int pageSize = 10,string name ="",string description = "",string material = "")
        {
            try
            {
                var response = await _http.GetAsync($"api/web/getblocks?pageNumber={pageNumber}&pageSize={pageSize}&name={name}&description={description}&material={material}");
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<IEnumerable<Block>>() : new List<Block>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return new List<Block>();
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
    }
}
