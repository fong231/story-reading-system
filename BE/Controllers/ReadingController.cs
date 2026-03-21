using Microsoft.AspNetCore.Mvc;
using BE.Patterns.Singleton;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingController : ControllerBase
    {
        private readonly IReadingProgressManager _progressManager;
        private readonly IReadingModeService _modeService;

        public ReadingController(
            IReadingProgressManager progressManager,
            IReadingModeService modeService)
        {
            _progressManager = progressManager;
            _modeService = modeService;
        }

        // ================================================================
        // SINGLETON PATTERN - Reading Progress
        // ================================================================

        // GET: api/Reading/Progress/5
        [HttpGet("Progress/{userId}")]
        public async Task<ActionResult<object>> GetReadingProgress(int userId)
        {
            var progress = await _progressManager.GetOrCreateProgressAsync(userId);

            return Ok(new
            {
                progress.ProgressId,
                progress.UserId,
                progress.TotalStoriesRead,
                progress.TotalChaptersRead,
                progress.LastReadPosition,
                progress.LastReadAt,
                CurrentStory = progress.CurrentStory != null ? new
                {
                    progress.CurrentStory.StoryId,
                    progress.CurrentStory.Title
                } : null,
                CurrentChapter = progress.CurrentChapter != null ? new
                {
                    progress.CurrentChapter.ChapterId,
                    progress.CurrentChapter.Title,
                    progress.CurrentChapter.ChapterNumber
                } : null
            });
        }

        // POST: api/Reading/Progress
        [HttpPost("Progress")]
        public async Task<ActionResult> UpdateReadingProgress([FromBody] UpdateProgressDto dto)
        {
            await _progressManager.UpdateProgressAsync(
                dto.UserId,
                dto.StoryId,
                dto.ChapterId,
                dto.Position
            );

            return Ok(new { message = "Progress updated successfully" });
        }

        // POST: api/Reading/Progress/Stats
        [HttpPost("Progress/Stats")]
        public async Task<ActionResult> IncrementStats([FromBody] IncrementStatsDto dto)
        {
            await _progressManager.IncrementStatsAsync(
                dto.UserId,
                dto.IsNewStory,
                dto.IsNewChapter
            );

            return Ok(new { message = "Stats updated successfully" });
        }

        // ================================================================
        // Reading Mode Management (Logic moved to Frontend)
        // ================================================================

        // GET: api/Reading/Mode/5
        [HttpGet("Mode/{userId}")]
        public async Task<ActionResult<object>> GetReadingMode(int userId)
        {
            var mode = await _modeService.GetOrCreateModeAsync(userId);

            return Ok(new
            {
                mode.ModeId,
                mode.UserId,
                mode.Theme,
                mode.NavigationMode,
                mode.FontSize,
                mode.FontFamily,
                mode.LineHeight
            });
        }

        // POST: api/Reading/Mode
        [HttpPost("Mode")]
        public async Task<ActionResult> UpdateReadingMode([FromBody] UpdateModeDto dto)
        {
            var mode = await _modeService.UpdateModeAsync(
                dto.UserId,
                dto.Theme,
                dto.NavigationMode,
                dto.FontSize,
                dto.FontFamily,
                dto.LineHeight
            );

            return Ok(new
            {
                message = "Reading mode updated successfully",
                mode.Theme,
                mode.NavigationMode,
                mode.FontSize,
                mode.FontFamily,
                mode.LineHeight
            });
        }
    }

    // DTOs
    public class UpdateProgressDto
    {
        public int UserId { get; set; }
        public int StoryId { get; set; }
        public int ChapterId { get; set; }
        public int Position { get; set; }
    }

    public class IncrementStatsDto
    {
        public int UserId { get; set; }
        public bool IsNewStory { get; set; }
        public bool IsNewChapter { get; set; }
    }

    public class UpdateModeDto
    {
        public int UserId { get; set; }
        public string Theme { get; set; } // Day, Night
        public string NavigationMode { get; set; } // Scroll, Flip
        public int? FontSize { get; set; }
        public string FontFamily { get; set; }
        public decimal? LineHeight { get; set; }
    }
}
