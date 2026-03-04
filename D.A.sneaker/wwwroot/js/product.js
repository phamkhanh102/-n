// lấy id từ URL
const params = new URLSearchParams(window.location.search);
const id = params.get("id");

async function loadProduct() {

    if (!id) {
        document.getElementById("product").innerHTML = "<h2>Không tìm thấy sản phẩm</h2>";
        return;
    }

    const r = await fetch("/api/products");
    const data = await r.json();

    const p = data.find(x => String(x.Id) === String(id));

    if (!p) {
        document.getElementById("product").innerHTML = "<h2>Sản phẩm không tồn tại</h2>";
        return;
    }

    renderProduct(p);
}

function renderProduct(p) {
    document.getElementById("product").innerHTML = `
    <div class="product-detail">
        <img class="detail-img" src="${p.ImageUrl || 'https://via.placeholder.com/400'}">

        <div class="detail-info">
            <h1>${p.Name}</h1>
            <h3>${p.Brand || ""}</h3>
            <div class="detail-price">${(p.Price || 0).toLocaleString()} đ</div>

            <button onclick='addToCart(${JSON.stringify({
        id: p.Id,
        name: p.Name,
        price: p.Price,
        image: p.ImageUrl
    })})'>
                Thêm vào giỏ
            </button>
        </div>
    </div>
    `;
}

function addToCart(product) {

    let cart = JSON.parse(localStorage.getItem("cart") || "[]");

    const exist = cart.find(x => x.id === product.id);

    if (exist) exist.qty++;
    else cart.push({ ...product, qty: 1 });

    localStorage.setItem("cart", JSON.stringify(cart));

    alert("Đã thêm vào giỏ 🛒");
}

loadProduct();

const id = new URLSearchParams(location.search).get("id");

fetch(`/api/products/${id}`)
    .then(r => r.json())
    .then(data => {

        document.querySelector(".title").innerText = data.product.name;
        document.querySelector(".desc p").innerText = data.product.description;

        // ảnh chính
        document.getElementById("mainImg").src = data.product.mainImageUrl;

        // gallery
        const thumbs = document.querySelector(".thumbs");
        thumbs.innerHTML = data.images.map(i =>
            `<img class="thumb" src="${i.imageUrl}">`
        ).join("");

        // size
        const sizeBox = document.getElementById("sizes");
        sizeBox.innerHTML = data.variants.map(v =>
            `<div class="size" data-id="${v.variantId}">
            ${v.sizeValue} (${v.stock})
        </div>`
        ).join("");

    });