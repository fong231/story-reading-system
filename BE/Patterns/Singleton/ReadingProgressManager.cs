using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Patterns.Singleton
{
    // ================================================================
    // SINGLETON PATTERN - Reading Progress Manager
    // Ensures only one reading progress instance per user
    // ================================================================
    public interface IReadingProgressManager
    {
        Task<ReadingProgress> GetOrCreateProgressAsync(int userId);
        Task UpdateProgressAsync(int userId, int storyId, int chapterId, int position);
        Task IncrementStatsAsync(int userId, bool isNewStory, bool isNewChapter);
    }

    public class ReadingProgressManager : IReadingProgressManager
    {
        private readonly StoryReaderDbContext _context;

        public ReadingProgressManager(StoryReaderDbContext context)
        {
            _context = context;
        }

        // Get or create SINGLETON instance for user
        public async Task<ReadingProgress> GetOrCreateProgressAsync(int userId)
        {
            // Try to get existing instance
            var progress = await _context.ReadingProgresses
                .Include(p => p.CurrentStory)
                .Include(p => p.CurrentChapter)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                // Create SINGLETON instance if not exists
                progress = new ReadingProgress
                {
                    UserId = userId,
                    TotalStoriesRead = 0,
                    TotalChaptersRead = 0,
                    LastReadPosition = 0,
                    LastReadAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ReadingProgresses.Add(progress);
                await _context.SaveChangesAsync();
            }

            return progress;
        }

        public async Task UpdateProgressAsync(int userId, int storyId, int chapterId, int position)
        {
            var progress = await GetOrCreateProgressAsync(userId);

            progress.CurrentStoryId = storyId;
            progress.CurrentChapterId = chapterId;
            progress.LastReadPosition = position;
            progress.LastReadAt = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task IncrementStatsAsync(int userId, bool isNewStory, bool isNewChapter)
        {
            var progress = await GetOrCreateProgressAsync(userId);

            if (isNewStory)
            {
                progress.TotalStoriesRead++;
            }

            if (isNewChapter)
            {
                progress.TotalChaptersRead++;
            }

            progress.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
