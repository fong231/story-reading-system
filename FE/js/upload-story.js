const BACKEND_URL = "https://localhost:7210";

let selectedCoverFile = null;

document.addEventListener("DOMContentLoaded", async () => {
    const user = JSON.parse(localStorage.getItem("currentUser"));
    if (!user) {
        showToast("Bạn cần đăng nhập!", "error");
        return window.location.href = "login.html";
    }

    // Xử lý chuyển đổi sidebar
    const sidebarItems = document.querySelectorAll('.sidebar-item');
    sidebarItems.forEach(item => {
        item.addEventListener('click', () => {
            // Xóa active cũ
            sidebarItems.forEach(i => i.classList.remove('active'));
            document.querySelectorAll('.section-panel').forEach(p => p.classList.remove('active'));

            // Thêm active mới
            item.classList.add('active');
            const targetId = item.getAttribute('data-target');
            document.getElementById(targetId).classList.add('active');
        });
    });

    // Xử lý chọn ảnh bìa (chỉ preview, không upload ngay)
    const coverContainer = document.getElementById('cover-container');
    const coverFileInput = document.getElementById('story-cover-file');
    const btnReUpload = document.getElementById('btn-re-upload');

    coverContainer.addEventListener('click', (e) => {
        if (e.target.id !== 'btn-re-upload') {
            coverFileInput.click();
        }
    });

    btnReUpload.addEventListener('click', (e) => {
        e.stopPropagation();
        coverFileInput.value = '';
        selectedCoverFile = null;
        document.getElementById('cover-preview').style.display = 'none';
        document.getElementById('upload-placeholder').style.display = 'block';
        btnReUpload.style.display = 'none';
    });

    coverFileInput.addEventListener('change', (e) => {
        const file = e.target.files[0];
        if (!file) return;

        selectedCoverFile = file;

        // Hiển thị preview
        const reader = new FileReader();
        reader.onload = (e) => {
            const preview = document.getElementById('cover-preview');
            const placeholder = document.getElementById('upload-placeholder');
            preview.src = e.target.result;
            preview.style.display = 'block';
            placeholder.style.display = 'none';
            btnReUpload.style.display = 'block';
        };
        reader.readAsDataURL(file);
    });

    // Load dữ liệu ban đầu
    await loadCategories();
    await loadMyStories(user.userId);

    // Lắng nghe sự kiện thay đổi truyện để cập nhật số chương
    const selectStory = document.getElementById("select-story");
    selectStory.addEventListener("change", () => {
        updateNextChapterNumber(selectStory.value);
    });

    // Cập nhật số chương cho truyện đầu tiên được chọn (nếu có)
    if (selectStory.value) {
        updateNextChapterNumber(selectStory.value);
    }
});

async function updateNextChapterNumber(storyId) {
    if (!storyId) return;
    try {
        const res = await fetch(`${BACKEND_URL}/api/Chapters/Story/${storyId}`);
        if (res.ok) {
            const chapters = await res.json();
            let nextNum = 1;
            if (chapters && chapters.length > 0) {
                // Tìm số chương lớn nhất
                const maxNum = Math.max(...chapters.map(c => c.chapterNumber || 0));
                nextNum = maxNum + 1;
            }
            document.getElementById("chapter-number").value = nextNum;
        }
    } catch (error) {
        console.error("Lỗi khi lấy số chương tiếp theo:", error);
    }
}

async function loadCategories() {
    try {
        const res = await fetch(`${BACKEND_URL}/api/Categories`);
        if (res.ok) {
            const categories = await res.json();
            document.getElementById("story-genre").innerHTML = categories
                .map(c => `<option value="${c.categoryId}">${c.name}</option>`)
                .join("");
        }
    } catch (error) {
        console.error("Lỗi khi tải thể loại:", error);
    }
}

