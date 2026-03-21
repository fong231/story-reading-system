using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Models
{
    // ============================================================================
    // User Entity
    // ============================================================================
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<StoryFollower> StoryFollowers { get; set; } = new List<StoryFollower>();
        public virtual ReadingProgress ReadingProgress { get; set; }
        public virtual ReadingMode ReadingMode { get; set; }
    }

    // ============================================================================
    // Category Entity - FACTORY PATTERN
    // ============================================================================
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    }

    // ============================================================================
    // Story Entity - FACTORY PATTERN
    // ============================================================================
    public class Story
    {
        [Key]
        public int StoryId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [MaxLength(500)]
        public string CoverImage { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int ViewCount { get; set; } = 0;

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0;

        public int TotalRatings { get; set; } = 0;

        [MaxLength(20)]
        public string Status { get; set; } = "Ongoing"; // Ongoing, Completed, Paused

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<StoryFollower> Followers { get; set; } = new List<StoryFollower>();
    }

    // ============================================================================
    // Chapter Entity
    // ============================================================================
    public class Chapter
    {
        [Key]
        public int ChapterId { get; set; }

        [Required]
        public int StoryId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public int ChapterNumber { get; set; }

        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        public int ViewCount { get; set; } = 0;
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; }

        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    // ============================================================================
    // ReadingProgress Entity - SINGLETON PATTERN
    // ============================================================================
    public class ReadingProgress
    {
        [Key]
        public int ProgressId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? CurrentStoryId { get; set; }
        public int? CurrentChapterId { get; set; }
        public int LastReadPosition { get; set; } = 0;
        public int TotalStoriesRead { get; set; } = 0;
        public int TotalChaptersRead { get; set; } = 0;
        public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CurrentStoryId")]
        public virtual Story CurrentStory { get; set; }

        [ForeignKey("CurrentChapterId")]
        public virtual Chapter CurrentChapter { get; set; }
    }

    // ============================================================================
    // ReadingMode Entity - STRATEGY PATTERN
    // ============================================================================
    public class ReadingMode
    {
        [Key]
        public int ModeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(20)]
        public string Theme { get; set; } = "Day"; // Day, Night

        [MaxLength(20)]
        public string NavigationMode { get; set; } = "Scroll"; // Scroll, Flip

        public int FontSize { get; set; } = 16;

        [MaxLength(50)]
        public string FontFamily { get; set; } = "Arial";

        [Column(TypeName = "decimal(3,1)")]
        public decimal LineHeight { get; set; } = 1.5m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    // ============================================================================
    // Bookmark Entity
    // ============================================================================
    public class Bookmark
    {
        [Key]
        public int BookmarkId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int StoryId { get; set; }

        [Required]
        public int ChapterId { get; set; }

        public int ScrollPosition { get; set; } = 0;
        public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter Chapter { get; set; }
    }

    // ============================================================================
    // Comment Entity
    // ============================================================================
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int StoryId { get; set; }

        [Required, Column(TypeName = "ntext")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; }
    }

    // ============================================================================
    // Rating Entity
    // ============================================================================
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int StoryId { get; set; }

        [Required, Range(1, 5)]
        public int Score { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; }
    }

    // ============================================================================
    // Notification Entity - OBSERVER PATTERN
    // ============================================================================
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int StoryId { get; set; }

        public int? ChapterId { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = "NewChapter"; // NewChapter, System

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter Chapter { get; set; }
    }

    // ============================================================================
    // StoryFollower Entity - OBSERVER PATTERN
    // ============================================================================
    public class StoryFollower
    {
        [Key]
        public int FollowId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int StoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; }
    }
}
