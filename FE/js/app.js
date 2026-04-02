const BACKEND_URL = "https://localhost:7210"; 

const currentUser = JSON.parse(localStorage.getItem("currentUser")) || { userId: 3 }; 
const CURRENT_USER_ID = currentUser.userId; 
const urlParams = new URLSearchParams(window.location.search);
const CURRENT_STORY_ID = parseInt(urlParams.get('storyId')); 
const CURRENT_CHAPTER_ID = parseInt(urlParams.get('chapterId'));

class ReaderContext {
    constructor(container) { this.container = container; this.strategy = null; }
    setStrategy(strategy) { this.strategy = strategy; this.strategy.apply(this.container); }
    
    setReadingMode(theme, nav) {
        const modeString = `${theme}-${nav}`;
        switch(modeString) {
            case 'day-scroll': this.setStrategy(new DayScrollStrategy()); break;
            case 'night-scroll': this.setStrategy(new NightScrollStrategy()); break;
            case 'day-flip': this.setStrategy(new DayFlipStrategy()); break;
            case 'night-flip': this.setStrategy(new NightFlipStrategy()); break;
        }
    }
    
    setFontSize(size) { this.container.style.fontSize = size + 'px'; this.strategy && this.strategy.recalculateHeight(this.container); }
    setFontFamily(family) { this.container.style.fontFamily = family; }
    setLineHeight(height) { this.container.style.lineHeight = height; this.strategy && this.strategy.recalculateHeight(this.container); }

    nextPage() {
        const content = document.getElementById('chapter-content');
        if (content && content.classList.contains('mode-flip')) {
            const width = content.clientWidth;
            content.scrollBy({ left: width + 40, behavior: 'smooth' });
        }
    }

    prevPage() {
        const content = document.getElementById('chapter-content');
        if (content && content.classList.contains('mode-flip')) {
            const width = content.clientWidth;
            content.scrollBy({ left: -(width + 40), behavior: 'smooth' });
        }
    }
}

