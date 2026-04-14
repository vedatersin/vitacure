(function () {
    const editors = Array.from(document.querySelectorAll("[data-product-gallery-editor='true']"));

    editors.forEach((editor) => {
        const coverInput = editor.querySelector("[data-gallery-cover-input]");
        const hiddenInput = editor.querySelector("[data-gallery-hidden]");
        const addButton = editor.querySelector("[data-gallery-add]");
        const list = editor.querySelector("[data-gallery-list]");
        const strip = editor.querySelector("[data-gallery-strip]");
        const mainPreview = editor.querySelector("[data-gallery-main-preview]");
        const template = document.getElementById("adminProductGalleryItemTemplate");

        if (!coverInput || !hiddenInput || !addButton || !list || !strip || !mainPreview || !template) {
            return;
        }

        const initialItems = hiddenInput.value
            .split(/\r?\n|,|;/)
            .map((value) => value.trim())
            .filter(Boolean);

        const render = () => {
            hiddenInput.value = Array.from(list.querySelectorAll("[data-gallery-item-input]"))
                .map((input) => input.value.trim())
                .filter(Boolean)
                .join("\n");

            strip.querySelectorAll("[data-gallery-strip-item='true']").forEach((item) => item.remove());

            Array.from(list.querySelectorAll("[data-gallery-item]")).forEach((item, index) => {
                const input = item.querySelector("[data-gallery-item-input]");
                const preview = item.querySelector("[data-gallery-item-preview]");
                const value = input?.value.trim() || "";

                item.dataset.index = String(index);
                if (preview) {
                    preview.src = value || coverInput.value || "/img/logo.png";
                }

                const tile = document.createElement("button");
                tile.type = "button";
                tile.className = "admin-product-media-tile";
                tile.setAttribute("data-gallery-strip-item", "true");
                tile.setAttribute("data-gallery-preview-src", value);
                tile.innerHTML = `<img src="${value || coverInput.value || "/img/logo.png"}" alt="Galeri gorseli" /><span>${index + 1}. Gorsel</span>`;

                tile.addEventListener("click", () => {
                    setActiveTile(tile, value || coverInput.value);
                });

                strip.appendChild(tile);
            });

            const active = strip.querySelector(".is-active") || strip.querySelector(".is-cover");
            if (active instanceof HTMLElement) {
                const src = active.getAttribute("data-gallery-preview-src") || coverInput.value;
                setActiveTile(active, src);
            }
        };

        const createItem = (value) => {
            const fragment = template.content.cloneNode(true);
            const item = fragment.querySelector("[data-gallery-item]");
            const input = fragment.querySelector("[data-gallery-item-input]");
            const previewButton = fragment.querySelector("[data-gallery-item-preview-button]");
            const upButton = fragment.querySelector("[data-gallery-item-up]");
            const downButton = fragment.querySelector("[data-gallery-item-down]");
            const removeButton = fragment.querySelector("[data-gallery-item-remove]");

            if (!(item instanceof HTMLElement) || !(input instanceof HTMLInputElement)) {
                return;
            }

            input.value = value || "";
            input.addEventListener("input", render);

            previewButton?.addEventListener("click", () => {
                mainPreview.setAttribute("src", input.value.trim() || coverInput.value);
                strip.querySelectorAll(".admin-product-media-tile").forEach((tile) => tile.classList.remove("is-active"));
            });

            upButton?.addEventListener("click", () => {
                const previous = item.previousElementSibling;
                if (previous) {
                    list.insertBefore(item, previous);
                    render();
                }
            });

            downButton?.addEventListener("click", () => {
                const next = item.nextElementSibling;
                if (next) {
                    list.insertBefore(next, item);
                    render();
                }
            });

            removeButton?.addEventListener("click", () => {
                item.remove();
                render();
            });

            list.appendChild(fragment);
        };

        const setActiveTile = (tile, src) => {
            strip.querySelectorAll(".admin-product-media-tile").forEach((item) => item.classList.remove("is-active"));
            tile.classList.add("is-active");
            mainPreview.setAttribute("src", src || coverInput.value);
        };

        coverInput.addEventListener("input", () => {
            const coverTile = strip.querySelector(".admin-product-media-tile.is-cover");
            if (coverTile) {
                coverTile.setAttribute("data-gallery-preview-src", coverInput.value.trim());
                const image = coverTile.querySelector("img");
                if (image) {
                    image.src = coverInput.value.trim() || "/img/logo.png";
                }
            }

            if (strip.querySelector(".admin-product-media-tile.is-cover.is-active")) {
                mainPreview.setAttribute("src", coverInput.value.trim() || "/img/logo.png");
            }

            render();
        });

        strip.querySelector(".admin-product-media-tile.is-cover")?.addEventListener("click", (event) => {
            const tile = event.currentTarget;
            if (tile instanceof HTMLElement) {
                setActiveTile(tile, coverInput.value.trim());
            }
        });

        addButton.addEventListener("click", () => {
            createItem("");
            render();
        });

        if (initialItems.length > 0) {
            initialItems.forEach((item) => createItem(item));
        }

        render();
    });
})();
