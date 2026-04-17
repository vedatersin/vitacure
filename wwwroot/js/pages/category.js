(function () {
    const favoriteStore = window.__vitacureFavoriteStore || (window.__vitacureFavoriteStore = new Set());
    const slots = [
        { x: -252, y: -72, scale: 0.72, z: 4, blur: "2px" },
        { x: -168, y: -28, scale: 0.83, z: 5, blur: "2px" },
        { x: -84, y: 18, scale: 0.94, z: 6, blur: "1px" },
        { x: 0, y: 58, scale: 1.18, z: 8, blur: "0px" },
        { x: 84, y: 18, scale: 0.94, z: 6, blur: "1px" },
        { x: 168, y: -28, scale: 0.83, z: 5, blur: "2px" },
        { x: 252, y: -72, scale: 0.72, z: 4, blur: "3px" }
    ];

    function getSlotsForCount(count) {
        if (count >= slots.length) {
            return slots;
        }

        const centerIndex = Math.floor(slots.length / 2);
        const start = Math.max(0, centerIndex - Math.floor(count / 2));
        return slots.slice(start, start + count);
    }

    function renderStars(value) {
        const numeric = Number.parseFloat(String(value).replace(",", "."));
        const rounded = Number.isFinite(numeric) ? Math.max(0, Math.min(5, Math.round(numeric))) : 5;
        return "\u2605\u2605\u2605\u2605\u2605".slice(0, rounded) + "\u2606\u2606\u2606\u2606\u2606".slice(rounded);
    }

    function initCoverflow(shell) {
        const products = JSON.parse(shell.dataset.products || "[]");
        if (!products.length) {
            return;
        }

        const stage = shell.querySelector("[data-coverflow-stage]");
        if (!stage) {
            return;
        }

        const pageRoot = shell.closest("[data-category-page]");
        const previous = shell.querySelector("[data-coverflow-prev]");
        const next = shell.querySelector("[data-coverflow-next]");
        const name = shell.querySelector("[data-coverflow-name]");
        const description = shell.querySelector("[data-coverflow-description]");
        const stars = shell.querySelector("[data-coverflow-stars]");
        const rating = shell.querySelector("[data-coverflow-rating]");
        const oldPrice = shell.querySelector("[data-coverflow-old-price]");
        const newPrice = shell.querySelector("[data-coverflow-new-price]");
        const heartButton = shell.querySelector("[data-coverflow-heart]");
        const cartButton = shell.querySelector("[data-coverflow-cart]");
        const inspectButton = shell.querySelector("[data-coverflow-inspect]");
        const tagButtons = Array.from(pageRoot?.querySelectorAll(".uyku-tag-btn") ?? []);
        const productGridAnchor = pageRoot?.querySelector("[data-product-grid-anchor]");
        const activeSlots = getSlotsForCount(products.length);
        const centerSlotIndex = Math.floor(activeSlots.length / 2);
        const transitionDurationMs = Number.parseInt(getComputedStyle(shell).getPropertyValue("--coverflow-transition-ms"), 10) || 420;
        let order = products.map((_, index) => index);
        let transitionState = null;
        let transitionTimer = null;
        let isAnimating = false;

        const items = products.map((product) => {
            const item = document.createElement("div");
            item.className = "uyku-coverflow-item";
            item.style.position = "absolute";
            item.style.left = "50%";
            item.style.top = "50%";
            item.style.transformOrigin = "center center";
            item.style.transition = `transform ${transitionDurationMs}ms cubic-bezier(0.22, 1, 0.36, 1), opacity 320ms ease, filter ${transitionDurationMs}ms ease`;

            const src = product.src || product.Src;
            const alt = product.alt || product.Alt;
            item.innerHTML = `<img src="${src}" alt="${alt}" style="width:250px; height:auto; object-fit:contain; display:block; filter:drop-shadow(0 18px 28px rgba(0,0,0,0.45));" />`;
            stage.appendChild(item);
            return item;
        });

        function getActiveProductIndex() {
            return order[centerSlotIndex];
        }

        function syncHeart(activeProduct) {
            if (!heartButton) {
                return;
            }

            const productId = activeProduct.id || activeProduct.Id || "";
            const isFavorite = productId ? favoriteStore.has(productId) : false;
            heartButton.dataset.favoriteProductId = productId;
            heartButton.classList.toggle("is-active", isFavorite);
            heartButton.setAttribute("aria-pressed", isFavorite ? "true" : "false");

            const icon = heartButton.querySelector("i");
            if (icon) {
                icon.classList.toggle("fa-regular", !isFavorite);
                icon.classList.toggle("fa-solid", isFavorite);
            }
        }

        function render() {
            items.forEach((item, index) => {
                const slotIndex = order.indexOf(index);
                const slot = activeSlots[slotIndex] || activeSlots[activeSlots.length - 1];
                const shouldWrapBehind = transitionState?.wrappingIndex === index;
                item.style.zIndex = String(shouldWrapBehind ? 1 : slot.z);
                item.style.filter = `blur(${slot.blur})`;
                item.style.transform = `translate(calc(-50% + ${slot.x}px), calc(-50% + ${slot.y}px)) scale(${slot.scale})`;
            });

            const activeProduct = products[getActiveProductIndex()];
            if (name) {
                name.textContent = activeProduct.name || activeProduct.Name || "";
            }

            if (description) {
                description.textContent = activeProduct.description || activeProduct.Description || "";
            }

            if (stars) {
                stars.textContent = renderStars(activeProduct.rating || activeProduct.Rating || "5");
            }

            if (rating) {
                rating.textContent = `${activeProduct.rating || activeProduct.Rating || "5"}/5 kullanici puani`;
            }

            if (oldPrice) {
                oldPrice.textContent = activeProduct.priceOld || activeProduct.PriceOld || "";
            }

            if (newPrice) {
                newPrice.textContent = activeProduct.priceNew || activeProduct.PriceNew || "";
            }

            if (inspectButton) {
                inspectButton.dataset.productHref = activeProduct.href || activeProduct.Href || "#";
            }

            if (cartButton) {
                cartButton.dataset.cartProductId = activeProduct.cartProductSlug || activeProduct.CartProductSlug || activeProduct.id || activeProduct.Id || "";
            }

            syncHeart(activeProduct);
        }

        function completeTransition() {
            if (transitionTimer) {
                clearTimeout(transitionTimer);
                transitionTimer = null;
            }

            transitionState = null;
            isAnimating = false;
            render();
        }

        function rotate(direction) {
            if (isAnimating) {
                return;
            }

            const wrappingIndex = direction === "next"
                ? order[order.length - 1]
                : order[0];

            if (direction === "next") {
                order.unshift(order.pop());
            } else {
                order.push(order.shift());
            }

            transitionState = { direction, wrappingIndex };
            isAnimating = true;
            render();

            transitionTimer = window.setTimeout(completeTransition, transitionDurationMs);
        }

        previous?.addEventListener("click", function () {
            rotate("prev");
        });

        next?.addEventListener("click", function () {
            rotate("next");
        });

        tagButtons.forEach((button) => {
            button.addEventListener("click", function (event) {
                event.preventDefault();
                productGridAnchor?.scrollIntoView({ behavior: "smooth", block: "start" });
            });
        });

        heartButton?.addEventListener("click", function () {
            const productId = heartButton.dataset.favoriteProductId || "";
            const isActive = heartButton.classList.toggle("is-active");
            heartButton.setAttribute("aria-pressed", isActive ? "true" : "false");

            if (productId) {
                if (isActive) {
                    favoriteStore.add(productId);
                } else {
                    favoriteStore.delete(productId);
                }
            }

            const icon = heartButton.querySelector("i");
            if (icon) {
                icon.classList.toggle("fa-regular", !isActive);
                icon.classList.toggle("fa-solid", isActive);
            }
        });

        inspectButton?.addEventListener("click", function () {
            const href = inspectButton.dataset.productHref;
            if (href) {
                window.location.href = href;
            }
        });

        if (cartButton) {
            cartButton.addEventListener("click", async function () {
                const productSlug = cartButton.dataset.cartProductId || "";
                if (!productSlug || !window.VitacureCart) {
                    return;
                }

                await window.VitacureCart.addItem(productSlug, cartButton, 1);
            });
        }

        render();
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-coverflow]").forEach(initCoverflow);
    });
})();
