class NightScrollStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.add('theme-night');
        container.classList.remove('mode-flip-parent');
        const content = document.getElementById('chapter-content');
        if (content) {
            content.classList.remove('mode-flip');
            content.scrollLeft = 0;
        }
        container.style.fontFamily = "Arial, sans-serif";
        this.recalculateHeight(container);
        console.log("Applied: Night Scroll Strategy");
    }
}
