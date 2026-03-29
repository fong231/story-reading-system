const BACKEND_URL = "https://localhost:7210";

document.getElementById("btn-login").addEventListener("click", async () => {
    const user = document.getElementById("username").value;
    // Giả lập check login từ danh sách Users (BE đã có Seed Data)
    try {
        const res = await fetch(`${BACKEND_URL}/api/Users`);
        const users = await res.json();
        const validUser = users.find(u => u.username === user);
        
        if (validUser) {
            localStorage.setItem("currentUser", JSON.stringify(validUser));
            window.location.href = validUser.role === "Author" ? "author.html" : "index.html";
        } else {
            alert("Sai tên đăng nhập!");
        }
    } catch (e) { alert("Lỗi kết nối BE!"); }
});