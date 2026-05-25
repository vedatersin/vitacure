(function () {
    const uploaders = Array.from(document.querySelectorAll("[data-category-image-upload='true']"));

    uploaders.forEach((uploader) => {
        const form = uploader.closest("form") || document.querySelector("form[data-admin-product-form='true']") || document.querySelector("form");
        const trigger = uploader.querySelector("[data-category-image-trigger]");
        const fileInput = uploader.querySelector("[data-category-file-input]");
        const imageInput = uploader.querySelector("[data-category-image-input]");
        const preview = uploader.querySelector("[data-category-image-preview]");
        const emptyState = uploader.querySelector("[data-category-image-empty]");
        const clearButton = uploader.querySelector("[data-category-image-clear]");
        const uploadUrl = uploader.getAttribute("data-upload-url");

        const syncState = (url) => {
            const hasImage = Boolean(url);
            if (imageInput) {
                imageInput.value = url || "";
            }
            if (preview) {
                preview.src = url || "";
                preview.hidden = !hasImage;
            }
            if (emptyState) {
                emptyState.hidden = hasImage;
            }
            if (clearButton) {
                clearButton.hidden = !hasImage;
            }
        };

        const uploadFile = async (file) => {
            if (!file || !uploadUrl || !form) {
                return;
            }

            const formData = new FormData();
            const token = form.querySelector("input[name='__RequestVerificationToken']")?.value;
            if (token) {
                formData.append("__RequestVerificationToken", token);
            }
            formData.append("file", file);
            formData.append("slug", form.querySelector("input[name='Slug']")?.value || "");

            trigger?.setAttribute("disabled", "disabled");

            try {
                const response = await fetch(uploadUrl, {
                    method: "POST",
                    body: formData,
                    credentials: "same-origin"
                });

                const payload = await response.json();
                if (!response.ok || !payload?.url) {
                    throw new Error(payload?.error || "Görsel yuklenemedi.");
                }

                syncState(payload.url);
            } catch (error) {
                window.alert(error instanceof Error ? error.message : "Görsel yuklenemedi.");
            } finally {
                trigger?.removeAttribute("disabled");
                if (fileInput) {
                    fileInput.value = "";
                }
            }
        };

        trigger?.addEventListener("click", () => fileInput?.click());
        fileInput?.addEventListener("change", () => {
            const file = fileInput.files?.[0];
            if (file) {
                uploadFile(file);
            }
        });
        clearButton?.addEventListener("click", () => syncState(""));

        syncState(imageInput?.value || "");
    });
})();
