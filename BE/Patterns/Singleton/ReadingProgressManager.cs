using Microsoft.EntityFrameworkCore;
using BE.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BE.Patterns.Singleton
{
    // ================================================================
    // SINGLETON PATTERN - Classic Implementation (Thread-Safe)
    // Đảm bảo chỉ có DUY NHẤT 1 instance manager trong suốt phiên làm việc.
    // ================================================================
    public interface IReadingProgressManager
    {
        Task<ReadingProgress> GetOrCreateProgressAsync(int userId);
        Task UpdateProgressAsync(int userId, int storyId, int chapterId, int position);
        Task IncrementStatsAsync(int userId, bool isNewStory, bool isNewChapter);
    }

    public sealed class ReadingProgressManager : IReadingProgressManager
    {
        // 1. Static Instance (Classic Singleton)
        private static ReadingProgressManager _instance = null;
        private static readonly object _lock = new object();

        // Cần IServiceProvider để lấy DbContext vì DbContext có Scope ngắn hơn Singleton
        private static IServiceProvider _serviceProvider;

        // 2. Private constructor
        private ReadingProgressManager() { }

        // 3. Truy cập instance qua thuộc tính static
        public static ReadingProgressManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ReadingProgressManager();
                        }
                    }
                }
                return _instance;
            }
        }

        // Phương thức để "tiêm" ServiceProvider vào Singleton khi khởi tạo App
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private StoryReaderDbContext GetDbContext()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ReadingProgressManager must be initialized with IServiceProvider.");
            
            // Vì Singleton sống lâu hơn DbContext, ta phải tạo 1 Scope mới để lấy DbContext
            var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<StoryReaderDbContext>();
        }

        // --- CÁC HÀM NGHIỆP VỤ (GIỮ NGUYÊN TÊN VÀ CHỮ KÝ CŨ) ---

        public async Task<ReadingProgress> GetOrCreateProgressAsync(int userId)
        {
            using (var context = GetDbContext())
            {
                var progress = await context.ReadingProgresses
                    .Include(p => p.CurrentStory)
                    .Include(p => p.CurrentChapter)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (progress == null)
                {
                    progress = new ReadingProgress
                    {
                        UserId = userId,
                        TotalStoriesRead = 0,
                        TotalChaptersRead = 0,
                        LastReadPosition = 0,
                        LastReadAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.ReadingProgresses.Add(progress);
                    await context.SaveChangesAsync();
                }

                return progress;
            }
        }

        public async Task UpdateProgressAsync(int userId, int storyId, int chapterId, int position)
        {
            using (var context = GetDbContext())
            {
                var progress = await GetOrCreateProgressAsync(userId);
                // Vì progress được lấy từ Context cũ đã bị dispose, ta cần attach nó lại hoặc lấy lại
                var dbProgress = await context.ReadingProgresses.FindAsync(progress.ProgressId);

                dbProgress.CurrentStoryId = storyId;
                dbProgress.CurrentChapterId = chapterId;
                dbProgress.LastReadPosition = position;
                dbProgress.LastReadAt = DateTime.UtcNow;
                dbProgress.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
            }
        }

        public async Task IncrementStatsAsync(int userId, bool isNewStory, bool isNewChapter)
        {
            using (var context = GetDbContext())
            {
                var progress = await GetOrCreateProgressAsync(userId);
                var dbProgress = await context.ReadingProgresses.FindAsync(progress.ProgressId);

                if (isNewStory) dbProgress.TotalStoriesRead++;
                if (isNewChapter) dbProgress.TotalChaptersRead++;

                dbProgress.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
    }
}
