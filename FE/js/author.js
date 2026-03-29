const BACKEND_URL = "https://localhost:7210";

document.addEventListener("DOMContentLoaded", async () => {
    const user = JSON.parse(localStorage.getItem("currentUser"));
    if (!user || user.role !== "Author") return window.location.href = "login.html";

    const res = await fetch(`${BACKEND_URL}/api/Stories`);
    const stories = await res.json();
    const myStories = stories.filter(s => s.authorId === user.userId);
    document.getElementById("select-story").innerHTML = myStories.map(s => `<option value="${s.storyId}">${s.title}</option>`).join('');
});

document.getElementById("btn-publish").addEventListener("click", async () => {
    const storyId = document.getElementById("select-story").value;
    const chapNum = document.getElementById("chapter-number").value;
    const title = document.getElementById("chapter-title").value;
    const content = document.getElementById("chapter-content").value;

    const res = await fetch(`${BACKEND_URL}/api/Chapters`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ storyId: parseInt(storyId), chapterNumber: parseInt(chapNum), title: title, content: content })
    });

    if (res.ok) {
        alert("Đăng chương thành công! Gửi thông báo đến người đọc...");
        const connection = new signalR.HubConnectionBuilder().withUrl(`${BACKEND_URL}/notificationHub`).build();
        await connection.start();
        await connection.invoke("SendStoryNotification", parseInt(storyId), { message: `Có chương mới: ${title}!` });
        await connection.stop();
        document.getElementById("chapter-title").value = "";
    }
});