using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IStoryService _storyService;

        public StoriesController(IStoryService storyService)
        {
            _storyService = storyService;
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
        public async Task<ActionResult<Story>> CreateStory([FromBody] CreateStoryDto dto)
        {
            var story = await _storyService.CreateStoryAsync(
                dto.Title,
                dto.Description,
                dto.CoverImage,
                dto.AuthorId,
                dto.CategoryId
            );

            return CreatedAtAction(nameof(GetStory), new { id = story.StoryId }, story);
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
    public class CreateStoryDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImage { get; set; }
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