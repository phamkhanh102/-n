let all = []

async function loadProducts() {

    const grid = document.getElementById("grid")

    grid.innerHTML = `
<div class="skeleton"></div>
<div class="skeleton"></div>
<div class="skeleton"></div>
<div class="skeleton"></div>
`

    try {

        const res = await fetch("/api/products")

        if (!res.ok) throw "API lỗi"

        const data = await res.json()

        all = data.map(p => ({

            id: p.Id,
            name: p.Name,
            brand: p.Brand,
            price: p.Price,
            image: p.ImageUrl

        }))

        renderProducts(all)

    }
    catch (e) {

        grid.innerHTML = `
<p style="padding:40px;text-align:center">
Không tải được sản phẩm
</p>
`

    }

}

function renderProducts(list) {

    const grid = document.getElementById("grid")

    grid.innerHTML = list.map(p => `

<div class="card">

<a href="product-detail.html?id=${p.id}">

<img class="product-img"
src="${p.image && p.image.startsWith('http')
            ? p.image
            : '/images/' + (p.image || 'no-image.png')}"
loading="lazy"
onerror="this.src='/images/no-image.png'">

</a>

<div class="title">${p.name}</div>

<div class="brand">${p.brand || ""}</div>

<div class="price">${(p.price || 0).toLocaleString()} đ</div>

<div class="card-actions">

<button onclick="addToCart(${p.id})">
🛒 Thêm
</button>

<a href="product-detail.html?id=${p.id}">
Xem
</a>

</div>

</div>

`).join("")

    updateCartUI()

}

function addToCart(id) {

    const product = all.find(p => p.id === id)

    if (!product) return

    let cart = JSON.parse(localStorage.getItem("cart") || "[]")

    const exist = cart.find(x => x.id === id)

    if (exist) {
        exist.qty++
    }
    else {
        cart.push({
            id: product.id,
            name: product.name,
            price: product.price,
            image: product.image,
            qty: 1
        })
    }

    localStorage.setItem("cart", JSON.stringify(cart))

    updateCartUI()

    alert("Đã thêm vào giỏ")

}

function updateCartUI() {

    const cart = JSON.parse(localStorage.getItem("cart") || "[]")

    let count = 0

    cart.forEach(i => count += i.qty)

    const el = document.getElementById("cartCount")

    if (el) el.innerText = count

}
let filters = {
    brand: "",
    priceMin: 0,
    priceMax: Infinity,
    size: "",
    sort: ""
}

// Guard: only attach filter listeners if the elements exist (products.html)
const _brandEl = document.getElementById("brandFilter");
const _priceEl = document.getElementById("priceFilter");
const _sizeEl  = document.getElementById("sizeFilter");
const _sortEl  = document.getElementById("sortFilter");

if (_brandEl) _brandEl.onchange = e => {
    filters.brand = e.target.value
    applyFilters()
}

if (_priceEl) _priceEl.onchange = e => {
    if (!e.target.value) {
        filters.priceMin = 0
        filters.priceMax = Infinity
    }
    else {
        const [min, max] = e.target.value.split("-")
        filters.priceMin = +min
        filters.priceMax = +max
    }
    applyFilters()
}

if (_sizeEl) _sizeEl.onchange = e => {
    filters.size = e.target.value
    applyFilters()
}

if (_sortEl) _sortEl.onchange = e => {
    filters.sort = e.target.value
    applyFilters()
}
function applyFilters() {

    let data = [...all]

    if (filters.brand) {

        data = data.filter(p => p.brand === filters.brand)

    }

    data = data.filter(p =>

        p.price >= filters.priceMin &&
        p.price <= filters.priceMax

    )

    if (filters.sort === "priceAsc")
        data.sort((a, b) => a.price - b.price)

    if (filters.sort === "priceDesc")
        data.sort((a, b) => b.price - a.price)

    if (filters.sort === "nameAsc")
        data.sort((a, b) => a.name.localeCompare(b.name))

    renderProducts(data)

}