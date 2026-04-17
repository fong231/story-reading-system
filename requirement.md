# Tóm Tắt các Design Pattern

## 1. Công Nghệ Sử Dụng

* **Backend:** ASP.NET Core Web API (C# / .NET 8.0).
* **Frontend:** HTML5, CSS3, JavaScript thuần (Vanilla JS).
* **Database:** SQL Server / SQLite.
* **Giao tiếp:** RESTful API & SignalR (Real-time).

---

## 2. Chi Tiết Các Design Patterns Áp Dụng

### 2.1. Factory Pattern (Quản lý thể loại truyện)

**Mục tiêu:** Không khởi tạo trực tiếp các đối tượng truyện một cách rời rạc. Sử dụng một `StoryFactory` để tập trung hóa việc tạo đối tượng.

* **Vị trí triển khai:** **Backend (C#)**.
* **Ứng dụng:** Tạo interface `IStoryFactory` và các class cụ thể như `ActionStory`, `HorrorStory`, `RomanceStory`, `DetectiveStory`. Factory sẽ dựa vào `GenreID` từ Database để trả về đúng đối tượng. Điều này giúp xử lý các logic đặc thù (ví dụ: truyện kinh dị sẽ tự động kích hoạt logic kiểm tra độ tuổi hoặc cảnh báo nội dung nhạy cảm).

### 2.2. Strategy Pattern (Quản lý chế độ đọc)

**Mục tiêu:** Cho phép thay đổi linh hoạt cách hiển thị nội dung chương truyện (Giao diện và Cách điều hướng) mà không làm thay đổi cấu trúc dữ liệu gốc.

* **Vị trí triển khai:** **Frontend (JS)**.
* **Cấu trúc triển khai:**
    * **Interface `IReadingStrategy`**: Định nghĩa bộ khung cho các chế độ đọc.
    * **Concrete Strategies**:
        * **`DayScrollStrategy`**: Chế độ đọc ban ngày, cuộn trang truyền thống (Font: Arial, LineHeight: 1.5).
        * **`NightScrollStrategy`**: Chế độ đọc ban đêm, bảo vệ mắt (Background tối, Text sáng).
        * **`DayFlipStrategy`**: Chế độ đọc ban ngày, lật trang như sách (Font: Georgia, LineHeight: 1.8).
        * **`NightFlipStrategy`**: Chế độ đọc ban đêm, lật trang như sách.
* **Các thuộc tính quản lý:**
    * `Theme`: Day (Sáng), Night (Tối).
    * `NavigationMode`: Scroll (Cuộn), Flip (Lật trang).
    * `Settings`: Tự động điều chỉnh `backgroundColor`, `textColor`, `fontSize`, `fontFamily`, và `lineHeight` tương ứng với từng chiến lược.
* **Ứng dụng:** Người dùng có thể chuyển đổi giữa các chế độ đọc khác nhau. Hệ thống sẽ sử dụng `ReadingModeService` để chọn đúng Strategy và áp dụng các thiết lập giao diện phù hợp mà không cần thay đổi logic xử lý nội dung.



### 2.3. Singleton Pattern (Quản lý tiến trình đọc)

**Mục tiêu:** Đảm bảo tính duy nhất và nhất quán của bộ đếm tiến trình trong một phiên làm việc (Session).

* **Vị trí triển khai:** **Backend (C#)**.
* **Ứng dụng:** Sử dụng một class `ReadingTracker` theo dạng Singleton. Nó đảm bảo chỉ có duy nhất một thực thể quản lý trạng thái đọc của người dùng hiện tại trên Server. Điều này ngăn chặn xung đột khi người dùng mở nhiều tab cùng lúc hoặc tránh việc ghi đè sai lệch vị trí chương đang đọc (Last Read Page).

### 2.4. Observer Pattern (Cập nhật UI khi có chương mới)

**Mục tiêu:** Tự động hóa việc gửi thông báo đến người dùng mà không cần họ phải tải lại trang (F5).

* **Vị trí triển khai:** **Cả hai (SignalR)**.
* **Ứng dụng:** * **Backend:** Đóng vai trò *Subject* (Người phát tin). Khi Admin/Tác giả đăng chương mới, Server sẽ phát một tín hiệu thông báo.
* **Frontend:** Đóng vai trò *Observer* (Người lắng nghe). JavaScript sẽ đăng ký (subscribe) vào channel thông báo. Khi có tín hiệu, UI sẽ tự động hiển thị Popup hoặc biểu tượng "New" trên danh sách truyện ngay lập tức.



### 2.5. Command Pattern (Quản lý hành động tương tác)

**Mục tiêu:** Đóng gói các yêu cầu tương tác thành đối tượng, cho phép quản lý lịch sử và hỗ trợ tính năng Hoàn tác (Undo/Redo).

* **Vị trí triển khai:** **Frontend (JS)**.
* **Execute:** Trên dialog để tùy chỉnh reading mode mỗi khi người dùng bấm lưu thì sẽ cập nhật lại UI phù hợp đồng thời lưu vào localStore thành 1 mảng json (mỗi phần tử chứa thông tin trong reading mode như font size, font family, line height, ...)
* **Undo:** Nếu người dùng bấm nhầm hoặc muốn quay lại trạng thái cũ, hệ thống sẽ lấy phần tử cuối trong mảng json của localStore và gọi hàm `undo()` để quay lại giao diện trước đó và loại phần tử đó khỏi mảng, bỏ phần tử đấy vào mảng redo và cũng lưu trong localStore.
* **Redo:** Nếu người dùng muốn quay lại trạng thái đã undo, hệ thống sẽ lấy phần tử cuối trong mảng redo và gọi hàm `redo()` để quay lại giao diện sau đó loại phần tử đó khỏi mảng redo và bỏ phần tử đấy vào mảng undo và cũng lưu trong localStore.
* **When close browser:** Khi người dùng đóng trình duyệt, lấy phần tử cuối cùng của mảng undo trong localStore gửi cho backend để lưu lại



### 2.6. Template Method Pattern (In báo cáo dành cho tác giả)

**Mục tiêu:** Định nghĩa một "khung xương" (skeleton) cho quy trình xuất báo cáo, đảm bảo tính thống nhất giữa các loại báo cáo khác nhau.

* **Vị trí triển khai:** **Backend (C#)**.
* **Ứng dụng:** Tạo một lớp trừu tượng `AuthorReportTemplate` với các bước cố định:
1. Mở kết nối Database.
2. Truy vấn dữ liệu (Trừu tượng).
3. Tính toán chỉ số (Trừu tượng).
4. Định dạng báo cáo (Trừu tượng).
5. Xuất file PDF/Excel (Cố định).


* Các lớp con như `RevenueReport` (Báo cáo doanh thu) hay `ViewGrowthReport` (Báo cáo lượt xem) chỉ cần ghi đè (override) các bước tính toán riêng biệt mà không phải viết lại quy trình xuất file.



- Hiển thị danh sách truyện với các thể loại khác nhau.
- Hỗ trợ đọc truyện với nhiều chế độ (ban ngày, ban đêm, cuộn, lật trang).
- Cho phép người dùng đánh dấu chương đang đọc và tiếp tục lần sau.
- Cung cấp tính năng bình luận, đánh giá truyện