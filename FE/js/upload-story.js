const BACKEND_URL = "https://localhost:7210";

document.addEventListener("DOMContentLoaded", async () => {
    const user = JSON.parse(localStorage.getItem("currentUser"));
    if (!user) {
        showToast("Bạn cần đăng nhập!", "error");
        return window.location.href = "login.html";
    }

    await loadCategories();
    await loadMyStories(user.userId);
});

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
            document.getElementById("select-story").innerHTML = stories
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
    const coverImage = document.getElementById("story-cover").value;

    if (!title || !description) {
        return showToast("Vui lòng nhập đầy đủ tên truyện và mô tả!", "info");
    }

    try {
        const res = await fetch(`${BACKEND_URL}/api/Stories`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                title,
                description,
                categoryId: parseInt(categoryId),
                coverImage,
                authorId: user.userId
            })
        });

        if (res.ok) {
            showToast("Tạo truyện mới thành công!", "success");
            document.getElementById("story-title").value = "";
            document.getElementById("story-description").value = "";
            document.getElementById("story-cover").value = "";
            await loadMyStories(user.userId);
        } else {
            showToast("Có lỗi xảy ra khi tạo truyện.", "error");
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

    if (!storyId || !title || !content) {
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