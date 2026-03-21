using BE.Models;
using BE.Patterns.TemplateMethod;
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

        // GET: api/Reports/Author/5?type=revenue&startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("Author/{authorId}")]
        public async Task<IActionResult> GenerateReport(
            int authorId,
            [FromQuery] string type,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                // TEMPLATE METHOD PATTERN: Sử dụng template
                var reportTemplate = type.ToLower() switch
                {
                    "revenue" => (AuthorReportTemplate)new RevenueReport(_context),
                    "viewgrowth" => new ViewGrowthReport(_context),
                    _ => throw new ArgumentException("Invalid report type")
                };
                var fileBytes = await reportTemplate.GenerateReportAsync(authorId, startDate, endDate);

                var fileName = $"{type}_report_{authorId}_{DateTime.UtcNow:yyyyMMdd}.txt";

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
