using BE.Models;

namespace BE.Patterns.Factory
{
    // ================================================================
    // FACTORY PATTERN - Concrete Factory Implementation
    // ================================================================
    public interface IStoryFactory
    {
        IStoryCategory CreateStory(string title, string description, int authorId, int categoryId);
        Story CreateDbStory(string title, string description, int authorId, int categoryId);
        IStoryCategory GetStoryType(int categoryId);
    }

    public class StoryFactory : IStoryFactory
    {
        public IStoryCategory CreateStory(string title, string description, int authorId, int categoryId)
        {
            IStoryCategory story = GetStoryType(categoryId);

            // Set common properties
            story.Title = title;
            story.Description = description;
            story.AuthorId = authorId;
            story.CategoryId = categoryId;
            story.ViewCount = 0;
            story.AverageRating = 0;
            story.TotalRatings = 0;
            story.Status = "Ongoing";
            story.CreatedAt = DateTime.UtcNow;
            story.IsActive = true;

            if (!story.Validate())
            {
                throw new InvalidOperationException($"Story validation failed for {story.GetType().Name}");
            }

            return story;
        }

        // Create database Story entity (for EF Core)
        public Story CreateDbStory(string title, string description, int authorId, int categoryId)
        {
            // Create polymorphic story first
            var storyBase = CreateStory(title, description, authorId, categoryId);

            // Map to database entity
            var dbStory = new Story
            {
                Title = storyBase.Title,
                Description = storyBase.Description + $"\n[{storyBase.GetSpecialFeature()}]",
                AuthorId = storyBase.AuthorId,
                CategoryId = storyBase.CategoryId,
                ViewCount = storyBase.ViewCount,
                AverageRating = storyBase.AverageRating,
                TotalRatings = storyBase.TotalRatings,
                Status = storyBase.Status,
                CreatedAt = storyBase.CreatedAt,
                IsActive = storyBase.IsActive
            };

            return dbStory;
        }

        public IStoryCategory GetStoryType(int categoryId)
        {
            return ((StoryCategory)categoryId) switch
            {
                StoryCategory.Action => new ActionStory(),
                StoryCategory.Horror => new HorrorStory(),
                StoryCategory.Romance => new RomanceStory(),
                StoryCategory.Detective => new DetectiveStory(),
                _ => new ActionStory() // Default
            };
        }
    }
}
