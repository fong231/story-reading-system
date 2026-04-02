const BACKEND_URL = "https://localhost:7210";
const currentUser = JSON.parse(localStorage.getItem("currentUser"));

if (!currentUser) {
    showToast("Bạn cần đăng nhập để truy cập trang này!", "error");
    setTimeout(() => window.location.href = "login.html", 2000);
}

document.addEventListener("DOMContentLoaded", async () => {
    const storySelect = document.getElementById('story-select');
    const btnPreview = document.getElementById('btn-preview');
    const btnDownload = document.getElementById('btn-download');
    const previewEl = document.getElementById('report-preview');

    // 1. Tải danh sách truyện của tác giả
    try {
        const res = await fetch(`${BACKEND_URL}/api/Stories/Author/${currentUser.userId}`);
        if (res.ok) {
            const stories = await res.json();
            stories.forEach(s => {
                const opt = document.createElement('option');
                opt.value = s.storyId;
                opt.textContent = s.title;
                storySelect.appendChild(opt);
            });
        }
    } catch (e) {
        console.error("Lỗi tải danh sách truyện:", e);
        showToast("Lỗi khi tải danh sách truyện!", "error");
    }

    // 2. Click Xem Trước
    btnPreview.addEventListener('click', async () => {
        const storyId = storySelect.value;
        const type = document.getElementById('report-type').value;
        const startDate = document.getElementById('start-date').value;
        const endDate = document.getElementById('end-date').value;

        if (!storyId) return showToast("Vui lòng chọn truyện!", "info");

        try {
            const url = `${BACKEND_URL}/api/Reports/Author/${currentUser.userId}?type=${type}&startDate=${startDate}&endDate=${endDate}&storyId=${storyId}`;
            const res = await fetch(url);
            
            if (res.ok) {
                const text = await res.text();
                previewEl.textContent = text;
                previewEl.style.display = 'block';
                btnDownload.style.display = 'block';
                showToast("Đã tạo bản xem trước!", "success");
            } else {
                showToast("Không thể tải bản xem trước!", "error");
            }
        } catch (e) {
            showToast("Lỗi kết nối server!", "error");
        }
    });

    // 3. Click Tải về
    btnDownload.addEventListener('click', () => {
        const content = previewEl.textContent;
        const type = document.getElementById('report-type').value;
        const blob = new Blob([content], { type: 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${type}_report_${new Date().getTime()}.txt`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
        showToast("Bắt đầu tải về...", "success");
    });
});