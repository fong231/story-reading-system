using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Controllers
{
    // ================================================================
    // Categories Controller
    // ================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly StoryReaderDbContext _context;

        public CategoriesController(StoryReaderDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null || !category.IsActive)
                return NotFound();

            return category;
        }
    }

    // ================================================================
    // Comments Controller
    // ================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly StoryReaderDbContext _context;

        public CommentsController(StoryReaderDbContext context)
        {
            _context = context;
        }

        // GET: api/Comments/Story/5
        [HttpGet("Story/{storyId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCommentsByStory(int storyId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.StoryId == storyId && c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.CommentId,
                    c.Content,
                    c.CreatedAt,
                    User = new { c.User.UserId, c.User.Username }
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment([FromBody] CreateCommentDto dto)
        {
            var comment = new Comment
            {
                UserId = dto.UserId,
                StoryId = dto.StoryId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCommentsByStory), 
                new { storyId = comment.StoryId }, comment);
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            comment.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // ================================================================
    // Ratings Controller
    // ================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly StoryReaderDbContext _context;

        public RatingsController(StoryReaderDbContext context)
        {
            _context = context;
        }

        // GET: api/Ratings/Story/5
        [HttpGet("Story/{storyId}")]
        public async Task<ActionResult<object>> GetStoryRatings(int storyId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.StoryId == storyId)
                .ToListAsync();

            var average = ratings.Any() ? ratings.Average(r => r.Score) : 0;
            var total = ratings.Count;

            return Ok(new
            {
                StoryId = storyId,
                AverageRating = Math.Round(average, 2),
                TotalRatings = total,
                Distribution = new
                {
                    Star5 = ratings.Count(r => r.Score == 5),
                    Star4 = ratings.Count(r => r.Score == 4),
                    Star3 = ratings.Count(r => r.Score == 3),
                    Star2 = ratings.Count(r => r.Score == 2),
                    Star1 = ratings.Count(r => r.Score == 1)
                }
            });
        }

        // GET: api/Ratings/User/5/Story/10
        [HttpGet("User/{userId}/Story/{storyId}")]
        public async Task<ActionResult<Rating>> GetUserRating(int userId, int storyId)
        {
            var rating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.StoryId == storyId);

            if (rating == null)
                return NotFound();

            return rating;
        }

        // POST: api/Ratings
        [HttpPost]
        public async Task<ActionResult<Rating>> CreateOrUpdateRating([FromBody] CreateRatingDto dto)
        {
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.StoryId == dto.StoryId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Score = dto.Score;
                await _context.SaveChangesAsync();
                return Ok(existingRating);
            }
            else
            {
                // Create new rating
                var rating = new Rating
                {
                    UserId = dto.UserId,
                    StoryId = dto.StoryId,
                    Score = dto.Score,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserRating),
                    new { userId = rating.UserId, storyId = rating.StoryId }, rating);
            }
        }

        // DELETE: api/Ratings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
                return NotFound();

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // ================================================================
    // Bookmarks Controller
    // ================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class BookmarksController : ControllerBase
    {
        private readonly StoryReaderDbContext _context;

        public BookmarksController(StoryReaderDbContext context)
        {
            _context = context;
        }

        // GET: api/Bookmarks/User/5
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserBookmarks(int userId)
        {
            var bookmarks = await _context.Bookmarks
                .Include(b => b.Story)
                .Include(b => b.Chapter)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.LastReadAt)
                .Select(b => new
                {
                    b.BookmarkId,
                    b.ScrollPosition,
                    b.LastReadAt,
                    Story = new
                    {
                        b.Story.StoryId,
                        b.Story.Title,
                        b.Story.CoverImage
                    },
                    Chapter = new
                    {
                        b.Chapter.ChapterId,
                        b.Chapter.Title,
                        b.Chapter.ChapterNumber
                    }
                })
                .ToListAsync();

            return Ok(bookmarks);
        }

        // POST: api/Bookmarks
        [HttpPost]
        public async Task<ActionResult<Bookmark>> CreateOrUpdateBookmark([FromBody] CreateBookmarkDto dto)
        {
            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == dto.UserId && b.StoryId == dto.StoryId);

            if (existingBookmark != null)
            {
                // Update existing bookmark
                existingBookmark.ChapterId = dto.ChapterId;
                existingBookmark.ScrollPosition = dto.ScrollPosition;
                existingBookmark.LastReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(existingBookmark);
            }
            else
            {
                // Create new bookmark
                var bookmark = new Bookmark
                {
                    UserId = dto.UserId,
                    StoryId = dto.StoryId,
                    ChapterId = dto.ChapterId,
                    ScrollPosition = dto.ScrollPosition,
                    LastReadAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookmarks.Add(bookmark);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserBookmarks),
                    new { userId = bookmark.UserId }, bookmark);
            }
        }

        // DELETE: api/Bookmarks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookmark(int id)
        {
            var bookmark = await _context.Bookmarks.FindAsync(id);
            if (bookmark == null)
                return NotFound();

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTOs
    public class CreateCommentDto
    {
        public int UserId { get; set; }
        public int StoryId { get; set; }
        public string Content { get; set; }
    }

    public class CreateRatingDto
    {
        public int UserId { get; set; }
        public int StoryId { get; set; }
        public int Score { get; set; }
    }

    public class CreateBookmarkDto
    {
        public int UserId { get; set; }
        public int StoryId { get; set; }
        public int ChapterId { get; set; }
        public int ScrollPosition { get; set; }
    }
}
