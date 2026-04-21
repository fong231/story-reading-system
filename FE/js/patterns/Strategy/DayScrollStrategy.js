class DayScrollStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.remove('theme-night');
        container.classList.remove('mode-flip-parent');
        const content = document.getElementById('chapter-content');
        if (content) {
            content.classList.remove('mode-flip');
            content.scrollLeft = 0;
        }
        container.style.fontFamily = "Arial, sans-serif";
        this.recalculateHeight(container);
        console.log("[Strategy Pattern] Apply Day Scroll Strategy");
    }
}
