/**
 * Utility functions for the StoryReader frontend
 */

function showToast(message, type = 'info') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <span>${message}</span>
        <span style="margin-left: 15px; cursor: pointer; font-weight: bold;" onclick="this.parentElement.remove()">&times;</span>
    `;

    container.appendChild(toast);

    // Tự động xóa sau 3 giây
    setTimeout(() => {
        if (toast.parentElement) {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(100%)';
            toast.style.transition = 'all 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        }
    }, 3000);
}

// Chặn alert truyền thống và chuyển sang toast
// window.alert = (msg) => showToast(msg, 'info'); 
// Không nên ghi đè hoàn toàn để tránh side-effect, ta sẽ gọi showToast thủ công.