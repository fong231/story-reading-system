using BE.Models;

namespace BE.Patterns.Factory
{
    // ================================================================
    // FACTORY PATTERN - Base Story Class
    // ================================================================
    public abstract class IStoryCategory
    {
        public int StoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        public int ViewCount { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Template method - mỗi loại story có cách validate riêng
        public abstract bool Validate();

        // Template method - mỗi loại story có logic đặc thù
        public abstract string GetSpecialFeature();
    }

    // ================================================================
    // Concrete Story Classes - Polymorphism
    // ================================================================

    public class ActionStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Action stories might need action tag validation
            return !string.IsNullOrEmpty(Title) && Title.Length >= 3;
        }

        public override string GetSpecialFeature()
        {
            return "High-paced action scenes, combat sequences";
        }
    }

    public class HorrorStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Horror stories might need age verification
            return !string.IsNullOrEmpty(Title) && Description?.Contains("18+") == false;
        }

        public override string GetSpecialFeature()
        {
            return "Warning: May contain scary content. Age restriction: 16+";
        }
    }

    public class RomanceStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Romance stories might have relationship validation
            return !string.IsNullOrEmpty(Title);
        }

        public override string GetSpecialFeature()
        {
            return "Heartwarming love story, emotional journey";
        }
    }

    public class DetectiveStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Detective stories might need mystery elements
            return !string.IsNullOrEmpty(Title) && Description?.Length > 50;
        }

        public override string GetSpecialFeature()
        {
            return "Mystery solving, clues and investigation";
        }
    }

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
