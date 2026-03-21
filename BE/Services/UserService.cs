using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Services
{
    public interface IUserService
    {
        Task<User> LoginAsync(string usernameOrEmail, string password);
        Task<IEnumerable<object>> GetActiveUsersAsync();
        Task<object> GetUserDetailsAsync(int id);
        Task<bool> CreateTestUsersAsync();
    }

    public class UserService : IUserService
    {
        private readonly StoryReaderDbContext _context;

        public UserService(StoryReaderDbContext context)
        {
            _context = context;
        }

        public async Task<User> LoginAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    (u.Username == usernameOrEmail || u.Email == usernameOrEmail) 
                    && u.IsActive);

            if (user == null || user.PasswordHash != password)
            {
                return null;
            }

            return user;
        }

        public async Task<IEnumerable<object>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<object> GetUserDetailsAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Stories)
                .Include(u => u.ReadingProgress)
                .Include(u => u.ReadingMode)
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsActive);

            if (user == null) return null;

            return new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.CreatedAt,
                TotalStories = user.Stories.Count(s => s.IsActive),
                ReadingProgress = user.ReadingProgress != null ? new
                {
                    user.ReadingProgress.TotalStoriesRead,
                    user.ReadingProgress.TotalChaptersRead
                } : null,
                ReadingMode = user.ReadingMode != null ? new
                {
                    user.ReadingMode.Theme,
                    user.ReadingMode.NavigationMode
                } : null
            };
        }

        public async Task<bool> CreateTestUsersAsync()
        {
            var existingUsers = await _context.Users.AnyAsync();
            if (existingUsers) return false;

            var testUsers = new List<User>
            {
                new User { Username = "admin", Email = "admin@example.com", PasswordHash = "admin123", CreatedAt = DateTime.UtcNow, IsActive = true },
                new User { Username = "user1", Email = "user1@example.com", PasswordHash = "user123", CreatedAt = DateTime.UtcNow, IsActive = true },
                new User { Username = "user2", Email = "user2@example.com", PasswordHash = "user123", CreatedAt = DateTime.UtcNow, IsActive = true }
            };

            _context.Users.AddRange(testUsers);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
