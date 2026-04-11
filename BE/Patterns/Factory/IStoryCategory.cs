using System;

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
        // Trả về null nếu thành công, hoặc string chứa lỗi cụ thể nếu thất bại
        public abstract string Validate();

        // Template method - mỗi loại story có logic đặc thù
        public abstract string GetSpecialFeature();
    }
}
