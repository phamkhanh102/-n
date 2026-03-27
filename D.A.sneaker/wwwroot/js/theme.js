/**
 * theme.js – SPARK unified theme system
 * Key: 'spark-dark' (đồng nhất với layout.js)
 * Class: thêm vào cả html VÀ body
 */

const DARK_KEY = 'spark-dark';

function applyTheme(dark) {
    if (dark) {
        document.documentElement.classList.add('dark');
        document.body.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
        document.body.classList.remove('dark');
    }

    // Sync icon trong floating widget (layout.js inject)
    const iconEl = document.getElementById('sparkThemeIcon');
    if (iconEl) iconEl.textContent = dark ? '☀️' : '🌙';

    // Sync icon cũ (nếu trang nào còn dùng .theme-btn)
    const oldBtn = document.querySelector('.theme-btn');
    if (oldBtn) oldBtn.innerHTML = dark ? '☀️' : '🌙';

    // Sync sparkThemeBtn trong header (layout.js inject)
    const headerIcon = document.querySelector('#sparkThemeBtn #sparkThemeIcon');
    if (headerIcon) headerIcon.textContent = dark ? '☀️' : '🌙';
}

function toggleTheme() {
    const isDark = document.documentElement.classList.contains('dark');
    const next = !isDark;
    localStorage.setItem(DARK_KEY, next);
    applyTheme(next);
}

function loadTheme() {
    const saved = localStorage.getItem(DARK_KEY);

    if (saved !== null) {
        applyTheme(saved === 'true');
        return;
    }

    // Chưa chọn → theo hệ điều hành
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    applyTheme(prefersDark);
}

// Chạy ngay khi file được load (trước khi render)
loadTheme();

// Lắng nghe thay đổi OS theme
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
    // Chỉ theo OS nếu user chưa tự chọn
    if (localStorage.getItem(DARK_KEY) === null) {
        applyTheme(e.matches);
    }
});