// ── Global SVG placeholder generator (replaces via.placeholder.com) ──
// Usage: _placeholderSvg(300, 200, 'Sneaker') → data:image/svg+xml,... 
window._placeholderSvg = function(w, h, text) {
  const t = encodeURIComponent(text || '');
  return `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='${w}' height='${h}'%3E%3Crect width='${w}' height='${h}' fill='%23f3f4f6'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%239ca3af' font-family='sans-serif' font-size='${Math.max(12, Math.min(18, w/15))}'%3E${t}%3C/text%3E%3C/svg%3E`;
};
var _placeholderSvg = window._placeholderSvg;

; (function applyThemeEarly() {
  const isDark = localStorage.getItem('spark-dark') === 'true';
  if (isDark) {
    document.documentElement.classList.add('dark');
    document.body && document.body.classList.add('dark');
  }
})();

(function () {
  // Inject Inter font globally if not already loaded
  if (!document.getElementById('spark-inter-font')) {
    const fLink = document.createElement('link');
    fLink.id   = 'spark-inter-font';
    fLink.rel  = 'stylesheet';
    fLink.href = 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Barlow+Condensed:wght@600;800;900&display=swap';
    document.head.appendChild(fLink);
    // Apply font to body
    const fStyle = document.createElement('style');
    fStyle.id = 'spark-font-override';
    fStyle.textContent = 'body, button, input, select, textarea { font-family: "Inter", "Barlow", sans-serif !important; }';
    document.head.appendChild(fStyle);
  }

  const headerHTML = `
<header class="header">

<!-- TOP BAR -->
<div class="topbar">
   🚚 Free ship toàn quốc &nbsp;|&nbsp; 🔥 Sale up to 50% &nbsp;|&nbsp; 💳 Thanh toán dễ dàng &amp; an toàn
</div>

<!-- MAIN ROW -->
<div class="header-main">

  <div style="display:flex;align-items:center;gap:14px">
    <button id="hamburgerBtn" class="hamburger-btn" aria-label="Open menu" onclick="toggleMenu()">
      <svg viewBox="0 0 24 24" width="22" height="22" fill="currentColor"><path d="M3 6h18v2H3V6zm0 5h18v2H3v-2zm0 5h18v2H3v-2z"/></svg>
    </button>
    <a href="/page/shop.html" class="logo">SPARK</a>
  </div>

  <div class="search">
    <select>
      <option>Tất cả danh mục</option>
      <option>Thể thao</option>
      <option>streetwear</option>
      <option>Classic</option>
      <option>Luxury</option>
    </select>
    <div style="width:1px;height:20px;background:var(--border,#ddd)"></div>
    <input id="searchInput" placeholder="Tìm kiếm sản phẩm..." class="search-input">
    <button class="search-btn" aria-label="Tìm kiếm" onclick="doSearch()">
      <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35" stroke-linecap="round"/></svg>
    </button>
  </div>

  <div class="header-actions">
  <div class="hdr-icon-group hdr-account-wrap" id="accountWrap">
      <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
      <span class="hdr-icon-label" id="accountLabel">Tài khoản</span>
      <div class="hdr-account-dropdown" id="accountDropdown">
        <div class="hdr-account-dropdown-inner">
          <a href="/page/account.html" class="hdr-dd-item">
            <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            Tài khoản
          </a>
          <a href="/page/orders.html" class="hdr-dd-item">
            <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 01-8 0"/></svg>
            Đơn hàng
          </a>
          <a href="/page/account.html#password" class="hdr-dd-item">
            <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>
            Đổi mật khẩu
          </a>
          <div class="hdr-dd-divider" id="hdrLogoutDivider" style="display:none"></div>
          <button class="hdr-dd-item hdr-dd-logout" id="hdrLogoutBtn" onclick="logout()" style="display:none">
            <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
            Đăng xuất
          </button>
        </div>
      </div>
    </div>
    <div class="hdr-icon-group" onclick="location.href='/page/wishlist.html'" style="position:relative">
      <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 00-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 00-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 000-7.78z"/></svg>
      <span id="wishlistCount" style="position:absolute;top:-6px;right:-8px;background:#ef4444;color:white;font-size:10px;padding:2px 5px;border-radius:999px;font-weight:700;min-width:18px;text-align:center;display:none">0</span>
      <span class="hdr-icon-label">Yêu thích</span>
    </div>
    <a href="/page/cart.html" class="cart-link hdr-icon-group" style="text-decoration:none;color:inherit">
      <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 01-8 0"/></svg>
      <span id="cartCount" style="position:absolute;top:-6px;right:-8px;background:#ef4444;color:white;font-size:10px;padding:2px 5px;border-radius:999px;font-weight:700;min-width:18px;text-align:center;display:none">0</span>
      <span class="hdr-icon-label">Giỏ hàng</span>
    </a>
    <!-- THEME TOGGLE IN HEADER -->
    <button id="sparkThemeBtn" class="hdr-icon-group" title="Chuyển sáng/tối" aria-label="Chuyển giao diện"
      style="background:none;border:none;cursor:pointer;padding:4px;color:inherit;transition:transform .3s">
      <span id="sparkThemeIcon" style="font-size:20px;line-height:1">🌙</span>
      <span class="hdr-icon-label">Giao diện</span>
    </button>
  </div>

</div>

<!-- NAV -->
<div class="nav">
  <a href="/page/shop.html" class="nav-link">TRANG CHỦ</a>
  <a href="/page/products.html" class="nav-link">SẢN PHẨM</a>
  <a href="/page/brand.html" class="nav-link">THƯƠNG HIỆU</a>
  <a href="/page/about.html" class="nav-link">VỀ CHÚNG TÔI</a>
  <a href="/page/blog.html" class="nav-link">BLOG</a>
  <a href="/page/support.html" class="nav-link">HỖ TRỢ</a>
  <a href="/page/products.html" class="nav-link nav-sale">SALE</a>
</div>

</header>
`;

  // ── Account dropdown CSS ─────────────────────────────────
  if (!document.getElementById('spark-account-dd-css')) {
    const s = document.createElement('style');
    s.id = 'spark-account-dd-css';
    s.textContent = `
      .hdr-account-wrap { position: relative; user-select: none; cursor: pointer; }
      .hdr-account-dropdown {
        display: none; position: absolute; top: 100%; left: 50%;
        transform: translateX(-50%);
        padding-top: 10px;
        min-width: 210px; z-index: 2000;
      }
      .hdr-account-dropdown-inner {
        background: #fff; border: 1px solid #e5e7eb;
        border-radius: 12px; box-shadow: 0 12px 32px rgba(0,0,0,.13);
        padding: 6px; animation: ddFadeIn .18s ease;
      }
      .hdr-account-wrap:hover .hdr-account-dropdown { display: block; }
      @keyframes ddFadeIn { from { opacity:0; transform: translateY(-6px); } to { opacity:1; transform: translateY(0); } }
      .hdr-dd-item {
        display: flex; align-items: center; gap: 9px; width: 100%; padding: 10px 13px;
        border-radius: 8px; font-size: 13px; font-weight: 600; color: #374151;
        text-decoration: none; border: none; background: transparent; cursor: pointer;
        transition: background .15s, color .15s; white-space: nowrap; text-align: left;
      }
      .hdr-dd-item:hover { background: #f3f4f6; color: #111; }
      .hdr-dd-item svg { flex-shrink: 0; color: #6b7280; }
      .hdr-dd-item:hover svg { color: #111; }
      .hdr-dd-logout { color: #ef4444 !important; }
      .hdr-dd-logout:hover { background: #fef2f2 !important; }
      .hdr-dd-logout svg { color: #ef4444 !important; }
      .hdr-dd-divider { height: 1px; background: #f0f0f0; margin: 4px 6px; }
      body.dark .hdr-account-dropdown-inner, .dark .hdr-account-dropdown-inner {
        background: #1e293b; border-color: #334155; box-shadow: 0 12px 32px rgba(0,0,0,.5);
      }
      body.dark .hdr-dd-item, .dark .hdr-dd-item { color: #cbd5e1; }
      body.dark .hdr-dd-item:hover, .dark .hdr-dd-item:hover { background: #334155; color: #f1f5f9; }
      body.dark .hdr-dd-item:hover svg, .dark .hdr-dd-item:hover svg { color: #f1f5f9; }
      body.dark .hdr-dd-divider, .dark .hdr-dd-divider { background: #334155; }
      body.dark .hdr-dd-logout, .dark .hdr-dd-logout { color: #f87171 !important; }
      body.dark .hdr-dd-logout:hover, .dark .hdr-dd-logout:hover { background: rgba(239,68,68,.15) !important; }
    `;
    document.head.appendChild(s);
  }

  // Inject into pages that use id="header" and id="footer"
  const hdr = document.getElementById("header") || document.getElementById("site-header");
  const ftr = document.getElementById("footer") || document.getElementById("site-footer");
  if (hdr) hdr.innerHTML = headerHTML;

  // ── Show/hide logout based on login state (runs AFTER innerHTML set) ─
  (function initAccountDropdown() {
    const token = localStorage.getItem('token');
    const logoutBtn = document.getElementById('hdrLogoutBtn');
    const logoutDivider = document.getElementById('hdrLogoutDivider');
    const accountLabel = document.getElementById('accountLabel');
    if (token) {
      if (logoutBtn) logoutBtn.style.display = 'flex';
      if (logoutDivider) logoutDivider.style.display = 'block';
      const name = localStorage.getItem('name') || localStorage.getItem('email') || '';
      if (accountLabel && name) {
        const shortName = name.split(' ').pop() || 'Tài khoản';
        accountLabel.textContent = shortName.length > 12 ? 'Tài khoản' : shortName;
      }
    }
  })();

  // ── Wishlist count badge ─────────────────────────────────
  (async function updateWishlistCount() {
    const token = localStorage.getItem('token');
    const badge = document.getElementById('wishlistCount');
    if (!badge) return;

    // Show cached count immediately
    const cached = parseInt(localStorage.getItem('wishlistCount') || '0', 10);
    if (cached > 0) { badge.textContent = cached; badge.style.display = 'inline-flex'; }

    if (!token) {
      localStorage.setItem('wishlistCount', '0');
      badge.style.display = 'none';
      return;
    }
    try {
      const res = await fetch('/api/wishlist', { headers: { 'Authorization': `Bearer ${token}` } });
      if (!res.ok) return;
      const list = await res.json();
      const count = Array.isArray(list) ? list.length : 0;
      localStorage.setItem('wishlistCount', count);
      if (count > 0) { badge.textContent = count; badge.style.display = 'inline-flex'; }
      else badge.style.display = 'none';
    } catch {}
  })();

  // Footer: only inject generic footer if page explicitly has a #footer div AND it's empty
  if (ftr && ftr.innerHTML.trim() === '') {
    ftr.innerHTML = `
<footer style="background:#111;color:#ccc;padding:40px 20px;margin-top:60px">
  <div style="max-width:1200px;margin:0 auto;display:grid;grid-template-columns:2fr 1fr 1fr 1.5fr;gap:36px">
    <div>
      <div style="font-size:28px;font-weight:900;color:#fff;margin-bottom:12px;font-family:'Barlow Condensed',sans-serif">SPARK</div>
      <p style="font-size:13px;color:#888;line-height:1.7">Nơi bạn tìm thấy đôi giày phù hợp nhất.</p>
    </div>
    <div>
      <h4 style="color:#fff;font-size:13px;margin-bottom:14px;text-transform:uppercase">Cửa hàng</h4>
      <a href="/page/shop.html" style="display:block;color:#888;font-size:13px;margin-bottom:8px;text-decoration:none">Sneaker</a>
      <a href="/page/cart.html" style="display:block;color:#888;font-size:13px;margin-bottom:8px;text-decoration:none">Giỏ hàng</a>
    </div>
    <div>
      <h4 style="color:#fff;font-size:13px;margin-bottom:14px;text-transform:uppercase">Hỗ trợ</h4>
      <a style="display:block;color:#888;font-size:13px;margin-bottom:8px;text-decoration:none">Chính sách</a>
      <a style="display:block;color:#888;font-size:13px;margin-bottom:8px;text-decoration:none">Liên hệ</a>
    </div>
    <div>
      <h4 style="color:#fff;font-size:13px;margin-bottom:10px;text-transform:uppercase">Nhận ưu đãi</h4>
      <div style="display:flex;gap:8px;margin-top:10px">
        <input placeholder="Email" style="flex:1;padding:10px;background:#222;border:1px solid #333;border-radius:6px;color:#fff;font-size:13px;outline:none">
        <button style="background:#facc15;color:#111;border:none;padding:10px 14px;border-radius:6px;font-weight:700;cursor:pointer;font-size:12px">GỬI</button>
      </div>
    </div>
  </div>
  <div style="border-top:1px solid #222;margin-top:36px;padding-top:18px;text-align:center;font-size:12px;color:#555">
    © 2026 SPARK. Tất cả quyền được bảo lưu.
  </div>
</footer>`;
  }

  // ── Attach header theme toggle listener (after header injection) ────
  const hdrThemeBtn = document.getElementById('sparkThemeBtn');
  const hdrThemeIcon = document.getElementById('sparkThemeIcon');

  if (hdrThemeBtn && hdrThemeIcon) {
    // Sync icon with current state
    hdrThemeIcon.textContent = document.documentElement.classList.contains('dark') ? '☀️' : '🌙';

    hdrThemeBtn.addEventListener('click', () => {
      const isDark = !document.documentElement.classList.contains('dark');
      document.documentElement.classList.toggle('dark', isDark);
      document.body.classList.toggle('dark', isDark);
      hdrThemeIcon.textContent = isDark ? '☀️' : '🌙';
      localStorage.setItem('spark-dark', isDark);
      hdrThemeBtn.style.transform = 'rotate(360deg) scale(1.1)';
      setTimeout(() => { hdrThemeBtn.style.transform = ''; }, 350);
    });
  }

})();

