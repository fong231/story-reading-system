// js/app.js
const BACKEND_URL = "https://localhost:5001"; // Đổi port cho khớp
const CURRENT_USER_ID = 1; // Fake UserID cho FE
const urlParams = new URLSearchParams(window.location.search);
const CURRENT_STORY_ID = urlParams.get('storyId') || 1; 
const CURRENT_CHAPTER_ID = 1;

// --- Khởi tạo Context cho Strategy Pattern ---
class ReaderContext {
    constructor(container) {
        this.container = container;
        this.strategy = null;
    }
    setStrategy(strategy) {
        this.strategy = strategy;
        this.strategy.apply(this.container);
    }
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
    const container = document.getElementById("reader-container");
    const selectMode = document.getElementById("reading-mode-select");
    const btnUndo = document.getElementById("btn-undo");
    const btnRedo = document.getElementById("btn-redo");

    const readerContext = new ReaderContext(container);
    const invoker = new SettingsInvoker();

    // 1. TẢI CẤU HÌNH GIAO DIỆN (Nếu có từ API)
    let currentMode = 'day-scroll';
    try {
        const modeRes = await fetch(`${BACKEND_URL}/api/Reading/Mode/${CURRENT_USER_ID}`);
        if(modeRes.ok) {
            const modeData = await modeRes.json();
            // Map từ BE (Day/Night, Scroll/Flip) về modeString của FE
            currentMode = `${modeData.theme.toLowerCase()}-${modeData.navigationMode.toLowerCase()}`;
            selectMode.value = currentMode;
        }
    } catch(e) { console.warn("Dùng mode mặc định"); }
    
    readerContext.setMode(currentMode);

    // 2. LẤY TIẾN TRÌNH ĐỌC CŨ (Singleton Pattern BE)
    try {
        const progRes = await fetch(`${BACKEND_URL}/api/Reading/Progress/${CURRENT_USER_ID}`);
        if(progRes.ok) {
            const progData = await progRes.json();
            console.log("Tiến trình đọc cũ:", progData);
            // Cuộn trang tới vị trí đọc lần trước
            if(progData.lastReadPosition > 0) {
                window.scrollTo({ top: progData.lastReadPosition, behavior: 'smooth' });
            }
        }
    } catch(e) { console.error("Không lấy được tiến trình đọc"); }

    // 3. XỬ LÝ LƯU TIẾN TRÌNH KHI CUỘN TRANG (Throttling để tránh gọi API liên tục)
    let scrollTimeout;
    window.addEventListener("scroll", () => {
        if(scrollTimeout) clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(() => {
            const position = Math.round(window.scrollY);
            fetch(`${BACKEND_URL}/api/Reading/Progress`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    userId: CURRENT_USER_ID,
                    storyId: CURRENT_STORY_ID,
                    chapterId: CURRENT_CHAPTER_ID,
                    position: position
                })
            }).then(() => console.log("Đã lưu tiến trình ở vị trí:", position));
        }, 2000); // 2 giây sau khi ngưng cuộn mới lưu
    });

    // 4. CÁC NÚT ĐIỀU KHIỂN MODE (Command Pattern)
    const updateButtons = () => {
        btnUndo.disabled = !invoker.canUndo();
        btnRedo.disabled = !invoker.canRedo();
    };

    selectMode.addEventListener("change", (e) => {
        const newMode = e.target.value;
        const command = new ChangeReadingModeCommand(readerContext, newMode, currentMode);
        invoker.executeCommand(command);
        currentMode = newMode;
        updateButtons();
    });

    btnUndo.addEventListener("click", () => {
        const restored = invoker.undo();
        if (restored) { currentMode = restored; selectMode.value = currentMode; updateButtons(); }
    });

    btnRedo.addEventListener("click", () => {
        const restored = invoker.redo();
        if (restored) { currentMode = restored; selectMode.value = currentMode; updateButtons(); }
    });

    // 5. SIGNALR OBSERVER PATTERN
    const notificationObserver = new NotificationObserver("noti-badge");
    notificationObserver.connect(BACKEND_URL);
    document.querySelector(".notification").addEventListener("click", () => notificationObserver.markAsRead());

    // 6. GỬI CẤU HÌNH CUỐI CÙNG VỀ BE KHI ĐÓNG TRANG
    window.addEventListener("beforeunload", () => {
        const theme = currentMode.includes('day') ? 'Day' : 'Night';
        const nav = currentMode.includes('scroll') ? 'Scroll' : 'Flip';
        
        const payload = JSON.stringify({
            userId: CURRENT_USER_ID,
            theme: theme,
            navigationMode: nav,
            fontSize: 16,
            fontFamily: "Arial",
            lineHeight: 1.5
        });

        // sendBeacon giúp gửi API ngay cả khi tab đã đóng
        navigator.sendBeacon(`${BACKEND_URL}/api/Reading/Mode`, new Blob([payload], { type: 'application/json' }));
    });
});