async function loadMyStories(userId) {
    try {
        const res = await fetch(`${BACKEND_URL}/api/Stories/Author/${userId}`);
        if (res.ok) {
            const stories = await res.json();
            const selectStory = document.getElementById("select-story");
            selectStory.innerHTML = stories
                .map(s => `<option value="${s.storyId}">${s.title}</option>`)
                .join("");
        }
    } catch (error) {
        console.error("Lỗi khi tải danh sách truyện của tôi:", error);
    }
}

// Xử lý tạo Truyện Mới
document.getElementById("btn-create-story").addEventListener("click", async () => {
    const user = JSON.parse(localStorage.getItem("currentUser"));
    const title = document.getElementById("story-title").value;
    const description = document.getElementById("story-description").value;
    const categoryId = document.getElementById("story-genre").value;

    if (!title || !description) {
        return showToast("Vui lòng nhập đầy đủ tên truyện và mô tả!", "info");
    }

    if (!selectedCoverFile) {
        return showToast("Vui lòng chọn ảnh bìa cho truyện!", "info");
    }

    // Sử dụng FormData để gửi cả file và text
    const formData = new FormData();
    formData.append("Title", title);
    formData.append("Description", description);
    formData.append("CategoryId", categoryId);
    formData.append("AuthorId", user.userId);
    formData.append("CoverFile", selectedCoverFile);

    try {
        showToast("Đang tạo truyện...", "info");
        const res = await fetch(`${BACKEND_URL}/api/Stories`, {
            method: "POST",
            body: formData
        });

        if (res.ok) {
            showToast("Tạo truyện mới thành công!", "success");
            // Reset form
            document.getElementById("story-title").value = "";
            document.getElementById("story-description").value = "";
            selectedCoverFile = null;
            document.getElementById("story-cover-file").value = "";
            document.getElementById('cover-preview').style.display = 'none';
            document.getElementById('upload-placeholder').style.display = 'block';
            document.getElementById('btn-re-upload').style.display = 'none';
            
            await loadMyStories(user.userId);
            
            const selectStory = document.getElementById("select-story");
            if (selectStory.value) {
                updateNextChapterNumber(selectStory.value);
            }
        } else {
            const errorMsg = await res.text();
            showToast(errorMsg || "Có lỗi xảy ra khi tạo truyện.", "error");
        }
    } catch (error) {
        showToast("Lỗi kết nối máy chủ!", "error");
    }
});

// Xử lý đăng Chương Mới
document.getElementById("btn-publish-chapter").addEventListener("click", async () => {
    const storyId = document.getElementById("select-story").value;
    const chapNum = document.getElementById("chapter-number").value;
    const title = document.getElementById("chapter-title").value;
    const rawContent = document.getElementById("chapter-content").value;

    if (!storyId || !title || !rawContent) {
        return showToast("Vui lòng chọn truyện và nhập đầy đủ thông tin chương!", "info");
    }

    const processedContent = rawContent
        .split('\n')
        .map(line => line.trim())
        .map(line => `<div>${line}</div>`)
        .join('');

    try {
        const res = await fetch(`${BACKEND_URL}/api/Chapters`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                storyId: parseInt(storyId),
                chapterNumber: parseInt(chapNum),
                title: title,
                content: processedContent
            })
        });

        if (res.ok) {
            showToast("Đăng chương thành công! Đang gửi thông báo...", "success");

            // Kích hoạt thông báo qua SignalR
            try {
                const connection = new signalR.HubConnectionBuilder()
                    .withUrl(`${BACKEND_URL}/notificationHub`)
                    .build();

                await connection.start();
                await connection.invoke("SendStoryNotification", parseInt(storyId), {
                    message: `Truyện bạn theo dõi có chương mới: ${title}!`
                });
                await connection.stop();
            } catch (sigErr) {
                console.warn("Lỗi gửi SignalR, nhưng chương đã đăng thành công.", sigErr);
            }

            document.getElementById("chapter-title").value = "";
            document.getElementById("chapter-content").value = "";
            document.getElementById("chapter-number").value = parseInt(chapNum) + 1;
        } else {
            showToast("Lỗi khi đăng chương.", "error");
        }
    } catch (error) {
        showToast("Lỗi kết nối máy chủ!", "error");
    }
});