using Microsoft.AspNetCore.Mvc;
using SmartCut.Shared.Models;
using SmartCut.Shared.Interfaces;
namespace SmartCut.Web.Controller
{
    public class WebController : ControllerBase
    {
        [ApiController]
        [Route("api/[controller]")]
        private readonly ILogger<WebController> _logger;
        private readonly IDataService _data;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WebController(ILogger<WebController> logger, IDataService data,
                                IConfiguration configuration, HttpClient httpClient,
                                IWebHostEnvironment env)
        {
            _logger = logger;
            _data = data;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost("createblock")]
        public async Task<IActionResult> CreateBlock([FromBody] Block block)
        {
            try
            {
                if (block == null)
                {
                    return BadRequest("Block is null.");
                }
                var result = await _data.CreateBlockAsync(block);
                return result ? Ok("Block created successfully.") : StatusCode(500, "A problem happened while handling your request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
