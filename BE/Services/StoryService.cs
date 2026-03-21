using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Patterns.Factory;

namespace BE.Services
{
    public interface IStoryService
    {
        Task<IEnumerable<object>> GetAllStoriesAsync();
        Task<object> GetStoryByIdAsync(int id);
        Task<IEnumerable<object>> GetStoriesByCategoryAsync(int categoryId);
        Task<Story> CreateStoryAsync(string title, string description, string coverImage, int authorId, int categoryId);
        Task<bool> UpdateStoryAsync(int id, string title, string description, string coverImage, string status);
        Task<bool> DeleteStoryAsync(int id);
    }

    public class StoryService : IStoryService
    {
        private readonly StoryReaderDbContext _context;
        private readonly IStoryFactory _storyFactory;

        public StoryService(StoryReaderDbContext context, IStoryFactory storyFactory)
        {
            _context = context;
            _storyFactory = storyFactory;
        }

        public async Task<IEnumerable<object>> GetAllStoriesAsync()
        {
            return await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    s.CoverImage,
                    s.ViewCount,
                    s.AverageRating,
                    s.TotalRatings,
                    s.Status,
                    s.CreatedAt,
                    Author = new { s.Author.UserId, s.Author.Username },
                    Category = new { s.Category.CategoryId, s.Category.Name },
                    TotalChapters = s.Chapters.Count(c => c.IsActive)
                })
                .ToListAsync();
        }

        public async Task<object> GetStoryByIdAsync(int id)
        {
            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .Include(s => s.Chapters.Where(c => c.IsActive))
                .FirstOrDefaultAsync(s => s.StoryId == id && s.IsActive);

            if (story == null) return null;

            story.ViewCount++;
            await _context.SaveChangesAsync();

            return new
            {
                story.StoryId,
                story.Title,
                story.Description,
                story.CoverImage,
                story.ViewCount,
                story.AverageRating,
                story.TotalRatings,
                story.Status,
                story.CreatedAt,
                Author = new { story.Author.UserId, story.Author.Username },
                Category = new { story.Category.CategoryId, story.Category.Name },
                Chapters = story.Chapters.OrderBy(c => c.ChapterNumber).Select(c => new
                {
                    c.ChapterId,
                    c.Title,
                    c.ChapterNumber,
                    c.ViewCount,
                    c.PublishedAt
                })
            };
        }

        public async Task<IEnumerable<object>> GetStoriesByCategoryAsync(int categoryId)
        {
            return await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .Where(s => s.CategoryId == categoryId && s.IsActive)
                .OrderByDescending(s => s.ViewCount)
                .Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    s.CoverImage,
                    s.ViewCount,
                    s.AverageRating,
                    s.TotalRatings,
                    s.Status,
                    Author = new { s.Author.UserId, s.Author.Username },
                    Category = new { s.Category.CategoryId, s.Category.Name },
                    StoryType = _storyFactory.GetStoryType(categoryId)
                })
                .ToListAsync();
        }

        public async Task<Story> CreateStoryAsync(string title, string description, string coverImage, int authorId, int categoryId)
        {
            var story = _storyFactory.CreateDbStory(title, description, authorId, categoryId);
            if (!string.IsNullOrEmpty(coverImage)) story.CoverImage = coverImage;

            _context.Stories.Add(story);
            await _context.SaveChangesAsync();
            return story;
        }

        public async Task<bool> UpdateStoryAsync(int id, string title, string description, string coverImage, string status)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story == null) return false;

            story.Title = title ?? story.Title;
            story.Description = description ?? story.Description;
            story.CoverImage = coverImage ?? story.CoverImage;
            story.Status = status ?? story.Status;
            story.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStoryAsync(int id)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story == null) return false;

            story.IsActive = false;
            story.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
