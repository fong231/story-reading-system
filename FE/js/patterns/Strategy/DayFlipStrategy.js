class DayFlipStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.remove('theme-night');
        container.classList.add('mode-flip-parent');
        const content = document.getElementById('chapter-content');
        if (content) {
            content.classList.add('mode-flip');
            content.scrollLeft = 0;
            container.style.fontFamily = "Georgia, serif";
            this.recalculateHeight(container);
        }
        console.log("[Strategy Pattern] Apply Day Flip Strategy");
    }
    recalculateHeight(_container) {
        const title = document.getElementById('chapter-title');
        const content = document.getElementById('chapter-content');
        if (!content || !title) return;

        // Xóa inline overflow của Scroll mode để CSS .mode-flip có hiệu lực
        content.style.overflowX = '';
        content.style.overflowY = '';

        const computedStyle = window.getComputedStyle(content);
        const rowHeight = parseFloat(computedStyle.lineHeight) || 27;

        const availableHeight = window.innerHeight * 0.7 - 80;
        const titleHeight = title.offsetHeight + parseInt(window.getComputedStyle(title).marginBottom || 0);

        const targetContentHeight = availableHeight - titleHeight;
        const rows = Math.floor(targetContentHeight / rowHeight);
        const finalContentHeight = rows * rowHeight;

        content.style.height = finalContentHeight + 'px';
        content.style.columnWidth = content.clientWidth + 'px';
    }
}
