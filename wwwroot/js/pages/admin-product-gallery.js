(function () {
    const editors = Array.from(document.querySelectorAll("[data-product-gallery-editor='true']"));
    const fallbackImage = "/img/logo.png";

    editors.forEach((editor) => {
        const form = editor.closest("form");
        const coverInput = editor.querySelector("[data-gallery-cover-input]");
        const hiddenInput = editor.querySelector("[data-gallery-hidden]");
        const strip = editor.querySelector("[data-gallery-strip]");
        const mainPreview = editor.querySelector("[data-gallery-main-preview]");
        const mainTrigger = editor.querySelector("[data-gallery-main-trigger]");
        const mainEmpty = editor.querySelector("[data-gallery-main-empty]");
        const mainRemove = editor.querySelector("[data-gallery-main-remove]");
        const fileInput = editor.querySelector("[data-gallery-file-input]");
        const uploadUrl = editor.getAttribute("data-gallery-upload-url") || "";
        const maxItems = Number.parseInt(editor.getAttribute("data-gallery-max-items") || "4", 10);
        const antiforgeryToken = form?.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
        const slugInput = form?.querySelector('input[name="Slug"]');
        const modal = document.querySelector("[data-gallery-modal]");
        const modalImage = document.querySelector("[data-gallery-modal-image]");
        const modalCloseButtons = Array.from(document.querySelectorAll("[data-gallery-modal-close]"));

        if (!coverInput || !hiddenInput || !strip || !mainPreview || !mainTrigger || !fileInput) {
            return;
        }

        const initialItems = [coverInput.value.trim()]
            .concat(
                hiddenInput.value
                    .split(/\r?\n|,|;/)
                    .map((value) => value.trim())
                    .filter(Boolean)
            )
            .filter(Boolean)
            .slice(0, Math.max(maxItems, 1));

        let items = initialItems;
        let dragIndex = -1;

        const syncHiddenInputs = () => {
            coverInput.value = items[0] || "";
            hiddenInput.value = items.slice(1).join("\n");
        };

        const renderMainPreview = () => {
            const activeImage = items[0] || "";
            if (activeImage) {
                mainPreview.src = activeImage;
                mainPreview.hidden = false;
                mainRemove?.removeAttribute("hidden");
                mainEmpty?.setAttribute("hidden", "hidden");
                return;
            }

            mainPreview.hidden = true;
            mainPreview.removeAttribute("src");
            mainRemove?.setAttribute("hidden", "hidden");
            mainEmpty?.removeAttribute("hidden");
        };

        const openModal = (src) => {
            if (!modal || !modalImage || !src) {
                return;
            }

            modalImage.src = src;
            modal.hidden = false;
            document.body.classList.add("is-modal-open");
        };

        const closeModal = () => {
            if (!modal) {
                return;
            }

            modal.hidden = true;
            document.body.classList.remove("is-modal-open");
        };

        const renderStrip = () => {
            strip.innerHTML = "";

            items.forEach((src, index) => {
                const tile = document.createElement("button");
                tile.type = "button";
                tile.className = `admin-product-media-tile${index === 0 ? " is-active" : ""}`;
                tile.draggable = true;
                tile.innerHTML = `
                    <img src="${src || fallbackImage}" alt="Urun gorseli ${index + 1}" />
                    <span>${index === 0 ? "Kapak" : `${index + 1}. Gorsel`}</span>
                    <span class="admin-product-media-tile-overlay">Kapak yap</span>
                    <span class="admin-product-media-tile-remove"><i class="fa-solid fa-xmark"></i></span>
                `;

                tile.addEventListener("click", () => {
                    if (index === 0) {
                        openModal(src);
                        return;
                    }

                    const [selected] = items.splice(index, 1);
                    items.unshift(selected);
                    render();
                });

                tile.addEventListener("dragstart", (event) => {
                    dragIndex = index;
                    tile.classList.add("is-dragging");
                    event.dataTransfer?.setData("text/plain", String(index));
                });

                tile.addEventListener("dragend", () => {
                    dragIndex = -1;
                    tile.classList.remove("is-dragging");
                });

                tile.addEventListener("dragover", (event) => {
                    event.preventDefault();
                    tile.classList.add("is-drop-target");
                });

                tile.addEventListener("dragleave", () => {
                    tile.classList.remove("is-drop-target");
                });

                tile.addEventListener("drop", (event) => {
                    event.preventDefault();
                    tile.classList.remove("is-drop-target");
                    const fromIndex = Number.parseInt(event.dataTransfer?.getData("text/plain") || String(dragIndex), 10);
                    if (Number.isNaN(fromIndex) || fromIndex === index || fromIndex < 0 || fromIndex >= items.length) {
                        return;
                    }

                    const [moved] = items.splice(fromIndex, 1);
                    items.splice(index, 0, moved);
                    render();
                });

                tile.querySelector(".admin-product-media-tile-remove")?.addEventListener("click", (event) => {
                    event.preventDefault();
                    event.stopPropagation();

                    if (!window.confirm("Bu gorseli silmek istiyor musunuz?")) {
                        return;
                    }

                    items.splice(index, 1);
                    render();
                });

                strip.appendChild(tile);
            });

            if (items.length < maxItems) {
                const addTile = document.createElement("button");
                addTile.type = "button";
                addTile.className = "admin-product-media-tile is-add";
                addTile.innerHTML = `
                    <span class="admin-product-media-add-icon"><i class="fa-solid fa-plus"></i></span>
                    <span>Gorsel Ekle</span>
                `;
                addTile.addEventListener("click", () => fileInput.click());
                strip.appendChild(addTile);
            }
        };

        const render = () => {
            syncHiddenInputs();
            renderMainPreview();
            renderStrip();
        };

        const uploadImage = async (file) => {
            const formData = new FormData();
            formData.append("file", file);
            formData.append("slug", slugInput?.value?.trim() || "product");
            formData.append("__RequestVerificationToken", antiforgeryToken);

            const response = await fetch(uploadUrl, {
                method: "POST",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: formData
            });

            if (!response.ok) {
                const payload = await response.json().catch(() => ({}));
                throw new Error(payload.error || "Gorsel yuklenemedi.");
            }

            const payload = await response.json();
            return payload.url || "";
        };

        fileInput.addEventListener("change", async () => {
            const [file] = fileInput.files || [];
            if (!file) {
                return;
            }

            if (items.length >= maxItems) {
                window.alert(`En fazla ${maxItems} gorsel ekleyebilirsiniz.`);
                fileInput.value = "";
                return;
            }

            try {
                const uploadedUrl = await uploadImage(file);
                if (uploadedUrl) {
                    items.push(uploadedUrl);
                    render();
                }
            } catch (error) {
                window.alert(error instanceof Error ? error.message : "Gorsel yuklenemedi.");
            } finally {
                fileInput.value = "";
            }
        });

        mainTrigger.addEventListener("click", () => {
            if (items[0]) {
                openModal(items[0]);
            }
        });

        mainRemove?.addEventListener("click", (event) => {
            event.preventDefault();
            event.stopPropagation();

            if (!items[0] || !window.confirm("Bu gorseli silmek istiyor musunuz?")) {
                return;
            }

            items.shift();
            render();
        });

        modalCloseButtons.forEach((button) => {
            button.addEventListener("click", closeModal);
        });

        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape") {
                closeModal();
            }
        });

        render();
    });
})();
