using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChaptersController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public ChaptersController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        // GET: api/Chapters/Story/5
        [HttpGet("Story/{storyId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetChaptersByStory(int storyId)
        {
            var chapters = await _chapterService.GetChaptersByStoryAsync(storyId);
            return Ok(chapters);
        }

        // GET: api/Chapters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetChapter(int id)
        {
            var chapter = await _chapterService.GetChapterByIdAsync(id);
            if (chapter == null) return NotFound();

            return Ok(chapter);
        }

        // POST: api/Chapters
        [HttpPost]
        public async Task<ActionResult<Chapter>> CreateChapter([FromBody] CreateChapterDto dto)
        {
            var chapter = await _chapterService.CreateChapterAsync(
                dto.StoryId,
                dto.Title,
                dto.ChapterNumber,
                dto.Content
            );

            if (chapter == null) return NotFound("Story not found");

            return CreatedAtAction(nameof(GetChapter), new { id = chapter.ChapterId }, chapter);
        }

        // PUT: api/Chapters/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChapter(int id, [FromBody] UpdateChapterDto dto)
        {
            var success = await _chapterService.UpdateChapterAsync(id, dto.Title, dto.Content);
            if (!success) return NotFound();

            return NoContent();
        }

        // DELETE: api/Chapters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            var success = await _chapterService.DeleteChapterAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }

    // DTOs
    public class CreateChapterDto
    {
        public int StoryId { get; set; }
        public string Title { get; set; }
        public int ChapterNumber { get; set; }
        public string Content { get; set; }
    }

    public class UpdateChapterDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
