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
    }
}
