/* RADIO TOGGLE FIX */
let lastPrice = null;
document.querySelectorAll("input[name=price]").forEach(radio => {

    radio.addEventListener("mousedown", function (e) {

        if (this.checked) {
            this.dataset.waschecked = "true";
        } else {
            this.dataset.waschecked = "false";
        }

    });

    radio.addEventListener("click", function (e) {

        if (this.dataset.waschecked === "true") {
            this.checked = false;
            filters.priceMin = 0;
            filters.priceMax = Infinity;
        }
        else {
            const [min, max] = this.value.split("-");
            filters.priceMin = +min;
            filters.priceMax = +max;
        }

        applyFilters();
    });

});


/* LOAD DATA */
async function load() {

    try {
        const r = await fetch('/api/products');

        if (!r.ok) throw new Error("API lỗi");

        const data = await r.json();

        all = data.map(p => ({
            id: p.Id,
            name: p.Name,
            brand: p.Brand,
            price: p.Price,
            imageUrl: p.ImageUrl
        }));

    } catch (e) {

        console.warn("API chết → dùng dữ liệu giả");

        // DATA FALLBACK (để web luôn hiển thị)
        all = [
            { id: 1, name: "Nike Air Force 1", brand: "Nike", price: 3200000, imageUrl: "" },
            { id: 2, name: "Adidas Ultraboost 22", brand: "Adidas", price: 4500000, imageUrl: "" },
            { id: 3, name: "Converse Chuck 70", brand: "Converse", price: 2100000, imageUrl: "" },
            { id: 4, name: "Nike ZoomX Vaporfly", brand: "Nike", price: 6500000, imageUrl: "" }
        ];
    }

    applyFilters();
}