// SIDEBAR TOGGLE
function toggleMenu() {
  let sidebar = document.getElementById('sidebar');
  let overlay = document.getElementById('overlay');

  if (!sidebar) {
    // create sidebar element with close button and ARIA
    sidebar = document.createElement('div');
    sidebar.id = 'sidebar';
    sidebar.className = 'sidebar';
    sidebar.setAttribute('role', 'dialog');
    sidebar.setAttribute('aria-modal', 'true');
    sidebar.setAttribute('aria-hidden', 'true');
    sidebar.setAttribute('tabindex', '-1');
    sidebar.innerHTML = `
            <button class="close-sidebar" aria-label="Close menu">×</button>
            <nav class="sidebar-nav">
              <a href="/page/shop.html">🏠 Trang chủ</a>
              <a href="/page/products.html">👟 Sản phẩm</a>
              <a href="/page/cart.html">🛒 Giỏ hàng</a>
              <a href="/page/about.html">ℹ️ Về chúng tôi</a>
              <a href="/page/login.html">👤 Đăng nhập</a>
              <a href="/page/register.html">📝 Đăng ký</a>
            </nav>
        `;
    document.body.appendChild(sidebar);
    // close button handler
    sidebar.querySelector('.close-sidebar').addEventListener('click', closeSidebar);
  }

  if (!overlay) {
    overlay = document.createElement('div');
    overlay.id = 'overlay';
    overlay.className = 'overlay';
    overlay.setAttribute('aria-hidden', 'true');
    overlay.onclick = closeSidebar;
    document.body.appendChild(overlay);
  }

  // open or close
  openSidebar();

  function openSidebar() {
    const wasOpen = sidebar.classList.contains('show');
    if (wasOpen) {
      closeSidebar();
      return;
    }
    // store last focused element
    window.__lastFocus = document.activeElement;

    sidebar.classList.add('show');
    overlay.classList.add('show');
    sidebar.setAttribute('aria-hidden', 'false');
    overlay.setAttribute('aria-hidden', 'false');

    // focus trap: focus first focusable in sidebar
    const focusable = Array.from(sidebar.querySelectorAll('a,button,input,select,textarea,[tabindex]:not([tabindex="-1"])'))
      .filter(el => !el.hasAttribute('disabled'));
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (first) first.focus();

    function handleKey(e) {
      if (e.key === 'Escape') {
        e.preventDefault();
        closeSidebar();
        return;
      }
      if (e.key === 'Tab') {
        // trap
        const active = document.activeElement;
        if (e.shiftKey) {
          if (active === first) {
            e.preventDefault();
            last.focus();
          }
        } else {
          if (active === last) {
            e.preventDefault();
            first.focus();
          }
        }
      }
    }

    document.addEventListener('keydown', handleKey);
    // store handler to remove later
    sidebar.__handleKey = handleKey;
  }

  function closeSidebar() {
    if (!sidebar) return;
    sidebar.classList.remove('show');
    overlay.classList.remove('show');
    sidebar.setAttribute('aria-hidden', 'true');
    overlay.setAttribute('aria-hidden', 'true');
    // remove key handler
    if (sidebar.__handleKey) document.removeEventListener('keydown', sidebar.__handleKey);
    // restore focus
    const last = window.__lastFocus || document.getElementById('hamburgerBtn');
    if (last) last.focus();
  }
}

