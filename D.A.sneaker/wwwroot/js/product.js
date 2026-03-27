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


