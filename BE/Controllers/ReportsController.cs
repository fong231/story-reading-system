using BE.Models;
using BE.Services;
using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly StoryReaderDbContext _context;

        public ReportsController(StoryReaderDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports/Author/5?type=revenue&startDate=2024-01-01&endDate=2024-12-31&storyId=10
        [HttpGet("Author/{authorId}")]
        public async Task<IActionResult> GenerateReport(
            int authorId,
            [FromQuery] string type,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? storyId = null)
        {
            try
            {
                // TEMPLATE METHOD PATTERN: Sử dụng template
                var reportService = new ReportService(_context);
                var fileBytes = await reportService.GenerateReport(authorId, startDate, endDate, type, storyId);

                string fileName;
                if (storyId.HasValue)
                {
                    fileName = $"{type}_report_story_{storyId}_{DateTime.UtcNow:yyyyMMdd}.txt";
                }
                else
                {
                    fileName = $"{type}_report_author_{authorId}_{DateTime.UtcNow:yyyyMMdd}.txt";
                }

                return File(fileBytes, "text/plain", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating report", error = ex.Message });
            }
        }

        // GET: api/Reports/Story/10?type=revenue&startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("Story/{storyId}")]
        public async Task<IActionResult> GenerateStoryReport(
            int storyId,
            [FromQuery] string type,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                // Tìm AuthorId của truyện
                var story = await _context.Stories.FindAsync(storyId);
                if (story == null) return NotFound(new { message = "Story not found" });

                var reportService = new ReportService(_context);
                var fileBytes = await reportService.GenerateReport(story.AuthorId, startDate, endDate, type, storyId);

                var fileName = $"{type}_report_story_{storyId}_{DateTime.UtcNow:yyyyMMdd}.txt";

                return File(fileBytes, "text/plain", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating report", error = ex.Message });
            }
        }

        // GET: api/Reports/Types
        [HttpGet("Types")]
        public ActionResult<object> GetReportTypes()
        {
            return Ok(new
            {
                types = new[]
                {
                    new { id = "revenue", name = "Revenue Report", description = "Doanh thu từ lượt xem" },
                    new { id = "viewgrowth", name = "View Growth Report", description = "Tăng trưởng lượt xem" },
                    new { id = "engagement", name = "Engagement Report", description = "Tương tác của độc giả" }
                }
            });
        }
    }
}
