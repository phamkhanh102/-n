function openCart() {
    window.location.href = "cart.html";
}

function toggleMenu() {
    const sidebar = document.getElementById("sidebar")
    const overlay = document.getElementById("overlay")
    const hamburger = document.querySelector(".hamburger")

    if (sidebar) sidebar.classList.toggle("show")
    if (overlay) overlay.classList.toggle("show")
    if (hamburger) hamburger.classList.toggle("active")
}

// Simple safe search helper: only run if elements exist
function initHeaderSearch(qInput, data, onRender) {
    if (!qInput || !data) return

    qInput.addEventListener('input', function () {
        const q = this.value.trim().toLowerCase()
        let filtered = data
        if (q) filtered = data.filter(p => (p.name || '').toLowerCase().includes(q))
        if (typeof onRender === 'function') onRender(filtered)
    })
}
