function toggleTheme() {

    const root = document.documentElement
    const btn = document.querySelector(".theme-btn")

    root.classList.toggle("dark")

    if (root.classList.contains("dark")) {
        localStorage.setItem("theme", "dark")
        if (btn) btn.innerHTML = "☀️"
    } else {
        localStorage.setItem("theme", "light")
        if (btn) btn.innerHTML = "🌙"
    }

}

function loadTheme() {

    const saved = localStorage.getItem("theme")

    // nếu user đã chọn theme
    if (saved) {
        document.documentElement.classList.toggle("dark", saved === "dark")
        return
    }

    // nếu chưa chọn → theo hệ điều hành
    if (window.matchMedia("(prefers-color-scheme: dark)").matches) {
        document.documentElement.classList.add("dark")
    }

}

loadTheme()