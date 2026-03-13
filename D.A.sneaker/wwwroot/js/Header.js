function openCart() {
    window.location.href = "cart.html";
}
function toggleMenu() {
    document.getElementById("sidebar").classList.toggle("show")
    document.getElementById("overlay").classList.toggle("show")
    document.querySelector(".hamburger").classList.toggle("active")
}