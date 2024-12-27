using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(Roles.Admin))] // Yêu cầu xác thực với vai trò Admin  
    public class StockApiController : ControllerBase
    {
        private readonly IStockRepository _stockRepo;
        private readonly IMapper _mapper;

        public StockApiController(IStockRepository stockRepo, IMapper mapper)
        {
            _stockRepo = stockRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetStocks(string sTerm = "")
        {
            var stocks = await _stockRepo.GetStocks(sTerm);
            var stockDTOs = _mapper.Map<List<StockDTO>>(stocks);
            return Ok(stockDTOs); // Trả về danh sách các stock  
        }

        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetStockByBookId(int bookId)
        {
            var existingStock = await _stockRepo.GetStockByBookId(bookId);
            if (existingStock == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy stock  
            }

            var stockDTO = _mapper.Map<StockDTO>(existingStock);
            return Ok(stockDTO); // Trả về thông tin stock  
        }

        [HttpPost]
        public async Task<IActionResult> ManageStock([FromBody] StockDTO stock)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Nếu mô hình không hợp lệ, trả về 400 Bad Request  
            }
            try
            {
                await _stockRepo.ManageStock(stock);
                return NoContent(); // Thay thế stock thành công, trả về 204 No Content  
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while managing stock."); // Trả về 500 Internal Server Error  
            }
        }
    }
}