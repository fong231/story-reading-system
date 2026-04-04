using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Patterns.Observer;

namespace BE.Services
{
    public interface IChapterService
    {
        Task<IEnumerable<object>> GetChaptersByStoryAsync(int storyId);
        Task<object> GetChapterByIdAsync(int id);
        Task<Chapter> CreateChapterAsync(int storyId, string title, int chapterNumber, string content);
        Task<bool> UpdateChapterAsync(int id, string title, string content);
        Task<bool> DeleteChapterAsync(int id);
    }

    public class ChapterService : IChapterService
    {
        private readonly StoryReaderDbContext _context;
        private readonly IStoryObserver _storyObserver;

        public ChapterService(StoryReaderDbContext context, IStoryObserver storyObserver)
        {
            _context = context;
            _storyObserver = storyObserver;
        }

        public async Task<IEnumerable<object>> GetChaptersByStoryAsync(int storyId)
        {
            return await _context.Chapters
                .Where(c => c.StoryId == storyId && c.IsActive)
                .OrderBy(c => c.ChapterNumber)
                .Select(c => new
                {
                    c.ChapterId,
                    c.Title,
                    c.ChapterNumber,
                    c.ViewCount,
                    c.PublishedAt
                })
                .ToListAsync();
        }

        public async Task<object> GetChapterByIdAsync(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .ThenInclude(s => s.Author)
                .FirstOrDefaultAsync(c => c.ChapterId == id && c.IsActive);

            if (chapter == null) return null;

            chapter.ViewCount++;
            await _context.SaveChangesAsync();

            return new
            {
                chapter.ChapterId,
                chapter.Title,
                chapter.ChapterNumber,
                chapter.Content,
                chapter.ViewCount,
                chapter.PublishedAt,
                Story = new
                {
                    chapter.Story.StoryId,
                    chapter.Story.Title,
                    Author = new
                    {
                        chapter.Story.Author.UserId,
                        chapter.Story.Author.Username
                    }
                }
            };
        }

        public async Task<Chapter> CreateChapterAsync(int storyId, string title, int chapterNumber, string content)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null) return null;

            var chapter = new Chapter
            {
                StoryId = storyId,
                Title = title,
                ChapterNumber = chapterNumber,
                Content = content,
                ViewCount = 0,
                PublishedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            // Notify followers
            var message = $"Chương mới \"{chapter.Title}\" vừa được cập nhật!";
            await _storyObserver.NotifyFollowersAsync(storyId, message, chapter.ChapterId);

            return chapter;
        }

        public async Task<bool> UpdateChapterAsync(int id, string title, string content)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return false;

            chapter.Title = title ?? chapter.Title;
            chapter.Content = content ?? chapter.Content;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteChapterAsync(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return false;

            chapter.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
