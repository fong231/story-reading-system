// js/patterns/Observer.js

class NotificationObserver {
    constructor(badgeElementId) {
        this.badgeElement = document.getElementById(badgeElementId);
        this.notificationCount = 0;
        this.connection = null;
        this.notifications = JSON.parse(localStorage.getItem('user_notifications')) || [];
        
        // Tạo dropdown element nếu chưa có
        this.initDropdown();
    }

    initDropdown() {
        const notiContainer = document.querySelector('.notification');
        if (!notiContainer) return;

        // Xóa dropdown cũ nếu có
        const oldDropdown = document.getElementById('noti-dropdown');
        if (oldDropdown) oldDropdown.remove();

        const dropdown = document.createElement('div');
        dropdown.id = 'noti-dropdown';
        dropdown.className = 'notification-dropdown';
        dropdown.innerHTML = `
            <div class="notification-header">Thông báo mới</div>
            <div id="noti-list">
                <div class="notification-empty">Không có thông báo mới</div>
            </div>
        `;
        notiContainer.appendChild(dropdown);

        // Click vào chuông để toggle dropdown
        notiContainer.addEventListener('click', (e) => {
            e.stopPropagation();
            dropdown.classList.toggle('show');
            if (dropdown.classList.contains('show')) {
                this.markAsRead();
            }
        });

        // Click ra ngoài để đóng
        document.addEventListener('click', () => {
            dropdown.classList.remove('show');
        });

        this.renderNotifications();
        this.updateBadge();
        
        // Load thông báo từ backend khi khởi tạo
        this.fetchNotificationsFromBackend();
    }

    async fetchNotificationsFromBackend() {
        const user = JSON.parse(localStorage.getItem("currentUser"));
        if (!user) return;

        try {
            const res = await fetch(`${BACKEND_URL}/api/Notifications/User/${user.userId}`);
            if (res.ok) {
                const backendNotis = await res.json();
                // Merge với local notifications hoặc thay thế tùy logic. Ở đây ta ưu tiên backend.
                this.notifications = backendNotis.map(n => ({
                    id: n.notificationId,
                    message: n.message,
                    time: new Date(n.createdAt).toLocaleString(),
                    isRead: n.isRead,
                    storyId: n.story.storyId
                }));
                localStorage.setItem('user_notifications', JSON.stringify(this.notifications));
                this.updateBadge();
                this.renderNotifications();
            }
        } catch (e) {
            console.error("Lỗi tải thông báo từ backend:", e);
        }
    }

    // Khởi tạo kết nối SignalR tới Backend
    async connect(backendUrl) {
        if (this.connection) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${backendUrl}/notificationHub`)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.connection.on("ReceiveNotification", (notification) => {
            console.log("🔔 CÓ THÔNG BÁO MỚI:", notification);
            // Hiển thị Toast thông báo nhanh
            if (typeof showToast === 'function') {
                showToast(notification.message || notification, "info");
            }
            this.addNotification(notification);
        });

        try {
            await this.connection.start();
            const user = JSON.parse(localStorage.getItem("currentUser"));
            if (user && user.userId) {
                await this.connection.invoke("JoinUserGroup", parseInt(user.userId));
            }
        } catch (err) {
            console.error("❌ Lỗi kết nối SignalR: ", err);
        }
    }

    addNotification(notification) {
        const newNoti = {
            id: Date.now(),
            message: notification.message || notification,
            time: new Date().toLocaleString(),
            isRead: false
        };
        this.notifications.unshift(newNoti);
        if (this.notifications.length > 20) this.notifications.pop(); 
        
        localStorage.setItem('user_notifications', JSON.stringify(this.notifications));
        this.updateBadge();
        this.renderNotifications();
    }

    updateBadge() {
        if (this.badgeElement) {
            const unreadCount = this.notifications.filter(n => !n.isRead).length;
            this.badgeElement.textContent = unreadCount;
            if (unreadCount > 0) {
                this.badgeElement.classList.remove('hidden');
                this.badgeElement.style.transform = "scale(1.5)";
                setTimeout(() => { this.badgeElement.style.transform = "scale(1)"; }, 300);
            } else {
                this.badgeElement.classList.add('hidden');
            }
        }
    }

    renderNotifications() {
        const listEl = document.getElementById('noti-list');
        if (!listEl) return;

        if (this.notifications.length === 0) {
            listEl.innerHTML = '<div class="notification-empty">Không có thông báo mới</div>';
            return;
        }

        listEl.innerHTML = this.notifications.map(n => `
            <div class="notification-item ${n.isRead ? '' : 'unread'}" onclick="window.location.href='story-detail.html?storyId=${n.storyId || ''}'">
                <div>${n.message}</div>
                <div style="font-size: 11px; color: #888; margin-top: 5px;">${n.time}</div>
            </div>
        `).join('');
    }

    async markAsRead() {
        const user = JSON.parse(localStorage.getItem("currentUser"));
        if (user) {
            try {
                // Gọi backend để đánh dấu đã đọc hết
                await fetch(`${BACKEND_URL}/api/Notifications/User/${user.userId}/MarkAllRead`, { method: 'PUT' });
            } catch (e) { console.error(e); }
        }

        this.notifications.forEach(n => n.isRead = true);
        localStorage.setItem('user_notifications', JSON.stringify(this.notifications));
        this.updateBadge();
        
        setTimeout(() => this.renderNotifications(), 1000);
    }
}