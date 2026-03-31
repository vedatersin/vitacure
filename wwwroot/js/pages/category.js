(function () {
    const slots = [
        { x: -252, y: -118, scale: 0.72, z: 0, blur: "2px" },
        { x: -168, y: -78, scale: 0.83, z: 1, blur: "2px" },
        { x: -84, y: -34, scale: 0.94, z: 2, blur: "2px" },
        { x: 0, y: 26, scale: 1.18, z: 5, blur: "0px" },
        { x: 84, y: -34, scale: 0.94, z: 2, blur: "2px" },
        { x: 168, y: -78, scale: 0.83, z: 1, blur: "2px" },
        { x: 252, y: -118, scale: 0.72, z: 0, blur: "2px" }
    ];

    function initCoverflow(shell) {
        const products = JSON.parse(shell.dataset.products || "[]");
        const stage = shell.querySelector("[data-coverflow-stage]");
        const previous = shell.querySelector("[data-coverflow-prev]");
        const next = shell.querySelector("[data-coverflow-next]");
        const name = shell.querySelector("[data-coverflow-name]");
        const description = shell.querySelector("[data-coverflow-description]");
        const stars = shell.querySelector("[data-coverflow-stars]");
        const rating = shell.querySelector("[data-coverflow-rating]");
        const oldPrice = shell.querySelector("[data-coverflow-old-price]");
        const newPrice = shell.querySelector("[data-coverflow-new-price]");
        let order = products.map((_, index) => index);
        const items = products.map((product) => {
            const item = document.createElement("div");
            item.className = "uyku-coverflow-item";
            item.style.position = "absolute";
            item.style.left = "50%";
            item.style.top = "50%";
            item.style.transformOrigin = "center center";
            item.style.transition = "transform 420ms cubic-bezier(0.22, 1, 0.36, 1), opacity 320ms ease, filter 420ms ease";
            const src = product.src || product.Src;
            const alt = product.alt || product.Alt;
            item.innerHTML = `<img src="${src}" alt="${alt}" style="width:250px; height:auto; object-fit:contain; display:block; filter:drop-shadow(0 18px 28px rgba(0,0,0,0.45));" />`;
            stage.appendChild(item);
            return item;
        });

        function render() {
            items.forEach((item, index) => {
                const slotIndex = order.indexOf(index);
                const slot = slots[slotIndex];
                item.style.zIndex = String(slot.z);
                item.style.filter = `blur(${slot.blur})`;
                item.style.transform = `translate(calc(-50% + ${slot.x}px), calc(-50% + ${slot.y}px)) scale(${slot.scale})`;
            });

            const activeProduct = products[order[3]];
            name.textContent = activeProduct.name || activeProduct.Name;
            description.textContent = activeProduct.description || activeProduct.Description || "";
            stars.textContent = "★★★★★";
            rating.textContent = `${activeProduct.rating || activeProduct.Rating}/5 kullanıcı puanı`;
            oldPrice.textContent = activeProduct.priceOld || activeProduct.PriceOld;
            newPrice.textContent = activeProduct.priceNew || activeProduct.PriceNew;
        }

        previous?.addEventListener("click", function () {
            order.push(order.shift());
            render();
        });

        next?.addEventListener("click", function () {
            order.unshift(order.pop());
            render();
        });

        render();
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-coverflow]").forEach(initCoverflow);

        document.querySelectorAll(".uyku-tag-btn").forEach((button) => {
            button.addEventListener("click", function () {
                document.querySelectorAll(".uyku-tag-btn").forEach((item) => item.classList.remove("active"));
                button.classList.add("active");
            });
        });
    });
})();