document.addEventListener("DOMContentLoaded", async () => {
    // 1. TẢI NỘI DUNG CHƯƠNG TỪ BE + ĐIỀU HƯỚNG CHƯƠNG PREV/NEXT
    if (CURRENT_CHAPTER_ID) {
        const res = await fetch(`${BACKEND_URL}/api/Chapters/${CURRENT_CHAPTER_ID}`);
        if (res.ok) {
            const chapter = await res.json();
            document.getElementById("chapter-title").textContent = `Chương ${chapter.chapterNumber}: ${chapter.title}`;
            document.getElementById("chapter-content").innerHTML = chapter.content;
        }
    }

    // Lấy danh sách chương để xác định prev/next
    if (CURRENT_STORY_ID && CURRENT_CHAPTER_ID) {
        try {
            const chapRes = await fetch(`${BACKEND_URL}/api/Chapters/Story/${CURRENT_STORY_ID}`);
            if (chapRes.ok) {
                const chapters = await chapRes.json(); // [{ChapterId, ChapterNumber, ...}]
                // Sắp xếp theo số chương
                chapters.sort((a, b) => a.chapterNumber - b.chapterNumber);
                const idx = chapters.findIndex(c => c.chapterId === CURRENT_CHAPTER_ID);

                const btnPrev = document.getElementById("btn-prev-chapter");
                const btnNext = document.getElementById("btn-next-chapter");

                // Chương trước
                if (idx > 0) {
                    const prevChapter = chapters[idx - 1];
                    btnPrev.disabled = false;
                    btnPrev.title = `Chương ${prevChapter.chapterNumber}: ${prevChapter.title}`;
                    btnPrev.addEventListener("click", () => {
                        window.location.href = `reader.html?storyId=${CURRENT_STORY_ID}&chapterId=${prevChapter.chapterId}`;
                    });
                }

                // Chương tiếp theo
                if (idx !== -1 && idx < chapters.length - 1) {
                    const nextChapter = chapters[idx + 1];
                    btnNext.disabled = false;
                    btnNext.title = `Chương ${nextChapter.chapterNumber}: ${nextChapter.title}`;
                    btnNext.addEventListener("click", () => {
                        window.location.href = `reader.html?storyId=${CURRENT_STORY_ID}&chapterId=${nextChapter.chapterId}`;
                    });
                }
            }
        } catch (e) {
            console.warn("Không thể tải danh sách chương:", e);
        }
    }

    // 2. KHỞI TẠO BIẾN
    const modal = document.getElementById("settings-modal");
    const btnSettings = document.getElementById("btn-settings");
    const closeSettings = document.getElementById("close-settings");
    const btnCancel = document.getElementById("btn-cancel-settings");
    const btnSave = document.getElementById("btn-save-settings");

    const container = document.getElementById("reader-container");
    const themeSelect = document.getElementById("theme-select");
    const navModeSelect = document.getElementById("nav-mode-select");
    const fontSizeInput = document.getElementById("font-size-input");
    const fontFamilySelect = document.getElementById("font-family-select");
    const lineHeightSelect = document.getElementById("line-height-select");
    const btnUndo = document.getElementById("btn-undo"), btnRedo = document.getElementById("btn-redo");
    
    const readerContext = new ReaderContext(container);
    const invoker = new SettingsInvoker();

    let currentSettings = {
        theme: 'day',
        navMode: 'scroll',
        fontSize: 18,
        fontFamily: 'Arial',
        lineHeight: '1.5'
    };

    // 3. HÀM HỖ TRỢ
    const applyToUI = (s) => {
        readerContext.setReadingMode(s.theme, s.navMode);
        readerContext.setFontSize(s.fontSize);
        readerContext.setFontFamily(s.fontFamily);
        readerContext.setLineHeight(s.lineHeight);
    };

    const syncInputs = (s) => {
        themeSelect.value = s.theme;
        navModeSelect.value = s.navMode;
        fontSizeInput.value = s.fontSize;
        fontFamilySelect.value = s.fontFamily;
        lineHeightSelect.value = s.lineHeight;
    };

    const updateBtns = () => { 
        btnUndo.disabled = !invoker.canUndo(); 
        btnRedo.disabled = !invoker.canRedo(); 
    };

    const closeModal = () => {
        modal.style.display = "none";
        // Khi đóng modal, khôi phục UI về trạng thái currentSettings (loại bỏ preview)
        applyToUI(currentSettings);
    };

    // 4. TẢI CÀI ĐẶT BAN ĐẦU TỪ BACKEND
    try {
        const modeRes = await fetch(`${BACKEND_URL}/api/Reading/Mode/${CURRENT_USER_ID}`);
        if (modeRes.ok) {
            const data = await modeRes.json();
            currentSettings = {
                theme: data.theme.toLowerCase(),
                navMode: data.navigationMode.toLowerCase(),
                fontSize: data.fontSize,
                fontFamily: data.fontFamily,
                lineHeight: data.lineHeight.toString()
            };
        } else {
            const saved = localStorage.getItem('reader_settings');
            if (saved) currentSettings = JSON.parse(saved);
        }
    } catch (e) {
        const saved = localStorage.getItem('reader_settings');
        if (saved) currentSettings = JSON.parse(saved);
    }

    applyToUI(currentSettings);
    syncInputs(currentSettings);
    
    // Quan trọng: Reset lịch sử undo/redo khi mới vào để mốc đầu tiên là dữ liệu từ Backend
    invoker.clearHistory();
    updateBtns();

    // 5. EVENT LISTENERS
    btnSettings.onclick = () => {
        syncInputs(currentSettings);
        modal.style.display = "block";
    };

    closeSettings.onclick = closeModal;
    btnCancel.onclick = closeModal;
    window.onclick = (e) => { if (e.target == modal) closeModal(); };

    // Preview khi thay đổi input (Chỉ đổi UI, không update currentSettings, không tạo Command)
    const handlePreview = () => {
        const previewS = {
            theme: themeSelect.value,
            navMode: navModeSelect.value,
            fontSize: parseInt(fontSizeInput.value),
            fontFamily: fontFamilySelect.value,
            lineHeight: lineHeightSelect.value
        };
        applyToUI(previewS);
    };

    [themeSelect, navModeSelect, fontSizeInput, fontFamilySelect, lineHeightSelect].forEach(el => {
        el.addEventListener("change", handlePreview);
    });

    // LƯU CẤU HÌNH: Thực thi Command và lưu Backend
    btnSave.addEventListener("click", async () => {
        const newSettings = {
            theme: themeSelect.value,
            navMode: navModeSelect.value,
            fontSize: parseInt(fontSizeInput.value),
            fontFamily: fontFamilySelect.value,
            lineHeight: lineHeightSelect.value
        };

        // Tạo Command để lưu vào Stack (phục vụ undo/redo)
        const oldSettingsForCmd = { ...currentSettings, mode: `${currentSettings.theme}-${currentSettings.navMode}` };
        const newSettingsForCmd = { ...newSettings, mode: `${newSettings.theme}-${newSettings.navMode}` };
        
        const cmd = new ChangeReadingModeCommand(readerContext, newSettingsForCmd, oldSettingsForCmd);
        invoker.executeCommand(cmd);
        
        // Cập nhật mốc cài đặt hiện tại
        currentSettings = newSettings;
        updateBtns();

        // Lưu vĩnh viễn lên Backend
        const body = {
            userId: CURRENT_USER_ID,
            theme: newSettings.theme.charAt(0).toUpperCase() + newSettings.theme.slice(1),
            navigationMode: newSettings.navMode.charAt(0).toUpperCase() + newSettings.navMode.slice(1),
            fontSize: newSettings.fontSize,
            fontFamily: newSettings.fontFamily,
            lineHeight: parseFloat(newSettings.lineHeight)
        };

        try {
            const res = await fetch(`${BACKEND_URL}/api/Reading/Mode`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            localStorage.setItem('reader_settings', JSON.stringify(currentSettings));
            if (res.ok) {
                showToast("Đã lưu cấu hình lên máy chủ!", "success");
            }
        } catch (e) {
            console.error("Lỗi lưu backend", e);
            showToast("Lỗi kết nối máy chủ!", "error");
        }
        
        // Đóng modal mà không gọi lại applyToUI(currentSettings) của hàm closeModal mặc định
        modal.style.display = "none";
    });

    // Undo/Redo: Khi nhấn nút này, currentSettings cũng được cập nhật theo
    btnUndo.addEventListener("click", () => {
        const s = invoker.undo(); 
        if(s){ 
            const parts = s.mode.split('-');
            currentSettings = { ...s, theme: parts[0], navMode: parts[1] };
            applyToUI(currentSettings);
            syncInputs(currentSettings);
            updateBtns(); 
            showToast("Đã hoàn tác!", "info");
        }
    });

    btnRedo.addEventListener("click", () => {
        const s = invoker.redo(); 
        if(s){ 
            const parts = s.mode.split('-');
            currentSettings = { ...s, theme: parts[0], navMode: parts[1] };
            applyToUI(currentSettings);
            syncInputs(currentSettings);
            updateBtns(); 
            showToast("Đã làm lại!", "info");
        }
    });

    // 6. CÁC LOGIC KHÁC (Progress, Bookmark, Comment...)
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

    document.getElementById("btn-bookmark").addEventListener("click", async () => {
        try {
            const res = await fetch(`${BACKEND_URL}/api/Bookmarks`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: CURRENT_USER_ID, storyId: CURRENT_STORY_ID, chapterId: CURRENT_CHAPTER_ID, scrollPosition: Math.round(window.scrollY) })
            });
            if (res.ok) {
                showToast("Đã bookmark thành công!", "success");
            }
        } catch(e) { showToast("Lỗi kết nối!", "error"); }
    });

    document.getElementById("btn-back").addEventListener("click", () => {
        window.location.href = `story-detail.html?storyId=${CURRENT_STORY_ID}`;
    });

    document.getElementById("btn-prev-page").addEventListener("click", () => readerContext.prevPage());
    document.getElementById("btn-next-page").addEventListener("click", () => readerContext.nextPage());

    const loadComments = async () => {
        const res = await fetch(`${BACKEND_URL}/api/Comments/Story/${CURRENT_STORY_ID}`);
        if(!res.ok) return;
        const comments = await res.json();
        document.getElementById("comment-list").innerHTML = comments.map(c => `<p><b>User ${c.userId}:</b> ${c.content}</p>`).join('');
    };
    document.getElementById("btn-submit-comment").addEventListener("click", async () => {
        const text = document.getElementById("comment-text").value;
        if(!text) return showToast("Vui lòng nhập bình luận!", "info");
        try {
            const res = await fetch(`${BACKEND_URL}/api/Comments`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: CURRENT_USER_ID, storyId: CURRENT_STORY_ID, content: text })
            });
            if (res.ok) {
                showToast("Gửi bình luận thành công!", "success");
                document.getElementById("comment-text").value = "";
                loadComments();
            }
        } catch (e) { showToast("Lỗi gửi bình luận!", "error"); }
    });
    loadComments();

    const observer = new NotificationObserver("noti-badge");
    observer.connect(BACKEND_URL);
});