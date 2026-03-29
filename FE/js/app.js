const BACKEND_URL = "https://localhost:7210"; 

const currentUser = JSON.parse(localStorage.getItem("currentUser")) || { userId: 3 }; // Lấy User từ Session
const CURRENT_USER_ID = currentUser.userId; 
const urlParams = new URLSearchParams(window.location.search);
const CURRENT_STORY_ID = parseInt(urlParams.get('storyId')); 
const CURRENT_CHAPTER_ID = parseInt(urlParams.get('chapterId'));

class ReaderContext {
    constructor(container) { this.container = container; this.strategy = null; }
    setStrategy(strategy) { this.strategy = strategy; this.strategy.apply(this.container); }
    setMode(modeString) {
        switch(modeString) {
            case 'day-scroll': this.setStrategy(new DayScrollStrategy()); break;
            case 'night-scroll': this.setStrategy(new NightScrollStrategy()); break;
            case 'day-flip': this.setStrategy(new DayFlipStrategy()); break;
            case 'night-flip': this.setStrategy(new NightFlipStrategy()); break;
        }
    }
}

document.addEventListener("DOMContentLoaded", async () => {
    // 1. TẢI NỘI DUNG CHƯƠNG TỪ BE
    if (CURRENT_CHAPTER_ID) {
        const res = await fetch(`${BACKEND_URL}/api/Chapters/${CURRENT_CHAPTER_ID}`);
        if (res.ok) {
            const chapter = await res.json();
            document.getElementById("chapter-title").textContent = `Chương ${chapter.chapterNumber}: ${chapter.title}`;
            document.getElementById("chapter-content").innerHTML = chapter.content;
        }
    }

    // 2. LOGIC COMMAND & STRATEGY (UI Mode)
    const container = document.getElementById("reader-container");
    const selectMode = document.getElementById("reading-mode-select");
    const btnUndo = document.getElementById("btn-undo"), btnRedo = document.getElementById("btn-redo");
    const readerContext = new ReaderContext(container);
    const invoker = new SettingsInvoker();
    
    let currentMode = 'day-scroll';
    readerContext.setMode(currentMode); // Set default

    const updateBtns = () => { btnUndo.disabled = !invoker.canUndo(); btnRedo.disabled = !invoker.canRedo(); };
    selectMode.addEventListener("change", (e) => {
        const cmd = new ChangeReadingModeCommand(readerContext, e.target.value, currentMode);
        invoker.executeCommand(cmd);
        currentMode = e.target.value; updateBtns();
    });
    btnUndo.addEventListener("click", () => {
        const m = invoker.undo(); if(m){ currentMode = m; selectMode.value = m; updateBtns(); }
    });
    btnRedo.addEventListener("click", () => {
        const m = invoker.redo(); if(m){ currentMode = m; selectMode.value = m; updateBtns(); }
    });

    // 3. SINGLETON PROGRESS (Tự động scroll tới vị trí đọc cũ)
    try {
        const progRes = await fetch(`${BACKEND_URL}/api/Reading/Progress/${CURRENT_USER_ID}`);
        if(progRes.ok) {
            const progData = await progRes.json();
            if(progData.currentChapterId === CURRENT_CHAPTER_ID && progData.lastReadPosition > 0) {
                window.scrollTo({ top: progData.lastReadPosition, behavior: 'smooth' });
            }
        }
    } catch(e) {}

    let scrollTimeout;
    window.addEventListener("scroll", () => {
        if(scrollTimeout) clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(() => {
            fetch(`${BACKEND_URL}/api/Reading/Progress`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: CURRENT_USER_ID, storyId: CURRENT_STORY_ID, chapterId: CURRENT_CHAPTER_ID, position: Math.round(window.scrollY) })
            });
        }, 2000); 
    });

    // 4. BOOKMARK
    document.getElementById("btn-bookmark").addEventListener("click", async () => {
        try {
            await fetch(`${BACKEND_URL}/api/Bookmarks`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: CURRENT_USER_ID, storyId: CURRENT_STORY_ID, chapterId: CURRENT_CHAPTER_ID, scrollPosition: Math.round(window.scrollY) })
            });
            alert("Đã bookmark thành công!");
        } catch(e) { alert("Lỗi!"); }
    });

    // 5. RATING
    document.querySelectorAll("#star-rating span").forEach(star => {
        star.addEventListener("click", async (e) => {
            const score = e.target.getAttribute("data-value");
            document.querySelectorAll("#star-rating span").forEach(s => s.style.color = s.getAttribute("data-value") <= score ? "#f39c12" : "#ccc");
            await fetch(`${BACKEND_URL}/api/Ratings`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: CURRENT_USER_ID, storyId: CURRENT_STORY_ID, score: parseInt(score) })
            });
        });
    });

    // 6. COMMENTS
    const loadComments = async () => {
        const res = await fetch(`${BACKEND_URL}/api/Comments/Story/${CURRENT_STORY_ID}`);
        if(!res.ok) return;
        const comments = await res.json();
        document.getElementById("comment-list").innerHTML = comments.map(c => `<p><b>User ${c.userId}:</b> ${c.content}</p>`).join('');
    };
    document.getElementById("btn-submit-comment").addEventListener("click", async () => {
        const text = document.getElementById("comment-text").value;
        if(!text) return;
        await fetch(`${BACKEND_URL}/api/Comments`, {
            method: 'POST', headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: CURRENT_USER_ID, storyId: CURRENT_STORY_ID, content: text })
        });
        document.getElementById("comment-text").value = "";
        loadComments();
    });
    loadComments();

    // 7. SIGNALR OBSERVER
    const observer = new NotificationObserver("noti-badge");
    observer.connect(BACKEND_URL);
});