const BACKEND_URL = "https://localhost:7210";

document.addEventListener("DOMContentLoaded", async () => {
    const user = JSON.parse(localStorage.getItem("currentUser"));
    if(user) document.getElementById("user-greeting").textContent = `Chào, ${user.username}`;
    else window.location.href = 'login.html'; // Bắt buộc login

    const observer = new NotificationObserver("noti-badge");
    observer.connect(BACKEND_URL);

    await loadCategories();
    await loadStories("all");

    document.getElementById("category-filter").addEventListener("change", (e) => {
        loadStories(e.target.value);
    });

    // Logic Xuất Báo Cáo đã chuyển qua trang export-report.html
});

async function loadCategories() {
    try {
        const res = await fetch(`${BACKEND_URL}/api/Categories`); 
        if (!res.ok) return;
        const categories = await res.json();
        categories.forEach(c => {
            document.getElementById("category-filter").innerHTML += `<option value="${c.categoryId}">${c.name}</option>`;
        });
    } catch (e) { console.warn("Lỗi tải Category", e); }
}

async function loadStories(categoryId) {
    const listEl = document.getElementById("story-list");
    listEl.innerHTML = "<p>Đang tải...</p>";
    try {
        const url = categoryId === "all" ? `${BACKEND_URL}/api/Stories` : `${BACKEND_URL}/api/Stories/Category/${categoryId}`;
        const res = await fetch(url);
        const stories = await res.json();
        
        listEl.innerHTML = stories.length === 0 ? "<p>Không có truyện.</p>" : stories.map(story => `
            <div class="story-card" onclick="window.location.href='story-detail.html?storyId=${story.storyId}'">
                <img src="${story.coverImage}" alt="Cover" style="margin-bottom: 10px;">
                <h3 style="margin-bottom: 5px;">${story.title}</h3>
                <div class="badge-category" style="width: fit-content; margin-bottom: 10px;">${story.category ? story.category.name : 'Chưa phân loại'}</div>
                <p style="font-size: 14px; color: #666;">${story.description.substring(0, 50)}...</p>
            </div>
        `).join('');
    } catch (e) { listEl.innerHTML = "<p>Lỗi tải truyện</p>"; }
}