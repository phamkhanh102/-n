if (localStorage.getItem("role") !== "Admin") {
    alert("Bạn không có quyền")
    window.location = "shop.html"
}
const api = "/api/products"

async function loadProducts() {

    const res = await fetch(api)

    const data = await res.json()

    const table = document.getElementById("tableProducts")

    table.innerHTML = data.map(p => `

<tr>

<td>${p.Id}</td>

<td>
<input value="${p.Name}" id="name-${p.Id}">
</td>

<td>
<input value="${p.Brand}" id="brand-${p.Id}">
</td>

<td>
<input value="${p.Price}" id="price-${p.Id}">
</td>

<td>

<button onclick="updateProduct(${p.Id})">Sửa</button>

<button onclick="deleteProduct(${p.Id})">Xoá</button>

</td>

</tr>

`).join("")

}
async function loadDashboard() {
    const token = localStorage.getItem("token");

    if (!token) {
        alert("Chưa đăng nhập");
        return;
    }

    const res = await fetch("https://localhost:7244/api/admin/dashboard", {
        headers: {
            "Authorization": "Bearer " + token
        }
    });

    if (res.status === 401) {
        alert("Token hết hạn hoặc không hợp lệ");
        return;
    }

    const data = await res.json();

    console.log(data);

    document.getElementById("revenue").innerText = data.revenue;
    document.getElementById("orders").innerText = data.totalOrders;
    document.getElementById("users").innerText = data.users;
    document.getElementById("products").innerText = data.products;
}

loadDashboard();
loadProducts()
async function createProduct() {

    const name = document.getElementById("name").value
    const brand = document.getElementById("brand").value
    const price = parseFloat(document.getElementById("price").value)
    const image = document.getElementById("image").value

    await fetch(api, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({

            name: name,
            brand: brand,
            price: price,
            imageUrl: image

        })
    })

    loadProducts()

}
async function deleteProduct(id) {

    if (!confirm("Xoá sản phẩm?")) return

    await fetch(api + "/" + id, {
        method: "DELETE"
    })

    loadProducts()

}