// ACCOUNT DROPDOWN
function toggleAccount() {
  const menu = document.getElementById("accountMenu");
  if (menu) menu.classList.toggle("show");
}

// LOGOUT
function logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('role');
  localStorage.removeItem('name');
  localStorage.removeItem('email');
  localStorage.removeItem('user');
  // Xóa lịch sử chat khi đổi tài khoản
  sessionStorage.removeItem('spark_chat_v2');
  location.href = '/page/login.html';
}

// CLOSE ACCOUNT WHEN CLICK OUTSIDE
document.addEventListener("click", function (e) {
  if (!e.target.closest(".account")) {
    const menu = document.getElementById("accountMenu");
    if (menu) menu.classList.remove("show");
  }
});

// SEARCH
function doSearch() {
  const q = (document.getElementById('searchInput') || {}).value || '';
  if (q.trim()) {
    location.href = '/page/products.html?q=' + encodeURIComponent(q.trim());
  }
}

// ============================================================
//  FLOATING WIDGETS: AI CHAT + THEME TOGGLE
// ============================================================

(function injectWidgets() {

  // Inject AI.css nếu chưa có
  if (!document.getElementById('spark-ai-css')) {
    const link = document.createElement('link');
    link.id = 'spark-ai-css';
    link.rel = 'stylesheet';
    link.href = '/css/AI.css';
    document.head.appendChild(link);
  }

  // ── Sản phẩm gợi ý – tải từ API thực ─────────────────────
  let SUGGEST_PRODUCTS = [];

  (async function loadSuggestProducts() {
    try {
      const res = await fetch('/api/products');
      if (!res.ok) return;
      const data = await res.json();
      const all = Array.isArray(data) ? data : [];
      SUGGEST_PRODUCTS = all.slice(0, 12).map(p => {
        // API dùng PropertyNamingPolicy = null => PascalCase
        // Hỗ trợ cả PascalCase và camelCase
        const id    = p.Id    ?? p.id    ?? 0;
        const name  = p.Name  ?? p.name  ?? '';
        const brand = p.Brand ?? p.brand ?? '';
        const price = p.Price ?? p.price ?? 0;
        const imgRaw = p.ImageUrl ?? p.imageUrl ?? '';
        const img = imgRaw
          ? (imgRaw.startsWith('http') ? imgRaw : '/' + imgRaw.replace(/^\//, ''))
          : 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=200&q=80';
        return { id, name, brand, price: Number(price).toLocaleString('vi-VN') + 'đ', img, url: `/page/product-detail.html?id=${id}` };
      }).filter(p => p.name); // bỏ qua sản phẩm không có tên
    } catch {}
  })();

  // ── AI Bot keyword responses ─── (brands đặt trước keywords chung) ──
  const BOT_RESPONSES = [
    { keys: ['xin chào', 'hello', 'hi', 'chào'], reply: 'Xin chào! 👋 Tôi là SPARK AI! Bạn đang tìm loại giày nào? Tôi có thể tìm theo hãng, mức giá hoặc mục đích sử dụng nhé.' },
    // ── Hãng giày (ưu tiên cao nhất) ──
    { keys: ['nike'], reply: '👟 Nike có tại SPARK – đang tìm sản phẩm Nike cho bạn...' },
    { keys: ['adidas'], reply: '🟢 Adidas có tại SPARK – đang tìm sản phẩm Adidas cho bạn...' },
    { keys: ['puma'], reply: '🐈 Puma có tại SPARK – đang tìm sản phẩm Puma cho bạn...' },
    { keys: ['new balance', 'newbalance'], reply: '🔵 New Balance có tại SPARK – đang tìm thông tin cho bạn...' },
    { keys: ['converse'], reply: '⭐ Converse có tại SPARK – đang tìm các mẫu cho bạn...' },
    { keys: ['vans'], reply: '🏄 Vans có tại SPARK – đang tìm các mẫu cho bạn...' },
    { keys: ['jordan', 'air jordan'], reply: '🏀 Air Jordan có tại SPARK – dòng giày huyền thoại của Nike!' },
    { keys: ['reebok'], reply: '💪 Reebok có tại SPARK – thương hiệu thể thao uy tín!' },
    // ── Thông tin chung ──
    { keys: ['size', 'cỡ', 'số giày'], reply: '📏 SPARK có đầy đủ size từ 36–43.\n• EU 36 = ≤22.5cm\n• EU 38 = ≤23.5cm\n• EU 40 = ≤25cm\n• EU 42 = ≤26.5cm\nBạn cần hỗ trợ thêm không?' },
    { keys: ['sale', 'ưu đãi', 'khuyến mãi', 'giảm giá', 'rẻ'], reply: '🔥 SPARK đang có nhiều ưu đãi:\n• Sale up to 50% cho giày Nike & Adidas\n• Mã SPARK10 giảm thêm 10%\n• Miễn phí ship đơn từ 500k\nBạn muốn xem sản phẩm nào?' },
    { keys: ['giá bao nhiêu', 'bao nhiêu tiền', 'bao nhiêu', 'giá cả'], reply: '💰 Giá giày tại SPARK từ 800.000đ – 5.000.000đ tùy hãng và dòng sản phẩm. Bạn có ngân sách khoảng bao nhiêu?' },
    { keys: ['ship', 'giao hàng', 'vận chuyển'], reply: '🚚 Chính sách giao hàng:\n• Miễn phí ship đơn ≥ 500.000đ\n• Giao 1–2 ngày tại HN & HCM\n• Giao 2–4 ngày tỉnh thành khác' },
    { keys: ['đổi', 'trả hàng', 'hoàn tiền', 'refund', 'return'], reply: '🔄 SPARK hỗ trợ đổi trả:\n• Đổi size miễn phí trong 30 ngày\n• Hoàn tiền 100% nếu hàng lỗi\n• Liên hệ hotline: 1800-1234' },
    { keys: ['đơn hàng', 'kiểm tra đơn', 'theo dõi đơn'], reply: '📦 Để kiểm tra đơn hàng, vui lòng đăng nhập tài khoản và vào mục "Đơn hàng". Bạn có tài khoản chưa?' },
    { keys: ['cảm ơn', 'thanks', 'thank you', 'camon'], reply: '😊 Rất vui được giúp bạn! Chúc bạn mua sắm vui vẻ tại SPARK! 👟' },
    { keys: ['bảo hành', 'warranty'], reply: '✅ Bảo hành tại SPARK:\n• 100% hàng chính hãng có tem\n• Bảo hành theo nhà sản xuất 6–12 tháng\n• Đổi mới trong 7 ngày nếu lỗi form' },
    { keys: ['thể thao', 'chạy bộ', 'running'], reply: '🏃 Giày thể thao/chạy bộ hot nhất:\n• Nike ZoomX – nhẹ, đàn hồi tốt\n• Adidas Ultraboost – êm chân\n• ASICS Gel – hỗ trợ khớp tốt' },
    { keys: ['streetwear', 'thời trang', 'casual'], reply: '😎 Giày thời trang streetwear:\n• Nike Air Force 1 – classic mọi thời đại\n• Adidas Stan Smith – tối giản\n• Converse Chuck Taylor – retro cá tính' },
  ];

  // Hàm match theo word-boundary để tránh "gia" khớp bên trong "giay"
  function matchKeys(query, keys) {
    const norm = t => t.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    const q = norm(query);
    const qPad = ' ' + q + ' '; // căn biên trái/phải bằng dấu cách
    return keys.some(k => {
      const nk = norm(k);
      // Khớp như từ riêng (bao quanh bởi khoảng trắng hoặc đầu/cuối chuỗi)
      return qPad.includes(' ' + nk + ' ') || q === nk;
    });
  }

  function getBotReply(text) {
    for (const r of BOT_RESPONSES) {
      if (matchKeys(text, r.keys)) return r.reply;
    }
    // Smart fallback: chỉ tìm khi name/brand đủ dài và có trong query
    const norm = t => t.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    const q = norm(text);
    const qPad = ' ' + q + ' ';
    if (SUGGEST_PRODUCTS.length && q.length >= 3) {
      const match = SUGGEST_PRODUCTS.find(p => {
        const n = norm(p.name  || '');
        const b = norm(p.brand || '');
        // Chỉ match khi tên/hãng không rỗng VÀ có trong query
        return (n.length > 2 && q.includes(n)) ||
               (b.length > 2 && q.includes(b));
      });
      if (match)
        return `👟 Tôi tìm thấy <b>${match.name}</b> – giá ${match.price}. Bạn muốn xem không?`;
    }
    // Sentinel: câu không khớp keyword nào → để sendMessage gọi Ollama
    return '__UNKNOWN__';
  }

  // ── Widget HTML ────────────────────────────────────────────
  const widgetHTML = `
<div class="spark-float-group" id="sparkFloatGroup">

  <button class="spark-ai-btn" id="sparkAiBtn" aria-label="Mở AI tư vấn" aria-expanded="false">
    <span class="ai-badge" id="aiBadge">1</span>
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
      <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
      <circle cx="9" cy="10" r="1" fill="currentColor"/>
      <circle cx="12" cy="10" r="1" fill="currentColor"/>
      <circle cx="15" cy="10" r="1" fill="currentColor"/>
    </svg>
  </button>
</div>

<!-- AI PANEL -->
<div class="spark-ai-panel" id="sparkAiPanel" role="dialog" aria-label="AI tư vấn SPARK" aria-hidden="true">

  <!-- HEADER -->
  <div class="ai-panel-header">
    <div class="ai-avatar">🤖</div>
    <div class="ai-header-info">
      <div class="ai-header-name">SPARK Trợ Lý AI</div>
      <div class="ai-header-status">
        <div class="ai-status-dot"></div>
        <span id="aiStatusText">Đang hoạt động</span>
      </div>
    </div>
    <div style="display:flex;gap:6px;align-items:center">
      <button id="aiClearBtn" title="Xóa lịch sử chat"
        style="background:rgba(255,255,255,.1);border:none;color:#fff;width:28px;height:28px;border-radius:50%;cursor:pointer;font-size:14px;display:flex;align-items:center;justify-content:center;transition:background .2s"
        onmouseover="this.style.background='rgba(255,255,255,.22)'" onmouseout="this.style.background='rgba(255,255,255,.1)'">
        🗑
      </button>
      <button class="ai-header-close" id="sparkAiClose" aria-label="Đóng chat">✕</button>
    </div>
  </div>

  <!-- QUICK ACTIONS -->
  <div class="ai-quick-actions">
    <button class="ai-quick-btn" onclick="sendQuick('Tư vấn chọn size')">📏 Chọn size</button>
    <button class="ai-quick-btn" onclick="sendQuick('Xem sale hôm nay')">🔥 Sale</button>
    <button class="ai-quick-btn" onclick="sendQuick('Chính sách đổi trả')">🔄 Đổi trả</button>
    <button class="ai-quick-btn" onclick="sendQuick('Kiểm tra đơn hàng')">📦 Đơn hàng</button>
  </div>

  <!-- MESSAGES -->
  <div class="ai-messages" id="aiMessages"></div>

  <!-- INPUT -->
  <div class="ai-input-row">
    <input type="text" id="aiInput" placeholder="Hỏi về Nike, Adidas, size giày..." maxlength="300" autocomplete="off">
    <button class="ai-send-btn" id="aiSendBtn" aria-label="Gửi">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
        <line x1="22" y1="2" x2="11" y2="13"/>
        <polygon points="22 2 15 22 11 13 2 9 22 2"/>
      </svg>
    </button>
  </div>

</div>`;


  // Inject vào cuối body
  const wrapDiv = document.createElement('div');
  wrapDiv.innerHTML = widgetHTML;
  document.body.appendChild(wrapDiv);

  // ── References ─────────────────────────────────────────────
  const panel    = document.getElementById('sparkAiPanel');
  const aiBtn    = document.getElementById('sparkAiBtn');
  const closeBtn = document.getElementById('sparkAiClose');
  const clearBtn = document.getElementById('aiClearBtn');
  const messages = document.getElementById('aiMessages');
  const input    = document.getElementById('aiInput');
  const sendBtn  = document.getElementById('aiSendBtn');
  const badge    = document.getElementById('aiBadge');

  let isOpen = false;

  // ════════════════════════════════════════════════════════
  //  CHAT HISTORY – sessionStorage (tự xóa khi đóng tab)
  // ════════════════════════════════════════════════════════
  const HISTORY_KEY = 'spark_chat_v2'; // v2: xuầt lý broken data cũ
  const MAX_HISTORY = 40;

  // Xóa key cũ (tránh load lại broken cards)
  try { localStorage.removeItem('spark_chat_history'); } catch {}

  function saveHistory() {
    const items = messages.querySelectorAll('.ai-msg[data-saveable]');
    const arr = Array.from(items).map(el => ({
      type: el.classList.contains('user') ? 'user' : 'bot',
      html: el.querySelector('.bubble')?.innerHTML || ''
    })).filter(x => !x.html.includes('undefined') && !x.html.includes('NaNđ'));
    try {
      sessionStorage.setItem(HISTORY_KEY, JSON.stringify(arr.slice(-MAX_HISTORY)));
    } catch(e) {}
  }

  function loadHistory() {
    try {
      const raw = sessionStorage.getItem(HISTORY_KEY);
      if (!raw) return false;
      const arr = JSON.parse(raw);
      if (!arr || !arr.length) return false;
      arr.forEach(item => renderMsg(item.html, item.type, false));
      messages.scrollTop = messages.scrollHeight;
      return true;
    } catch(e) { return false; }
  }

  function clearHistory() {
    sessionStorage.removeItem(HISTORY_KEY);
    messages.innerHTML = '';
    renderMsg('Xin chào! 👋 Tôi là <strong>SPARK AI</strong>, trợ lý mua sắm thông minh của bạn.<br>Tôi có thể giúp bạn tìm giày, tư vấn size, xem ưu đãi...', 'bot', false);
  }

  // renderMsg: render 1 bubble vào DOM, optionally save to history
  function renderMsg(html, type, doSave = true) {
    const div = document.createElement('div');
    div.className = `ai-msg ${type}`;
    div.setAttribute('data-saveable', '1');
    if (type === 'bot') {
      div.innerHTML = `<div class="bot-icon">🤖</div><div class="bubble">${html}</div>`;
    } else {
      div.innerHTML = `<div class="bubble">${html}</div>`;
    }
    messages.appendChild(div);
    messages.scrollTop = messages.scrollHeight;
    if (doSave) saveHistory();
    return div;
  }

  // ── Toggle chat ─────────────────────────────────────────────
  function openChat() {
    isOpen = true;
    panel.classList.add('show');
    aiBtn.classList.add('open');
    aiBtn.setAttribute('aria-expanded', 'true');
    panel.setAttribute('aria-hidden', 'false');
    badge.style.display = 'none';
    input.focus();
  }

  function closeChat() {
    isOpen = false;
    panel.classList.remove('show');
    aiBtn.classList.remove('open');
    aiBtn.setAttribute('aria-expanded', 'false');
    panel.setAttribute('aria-hidden', 'true');
  }

  aiBtn.addEventListener('click', () => isOpen ? closeChat() : openChat());
  closeBtn.addEventListener('click', closeChat);
  if (clearBtn) clearBtn.addEventListener('click', () => {
    if (confirm('Xóa toàn bộ lịch sử chat?')) clearHistory();
  });
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape' && isOpen) closeChat();
  });

  if (!loadHistory()) {
    renderMsg('Xin chào! 👋 Tôi là <strong>SPARK AI</strong>, trợ lý mua sắm thông minh của bạn.<br>Tôi có thể giúp bạn tìm giày, tư vấn size, xem ưu đãi...', 'bot', false);
  }

  function showTyping() {
    const div = document.createElement('div');
    div.className = 'ai-msg bot';
    div.id = 'aiTyping';
    div.innerHTML = `<div class="bot-icon">🤖</div><div class="bubble" style="padding:8px 14px"><div class="ai-typing"><span></span><span></span><span></span></div></div>`;
    messages.appendChild(div);
    messages.scrollTop = messages.scrollHeight;
  }

  function removeTyping() {
    const t = document.getElementById('aiTyping');
    if (t) t.remove();
  }

  // ── Detect brand từ câu hỏi ────────────────────────────────
  const BRANDS = ['nike','adidas','puma','converse','vans','jordan','reebok','new balance','asics','skechers','fila'];
  function detectBrand(text) {
    const norm = t => t.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    const q = ' ' + norm(text) + ' ';
    return BRANDS.find(b => q.includes(' ' + norm(b) + ' ')) || null;
  }

  // ── Render product cards (có filter theo brand) ────────────
  function showFilteredProducts(brand, apiData) {
    let items;

    if (apiData && apiData.length) {
      // Data từ Ollama API — normalize PascalCase/camelCase
      items = apiData.slice(0, 3).map(p => ({
        name:  p.Name  ?? p.name  ?? 'Sản phẩm',
        price: Number(p.Price ?? p.price ?? 0).toLocaleString('vi-VN') + 'đ',
        img:   (() => { const r = p.ImageUrl ?? p.imageUrl ?? ''; return r.startsWith('http') ? r : r ? '/' + r.replace(/^\//, '') : 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=200&q=80'; })(),
        url:   `/page/product-detail.html?id=${p.Id ?? p.id ?? ''}`
      }));
    } else if (SUGGEST_PRODUCTS.length) {
      // Lọc theo brand nếu có
      let pool = [...SUGGEST_PRODUCTS];
      if (brand) {
        const nb = brand.toLowerCase();
        const filtered = pool.filter(p =>
          (p.brand || '').toLowerCase().includes(nb) ||
          (p.name  || '').toLowerCase().includes(nb)
        );
        if (filtered.length > 0) pool = filtered; // dùng filtered nếu có kết quả
      }
      // Shuffle ngẫu nhiên, lấy 3
      pool.sort(() => Math.random() - 0.5);
      items = pool.slice(0, 3);
    } else {
      return; // DB chưa load, bỏ qua
    }

    const cardsHtml = `Đây là một số gợi ý dành cho bạn 👟<div class="ai-product-cards" style="margin-top:8px">${
      items.map(p => `
        <a class="ai-prod-card" href="${p.url}">
          <img src="${p.img}" alt="${p.name}" onerror="this.src='https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=200&q=80'">
          <div class="ai-prod-card-body">
            <div class="ai-prod-card-name">${p.name}</div>
            <div class="ai-prod-card-price">${p.price}</div>
          </div>
        </a>`).join('')
    }</div>`;
    renderMsg(cardsHtml, 'bot', true);
  }

  // ── Ollama async với AbortController ───────────────────────
  const statusEl = document.getElementById('aiStatusText');

  async function callOllamaAsync(text, signal) {
    const res = await fetch('/api/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(text),
      signal
    });
    if (!res.ok) throw new Error('HTTP ' + res.status);
    return res.json();
  }

  // ── HÀM GỢI Ý SẢN PHẨM: hiện typing "đang tìm..." rồi show cards ──
  function triggerProductSuggestion(brand, apiData) {
    // Bước 1: Sau 600ms hiện typing "đang tìm kiếm..."
    setTimeout(() => {
      showTyping();
      // Bước 2: Sau thêm 900ms xóa typing → hiện cards
      setTimeout(() => {
        removeTyping();
        showFilteredProducts(brand, apiData);
      }, 900);
    }, 600);
  }

  // ── sendMessage — logic chặt chẽ ──────────────────────────
  function sendMessage(text) {
    text = (text || '').trim();
    if (!text) return;

    renderMsg(text, 'user', true);
    input.value = '';
    input.disabled = true;
    sendBtn.disabled = true;

    const brand = detectBrand(text);

    // Các query cần hiện gợi ý sản phẩm
    const wantProducts = SUGGEST_PRODUCTS.length > 0 && (
      brand != null ||
      /giày|sản phẩm|tìm|recommend|shoe/i.test(text)
    );

    // ── BƯỚC 1: Keyword match → trả lời NGAY ──────────────
    const localReply = getBotReply(text);
    const isUnknown = localReply === '__UNKNOWN__';

    if (!isUnknown) {
      // Có keyword → trả lời ngay lập tức
      renderMsg(localReply.replace(/\n/g, '<br>'), 'bot', true);

      // Nếu cần gợi ý sản phẩm → giả lập "đang tìm kiếm..." rồi show
      if (wantProducts) triggerProductSuggestion(brand, null);

      input.disabled = false;
      sendBtn.disabled = false;
      input.focus();
      return;
    }

    // ── BƯỚC 2: Không match keyword → gọi Ollama 12s ─────
    showTyping();
    if (statusEl) statusEl.textContent = 'Đang suy nghĩ...';

    const controller = new AbortController();
    const timerId = setTimeout(() => controller.abort(), 12000);

    callOllamaAsync(text, controller.signal)
      .then(data => {
        clearTimeout(timerId);
        removeTyping();
        if (statusEl) statusEl.textContent = 'Đang hoạt động';

        if (data.type === 'products' && data.data?.length) {
          renderMsg(`Tôi tìm thấy <b>${data.data.length}</b> sản phẩm phù hợp.`, 'bot', true);
          triggerProductSuggestion(brand, data.data);
        } else {
          const reply = (data.message || '').trim();
          if (!reply || reply.toLowerCase().includes('ollama') || reply.includes('kết nối')) {
            renderMsg('Bạn có thể mô tả rõ hơn không? Ví dụ: "giày Nike chạy bộ", "Adidas size 42"...', 'bot', true);
          } else {
            renderMsg(reply.replace(/\n/g, '<br>'), 'bot', true);
          }
          if (wantProducts) triggerProductSuggestion(brand, null);
        }
      })
      .catch(err => {
        clearTimeout(timerId);
        removeTyping();
        if (statusEl) statusEl.textContent = 'Đang hoạt động';
        const msg = err.name === 'AbortError'
          ? 'Bạn có thể hỏi cụ thể hơn? Ví dụ: "giày Puma size 42", "Nike chạy bộ dưới 2 triệu"...'
          : 'Tôi chưa hiểu câu hỏi này. Hãy thử hỏi về: giày Nike, Puma, size, giá, giao hàng...';
        renderMsg(msg, 'bot', true);
      })
      .finally(() => {
        input.disabled = false;
        sendBtn.disabled = false;
        input.focus();
      });
  }

  function renderProductsText(arr) {
    if (!arr || !arr.length) return 'Không tìm thấy sản phẩm phù hợp.';
    return arr.slice(0, 3).map(p =>
      `<b>${p.Name || p.name}</b> – ${(p.Price || p.price || 0).toLocaleString('vi-VN')}đ`
    ).join('<br>');
  }

  window.sendQuick = function (text) {
    if (!isOpen) openChat();
    sendMessage(text);
  };

  sendBtn.addEventListener('click', () => sendMessage(input.value));
  input.addEventListener('keydown', e => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(input.value); }
  });

  // ── THEME TOGGLE ────────────────────────────────────────────
  // Theme is controlled by the header #sparkThemeBtn (injected in headerHTML above)
  // The floating theme button has been removed to avoid duplication.
  // Sync header icon on load:
  (function syncHeaderTheme() {
    const isDark = document.documentElement.classList.contains('dark');
    const icon = document.getElementById('sparkThemeIcon');
    if (icon) icon.textContent = isDark ? '☀️' : '🌙';
  })();

  // Update cart count on load
  const cartEl = document.getElementById('cartCount');
  if (cartEl) {
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    const total = cart.reduce((s, i) => s + (i.qty || 0), 0);
    cartEl.textContent = total;
    cartEl.style.display = total > 0 ? 'flex' : 'none';
  }

  // Search Enter key in header
  const searchInput = document.getElementById('searchInput');
  if (searchInput) {
    searchInput.addEventListener('keydown', e => {
      if (e.key === 'Enter') doSearch();
    });
  }

})();
