# Story Reading System 📖

Hệ thống đọc truyện trực tuyến được xây dựng bằng **ASP.NET Core (Backend)** và **Vanilla JavaScript (Frontend)**, áp dụng các Design Patterns (Factory, Strategy, Observer, Command, Template Method).

---

## 🛠 Yêu cầu hệ thống
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Khuyên dùng LocalDB cho môi trường phát triển)
- [Visual Studio Code](https://code.visualstudio.com/) hoặc Visual Studio 2022
- Extension **Live Server** (cho VS Code)

---

## Hướng dẫn chạy Backend (ASP.NET Core)

Bạn có thể chạy Backend theo một trong hai cách dưới đây:

### Cách 1: Clone từ GitHub và chạy
1. **Mở terminal** và clone repository:
   ```bash
   git clone https://github.com/your-username/story-reading-system.git
   cd story-reading-system/BE
   ```
2. **Khôi phục các gói (NuGet Packages):**
   ```bash
   dotnet restore
   ```
3. **Cập nhật cơ sở dữ liệu (Migration):**
   *Đảm bảo chuỗi kết nối trong `appsettings.json` đã đúng với SQL Server của bạn.*
   ```bash
   dotnet ef database update
   ```
4. **Chạy ứng dụng:**
   ```bash
   dotnet run
   ```

### Cách 2: Chạy khi đã có sẵn Project trên máy
1. **Mở thư mục `BE`** của project bằng Terminal hoặc Command Prompt:
   ```bash
   cd path/to/story-reading-system/BE
   ```
2. **Khôi phục và chạy:**
   ```bash
   dotnet restore
   dotnet run
   ```

> **Lưu ý:** Sau khi chạy thành công, Backend mặc định sẽ chạy tại: `https://localhost:7210` hoặc `http://localhost:7210`. Bạn có thể truy cập `https://localhost:7210/swagger` để xem tài liệu API.

---

## Hướng dẫn chạy Frontend (HTML/CSS/JS)

Frontend được viết bằng mã nguồn thuần, cách tốt nhất để chạy là sử dụng extension **Live Server** trong VS Code để tránh lỗi CORS và quản lý URL dễ dàng hơn.

### Các bước thực hiện:
1. **Mở VS Code**, chọn **File > Open Folder...** và mở thư mục `FE`.
2. **Cài đặt Live Server:**
   - Nhấn `Ctrl+Shift+X` để mở cửa sổ Extensions.
   - Tìm kiếm từ khóa **"Live Server"** (của tác giả Ritwick Dey).
   - Nhấn **Install**.
3. **Khởi chạy:**
   - Mở file `index.html` trong thư mục `FE`.
   - **Cách 1:** Chuột phải vào bất kỳ đâu trong file `index.html` và chọn **Open with Live Server**.
   - **Cách 2:** Nhấn vào nút **Go Live** ở góc dưới cùng bên phải của cửa sổ VS Code.
4. **Truy cập:** Trình duyệt sẽ tự động mở trang web tại địa chỉ mặc định là: `http://127.0.0.1:5500/index.html`.

---

## Cấu hình quan trọng
- **Backend URL:** Nếu Backend chạy ở cổng khác cổng `7210`, hãy cập nhật biến `BACKEND_URL` trong các file JavaScript tại thư mục `FE/js/`.
- **Database:** Bạn có thể thay đổi chuỗi kết nối tại `BE/appsettings.json`.

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StoryReaderDB;Trusted_Connection=True;"
```