(function () {
    const favoriteStore = window.__vitacureFavoriteStore || (window.__vitacureFavoriteStore = new Set());
    const transitionDurationMs = 420;
    const slots = [
        { x: -252, y: -72, scale: 0.72, z: 4, blur: "2px" },
        { x: -168, y: -28, scale: 0.83, z: 5, blur: "2px" },
        { x: -84, y: 18, scale: 0.94, z: 6, blur: "1px" },
        { x: 0, y: 58, scale: 1.18, z: 8, blur: "0px" },
        { x: 84, y: 18, scale: 0.94, z: 6, blur: "1px" },
        { x: 168, y: -28, scale: 0.83, z: 5, blur: "2px" },
        { x: 252, y: -72, scale: 0.72, z: 4, blur: "3px" }
    ];

    function normalizeTag(value) {
        return (value || "")
            .toLowerCase()
            .replace(/ı/g, "i")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .trim();
    }

    function findProductIndexByHints(products, hints) {
        const normalizedHints = hints.map((hint) => hint.toLowerCase());
        return products.findIndex((product) => {
            const name = (product.name || product.Name || "").toLowerCase();
            const src = (product.src || product.Src || "").toLowerCase();
            return normalizedHints.some((hint) => name.includes(hint) || src.includes(hint));
        });
    }

    function bindTagProducts(products, buttons) {
        const lookupRules = [
            { tagKey: "tumu", hints: ["magnezyum", "bottle_mag"] },
            { tagKey: "melatonin", hints: ["omega", "bottle_omega"] },
            { tagKey: "bitkisel", hints: ["multi", "bottle_multi"] },
            { tagKey: "cocuklar", hints: ["d3", "bottle_d3"] }
        ];

        buttons.forEach((button, index) => {
            const normalizedTag = normalizeTag(button.textContent);
            const matchedRule = lookupRules.find((rule) => normalizedTag.includes(rule.tagKey));
            const matchedIndex = matchedRule ? findProductIndexByHints(products, matchedRule.hints) : -1;
            button.dataset.productIndex = String(matchedIndex >= 0 ? matchedIndex : index % Math.max(products.length, 1));
        });
    }

    function renderStars(value) {
        const numeric = Number.parseFloat(String(value).replace(",", "."));
        const rounded = Number.isFinite(numeric) ? Math.max(0, Math.min(5, Math.round(numeric))) : 5;
        return "★★★★★".slice(0, rounded) + "☆☆☆☆☆".slice(rounded);
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
        const tagButtons = Array.from(document.querySelectorAll(".uyku-tag-btn"));
        const centerSlotIndex = Math.min(3, products.length - 1);
        let order = products.map((_, index) => index);
        let transitionState = null;
        let transitionTimer = null;
        let isAnimating = false;

        bindTagProducts(products, tagButtons);

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

        function getActiveProductIndex() {
            return order[centerSlotIndex];
        }

        function setActiveTagByProductIndex(productIndex) {
            if (!tagButtons.length) {
                return;
            }

            const targetIndex = tagButtons.findIndex((button) => Number.parseInt(button.dataset.productIndex || "", 10) === productIndex);
            if (targetIndex < 0) {
                return;
            }

            tagButtons.forEach((button, index) => {
                button.classList.toggle("active", index === targetIndex);
            });
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
                const slot = slots[slotIndex] || slots[slots.length - 1];
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
                rating.textContent = `${activeProduct.rating || activeProduct.Rating || "5"}/5 kullanıcı puanı`;
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
            setActiveTagByProductIndex(getActiveProductIndex());
        }

        function moveCenterToProduct(targetProductIndex) {
            if (!Number.isInteger(targetProductIndex) || targetProductIndex < 0 || targetProductIndex >= products.length) {
                return;
            }

            const maxSteps = products.length + 2;
            let steps = 0;
            while (getActiveProductIndex() !== targetProductIndex && steps < maxSteps) {
                order.push(order.shift());
                steps += 1;
            }
        }

        function activateTagAt(tagIndex) {
            if (!tagButtons.length) {
                return;
            }

            const normalizedTagIndex = ((tagIndex % tagButtons.length) + tagButtons.length) % tagButtons.length;
            const targetProductIndex = Number.parseInt(tagButtons[normalizedTagIndex].dataset.productIndex || "", 10);
            moveCenterToProduct(targetProductIndex);
            render();
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

        tagButtons.forEach((button, index) => {
            button.addEventListener("click", function () {
                activateTagAt(index);
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

        const initiallyActiveTag = Math.max(0, tagButtons.findIndex((button) => button.classList.contains("active")));
        if (tagButtons.length) {
            activateTagAt(initiallyActiveTag);
        } else {
            render();
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-coverflow]").forEach(initCoverflow);
    });
})();
