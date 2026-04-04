using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IStoryService _storyService;
        private readonly IWebHostEnvironment _environment;

        public StoriesController(IStoryService storyService, IWebHostEnvironment environment)
        {
            _storyService = storyService;
            _environment = environment;
        }

        // GET: api/Stories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetStories()
        {
            var stories = await _storyService.GetAllStoriesAsync();
            return Ok(stories);
        }

        // GET: api/Stories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetStory(int id)
        {
            var story = await _storyService.GetStoryByIdAsync(id);
            if (story == null) return NotFound();

            return Ok(story);
        }

        // GET: api/Stories/Category/1
        [HttpGet("Category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStoriesByCategory(int categoryId)
        {
            var stories = await _storyService.GetStoriesByCategoryAsync(categoryId);
            return Ok(stories);
        }

        // GET: api/Stories/Author/1
        [HttpGet("Author/{authorId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStoriesByAuthor(int authorId)
        {
            var stories = await _storyService.GetStoriesByAuthorAsync(authorId);
            return Ok(stories);
        }

        // POST: api/Stories
        [HttpPost]
        public async Task<ActionResult<Story>> CreateStory([FromForm] CreateStoryFormDto dto)
        {
            string coverImageUrl = dto.CoverImage; // Mặc định dùng URL nếu có

            // Kiểm tra xem đã chọn ảnh chưa
            if (dto.CoverFile == null || dto.CoverFile.Length == 0)
            {
                return BadRequest("Vui lòng chọn ảnh bìa cho truyện!");
            }

            // Xử lý upload file nếu có
            if (dto.CoverFile != null && dto.CoverFile.Length > 0)
            {
                var extension = Path.GetExtension(dto.CoverFile.FileName).ToLower();
                
                // Kiểm tra định dạng file (chỉ cho phép ảnh)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Định dạng ảnh không hợp lệ. Chỉ chấp nhận .jpg, .jpeg, .png, .gif, .webp");
                }

                var uploadPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads");
                
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.CoverFile.CopyToAsync(stream);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                coverImageUrl = $"{baseUrl}/uploads/{fileName}";
            }

            try
            {
                var story = await _storyService.CreateStoryAsync(
                    dto.Title,
                    dto.Description,
                    coverImageUrl,
                    dto.AuthorId,
                    dto.CategoryId
                );

                return CreatedAtAction(nameof(GetStory), new { id = story.StoryId }, story);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Stories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStory(int id, [FromBody] UpdateStoryDto dto)
        {
            var success = await _storyService.UpdateStoryAsync(
                id,
                dto.Title,
                dto.Description,
                dto.CoverImage,
                dto.Status
            );

            if (!success) return NotFound();

            return NoContent();
        }

        // DELETE: api/Stories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStory(int id)
        {
            var success = await _storyService.DeleteStoryAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }

    // DTOs
    public class CreateStoryFormDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? CoverImage { get; set; } // Giữ lại để hỗ trợ URL nếu cần
        public IFormFile? CoverFile { get; set; } // Hỗ trợ upload file
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateStoryDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImage { get; set; }
        public string Status { get; set; }
    }
}