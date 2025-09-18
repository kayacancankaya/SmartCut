using Microsoft.AspNetCore.Mvc;
using SmartCut.Shared.Models;
using SmartCut.Shared.Interfaces;
using System.Text.Json;
using SmartCut.Shared.Models.DTOs;
namespace SmartCut.Web.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebController : ControllerBase
    {
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

        [HttpGet("getallblocks")]
        public async Task<ActionResult<IEnumerable<Block>?>> GetAllBlocks()
        {
            try
            {
                var response = await _data.GetAllBlocksAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("getblocks")]
        public async Task<ActionResult<IEnumerable<Block>?>> GetBlocks(int pageNumber=10,int pageSize = 1,string name = "",string description = "",string material = "")
        {
            try
            {
                var response = await _data.GetBlocksAsync(pageNumber,pageSize,name,description,material);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("getorders")]
        public async Task<ActionResult<IEnumerable<Block>?>> GetOrders(int pageNumber=10,int pageSize = 1,string invoiceNumber = "",int line = 0 , string stockCode="",string stockName="",string customerCode="",string customerName="",string description = "")
        {
            try
            {
                var response = await _data.GetOrdersAsync(pageNumber, pageSize, invoiceNumber, line, stockCode, stockName, customerCode, customerName, description);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost("createorder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO order)
        {
            try
            {
                if (order == null)
                {
                    return BadRequest("Order is null.");
                }
                var result = await _data.CreateOrderAsync(order);
                return result ? Ok("Order created successfully.") : StatusCode(500, "A problem happened while handling your request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ifblockexists")]
        public async Task<ActionResult<int>> IfBlockExists(string blockName)
        {
            try
            {
                if (string.IsNullOrEmpty(blockName))
                {
                    return BadRequest("Block name is null.");
                }
                var response = await _data.CheckIfBlockExistsAsync(blockName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
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

        [HttpPost("importorders")]
        public async Task<IActionResult> ImportOrders([FromBody] List<OrderDTO> orders)
        {
            try
            {
                if (orders == null)
                {
                    return BadRequest("Orders are null.");
                }
                if (orders.Count <= 0)
                {
                    return BadRequest("Orders are empty.");
                }
                var result = await _data.ImportOrdersAsync(orders);
                return result ? Ok("Order created successfully.") : StatusCode(500, "A problem happened while handling your request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("importblocks")]
        public async Task<IActionResult> ImportBlocks([FromBody] List<BlockDTO> blocks)
        {
            try
            {
                if (blocks == null)
                {
                    return BadRequest("Blocks are null.");
                }
                if (blocks.Count <= 0)
                {
                    return BadRequest("Blocks are empty.");
                }
                var result = await _data.ImportBlocksAsync(blocks);
                return result ? Ok("Block created successfully.") : StatusCode(500, "A problem happened while handling your request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost("calculatecuttingplan")]
        public async Task<ActionResult<CuttingPlanDTO?>> CalculateCuttingPlan([FromBody] CalculationDTO dTO)
        {
            try
            {
                if (dTO == null)
                {
                    return BadRequest("Calculation data is null.");
                }
                var response = await _data.CalculateCuttingPlanAsync(dTO);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message.ToString());
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
