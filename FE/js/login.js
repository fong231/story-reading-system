const BACKEND_URL = "https://localhost:7210";

document.getElementById("btn-login").addEventListener("click", async () => {
    const user = document.getElementById("username").value;
    if (!user) {
        return showToast("Vui lòng nhập tên đăng nhập!", "error");
    }
    
    try {
        const res = await fetch(`${BACKEND_URL}/api/Users`);
        if (!res.ok) throw new Error("Không thể kết nối máy chủ");
        
        const users = await res.json();
        const validUser = users.find(u => u.username === user);
        
        if (validUser) {
            localStorage.setItem("currentUser", JSON.stringify(validUser));
            window.location.href = "index.html";
        } else {
            showToast("Sai tên đăng nhập!", "error");
        }
    } catch (e) { 
        showToast("Lỗi kết nối BE!", "error"); 
    }
});