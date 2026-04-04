using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Patterns.TemplateMethod
{
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
}
