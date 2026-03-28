// js/patterns/Observer.js

class NotificationObserver {
    constructor(badgeElementId) {
        this.badgeElement = document.getElementById(badgeElementId);
        this.notificationCount = 0;
        this.connection = null;
    }

    // Khởi tạo kết nối SignalR tới Backend
    async connect(backendUrl) {
        // Cấu hình SignalR Connection
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${backendUrl}/notificationHub`)
            .withAutomaticReconnect() // Tự động kết nối lại nếu rớt mạng
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Đăng ký lắng nghe sự kiện "ReceiveNotification" từ Server gửi về
        this.connection.on("ReceiveNotification", (notification) => {
            console.log("🔔 CÓ THÔNG BÁO MỚI:", notification);
            this.updateUI(notification);
        });

        try {
            await this.connection.start();
            console.log("✅ Đã kết nối SignalR thành công!");
            
            // Giả sử user hiện tại có ID là 1, ta join vào group của user đó 
            // để nhận thông báo (giống hàm em viết ở Backend)
            await this.connection.invoke("JoinUserGroup", 1); 
            
            // Hoặc nếu đang đọc truyện ID = 10, join vào group truyện đó
            // await this.connection.invoke("JoinStoryGroup", 10);
            
        } catch (err) {
            console.error("❌ Lỗi kết nối SignalR: ", err);
        }
    }

    // Hàm cập nhật UI khi Observer nhận được tín hiệu
    updateUI(notification) {
        this.notificationCount++;
        
        // Hiển thị số lượng lên Badge
        if (this.badgeElement) {
            this.badgeElement.textContent = this.notificationCount;
            this.badgeElement.classList.remove('hidden');
            
            // Thêm hiệu ứng rung/nháy cho đẹp (tùy chọn)
            this.badgeElement.style.transform = "scale(1.5)";
            setTimeout(() => {
                this.badgeElement.style.transform = "scale(1)";
            }, 300);
        }

        // Tùy chọn: Em có thể hiển thị một Toast/Alert nội dung thông báo ở đây
        // alert(`Thông báo mới: ${notification.message}`);
    }

    // Reset thông báo khi click vào chuông
    markAsRead() {
        this.notificationCount = 0;
        if (this.badgeElement) {
            this.badgeElement.textContent = 0;
            this.badgeElement.classList.add('hidden');
        }
    }
}