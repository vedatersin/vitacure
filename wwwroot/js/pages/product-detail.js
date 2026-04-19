(function () {
    const mainImage = document.querySelector("[data-product-main-image]");
    const thumbs = Array.from(document.querySelectorAll("[data-product-thumb='true']"));
    const variantButtons = Array.from(document.querySelectorAll("[data-product-variant-option='true']"));
    const selectedVariantLabel = document.querySelector("[data-product-detail-selected-variant]");
    const priceNode = document.querySelector("[data-product-detail-price]");
    const oldPriceNode = document.querySelector("[data-product-detail-old-price]");
    const stockNode = document.querySelector("[data-product-detail-stock]");
    const cartButton = document.querySelector("[data-cart-button]");

    if (mainImage && thumbs.length > 0) {
        thumbs.forEach((thumb) => {
            thumb.addEventListener("click", () => {
                const imageSrc = thumb.getAttribute("data-image-src");

                if (!imageSrc) {
                    return;
                }

                mainImage.setAttribute("src", imageSrc);

                thumbs.forEach((item) => {
                    item.classList.remove("is-active");
                    item.setAttribute("aria-pressed", "false");
                });

                thumb.classList.add("is-active");
                thumb.setAttribute("aria-pressed", "true");
            });
        });
    }

    if (variantButtons.length === 0) {
        return;
    }

    variantButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const variantId = button.getAttribute("data-variant-id") || "";
            const variantLabel = button.getAttribute("data-variant-label") || "";
            const variantPrice = button.getAttribute("data-variant-price") || "";
            const variantOldPrice = button.getAttribute("data-variant-old-price") || "";
            const variantStock = button.getAttribute("data-variant-stock") || "";

            variantButtons.forEach((item) => {
                item.classList.remove("is-active");
                item.setAttribute("aria-pressed", "false");
            });

            button.classList.add("is-active");
            button.setAttribute("aria-pressed", "true");

            if (selectedVariantLabel) {
                selectedVariantLabel.textContent = variantLabel;
            }

            if (priceNode) {
                priceNode.innerHTML = `&#8378; ${variantPrice}`;
            }

            if (oldPriceNode) {
                if (variantOldPrice) {
                    oldPriceNode.hidden = false;
                    oldPriceNode.innerHTML = `&#8378; ${variantOldPrice}`;
                } else {
                    oldPriceNode.hidden = true;
                    oldPriceNode.textContent = "";
                }
            }

            if (stockNode) {
                stockNode.textContent = variantStock;
            }

            if (cartButton) {
                cartButton.setAttribute("data-cart-variant-id", variantId);
            }
        });
    });
})();
