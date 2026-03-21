using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Patterns.TemplateMethod
{
    // ================================================================
    // TEMPLATE METHOD PATTERN - Abstract Report Template
    // ================================================================
    public abstract class AuthorReportTemplate
    {
        protected readonly StoryReaderDbContext _context;

        protected AuthorReportTemplate(StoryReaderDbContext context)
        {
            _context = context;
        }

        // TEMPLATE METHOD - Định nghĩa skeleton algorithm
        public async Task<byte[]> GenerateReportAsync(int authorId, DateTime startDate, DateTime endDate)
        {
            // 1. Mở kết nối Database (cố định)
            await OpenDatabaseConnection();

            // 2. Truy vấn dữ liệu (trừu tượng - mỗi report khác nhau)
            var data = await QueryDataAsync(authorId, startDate, endDate);

            // 3. Tính toán chỉ số (trừu tượng - mỗi report khác nhau)
            var calculatedData = CalculateMetrics(data);

            // 4. Định dạng báo cáo (trừu tượng - mỗi report khác nhau)
            var formattedReport = FormatReport(calculatedData);

            // 5. Xuất file PDF/Excel (cố định)
            var fileBytes = await ExportToFileAsync(formattedReport);

            // 6. Đóng kết nối (cố định)
            await CloseDatabaseConnection();

            return fileBytes;
        }

        // Protected methods - có thể override
        protected virtual async Task OpenDatabaseConnection()
        {
            // Database connection đã được quản lý bởi EF Core
            await Task.CompletedTask;
        }

        protected virtual async Task CloseDatabaseConnection()
        {
            await Task.CompletedTask;
        }

        // Abstract methods - bắt buộc override
        protected abstract Task<object> QueryDataAsync(int authorId, DateTime startDate, DateTime endDate);
        protected abstract object CalculateMetrics(object rawData);
        protected abstract string FormatReport(object calculatedData);

        // Concrete method - xuất file (cố định cho tất cả reports)
        protected virtual async Task<byte[]> ExportToFileAsync(string formattedContent)
        {
            // Đơn giản hóa: Convert string to bytes (production nên dùng PDF library)
            return await Task.FromResult(System.Text.Encoding.UTF8.GetBytes(formattedContent));
        }
    }

    // ================================================================
    // CONCRETE REPORT 1: Revenue Report (Báo cáo doanh thu)
    // ================================================================
    public class RevenueReport : AuthorReportTemplate
    {
        public RevenueReport(StoryReaderDbContext context) : base(context) { }

        protected override async Task<object> QueryDataAsync(int authorId, DateTime startDate, DateTime endDate)
        {
            // Truy vấn dữ liệu doanh thu (giả định có bảng Revenue)
            var stories = await _context.Stories
                .Where(s => s.AuthorId == authorId && s.IsActive)
                .Include(s => s.Chapters)
                .Include(s => s.Ratings)
                .ToListAsync();

            return stories;
        }

        protected override object CalculateMetrics(object rawData)
        {
            var stories = (List<Story>)rawData;

            // Tính toán revenue (giả định: 1000đ per view)
            var totalRevenue = stories.Sum(s => s.ViewCount * 1000);
            var totalViews = stories.Sum(s => s.ViewCount);
            var totalRatings = stories.Sum(s => s.TotalRatings);
            var avgRating = stories.Average(s => (double)s.AverageRating);

            return new
            {
                TotalRevenue = totalRevenue,
                TotalViews = totalViews,
                TotalRatings = totalRatings,
                AverageRating = Math.Round(avgRating, 2),
                StoryCount = stories.Count
            };
        }

        protected override string FormatReport(object calculatedData)
        {
            dynamic data = calculatedData;

            return $@"
================================================================================
                        BÁO CÁO DOANH THU TÁC GIẢ
================================================================================

Tổng doanh thu:        {data.TotalRevenue:N0} VNĐ
Tổng lượt xem:         {data.TotalViews:N0}
Tổng đánh giá:         {data.TotalRatings}
Điểm TB:               {data.AverageRating}/5.0
Số truyện:             {data.StoryCount}

================================================================================
";
        }
    }

    // ================================================================
    // CONCRETE REPORT 2: View Growth Report (Báo cáo lượt xem)
    // ================================================================
    public class ViewGrowthReport : AuthorReportTemplate
    {
        public ViewGrowthReport(StoryReaderDbContext context) : base(context) { }

        protected override async Task<object> QueryDataAsync(int authorId, DateTime startDate, DateTime endDate)
        {
            var stories = await _context.Stories
                .Where(s => s.AuthorId == authorId && s.IsActive)
                .Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.ViewCount,
                    s.CreatedAt,
                    ChapterCount = s.Chapters.Count(c => c.IsActive)
                })
                .ToListAsync();

            return stories;
        }

        protected override object CalculateMetrics(object rawData)
        {
            var stories = (List<dynamic>)rawData;

            // Tính growth rate
            var totalViews = stories.Sum(s => (int)s.ViewCount);
            var avgViewsPerStory = stories.Any() ? totalViews / stories.Count : 0;

            var topStories = stories
                .OrderByDescending(s => s.ViewCount)
                .Take(5)
                .ToList();

            return new
            {
                TotalViews = totalViews,
                AverageViewsPerStory = avgViewsPerStory,
                TopStories = topStories,
                TotalStories = stories.Count
            };
        }

        protected override string FormatReport(object calculatedData)
        {
            dynamic data = calculatedData;

            var report = $@"
================================================================================
                    BÁO CÁO TĂNG TRƯỞNG LƯỢT XEM
================================================================================

Tổng lượt xem:         {data.TotalViews:N0}
TB lượt xem/truyện:    {data.AverageViewsPerStory:N0}
Tổng số truyện:        {data.TotalStories}

TOP 5 TRUYỆN NHIỀU LƯỢT XEM:
";

            int rank = 1;
            foreach (var story in data.TopStories)
            {
                report += $"{rank}. {story.Title} - {story.ViewCount:N0} views\n";
                rank++;
            }

            report += "\n================================================================================\n";

            return report;
        }
    }

    // ================================================================
    // Report Service Interface
    // ================================================================
    public interface IReportService
    {
        Task<byte[]> GenerateRevenueReportAsync(int authorId, DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateViewGrowthReportAsync(int authorId, DateTime startDate, DateTime endDate);
    }

    public class ReportService : IReportService
    {
        private readonly StoryReaderDbContext _context;

        public ReportService(StoryReaderDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateRevenueReportAsync(int authorId, DateTime startDate, DateTime endDate)
        {
            var report = new RevenueReport(_context);
            return await report.GenerateReportAsync(authorId, startDate, endDate);
        }

        public async Task<byte[]> GenerateViewGrowthReportAsync(int authorId, DateTime startDate, DateTime endDate)
        {
            var report = new ViewGrowthReport(_context);
            return await report.GenerateReportAsync(authorId, startDate, endDate);
        }
    }
}
