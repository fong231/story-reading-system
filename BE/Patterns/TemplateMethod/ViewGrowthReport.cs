using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Patterns.TemplateMethod
{
    // ================================================================
    // CONCRETE REPORT 2: View Growth Report (Báo cáo lượt xem)
    // ================================================================
    public class ViewGrowthReport : AuthorReportTemplate
    {
        public ViewGrowthReport(StoryReaderDbContext context) : base(context) { }

        protected override async Task<object> QueryDataAsync(int authorId, DateTime startDate, DateTime endDate, int? storyId = null)
        {
            var query = _context.Stories
                .Where(s => s.AuthorId == authorId && s.IsActive);

            if (storyId.HasValue)
            {
                query = query.Where(s => s.StoryId == storyId.Value);
            }

            var stories = await query
                .Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.ViewCount,
                    s.CreatedAt,
                    ChapterCount = s.Chapters.Count(c => c.IsActive)
                })
                .ToListAsync();

            return new { Stories = stories, IsSingleStory = storyId.HasValue };
        }

        protected override object CalculateMetrics(object rawData)
        {
            dynamic data = rawData;
            var stories = ((IEnumerable<dynamic>)data.Stories).ToList();
            bool isSingleStory = data.IsSingleStory;

            // Tính growth rate
            var totalViews = stories.Sum(s => (int)s.ViewCount);
            var avgViewsPerStory = stories.Any() ? (double)totalViews / stories.Count : 0;
            var title = isSingleStory && stories.Any() ? stories.First().Title : "TẤT CẢ TRUYỆN";

            var topStories = !isSingleStory ? stories
                .OrderByDescending(s => (int)s.ViewCount)
                .Take(5)
                .ToList() : new List<dynamic>();

            return new
            {
                Title = title,
                TotalViews = totalViews,
                AverageViewsPerStory = avgViewsPerStory,
                TopStories = topStories,
                TotalStories = stories.Count,
                IsSingleStory = isSingleStory
            };
        }

        protected override string FormatReport(object calculatedData)
        {
            dynamic data = calculatedData;
            string reportTitle = data.IsSingleStory ? $"BÁO CÁO LƯỢT XEM: {data.Title}" : "BÁO CÁO TĂNG TRƯỞNG LƯỢT XEM TỔNG HỢP";

            var report = $@"
================================================================================
                    {reportTitle}
================================================================================

Tên truyện/Nhóm:       {data.Title}
Tổng lượt xem:         {data.TotalViews:N0}
{(data.IsSingleStory ? "" : $"TB lượt xem/truyện:    {data.AverageViewsPerStory:N0}")}
{(data.IsSingleStory ? "" : $"Tổng số truyện:        {data.TotalStories}")}
";

            if (!data.IsSingleStory && data.TopStories.Count > 0)
            {
                report += "\nTOP 5 TRUYỆN NHIỀU LƯỢT XEM:\n";
                int rank = 1;
                foreach (var story in data.TopStories)
                {
                    report += $"{rank}. {story.Title} - {story.ViewCount:N0} views\n";
                    rank++;
                }
            }

            report += "\n================================================================================\n";

            return report;
        }
    }
}
