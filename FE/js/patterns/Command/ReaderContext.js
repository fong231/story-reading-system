class ReaderContext {
    constructor(container) { this.container = container; this.strategy = null; }
    setStrategy(strategy) { this.strategy = strategy; this.strategy.apply(this.container); }

    setReadingMode(theme, nav) {
        const modeString = `${theme}-${nav}`;
        switch (modeString) {
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
            const gap = 40;
            const step = width + gap;

            const currentPage = Math.round(content.scrollLeft / step);
            const targetScroll = (currentPage + 1) * step;

            content.scrollTo({
                left: targetScroll,
                behavior: 'smooth'
            });
        }
    }

    prevPage() {
        const content = document.getElementById('chapter-content');
        if (content && content.classList.contains('mode-flip')) {
            const width = content.clientWidth;
            const gap = 40;
            const step = width + gap;

            const currentPage = Math.round(content.scrollLeft / step);
            const targetScroll = (currentPage - 1) * step;

            content.scrollTo({
                left: Math.max(0, targetScroll),
                behavior: 'smooth'
            });
        }
    }
}
