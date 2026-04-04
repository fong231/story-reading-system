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
            var stories = ((IEnumerable<dynamic>)rawData).ToList();

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
}
