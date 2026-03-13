function checkLogin() {

    const token = localStorage.getItem("token");
    const sidebar = document.getElementById("sidebar");

    if (!token) return;

    const user = parseJwt(token);

    let adminMenu = "";
    const loginBtn = document.querySelector(".login-btn");

    if (token) {
        const token = localStorage.getItem("token");

        if (!token) return;

        const user = parseJwt(token);

        const loginBtn = document.querySelector(".account-btn");

        if (loginBtn) {
            if (loginBtn && user) {
                loginBtn.innerHTML = `👤 ${user.name ?? user.email ?? "User"}`;
            }
        }
        const headerUser =
            user["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];

        document.querySelector(".account-btn").innerHTML =
            "👤 " + (headerUser || "User");
    }
    // ⭐ PHÂN QUYỀN Ở ĐÂY
    if (user.role === "Admin") {
        adminMenu = `
                    <a href="admin.html">⚙️ Quản trị</a>
                `;
    }

}

function logout() {
    localStorage.removeItem("token");
    window.location.href = "shop.html";
}

checkLogin();
function toggleAccount() {

    const menu = document.getElementById("accountMenu");

    menu.style.display =
        menu.style.display === "flex" ? "none" : "flex";

}