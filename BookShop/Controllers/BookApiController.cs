using BookShop.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace BookShop.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    [ApiController] // Đánh dấu đây là một API controller  
    [Route("api/[controller]")]
    public class BookApiController : ControllerBase
    {
        private readonly IBookRepository _bookRepo;
        private readonly IGenreRepository _genreRepo;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public BookApiController(IBookRepository bookRepo, IGenreRepository genreRepo, IFileService fileService, IMapper mapper)
        {
            _bookRepo = bookRepo;
            _genreRepo = genreRepo;
            _fileService = fileService;
            _mapper = mapper;
        }

        // GET: api/book  
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDTO>>> GetBooks()
        {
            var books = await _bookRepo.GetBooks();
            return Ok(books);
        }

        // GET: api/book/{id}  
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDTO>> GetBook(int id)
        {
            var book = await _bookRepo.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        // POST: api/book  
        [HttpPost]
        public async Task<ActionResult<BookDTO>> AddBook([FromForm] BookDTO bookToAdd)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (bookToAdd.ImageFile != null)
                {
                    if (bookToAdd.ImageFile.Length > 1 * 1024 * 1024)
                    {
                        return BadRequest("Image file cannot exceed 1 MB");
                    }
                    string[] allowedExtensions = { ".jpeg", ".jpg", ".png" };
                    string imageName = await _fileService.SaveFile(bookToAdd.ImageFile, allowedExtensions);
                    bookToAdd.Image = imageName;
                }

                Book book = _mapper.Map<Book>(bookToAdd);
                await _bookRepo.AddBook(book);
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookToAdd);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // PUT: api/book/{id}  
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookDTO bookToUpdate)
        {
            if (id != bookToUpdate.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string oldImage = "";
                if (bookToUpdate.ImageFile != null)
                {
                    if (bookToUpdate.ImageFile.Length > 1 * 1024 * 1024)
                    {
                        return BadRequest("Image file cannot exceed 1 MB");
                    }
                    string[] allowedExtensions = { ".jpeg", ".jpg", ".png" };
                    string imageName = await _fileService.SaveFile(bookToUpdate.ImageFile, allowedExtensions);
                    oldImage = bookToUpdate.Image; // Hold old image to delete it later  
                    bookToUpdate.Image = imageName;
                }

                Book book = _mapper.Map<Book>(bookToUpdate);
                await _bookRepo.UpdateBook(book);

                // Delete old image if it was replaced  
                if (!string.IsNullOrWhiteSpace(oldImage))
                {
                    _fileService.DeleteFile(oldImage);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // DELETE: api/book/{id}  
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _bookRepo.GetBookById(id);
                if (book == null)
                {
                    return NotFound();
                }

                await _bookRepo.DeleteBook(book);

                if (!string.IsNullOrWhiteSpace(book.Image))
                {
                    _fileService.DeleteFile(book.Image);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}