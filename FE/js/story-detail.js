const BACKEND_URL = "https://localhost:7210";
const urlParams = new URLSearchParams(window.location.search);
const storyId = parseInt(urlParams.get('storyId'));
const currentUser = JSON.parse(localStorage.getItem("currentUser"));

let isFollowing = false;
let userRating = 0;

document.addEventListener("DOMContentLoaded", async () => {
    if (!storyId) return;
    
    // Hiển thị info user nếu đã login
    if (currentUser) {
        document.getElementById("user-info").innerHTML = `<span>Chào, ${currentUser.username}</span> | <a href="#" onclick="logout()" style="color:white">Thoát</a>`;
    } else {
        document.getElementById("user-info").innerHTML = `<a href="login.html" style="color:white">Đăng nhập</a>`;
    }

    await loadStoryDetail();
    await loadComments();

    if (currentUser) {
        await checkFollowStatus();
        await checkUserRating();
    }

    const observer = new NotificationObserver("noti-badge");
    observer.connect(BACKEND_URL);
});

async function checkFollowStatus() {
    try {
        const res = await fetch(`${BACKEND_URL}/api/Notifications/IsFollowing/User/${currentUser.userId}/Story/${storyId}`);
        if (res.ok) {
            isFollowing = await res.json();
            updateFollowButton();
        }
    } catch (e) { console.error("Lỗi check follow:", e); }
}

function updateFollowButton() {
    const btn = document.getElementById("btn-follow");
    if (!btn) return;
    
    if (isFollowing) {
        btn.innerText = "Đã theo dõi";
        btn.classList.replace("btn-outline", "btn-success");
    } else {
        btn.innerText = "Theo dõi";
        btn.classList.replace("btn-success", "btn-outline");
    }
}

async function checkUserRating() {
    try {
        const res = await fetch(`${BACKEND_URL}/api/Ratings/User/${currentUser.userId}/Story/${storyId}`);
        if (res.ok) {
            const data = await res.json();
            userRating = data.score;
            updateRatingUI();
        }
    } catch (e) { /* Có thể user chưa rate */ }
}

function updateRatingUI() {
    const stars = document.querySelectorAll("#user-stars span");
    stars.forEach((star, index) => {
        star.innerText = (index < userRating) ? "★" : "☆";
        star.style.color = (index < userRating) ? "#f1c40f" : "#ccc";
    });
    
    // Thêm nút hủy đánh giá nếu user đã rate
    let cancelBtn = document.getElementById("btn-cancel-rating");
    if (userRating > 0) {
        if (!cancelBtn) {
            const section = document.querySelector(".rating-section");
            cancelBtn = document.createElement("a");
            cancelBtn.id = "btn-cancel-rating";
            cancelBtn.href = "#";
            cancelBtn.innerText = " (Hủy đánh giá)";
            cancelBtn.style.fontSize = "0.8em";
            cancelBtn.style.color = "#e74c3c";
            cancelBtn.style.marginLeft = "10px";
            cancelBtn.onclick = (e) => { e.preventDefault(); cancelRating(); };
            section.appendChild(cancelBtn);
        }
    } else if (cancelBtn) {
        cancelBtn.remove();
    }
}

async function cancelRating() {
    if (!currentUser) return;

    try {
        const res = await fetch(`${BACKEND_URL}/api/Ratings/User/${currentUser.userId}/Story/${storyId}`, {
            method: 'DELETE'
        });
        if (res.ok) {
            userRating = 0;
            updateRatingUI();
            showToast("Đã hủy đánh giá!", "success");
            setTimeout(loadStoryDetail, 1000);
        } else {
            showToast("Không thể hủy đánh giá!", "error");
        }
    } catch (e) { showToast("Lỗi kết nối!", "error"); }
}

