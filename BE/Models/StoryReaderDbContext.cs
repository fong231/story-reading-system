using Microsoft.EntityFrameworkCore;

namespace BE.Models
{
    public class StoryReaderDbContext : DbContext
    {
        public StoryReaderDbContext(DbContextOptions<StoryReaderDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ReadingProgress> ReadingProgresses { get; set; }
        public DbSet<ReadingMode> ReadingModes { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StoryFollower> StoryFollowers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================================================================
            // User Configuration
            // ================================================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ================================================================
            // Category Configuration - FACTORY PATTERN
            // ================================================================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.IsActive);
            });

            // ================================================================
            // Story Configuration - FACTORY PATTERN
            // ================================================================
            modelBuilder.Entity<Story>(entity =>
            {
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.ViewCount);

                entity.HasOne(e => e.Author)
                    .WithMany(u => u.Stories)
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Stories)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Chapter Configuration
            // ================================================================
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => e.PublishedAt);
                entity.HasIndex(e => new { e.StoryId, e.ChapterNumber }).IsUnique();

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Chapters)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // ReadingProgress Configuration - SINGLETON PATTERN
            // ================================================================
            modelBuilder.Entity<ReadingProgress>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne(u => u.ReadingProgress)
                    .HasForeignKey<ReadingProgress>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CurrentStory)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentStoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CurrentChapter)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentChapterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // ReadingMode Configuration - STRATEGY PATTERN
            // ================================================================
            modelBuilder.Entity<ReadingMode>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne(u => u.ReadingMode)
                    .HasForeignKey<ReadingMode>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // Bookmark Configuration
            // ================================================================
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => new { e.UserId, e.StoryId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookmarks)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Bookmarks)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Chapter)
                    .WithMany(c => c.Bookmarks)
                    .HasForeignKey(e => e.ChapterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Comment Configuration
            // ================================================================
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Comments)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Rating Configuration
            // ================================================================
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => new { e.UserId, e.StoryId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Ratings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Ratings)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Notification Configuration - OBSERVER PATTERN
            // ================================================================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.IsRead });
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Notifications)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Chapter)
                    .WithMany(c => c.Notifications)
                    .HasForeignKey(e => e.ChapterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // StoryFollower Configuration - OBSERVER PATTERN
            // ================================================================
            modelBuilder.Entity<StoryFollower>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => new { e.UserId, e.StoryId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.StoryFollowers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Followers)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Seed Data
            // ================================================================
            // Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "author_admin", Email = "author@test.com", PasswordHash = "hash123" },
                new User { UserId = 2, Username = "reader_01", Email = "reader@test.com", PasswordHash = "hash123" }
            );

            // Categories (for Factory Pattern)
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Hành Động", IsActive = true },
                new Category { CategoryId = 2, Name = "Kinh Dị", IsActive = true },
                new Category { CategoryId = 3, Name = "Lãng Mạn", IsActive = true },
                new Category { CategoryId = 4, Name = "Trinh Thám", IsActive = true }
            );

            // Stories
            modelBuilder.Entity<Story>().HasData(
                new Story
                {
                    StoryId = 1,
                    Title = "Huyền Thoại Sát Thủ",
                    AuthorId = 1,
                    CategoryId = 1,
                    CoverImage = "https://picsum.photos/200/300?random=1",
                    Description = "Truyện hành động hấp dẫn.",
                    ViewCount = 1000,
                    AverageRating = 4.5m,
                    TotalRatings = 1
                },
                new Story
                {
                    StoryId = 2,
                    Title = "Tiếng Vọng Đêm Khuya",
                    AuthorId = 1,
                    CategoryId = 2,
                    CoverImage = "https://picsum.photos/200/300?random=2",
                    Description = "Truyện kinh dị kịch tính.",
                    ViewCount = 500,
                    AverageRating = 4.0m,
                    TotalRatings = 1
                }
            );

            // Seed Chapters
            modelBuilder.Entity<Chapter>().HasData(
                new Chapter { ChapterId = 1, StoryId = 1, ChapterNumber = 1, Title = "Hồi kết bắt đầu", Content = "Nội dung chương 1..." },
                new Chapter { ChapterId = 2, StoryId = 2, ChapterNumber = 1, Title = "Bóng tối", Content = "Nội dung chương 1 kinh dị..." }
            );

            // Seed Follower (Test Observer)
            modelBuilder.Entity<StoryFollower>().HasData(
                new StoryFollower { FollowId = -1, UserId = 2, StoryId = 1, CreatedAt = DateTime.UtcNow }
            );

            // Seed ReadingProgress (Test Singleton - Tiến trình đọc duy nhất)
            modelBuilder.Entity<ReadingProgress>().HasData(
                new ReadingProgress
                {
                    ProgressId = -1,
                    UserId = 2,
                    CurrentStoryId = 1,
                    CurrentChapterId = 1,
                    LastReadPosition = 150,
                    TotalStoriesRead = 1,
                    TotalChaptersRead = 1,
                    LastReadAt = DateTime.UtcNow
                }
            );

            // Seed ReadingMode (Test Strategy - Lưu cấu hình đọc của User)
            modelBuilder.Entity<ReadingMode>().HasData(
                new ReadingMode
                {
                    ModeId = -1,
                    UserId = 2,
                    Theme = "Night",
                    NavigationMode = "Scroll",
                    FontSize = 18,
                    FontFamily = "Georgia",
                    LineHeight = 1.8m
                }
            );
        }
    }
}
