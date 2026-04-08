/**
 * SINGLETON PATTERN - Reading Progress Manager (Frontend)
 * Đảm bảo chỉ có một instance duy nhất quản lý bộ đếm tiến trình đọc 
 * của người dùng trong suốt phiên làm việc (Session).
 */
(function() {
    class ReadingProgressManager {
        constructor() {
            if (ReadingProgressManager.instance) {
                return ReadingProgressManager.instance;
            }

            this.progress = {
                storyId: null,
                chapterId: null,
                percentage: 0,
                lastUpdated: null
            };

            ReadingProgressManager.instance = this;
            console.log("[Singleton FE] ReadingProgressManager initialized.");
        }

        static getInstance() {
            if (!ReadingProgressManager.instance) {
                ReadingProgressManager.instance = new ReadingProgressManager();
            }
            return ReadingProgressManager.instance;
        }

        update(storyId, chapterId, percentage) {
            this.progress.storyId = storyId;
            this.progress.chapterId = chapterId;
            this.progress.percentage = Math.round(percentage);
            this.progress.lastUpdated = new Date();
            
            // Cập nhật UI ngay lập tức
            this.updateUI();
        }

        updateUI() {
            const bar = document.getElementById('progress-bar');
            const text = document.getElementById('reading-percentage');
            if (bar) bar.style.width = this.progress.percentage + '%';
            if (text) text.innerText = this.progress.percentage + '%';
        }

        getProgress() {
            return this.progress;
        }
    }

    // Export ra Window để các script khác (non-module) dùng được
    window.ReadingProgressManager = ReadingProgressManager;
    window.readingManager = ReadingProgressManager.getInstance();
})();
