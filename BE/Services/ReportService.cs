using BE.Models;
using BE.Patterns.TemplateMethod;

namespace BE.Services
{
    // ================================================================
    // Report Service Interface
    // ================================================================
    public interface IReportService
    {
        Task<byte[]> GenerateReport(int authorId, DateTime startDate, DateTime endDate, string reportType);
    }

    public class ReportService : IReportService
    {
        private readonly StoryReaderDbContext _context;

        public ReportService(StoryReaderDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateReport(int authorId, DateTime startDate, DateTime endDate, string reportType)
        {
            var reportTemplate = reportType.ToLower() switch
            {
                "revenue" => (AuthorReportTemplate)new RevenueReport(_context),
                "viewgrowth" => new ViewGrowthReport(_context),
                _ => throw new ArgumentException("Invalid report type")
            };
            return await reportTemplate.GenerateReportAsync(authorId, startDate, endDate);
        }
    }
}
