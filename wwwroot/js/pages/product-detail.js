(function () {
    const mainImage = document.querySelector("[data-product-main-image]");
    const thumbs = Array.from(document.querySelectorAll("[data-product-thumb='true']"));

    if (!mainImage || thumbs.length === 0) {
        return;
    }

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
})();
