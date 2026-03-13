document.querySelectorAll("input[name=sortName]").forEach(radio => {

    radio.addEventListener("change", function () {

        const value = this.value;

        if (value === "az") {
            all.sort((a, b) => a.name.localeCompare(b.name));
        }

        if (value === "za") {
            all.sort((a, b) => b.name.localeCompare(a.name));
        }

        applyFilters();

    });

});
function renderProductCard(p) {

    return `

<div class="card">

<a href="product-detail.html?id=${p.id}">

<img class="product-img"
src="${p.imageUrl && p.imageUrl.startsWith('http') ? p.imageUrl : '/images/' + (p.imageUrl || '')}"
onerror="this.src='https://via.placeholder.com/300x200?text=No+Image'">

</a>

<div class="title">${p.name}</div>

<div class="brand">${p.brand || ''}</div>

<div class="price">${(p.price || 0).toLocaleString()} đ</div>

<div class="card-actions">

<button onclick="addToCartById(${p.id})">
🛒 Thêm
</button>

<a href="product-detail.html?id=${p.id}">
Xem
</a>

</div>

</div>

`;
}
function render(list) {

    document.getElementById("count").innerText = list.length + " sản phẩm";

    document.getElementById("grid").innerHTML =
        list.map(p => renderProductCard(p)).join("");

    updateCartUI();
}