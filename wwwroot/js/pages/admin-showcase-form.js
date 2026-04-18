document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector("[data-showcase-form]");
    if (!form) {
        return;
    }

    const hiddenInput = form.querySelector('input[name="BackgroundImageUrl"]');
    const fileInput = form.querySelector("[data-showcase-background-file-input]");
    const previewButton = form.querySelector("[data-showcase-background-preview]");
    const previewImage = form.querySelector("[data-showcase-background-preview-image]");
    const previewEmpty = form.querySelector("[data-showcase-background-empty]");
    const removeButton = form.querySelector("[data-showcase-background-remove]");
    const modal = document.querySelector("[data-showcase-image-modal]");
    const modalImage = document.querySelector("[data-showcase-image-modal-image]");
    const modalCloseButtons = Array.from(document.querySelectorAll("[data-showcase-image-modal-close]"));
    const tagsHiddenInput = form.querySelector("[data-showcase-tags-hidden]");
    const tagsList = form.querySelector("[data-showcase-tags-list]");
    const tagsInput = form.querySelector("[data-showcase-tags-input]");
    const descriptionInput = form.querySelector("[data-showcase-description-input]");
    const iconInput = form.querySelector('input[name="IconClass"]');
    const iconPreview = form.querySelector("[data-showcase-icon-preview]");
    const categoryOptions = Array.from(form.querySelectorAll("[data-showcase-category-option]"));
    const slotElements = Array.from(form.querySelectorAll("[data-showcase-slot]"));
    const selectedInputsContainer = form.querySelector("[data-showcase-selected-inputs]");
    const pickerBackdrop = document.querySelector("[data-showcase-picker]");
    const pickerCloseButton = document.querySelector("[data-showcase-picker-close]");
    const pickerItems = Array.from(document.querySelectorAll("[data-showcase-picker-item]"));
    const pickerSearch = document.querySelector("[data-showcase-product-search]");
    const pickerCategoryFilter = document.querySelector("[data-showcase-product-category-filter]");
    const pickerTagFilter = document.querySelector("[data-showcase-product-tag-filter]");
    const pickerEmpty = document.querySelector("[data-showcase-picker-empty]");
    const themeOptions = Array.from(form.querySelectorAll("[data-showcase-theme-option]"));
    const themeCards = Array.from(form.querySelectorAll("[data-showcase-theme-card]"));
    const descriptionLimit = Number.parseInt(descriptionInput?.dataset.showcaseDescriptionLimit || "0", 10);

    const productLookup = new Map(
        pickerItems.map((item) => [
            Number.parseInt(item.dataset.productId || "0", 10),
            {
                id: Number.parseInt(item.dataset.productId || "0", 10),
                name: item.dataset.productName || "",
                categoryName: item.dataset.productCategory || "",
                categorySlug: item.dataset.categorySlug || "",
                categoryId: Number.parseInt(item.dataset.categoryId || "0", 10),
                imageUrl: item.querySelector("img")?.getAttribute("src") || "",
                tagNames: (item.dataset.productTags || "")
                    .split("|")
                    .map((tag) => tag.trim())
                    .filter(Boolean)
            }
        ])
    );

    let selectedSlotIndex = 0;
    let selectedPickerProductId = 0;
    let featuredProductIds = slotElements
        .map((slot) => Number.parseInt(slot.dataset.productId || "0", 10))
        .map((value) => (value > 0 ? value : 0));
    let currentPreviewUrl = hiddenInput?.value?.trim() || "";
    let uploadedPreviewUrl = "";
    let tags = parseTags(tagsHiddenInput?.value || "");

    function normalizeText(value) {
        return (value || "")
            .toLocaleLowerCase("tr-TR")
            .replaceAll("ı", "i")
            .replaceAll("İ", "i")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .trim();
    }

    function parseTags(value) {
        return value
            .split(/[\r\n,]+/)
            .map((item) => item.trim())
            .filter(Boolean);
    }

    function getShowcaseTags() {
        return parseTags(tagsHiddenInput?.value || "")
            .map(normalizeText)
            .filter(Boolean);
    }

    function getFeaturedProducts() {
        return featuredProductIds
            .map((productId) => productLookup.get(productId))
            .filter(Boolean);
    }

    function syncTags() {
        if (tagsHiddenInput) {
            tagsHiddenInput.value = tags.join(", ");
        }
    }

    function renderTags() {
        if (!tagsList) {
            return;
        }

        tagsList.innerHTML = tags
            .map((tag, index) => `
                <span class="admin-showcase-tag-pill">
                    <span>${tag}</span>
                    <button type="button" class="admin-showcase-tag-pill-remove" data-tag-remove="${index}" aria-label="${tag} etiketini sil">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </span>
            `)
            .join("");

        syncTags();
        renderPickerFilters();
        filterPickerItems();
    }

    function addTagsFromText(value) {
        const nextTags = parseTags(value);
        if (nextTags.length === 0) {
            return;
        }

        nextTags.forEach((tag) => {
            if (!tags.some((item) => normalizeText(item) === normalizeText(tag))) {
                tags.push(tag);
            }
        });

        renderTags();
    }

    function setPreviewState(url) {
        currentPreviewUrl = url || "";

        if (previewButton) {
            previewButton.classList.toggle("is-empty", currentPreviewUrl.length === 0);
        }

        if (previewImage) {
            if (currentPreviewUrl) {
                previewImage.src = currentPreviewUrl;
                previewImage.hidden = false;
            } else {
                previewImage.hidden = true;
                previewImage.removeAttribute("src");
            }
        }

        if (previewEmpty) {
            previewEmpty.hidden = currentPreviewUrl.length > 0;
        }

        if (removeButton) {
            removeButton.hidden = currentPreviewUrl.length === 0;
        }
    }

    function clearBackgroundValidation() {
        const validationMessage = form.querySelector('[data-valmsg-for="BackgroundImageUrl"]');
        if (validationMessage) {
            validationMessage.textContent = "";
            validationMessage.classList.remove("field-validation-error");
            validationMessage.classList.add("field-validation-valid");
        }

        if (hiddenInput) {
            hiddenInput.classList.remove("input-validation-error");
            hiddenInput.setAttribute("aria-invalid", "false");
        }
    }

    function setDescriptionValidationMessage(message) {
        const validationMessage = form.querySelector('[data-valmsg-for="Description"]');
        if (!validationMessage) {
            return;
        }

        validationMessage.textContent = message;
        validationMessage.classList.toggle("field-validation-error", message.length > 0);
        validationMessage.classList.toggle("field-validation-valid", message.length === 0);

        if (descriptionInput) {
            descriptionInput.classList.toggle("input-validation-error", message.length > 0);
            descriptionInput.setAttribute("aria-invalid", message.length > 0 ? "true" : "false");
        }
    }

    function validateDescription() {
        if (!descriptionInput || descriptionLimit <= 0) {
            return true;
        }

        const isValid = descriptionInput.value.length <= descriptionLimit;
        setDescriptionValidationMessage(isValid ? "" : `Aciklama en fazla ${descriptionLimit} karakter olabilir.`);
        return isValid;
    }

    function syncThemeCards() {
        themeCards.forEach((card) => {
            const option = card.querySelector("[data-showcase-theme-option]");
            card.classList.toggle("is-selected", Boolean(option?.checked));
        });
    }

    function resetImageSelection() {
        if (uploadedPreviewUrl) {
            URL.revokeObjectURL(uploadedPreviewUrl);
            uploadedPreviewUrl = "";
        }

        if (fileInput) {
            fileInput.value = "";
        }

        if (hiddenInput) {
            hiddenInput.value = "";
        }

        setPreviewState("");
    }

    function openImageModal() {
        if (!modal || !modalImage || !currentPreviewUrl) {
            return;
        }

        modalImage.src = currentPreviewUrl;
        modal.hidden = false;
        document.body.classList.add("is-modal-open");
    }

    function closeImageModal() {
        if (!modal) {
            return;
        }

        modal.hidden = true;
        document.body.classList.remove("is-modal-open");
    }

    function renderSelectedInputs() {
        if (!selectedInputsContainer) {
            return;
        }

        selectedInputsContainer.innerHTML = featuredProductIds
            .filter((id) => id > 0)
            .map((id) => `<input type="hidden" name="SelectedFeaturedProductIds" value="${id}" />`)
            .join("");
    }

    function getScopedProducts() {
        const selectedCategoryIds = categoryOptions
            .filter((option) => option.checked)
            .map((option) => Number.parseInt(option.value, 10))
            .filter((value) => value > 0);
        const showcaseTags = getShowcaseTags();
        const allProducts = Array.from(productLookup.values());
        const scopedProducts = allProducts.filter((product) => {
            const matchesCategory = selectedCategoryIds.includes(product.categoryId);
            const matchesTag = product.tagNames.some((tag) => showcaseTags.includes(normalizeText(tag)));
            return matchesCategory || matchesTag;
        });

        if (scopedProducts.length > 0) {
            return scopedProducts;
        }

        const featuredProducts = getFeaturedProducts();
        if (featuredProducts.length === 0) {
            return [];
        }

        const fallbackCategoryIds = [...new Set(featuredProducts.map((product) => product.categoryId).filter((value) => value > 0))];
        const fallbackTags = [...new Set(featuredProducts.flatMap((product) => product.tagNames.map(normalizeText)).filter(Boolean))];

        return allProducts.filter((product) => {
            const matchesFallbackCategory = fallbackCategoryIds.includes(product.categoryId);
            const matchesFallbackTag = product.tagNames.some((tag) => fallbackTags.includes(normalizeText(tag)));
            const isCurrentFeatured = featuredProductIds.includes(product.id);
            return matchesFallbackCategory || matchesFallbackTag || isCurrentFeatured;
        });
    }

    function fillSelectOptions(selectElement, options, defaultLabel) {
        if (!selectElement) {
            return;
        }

        const currentValue = selectElement.value;
        const normalizedOptions = options.filter(Boolean);
        const uniqueOptions = [...new Map(normalizedOptions.map((item) => [item.value, item])).values()];
        selectElement.innerHTML = `<option value="">${defaultLabel}</option>${uniqueOptions
            .map((item) => `<option value="${item.value}">${item.label}</option>`)
            .join("")}`;
        const hasOptions = uniqueOptions.length > 0;
        selectElement.hidden = !hasOptions;
        if (hasOptions && uniqueOptions.some((option) => option.value === currentValue)) {
            selectElement.value = currentValue;
        } else {
            selectElement.value = "";
        }
    }

    function renderPickerFilters() {
        const scopedProducts = getScopedProducts();

        const categoryFilterOptions = scopedProducts
            .map((product) => ({
                value: String(product.categoryId),
                label: product.categoryName
            }))
            .sort((left, right) => left.label.localeCompare(right.label, "tr"));

        const tagFilterOptions = scopedProducts
            .flatMap((product) => product.tagNames)
            .map((tag) => ({
                value: normalizeText(tag),
                label: tag
            }))
            .sort((left, right) => left.label.localeCompare(right.label, "tr"));

        fillSelectOptions(pickerCategoryFilter, categoryFilterOptions, "Tum kategoriler");
        fillSelectOptions(pickerTagFilter, tagFilterOptions, "Tum etiketler");
    }

    function filterPickerItems() {
        const scopedProducts = getScopedProducts();
        const scopedProductIds = new Set(scopedProducts.map((product) => product.id));
        const searchTerm = normalizeText(pickerSearch?.value || "");
        const selectedCategoryFilter = pickerCategoryFilter?.value || "";
        const selectedTagFilter = pickerTagFilter?.value || "";
        const currentSlotProductId = featuredProductIds[selectedSlotIndex] || 0;

        let visibleCount = 0;

        pickerItems.forEach((item) => {
            const productId = Number.parseInt(item.dataset.productId || "0", 10);
            const product = productLookup.get(productId);
            if (!product) {
                item.hidden = true;
                return;
            }

            const matchesScope = scopedProductIds.has(productId);
            const matchesCategoryFilter = selectedCategoryFilter.length === 0 || String(product.categoryId) === selectedCategoryFilter;
            const matchesTagFilter = selectedTagFilter.length === 0
                || product.tagNames.some((tag) => normalizeText(tag) === selectedTagFilter);
            const searchableContent = normalizeText(`${product.name} ${product.categoryName} ${product.tagNames.join(" ")}`);
            const matchesSearch = searchTerm.length === 0 || searchableContent.includes(searchTerm);
            const isVisible = matchesScope && matchesCategoryFilter && matchesTagFilter && matchesSearch;

            item.hidden = !isVisible;
            item.classList.toggle("is-selected", isVisible && productId === selectedPickerProductId);

            if (isVisible) {
                visibleCount += 1;
            }
        });

        if (pickerEmpty) {
            pickerEmpty.hidden = visibleCount > 0;
        }

        if (visibleCount === 0) {
            selectedPickerProductId = 0;
            return;
        }

        const currentSelectionVisible = pickerItems.some((item) =>
            !item.hidden && Number.parseInt(item.dataset.productId || "0", 10) === selectedPickerProductId);

        if (currentSlotProductId <= 0) {
            if (!currentSelectionVisible) {
                selectedPickerProductId = 0;
            }

            pickerItems.forEach((item) => {
                const productId = Number.parseInt(item.dataset.productId || "0", 10);
                item.classList.toggle("is-selected", !item.hidden && productId === selectedPickerProductId && selectedPickerProductId > 0);
            });
            return;
        }

        if (!currentSelectionVisible) {
            const firstVisible = pickerItems.find((item) => !item.hidden);
            selectedPickerProductId = Number.parseInt(firstVisible?.dataset.productId || "0", 10);
            firstVisible?.classList.add("is-selected");
        }
    }

    function renderSlots() {
        slotElements.forEach((slot, index) => {
            const productId = featuredProductIds[index] || 0;
            const product = productLookup.get(productId);

            slot.dataset.productId = String(productId);
            slot.setAttribute("draggable", product ? "true" : "false");
            slot.classList.toggle("has-product", Boolean(product));
            slot.innerHTML = `
                <span class="admin-showcase-product-slot-index">${index + 1}</span>
                <div class="admin-showcase-product-slot-body">
                    ${product ? `<img src="${product.imageUrl}" alt="${product.name}" />
                        <strong>${product.name}</strong>
                        <small>${product.categoryName}</small>`
                    : `<strong>Urun Sec</strong><small>Bu slot icin vitrin urunu ekle</small>`}
                </div>
            `;
        });

        renderSelectedInputs();
        renderPickerFilters();
        filterPickerItems();
    }

    function openPicker(slotIndex) {
        selectedSlotIndex = slotIndex;
        selectedPickerProductId = featuredProductIds[slotIndex] || 0;
        renderPickerFilters();
        filterPickerItems();

        if (pickerBackdrop) {
            pickerBackdrop.hidden = false;
        }
    }

    function closePicker() {
        if (pickerBackdrop) {
            pickerBackdrop.hidden = true;
        }
    }

    function moveItem(fromIndex, toIndex) {
        if (fromIndex === toIndex || fromIndex < 0 || toIndex < 0) {
            return;
        }

        const next = [...featuredProductIds];
        const moved = next[fromIndex] || 0;
        if (moved <= 0) {
            return;
        }

        const target = next[toIndex] || 0;
        next[toIndex] = moved;
        next[fromIndex] = target;
        featuredProductIds = next;
        renderSlots();
    }

    setPreviewState(currentPreviewUrl);
    renderTags();
    validateDescription();
    syncThemeCards();

    previewButton?.addEventListener("click", () => {
        if (!currentPreviewUrl) {
            fileInput?.click();
            return;
        }

        openImageModal();
    });

    removeButton?.addEventListener("click", (event) => {
        event.preventDefault();
        event.stopPropagation();
        resetImageSelection();
        fileInput?.click();
    });

    fileInput?.addEventListener("change", () => {
        const [file] = fileInput.files || [];
        if (!file) {
            return;
        }

        if (uploadedPreviewUrl) {
            URL.revokeObjectURL(uploadedPreviewUrl);
        }

        uploadedPreviewUrl = URL.createObjectURL(file);
        if (hiddenInput) {
            hiddenInput.value = file.name || "__uploaded__";
        }

        setPreviewState(uploadedPreviewUrl);
        clearBackgroundValidation();
    });

    modalCloseButtons.forEach((button) => {
        button.addEventListener("click", closeImageModal);
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeImageModal();
            closePicker();
        }
    });

    tagsInput?.addEventListener("keydown", (event) => {
        if (event.key === "Enter" || event.key === ",") {
            event.preventDefault();
            addTagsFromText(tagsInput.value);
            tagsInput.value = "";
            return;
        }

        if (event.key === "Backspace" && tagsInput.value.length === 0 && tags.length > 0) {
            tags = tags.slice(0, -1);
            renderTags();
        }
    });

    tagsInput?.addEventListener("blur", () => {
        addTagsFromText(tagsInput.value);
        tagsInput.value = "";
    });

    tagsInput?.addEventListener("paste", (event) => {
        const pastedText = event.clipboardData?.getData("text");
        if (!pastedText || !/[,\n\r]/.test(pastedText)) {
            return;
        }

        event.preventDefault();
        addTagsFromText(pastedText);
    });

    tagsList?.addEventListener("click", (event) => {
        const target = event.target instanceof HTMLElement
            ? event.target.closest("[data-tag-remove]")
            : null;

        if (!target) {
            return;
        }

        const index = Number.parseInt(target.getAttribute("data-tag-remove") || "-1", 10);
        if (index < 0) {
            return;
        }

        tags.splice(index, 1);
        renderTags();
    });

    descriptionInput?.addEventListener("input", () => {
        validateDescription();
    });

    themeCards.forEach((card) => {
        card.addEventListener("click", () => {
            const option = card.querySelector("[data-showcase-theme-option]");
            if (!option) {
                return;
            }

            option.checked = true;
            option.dispatchEvent(new Event("change", { bubbles: true }));
            syncThemeCards();
        });
    });

    themeOptions.forEach((option) => {
        option.addEventListener("change", syncThemeCards);
    });

    form.addEventListener("submit", (event) => {
        if (tagsInput?.value) {
            addTagsFromText(tagsInput.value);
            tagsInput.value = "";
        }

        syncTags();

        if (!validateDescription()) {
            event.preventDefault();
        }
    });

    iconInput?.addEventListener("input", () => {
        if (iconPreview) {
            iconPreview.className = iconInput.value.trim() || "fa-solid fa-sparkles";
        }
    });

    categoryOptions.forEach((option) => {
        option.addEventListener("change", () => {
            renderPickerFilters();
            filterPickerItems();
        });
    });

    slotElements.forEach((slot) => {
        const slotIndex = Number.parseInt(slot.dataset.slotIndex || "0", 10);

        slot.addEventListener("click", () => openPicker(slotIndex));
        slot.addEventListener("dragstart", (event) => {
            const productId = Number.parseInt(slot.dataset.productId || "0", 10);
            if (productId <= 0) {
                event.preventDefault();
                return;
            }

            event.dataTransfer?.setData("text/plain", String(slotIndex));
            slot.classList.add("is-dragging");
        });

        slot.addEventListener("dragend", () => {
            slot.classList.remove("is-dragging");
        });

        slot.addEventListener("dragover", (event) => {
            event.preventDefault();
            slot.classList.add("is-drop-target");
        });

        slot.addEventListener("dragleave", () => {
            slot.classList.remove("is-drop-target");
        });

        slot.addEventListener("drop", (event) => {
            event.preventDefault();
            slot.classList.remove("is-drop-target");
            const fromIndex = Number.parseInt(event.dataTransfer?.getData("text/plain") || "-1", 10);
            moveItem(fromIndex, slotIndex);
        });
    });

    pickerCloseButton?.addEventListener("click", closePicker);
    pickerBackdrop?.addEventListener("click", (event) => {
        if (event.target === pickerBackdrop) {
            closePicker();
        }
    });

    pickerItems.forEach((item) => {
        item.addEventListener("click", () => {
            const productId = Number.parseInt(item.dataset.productId || "0", 10);
            if (productId <= 0) {
                return;
            }

            const isAlreadyInAnotherSlot = featuredProductIds.some((id, index) => index !== selectedSlotIndex && id === productId);
            if (isAlreadyInAnotherSlot) {
                window.alert("Bu urun zaten vitrinde var.");
                return;
            }

            selectedPickerProductId = productId;
            featuredProductIds = featuredProductIds.map((id, index) =>
                index !== selectedSlotIndex && id === productId ? 0 : id);
            featuredProductIds[selectedSlotIndex] = productId;
            renderSlots();
            closePicker();
        });
    });

    pickerSearch?.addEventListener("input", filterPickerItems);
    pickerCategoryFilter?.addEventListener("change", filterPickerItems);
    pickerTagFilter?.addEventListener("change", filterPickerItems);

    renderSlots();
});
