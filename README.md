# Story Reading System 📖

Hệ thống đọc truyện trực tuyến được xây dựng bằng **ASP.NET Core (Backend)** và **Vanilla JavaScript (Frontend)**, áp dụng các Design Patterns (Factory, Strategy, Observer, Command, Template Method).

---

## 🛠 Yêu cầu hệ thống
- **Visual Studio 2022** (đã cài đặt .NET 8.0 SDK và SQL Server Express/LocalDB).
- **Visual Studio Code** (với extension **Live Server**) để chạy Frontend.

---

## 🚀 Hướng dẫn chạy Backend (ASP.NET Core)

Hệ thống được cấu hình để **tự động khởi tạo** cơ sở dữ liệu và dữ liệu mẫu ngay khi bạn nhấn chạy.

1. **Mở Project:** 
   - Truy cập thư mục `BE` và mở file `BE.sln` bằng Visual Studio 2022.
2. **Cấu hình Database (Tùy chọn):** 
   - Mặc định hệ thống sử dụng `(localdb)\mssqllocaldb`. Bạn có thể kiểm tra hoặc thay đổi tại `appsettings.json`.
3. **Chạy ứng dụng:**
   - Nhấn **F5** hoặc nút **Start (mũi tên xanh)** trên thanh công cụ của Visual Studio.
4. **Kiểm tra kết quả:**
   - Visual Studio sẽ tự động mở trình duyệt tại địa chỉ `https://localhost:7210/swagger`.
   - **Tự động hóa:** Lúc này, Database đã được tự động tạo và các dữ liệu mẫu (Truyện, Chương, User) đã được insert vào hệ thống.

---

## 🌐 Hướng dẫn chạy Frontend (HTML/CSS/JS)

1. **Mở thư mục `FE`** bằng Visual Studio Code.
2. **Khởi chạy Live Server:**
   - Mở file `index.html`.
   - Chuột phải chọn **Open with Live Server** hoặc nhấn nút **Go Live** ở góc dưới bên phải màn hình.
3. **Truy cập:** Trình duyệt sẽ mở trang web tại `http://127.0.0.1:5500/index.html`.

---

## ⚙️ Cấu hình quan trọng
- **Cơ sở dữ liệu:** Chuỗi kết nối nằm trong `BE/appsettings.json`.
  ```json
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StoryReaderDB;Trusted_Connection=True;"
  ```
- **Kết nối FE-BE:** Nếu Backend chạy ở cổng khác `7210`, hãy cập nhật `BACKEND_URL` trong các file JavaScript tại thư mục `FE/js/`.
