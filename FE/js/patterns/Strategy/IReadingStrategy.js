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
