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

        const res = await fetch('/api/products')

        const data = await res.json()

        all = data.map(p => ({

            id: p.Id,
            name: p.Name,
            price: p.Price,
            image: p.ImageUrl

        }))

        renderProducts(all)

    } catch {

        grid.innerHTML = "Lỗi tải sản phẩm"

    }

}


function renderProducts(list) {

    const grid = document.getElementById("grid")

    grid.innerHTML = list.map(p => `

<div class="card">

<div class="sale-badge">SALE</div>

<img class="product-img"
src="${p.image || '/images/no-image.png'}"
loading="lazy">

<div class="title">${p.name}</div>

<div class="price">${p.price.toLocaleString()} đ</div>

<button class="quick-add"
onclick="addToCart(${p.id})">
Thêm nhanh
</button>

</div>

`).join("")

}


loadProducts()