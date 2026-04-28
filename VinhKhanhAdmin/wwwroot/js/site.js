// VinhKhanh Admin - site.js
(() => {
    // Auto-dismiss flash alerts after 4s
    document.querySelectorAll('[data-auto-dismiss]').forEach(el => {
        setTimeout(() => {
            el.style.transition = 'all 0.4s';
            el.style.opacity = '0';
            el.style.marginTop = `-${el.offsetHeight + 16}px`;
            setTimeout(() => el.remove(), 400);
        }, 4000);
    });
})();