async function loadStoryDetail() {
    try {
        const res = await fetch(`${BACKEND_URL}/api/Stories/${storyId}`);
        const story = await res.json();
        
        // Load Rating info (chung)
        const ratingRes = await fetch(`${BACKEND_URL}/api/Ratings/Story/${storyId}`);
        const ratingData = await ratingRes.json();

        const ratingText = ratingData.totalRatings > 0 
            ? `${ratingData.averageRating}/5 (${ratingData.totalRatings} lượt)` 
            : "Chưa có đánh giá";

        let followBtnHtml = '';
        if (currentUser) {
            followBtnHtml = `<button id="btn-follow" class="btn btn-outline" onclick="toggleFollow()">Theo dõi</button>`;
        }

        let continueBtnHtml = '';
        if (currentUser) {
            try {
                const bookmarkRes = await fetch(`${BACKEND_URL}/api/Bookmarks/User/${currentUser.userId}`);
                const bookmarks = await bookmarkRes.json();
                const myBookmark = bookmarks.find(b => b.story.storyId === storyId);
                if (myBookmark) {
                    continueBtnHtml = `<button class="btn btn-success" onclick="continueReading(${myBookmark.chapter.chapterId})">Đọc tiếp (Ch. ${myBookmark.chapter.chapterNumber})</button>`;
                }
            } catch (e) { }
        }

        document.getElementById("story-info").innerHTML = `
            <img src="${story.coverImage || 'https://via.placeholder.com/200x300'}" class="story-cover">
            <div class="story-meta">
                <h2 style="margin-bottom: 10px;">${story.title}</h2>
                <p style="margin-bottom: 5px;"><strong>Tác giả:</strong> ${story.author.username}</p>
                <p style="margin-bottom: 10px;"><strong>Thể loại:</strong> ${story.category.name}</p>
                <p style="margin-bottom: 10px;"><strong>Lượt xem:</strong> ${story.viewCount} | <strong>Đánh giá:</strong> ${ratingText}</p>
                <div class="rating-section">
                    <span>Đánh giá của bạn: </span>
                    <span class="stars" id="user-stars">
                        ${[1, 2, 3, 4, 5].map(i => `<span onclick="submitRating(${i})" style="cursor:pointer">☆</span>`).join('')}
                    </span>
                </div>
                <div class="actions" style="margin-bottom: 10px;">
                    <button class="btn btn-primary" onclick="readFirstChapter()">Đọc từ đầu</button>
                    ${continueBtnHtml}
                    ${followBtnHtml}
                </div>
                <hr style="margin-bottom: 10px;">
                <p>${story.description}</p>
            </div>
        `;

        const chapters = story.chapters || [];
        document.getElementById("chapters-container").innerHTML = chapters.length > 0 ? chapters.map(c => `
            <a href="reader.html?storyId=${storyId}&chapterId=${c.chapterId}" 
               style="display:block; padding: 10px; border-bottom: 1px solid #eee; text-decoration: none; color: #333;">
                Chương ${c.chapterNumber}: ${c.title}
                <span style="float:right; color:#888; font-size:0.8em">${new Date(c.publishedAt).toLocaleDateString()}</span>
            </a>
        `).join('') : '<p>Chưa có chương nào.</p>';

        if (chapters.length > 0) {
            window.firstChapterId = chapters[0].chapterId;
        }

        // Sau khi render story-info xong, nếu đang follow thì update button ngay
        if (currentUser) {
            updateFollowButton();
            updateRatingUI();
        }

    } catch (e) { 
        console.error(e);
        showToast("Lỗi tải thông tin truyện!", "error");
    }
}

async function loadComments() {
    try {
        const res = await fetch(`${BACKEND_URL}/api/Comments/Story/${storyId}`);
        const comments = await res.json();
        
        document.getElementById("comments-list").innerHTML = comments.length > 0 ? comments.map(c => `
            <div class="comment-item">
                <span class="comment-user">${c.user.username}</span>
                <span class="comment-date">${new Date(c.createdAt).toLocaleString()}</span>
                <p style="margin-top:5px">${c.content}</p>
            </div>
        `).join('') : '<p>Chưa có bình luận nào. Hãy là người đầu tiên!</p>';
    } catch (e) { console.error(e); }
}

async function toggleFollow() {
    if (!currentUser) { showToast("Vui lòng đăng nhập!", "info"); return; }
    
    const endpoint = isFollowing ? "Unfollow" : "Follow";
    try {
        const res = await fetch(`${BACKEND_URL}/api/Notifications/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: currentUser.userId, storyId: storyId })
        });
        if (res.ok) {
            isFollowing = !isFollowing;
            updateFollowButton();
            showToast(isFollowing ? "Đã theo dõi truyện!" : "Đã bỏ theo dõi!", "success");
        } else {
            const data = await res.json();
            showToast(data.message || "Thao tác thất bại!", "error");
        }
    } catch (e) { showToast("Lỗi kết nối!", "error"); }
}

async function submitRating(score) {
    if (!currentUser) { showToast("Vui lòng đăng nhập!", "info"); return; }
    try {
        const res = await fetch(`${BACKEND_URL}/api/Ratings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: currentUser.userId, storyId: storyId, score: score })
        });
        if (res.ok) {
            userRating = score;
            updateRatingUI();
            showToast(`Bạn đã đánh giá ${score} sao!`, "success");
            // Reload story info to update average rating
            setTimeout(loadStoryDetail, 1000);
        }
    } catch (e) { showToast("Lỗi gửi đánh giá!", "error"); }
}

document.getElementById("btn-submit-comment").addEventListener("click", async () => {
    if (!currentUser) { showToast("Vui lòng đăng nhập!", "info"); return; }
    const content = document.getElementById("comment-content").value;
    if (!content.trim()) return;

    try {
        const res = await fetch(`${BACKEND_URL}/api/Comments`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: currentUser.userId, storyId: storyId, content: content })
        });
        if (res.ok) {
            showToast("Đã gửi bình luận!", "success");
            document.getElementById("comment-content").value = "";
            loadComments();
        }
    } catch (e) { showToast("Lỗi kết nối!", "error"); }
});

function readFirstChapter() {
    if (window.firstChapterId) {
        window.location.href = `reader.html?storyId=${storyId}&chapterId=${window.firstChapterId}`;
    } else {
        showToast("Truyện chưa có chương nào!", "info");
    }
}

function continueReading(chapterId) {
    window.location.href = `reader.html?storyId=${storyId}&chapterId=${chapterId}`;
}

function logout() {
    localStorage.removeItem("currentUser");
    window.location.reload();
}