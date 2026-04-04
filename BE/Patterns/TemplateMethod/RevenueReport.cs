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

        protected override async Task<object> QueryDataAsync(int authorId, DateTime startDate, DateTime endDate, int? storyId = null)
        {
            // Truy vấn dữ liệu doanh thu
            var query = _context.Stories
                .Where(s => s.AuthorId == authorId && s.IsActive);

            if (storyId.HasValue)
            {
                query = query.Where(s => s.StoryId == storyId.Value);
            }

            var stories = await query
                .Include(s => s.Chapters)
                .Include(s => s.Ratings)
                .ToListAsync();

            return new { Stories = stories, IsSingleStory = storyId.HasValue };
        }

        protected override object CalculateMetrics(object rawData)
        {
            dynamic data = rawData;
            var stories = (List<Story>)data.Stories;
            bool isSingleStory = data.IsSingleStory;

            // Tính toán revenue (giả định: 1000đ per view)
            var totalRevenue = stories.Sum(s => (long)s.ViewCount * 1000);
            var totalViews = stories.Sum(s => s.ViewCount);
            var totalRatings = stories.Sum(s => s.TotalRatings);
            var avgRating = stories.Any() ? stories.Average(s => (double)s.AverageRating) : 0;
            var title = isSingleStory && stories.Any() ? stories.First().Title : "TẤT CẢ TRUYỆN";

            return new
            {
                Title = title,
                TotalRevenue = totalRevenue,
                TotalViews = totalViews,
                TotalRatings = totalRatings,
                AverageRating = Math.Round(avgRating, 2),
                StoryCount = stories.Count,
                IsSingleStory = isSingleStory
            };
        }

        protected override string FormatReport(object calculatedData)
        {
            dynamic data = calculatedData;
            string reportTitle = data.IsSingleStory ? $"BÁO CÁO DOANH THU: {data.Title}" : "BÁO CÁO DOANH THU TỔNG HỢP";

            return $@"
================================================================================
                        {reportTitle}
================================================================================

Tên truyện/Nhóm:       {data.Title}
Tổng doanh thu:        {data.TotalRevenue:N0} VNĐ
Tổng lượt xem:         {data.TotalViews:N0}
Tổng đánh giá:         {data.TotalRatings}
Điểm TB:               {data.AverageRating}/5.0
{(data.IsSingleStory ? "" : $"Số truyện:             {data.StoryCount}")}

================================================================================
";
        }
    }
}
