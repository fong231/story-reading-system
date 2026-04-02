// Interface ảo
class IReadingStrategy {
    apply(container) { throw new Error("Method apply() must be implemented."); }
    // recalculateHeight mặc định dùng cho Scroll: xóa toàn bộ inline styles của Flip
    recalculateHeight(_container) {
        const content = document.getElementById('chapter-content');
        if (content) {
            content.style.height = '';
            content.style.overflowX = '';
            content.style.overflowY = '';
            content.style.columnWidth = '';  // xóa giá trị px do Flip set
            content.scrollLeft = 0;
        }
    }
}

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
        console.log("Applied: Day Scroll Strategy");
    }
}

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
        console.log("Applied: Day Flip Strategy");
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

class NightFlipStrategy extends IReadingStrategy {
    apply(container) {
        document.body.classList.add('theme-night');
        container.classList.add('mode-flip-parent');
        const content = document.getElementById('chapter-content');
        if (content) {
            content.classList.add('mode-flip');
            content.scrollLeft = 0;
            container.style.fontFamily = "Georgia, serif";
            this.recalculateHeight(container);
        }
        console.log("Applied: Night Flip Strategy");
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