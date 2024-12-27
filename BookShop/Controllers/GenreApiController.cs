using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(Roles.Admin))] // Yêu cầu xác thực với vai trò Admin  
    public class GenreApiController : ControllerBase
    {
        private readonly IGenreRepository _genreRepo;
        private readonly IMapper _mapper;

        public GenreApiController(IGenreRepository genreRepo, IMapper mapper)
        {
            _genreRepo = genreRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _genreRepo.GetGenres();
            var genreDTOs = _mapper.Map<List<GenreDTO>>(genres);
            return Ok(genreDTOs); // Trả về danh sách thể loại  
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenreById(int id)
        {
            var genre = await _genreRepo.GetGenreById(id);
            if (genre == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy thể loại  
            }
            var genreDTO = _mapper.Map<GenreDTO>(genre);
            return Ok(genreDTO); // Trả về thể loại  
        }

        [HttpPost]
        public async Task<IActionResult> AddGenre([FromBody] GenreDTO genre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Nếu mô hình không hợp lệ, trả về 400 Bad Request  
            }
            try
            {
                var genreToAdd = _mapper.Map<Genre>(genre);
                await _genreRepo.AddGenre(genreToAdd);
                return CreatedAtAction(nameof(GetGenreById), new { id = genreToAdd.Id }, genreToAdd); // Trả về 201 Created  
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to add genre."); // Trả về 500 Internal Server Error  
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] GenreDTO genreToUpdate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Nếu mô hình không hợp lệ, trả về 400 Bad Request  
            }
            try
            {
                var genre = await _genreRepo.GetGenreById(id);
                if (genre == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy thể loại  
                }
                var genreEntity = _mapper.Map(genreToUpdate, genre); // Cập nhật thể loại  
                await _genreRepo.UpdateGenre(genreEntity);
                return NoContent(); // Trả về 204 No Content  
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update genre."); // Trả về 500 Internal Server Error  
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            try
            {
                var genre = await _genreRepo.GetGenreById(id);
                if (genre == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy thể loại  
                }
                await _genreRepo.DeleteGenre(genre);
                return NoContent(); // Trả về 204 No Content  
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete genre."); // Trả về 500 Internal Server Error  
            }
        }
    }
}