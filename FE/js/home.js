const BACKEND_URL = "https://localhost:7210"; // Sửa lại port BE của em nếu khác

document.addEventListener("DOMContentLoaded", async () => {
    // 1. Lắng nghe thông báo (Observer Pattern)
    const notificationObserver = new NotificationObserver("noti-badge");
    notificationObserver.connect(BACKEND_URL);

    // 2. Tải danh sách truyện từ Backend (Factory Pattern ở BE)
    await loadStories();
});

async function loadStories() {
    const storyListEl = document.getElementById("story-list");
    try {
        const response = await fetch(`${BACKEND_URL}/api/Stories`);
        if (!response.ok) throw new Error("Lỗi mạng");
        
        const stories = await response.json();
        storyListEl.innerHTML = ""; // Xóa chữ "Đang tải..."

        stories.forEach(story => {
            const card = document.createElement("div");
            card.className = "story-card";
            card.innerHTML = `
                <img src="${story.coverImage || 'https://via.placeholder.com/200x300?text=No+Cover'}" alt="Cover">
                <h3>${story.title}</h3>
                <span class="badge-category">Thể loại: ${story.category.name}</span>
                <p style="font-size: 14px; color: #666;">${story.description}</p>
            `;
            // Khi bấm vào truyện, chuyển sang trang đọc
            card.addEventListener("click", () => {
                window.location.href = `reader.html?storyId=${story.storyId}`;
            });
            storyListEl.appendChild(card);
        });
    } catch (error) {
        console.error("Lỗi khi tải danh sách truyện:", error);
        storyListEl.innerHTML = "<p style='color:red;'>Không thể tải danh sách truyện lúc này.</p>";
    }
}