function parseJwt(token) {
    if (!token) return null;
    try {
        // handle base64url
        const payload = token.split('.')[1]
        const b64 = payload.replace(/-/g, '+').replace(/_/g, '/')
        const json = decodeURIComponent(atob(b64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
        }).join(''))
        return JSON.parse(json)
    } catch (e) {
        return null
    }
}

function checkLogin() {
    const token = localStorage.getItem('token')
    if (!token) return

    const user = parseJwt(token) || {}

    // account button (fallback to .login-btn)
    const accountBtn = document.querySelector('.account-btn') || document.querySelector('.login-btn')
    if (accountBtn) {
        const headerUser = user['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
        accountBtn.innerText = '👤 ' + (user.name || user.email || headerUser || 'User')
    }

    // show admin menu if role present in token or stored separately
    const isAdmin = (user.role === 'Admin') || (localStorage.getItem('role') === 'Admin')
    const adminEl = document.getElementById('adminMenu')
    if (adminEl) adminEl.style.display = isAdmin ? 'block' : 'none'

    // show account menu button if any
    const accountMenu = document.getElementById('accountMenu')
    if (accountMenu && accountBtn) accountMenu.style.display = 'block'
}

function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('role')
    const accountMenu = document.getElementById('accountMenu')
    if (accountMenu) accountMenu.style.display = 'none'
    location.reload()
}

checkLogin()

function toggleAccount() {
    const menu = document.getElementById('accountMenu')
    if (!menu) return
    menu.classList.toggle('show')
}

