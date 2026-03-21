using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Services
{
    // ================================================================
    // Reading Mode Service (Strategy Pattern removed - Logic moved to FE)
    // ================================================================
    public interface IReadingModeService
    {
        Task<ReadingMode> GetOrCreateModeAsync(int userId);
        Task<ReadingMode> UpdateModeAsync(int userId, string theme, string navigationMode, int? fontSize, string fontFamily, decimal? lineHeight);
    }

    public class ReadingModeService : IReadingModeService
    {
        private readonly StoryReaderDbContext _context;

        public ReadingModeService(StoryReaderDbContext context)
        {
            _context = context;
        }

        public async Task<ReadingMode> GetOrCreateModeAsync(int userId)
        {
            var mode = await _context.ReadingModes
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (mode == null)
            {
                mode = new ReadingMode
                {
                    UserId = userId,
                    Theme = "Day",
                    NavigationMode = "Scroll",
                    FontSize = 16,
                    FontFamily = "Arial",
                    LineHeight = 1.5m,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ReadingModes.Add(mode);
                await _context.SaveChangesAsync();
            }

            return mode;
        }

        public async Task<ReadingMode> UpdateModeAsync(int userId, string theme, string navigationMode, int? fontSize, string fontFamily, decimal? lineHeight)
        {
            var mode = await GetOrCreateModeAsync(userId);

            if (theme != null) mode.Theme = theme;
            if (navigationMode != null) mode.NavigationMode = navigationMode;
            if (fontSize.HasValue) mode.FontSize = fontSize.Value;
            if (fontFamily != null) mode.FontFamily = fontFamily;
            if (lineHeight.HasValue) mode.LineHeight = lineHeight.Value;
            
            mode.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return mode;
        }
    }
}
