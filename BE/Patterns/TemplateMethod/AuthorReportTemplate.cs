using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Patterns.TemplateMethod
{
    // ================================================================
    // TEMPLATE METHOD PATTERN - Abstract Report Template
    // ================================================================
    public abstract class AuthorReportTemplate
    {
        protected readonly StoryReaderDbContext _context;

        protected AuthorReportTemplate(StoryReaderDbContext context)
        {
            _context = context;
        }

        // TEMPLATE METHOD - Định nghĩa skeleton algorithm
        public async Task<byte[]> GenerateReportAsync(int authorId, DateTime startDate, DateTime endDate, int? storyId = null)
        {
            Console.WriteLine($"[Template Method Pattern] Generate Report for Author: {authorId}");
            // 1. Mở kết nối Database (cố định)
            await OpenDatabaseConnection();

            // 2. Truy vấn dữ liệu (trừu tượng - mỗi report khác nhau)
            var data = await QueryDataAsync(authorId, startDate, endDate, storyId);

            // 3. Tính toán chỉ số (trừu tượng - mỗi report khác nhau)
            var calculatedData = CalculateMetrics(data);

            // 4. Định dạng báo cáo (trừu tượng - mỗi report khác nhau)
            var formattedReport = FormatReport(calculatedData);

            // 5. Xuất file PDF/Excel (cố định)
            var fileBytes = await ExportToFileAsync(formattedReport);

            // 6. Đóng kết nối (cố định)
            await CloseDatabaseConnection();

            return fileBytes;
        }

        // Protected methods - có thể override
        protected virtual async Task OpenDatabaseConnection()
        {
            // Database connection đã được quản lý bởi EF Core
            await Task.CompletedTask;
        }

        protected virtual async Task CloseDatabaseConnection()
        {
            await Task.CompletedTask;
        }

        // Abstract methods - bắt buộc override
        protected abstract Task<object> QueryDataAsync(int authorId, DateTime startDate, DateTime endDate, int? storyId = null);
        protected abstract object CalculateMetrics(object rawData);
        protected abstract string FormatReport(object calculatedData);

        // Concrete method - xuất file (cố định cho tất cả reports)
        protected virtual async Task<byte[]> ExportToFileAsync(string formattedContent)
        {
            // Đơn giản hóa: Convert string to bytes (production nên dùng PDF library)
            return await Task.FromResult(System.Text.Encoding.UTF8.GetBytes(formattedContent));
        }
    }
}
