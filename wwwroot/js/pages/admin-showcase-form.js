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
    const descriptionInput = form.querySelector("[data-showcase-description-input]");
    const descriptionCounter = form.querySelector("[data-showcase-description-counter]");
    const iconInput = form.querySelector('input[name="IconClass"]');
    const iconColorInput = form.querySelector('input[name="IconColor"]');
    const iconPreview = form.querySelector("[data-showcase-icon-preview]");
    const tabButtons = Array.from(form.querySelectorAll("[data-showcase-tab]"));
    const tabPanels = Array.from(form.querySelectorAll("[data-showcase-tab-panel]"));
    const promptsTextarea = form.querySelector("[data-showcase-prompts-textarea]");
    const promptsList = form.querySelector("[data-showcase-prompts-list]");
    const promptsEmpty = form.querySelector("[data-showcase-prompts-empty]");
    const addPromptButton = form.querySelector("[data-showcase-add-prompt]");
    const primaryCategorySelect = form.querySelector('select[name="PrimaryCategoryId"]');
    const categoryPoolList = form.querySelector("[data-showcase-category-pool-list]");
    const categoryPoolEmpty = form.querySelector("[data-showcase-category-pool-empty]");
    const categorySummary = form.querySelector("[data-showcase-category-summary]");
    const selectedCategoriesContainer = form.querySelector("[data-showcase-selected-categories]");
    const categoryOptionElements = Array.from(form.querySelectorAll("[data-showcase-category-option]"));
    const slotElements = Array.from(form.querySelectorAll("[data-showcase-slot]"));
    const selectedInputsContainer = form.querySelector("[data-showcase-selected-inputs]");
    const pickerBackdrop = document.querySelector("[data-showcase-picker]");
    const pickerCloseButton = document.querySelector("[data-showcase-picker-close]");
    const pickerItems = Array.from(document.querySelectorAll("[data-showcase-picker-item]"));
    const pickerSearch = document.querySelector("[data-showcase-product-search]");
    const pickerCategoryFilter = document.querySelector("[data-showcase-product-category-filter]");
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

    const categoryLookup = categoryOptionElements
        .map((item) => ({
            id: Number.parseInt(item.dataset.categoryId || "0", 10),
            rawName: item.dataset.categoryName || "",
            name: (item.dataset.categoryName || "").split("/").pop()?.trim() || item.dataset.categoryName || "",
            parentId: item.dataset.categoryParentId ? Number.parseInt(item.dataset.categoryParentId, 10) : null,
            slug: item.dataset.categorySlug || ""
        }))
        .filter((item) => item.id > 0);

    let selectedSlotIndex = 0;
    let selectedPickerProductId = 0;
    let draggedCategoryId = 0;
    let featuredProductIds = slotElements
        .map((slot) => Number.parseInt(slot.dataset.productId || "0", 10))
        .map((value) => (value > 0 ? value : 0));
    let currentPreviewUrl = hiddenInput?.value?.trim() || "";
    let uploadedPreviewUrl = "";
    let draggedPromptIndex = -1;
    let activeTab = tabButtons[0]?.getAttribute("data-showcase-tab") || "info";
    let selectedCategoryIds = Array.from(selectedCategoriesContainer?.querySelectorAll('input[name="SelectedCategoryIds"]') || [])
        .map((input) => Number.parseInt(input.value || "0", 10))
        .filter((value) => value > 0);

    function normalizeText(value) {
        return (value || "")
            .toLocaleLowerCase("tr-TR")
            .replaceAll("i", "i")
            .replaceAll("I", "i")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .trim();
    }

    function getFeaturedProducts() {
        return featuredProductIds
            .map((productId) => productLookup.get(productId))
            .filter(Boolean);
    }

    function getPrimaryCategoryId() {
        return Number.parseInt(primaryCategorySelect?.value || "0", 10);
    }

    function getDescendantCategories(primaryCategoryId) {
        if (primaryCategoryId <= 0) {
            return [];
        }

        const descendants = [];
        const queue = [primaryCategoryId];
        while (queue.length > 0) {
            const currentId = queue.shift();
            const children = categoryLookup.filter((category) => category.parentId === currentId);
            children.forEach((child) => {
                descendants.push(child);
                queue.push(child.id);
            });
        }

        return descendants;
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

        if (descriptionCounter) {
            descriptionCounter.textContent = `${descriptionInput.value.length} / ${descriptionLimit}`;
        }

        const isValid = descriptionInput.value.length <= descriptionLimit;
        setDescriptionValidationMessage(isValid ? "" : `A�iklama en fazla ${descriptionLimit} karakter olabilir.`);
        return isValid;
    }

    function syncThemeCards() {
        themeCards.forEach((card) => {
            const option = card.querySelector("[data-showcase-theme-option]");
            card.classList.toggle("is-selected", Boolean(option?.checked));
        });
    }

    function syncTabState(tab) {
        activeTab = tab;
        tabButtons.forEach((button) => {
            button.classList.toggle("is-active", button.getAttribute("data-showcase-tab") === tab);
        });
    }

    function scrollToTab(tab, smooth = true) {
        const target = tabPanels.find((panel) => panel.getAttribute("data-showcase-tab-panel") === tab);
        if (!target) {
            return;
        }

        target.scrollIntoView({
            behavior: smooth ? "smooth" : "auto",
            block: "start"
        });

        syncTabState(tab);
    }

    function syncActiveTabFromScroll() {
        if (!tabPanels.length) {
            return;
        }

        const navHeight = form.querySelector(".admin-showcase-tab-nav")?.getBoundingClientRect().height ?? 0;
        const offset = navHeight + 148;
        const candidates = tabPanels
            .map((panel) => ({
                key: panel.getAttribute("data-showcase-tab-panel"),
                distance: Math.abs(panel.getBoundingClientRect().top - offset)
            }))
            .filter((item) => item.key);

        if (!candidates.length) {
            return;
        }

        candidates.sort((left, right) => left.distance - right.distance);
        const nextTab = candidates[0].key;
        if (nextTab && nextTab !== activeTab) {
            syncTabState(nextTab);
        }
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

    function syncSelectedCategories() {
        if (!selectedCategoriesContainer) {
            return;
        }

        const primaryCategoryId = getPrimaryCategoryId();
        const normalizedIds = [...new Set(selectedCategoryIds.filter((id) => id > 0))];
        const finalIds = primaryCategoryId > 0
            ? [primaryCategoryId, ...normalizedIds.filter((id) => id !== primaryCategoryId)]
            : normalizedIds;

        selectedCategoriesContainer.innerHTML = finalIds
            .map((id) => `<input type="hidden" name="SelectedCategoryIds" value="${id}" />`)
            .join("");

        if (categorySummary) {
            categorySummary.textContent = `${normalizedIds.length} alt kategori se�ildi`;
        }
    }

    function getPromptValues() {
        return (promptsTextarea?.value || "")
            .split(/\r?\n/)
            .map((value) => value.trim())
            .filter(Boolean);
    }

    function syncPromptsTextarea(values) {
        if (promptsTextarea) {
            promptsTextarea.value = values.join("\n");
        }
    }

    function renderPromptList() {
        if (!promptsList || !promptsEmpty) {
            return;
        }

        const values = getPromptValues();
        promptsList.innerHTML = values
            .map((value, index) => `
                <div class="admin-showcase-prompt-item" draggable="true" data-showcase-prompt-item data-prompt-index="${index}">
                    <button type="button" class="admin-showcase-prompt-grip" data-showcase-prompt-grip aria-label="Sirayi degistir">
                        <i class="fa-solid fa-grip-lines"></i>
                    </button>
                    <input type="text"
                           class="form-control auth-input"
                           value="${value.replaceAll("\"", "&quot;")}"
                           data-showcase-prompt-input
                           data-prompt-index="${index}" />
                    <button type="button" class="admin-showcase-prompt-remove" data-showcase-prompt-remove aria-label="Cumleyi sil">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
            `)
            .join("");

        promptsEmpty.hidden = values.length > 0;
    }

    function addPrompt(value = "") {
        const values = getPromptValues();
        values.push(value.trim());
        syncPromptsTextarea(values.filter(Boolean));
        renderPromptList();
    }

    function updatePrompt(index, value) {
        const values = getPromptValues();
        values[index] = value;
        syncPromptsTextarea(values.filter((item) => item.trim().length > 0));
        renderPromptList();
    }

    function removePrompt(index) {
        const values = getPromptValues();
        values.splice(index, 1);
        syncPromptsTextarea(values);
        renderPromptList();
    }

    function movePrompt(fromIndex, toIndex) {
        if (fromIndex === toIndex || fromIndex < 0 || toIndex < 0) {
            return;
        }

        const values = getPromptValues();
        const [moved] = values.splice(fromIndex, 1);
        values.splice(toIndex, 0, moved);
        syncPromptsTextarea(values);
        renderPromptList();
    }

    function moveSelectedCategory(categoryId, targetId) {
        if (!categoryId || !targetId || categoryId === targetId) {
            return;
        }

        const fromIndex = selectedCategoryIds.indexOf(categoryId);
        const toIndex = selectedCategoryIds.indexOf(targetId);
        if (fromIndex < 0 || toIndex < 0) {
            return;
        }

        const next = [...selectedCategoryIds];
        const [moved] = next.splice(fromIndex, 1);
        next.splice(toIndex, 0, moved);
        selectedCategoryIds = next;
    }

    function buildCategoryPillMarkup(category, options) {
        const { isSelected, isLocked, isDraggable } = options;
        const stateClass = isSelected ? "is-selected" : "is-available";
        const lockedClass = isLocked ? " is-locked" : "";
        const iconClass = isSelected
            ? (isLocked ? "fa-grip-lines" : "fa-xmark")
            : "fa-plus";
        const srLabel = isSelected
            ? (isLocked ? "Havuzda sabit alt kategori" : "Alt kategoriyi havuzdan cikar")
            : "Alt kategoriyi havuza geri ekle";

        return `
            <button type="button"
                    class="admin-showcase-category-pill ${stateClass}${lockedClass}"
                    data-showcase-category-pill="${category.id}"
                    draggable="${isDraggable ? "true" : "false"}"
                    aria-label="${srLabel}">
                <span>${category.name}</span>
                <i class="fa-solid ${iconClass}" aria-hidden="true"></i>
            </button>
        `;
    }

    function renderCategoryPool() {
        if (!categoryPoolList || !categoryPoolEmpty) {
            return;
        }

        const primaryCategoryId = getPrimaryCategoryId();
        const descendants = getDescendantCategories(primaryCategoryId);

        if (primaryCategoryId <= 0) {
            categoryPoolList.innerHTML = "";
            categoryPoolEmpty.hidden = false;
            categoryPoolEmpty.textContent = "Ana kategori se�ildiginde alt kategoriler burada g�r�necek.";
            selectedCategoryIds = [];
            syncSelectedCategories();
            renderPickerFilters();
            filterPickerItems();
            return;
        }

        if (descendants.length === 0) {
            categoryPoolList.innerHTML = "";
            categoryPoolEmpty.hidden = false;
            categoryPoolEmpty.textContent = "Bu ana kategori i�in alt kategori bulunmuyor. Havuz ana kategori ile calisacak.";
            selectedCategoryIds = [];
            syncSelectedCategories();
            renderPickerFilters();
            filterPickerItems();
            return;
        }

        if (selectedCategoryIds.length === 0) {
            selectedCategoryIds = descendants.map((category) => category.id);
        } else {
            selectedCategoryIds = selectedCategoryIds.filter((id) => descendants.some((category) => category.id === id));
        }

        const selectedCategories = selectedCategoryIds
            .map((id) => descendants.find((category) => category.id === id))
            .filter(Boolean);
        const unselectedCategories = descendants.filter((category) => !selectedCategoryIds.includes(category.id));
        const canRemove = selectedCategoryIds.length > 1;

        categoryPoolList.innerHTML = [
            ...selectedCategories.map((category) => buildCategoryPillMarkup(category, {
                isSelected: true,
                isLocked: !canRemove,
                isDraggable: canRemove
            })),
            ...unselectedCategories.map((category) => buildCategoryPillMarkup(category, {
                isSelected: false,
                isLocked: false,
                isDraggable: false
            }))
        ].join("");

        categoryPoolEmpty.hidden = true;
        syncSelectedCategories();
        renderPickerFilters();
        filterPickerItems();
    }

    function getScopedProducts() {
        const primaryCategoryId = getPrimaryCategoryId();
        const normalizedSelectedCategoryIds = [...new Set([
            ...selectedCategoryIds,
            ...(primaryCategoryId > 0 ? [primaryCategoryId] : [])
        ])];
        const allProducts = Array.from(productLookup.values());
        const scopedProducts = allProducts.filter((product) => normalizedSelectedCategoryIds.includes(product.categoryId));

        if (scopedProducts.length > 0) {
            return scopedProducts;
        }

        const featuredProducts = getFeaturedProducts();
        if (featuredProducts.length === 0) {
            return [];
        }

        const fallbackCategoryIds = [...new Set(featuredProducts.map((product) => product.categoryId).filter((value) => value > 0))];
        return allProducts.filter((product) => {
            const matchesFallbackCategory = fallbackCategoryIds.includes(product.categoryId);
            const isCurrentFeatured = featuredProductIds.includes(product.id);
            return matchesFallbackCategory || isCurrentFeatured;
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

        fillSelectOptions(pickerCategoryFilter, categoryFilterOptions, "T�m kategoriler");
    }

    function filterPickerItems() {
        const scopedProducts = getScopedProducts();
        const scopedProductIds = new Set(scopedProducts.map((product) => product.id));
        const searchTerm = normalizeText(pickerSearch?.value || "");
        const selectedCategoryFilter = pickerCategoryFilter?.value || "";
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
            const searchableContent = normalizeText(`${product.name} ${product.categoryName} ${product.tagNames.join(" ")}`);
            const matchesSearch = searchTerm.length === 0 || searchableContent.includes(searchTerm);
            const isVisible = matchesScope && matchesCategoryFilter && matchesSearch;

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
                    : `<strong>�r�n Se�</strong><small>Bu slot i�in vitrin �r�n� ekle</small>`}
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
    validateDescription();
    syncThemeCards();
    renderPromptList();
    renderCategoryPool();

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

    form.querySelector("[data-showcase-background-trigger]")?.addEventListener("click", () => {
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

    descriptionInput?.addEventListener("input", () => {
        validateDescription();
    });

    addPromptButton?.addEventListener("click", () => {
        addPrompt("");
        const lastInput = promptsList?.querySelector('[data-showcase-prompt-input][data-prompt-index]:last-of-type');
        if (lastInput instanceof HTMLInputElement) {
            lastInput.focus();
        }
    });

    promptsList?.addEventListener("input", (event) => {
        const target = event.target instanceof HTMLInputElement
            ? event.target.closest("[data-showcase-prompt-input]")
            : null;
        if (!(target instanceof HTMLInputElement)) {
            return;
        }

        const index = Number.parseInt(target.dataset.promptIndex || "-1", 10);
        if (index < 0) {
            return;
        }

        const values = getPromptValues();
        values[index] = target.value;
        if (promptsTextarea) {
            promptsTextarea.value = values.join("\n");
        }
    });

    promptsList?.addEventListener("change", (event) => {
        const target = event.target instanceof HTMLInputElement
            ? event.target.closest("[data-showcase-prompt-input]")
            : null;
        if (!(target instanceof HTMLInputElement)) {
            return;
        }

        const index = Number.parseInt(target.dataset.promptIndex || "-1", 10);
        if (index < 0) {
            return;
        }

        updatePrompt(index, target.value.trim());
    });

    promptsList?.addEventListener("click", (event) => {
        const removeTarget = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-prompt-remove]")
            : null;
        if (!removeTarget) {
            return;
        }

        const item = removeTarget.closest("[data-showcase-prompt-item]");
        const index = Number.parseInt(item?.getAttribute("data-prompt-index") || "-1", 10);
        if (index < 0) {
            return;
        }

        removePrompt(index);
    });

    promptsList?.addEventListener("dragstart", (event) => {
        const item = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-prompt-item]")
            : null;
        if (!item) {
            return;
        }

        draggedPromptIndex = Number.parseInt(item.getAttribute("data-prompt-index") || "-1", 10);
        item.classList.add("is-dragging");
        event.dataTransfer?.setData("text/plain", String(draggedPromptIndex));
    });

    promptsList?.addEventListener("dragend", (event) => {
        const item = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-prompt-item]")
            : null;
        item?.classList.remove("is-dragging");
        promptsList.querySelectorAll(".is-drop-target").forEach((node) => node.classList.remove("is-drop-target"));
        draggedPromptIndex = -1;
    });

    promptsList?.addEventListener("dragover", (event) => {
        const item = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-prompt-item]")
            : null;
        if (!item) {
            return;
        }

        event.preventDefault();
        promptsList.querySelectorAll(".is-drop-target").forEach((node) => node.classList.remove("is-drop-target"));
        item.classList.add("is-drop-target");
    });

    promptsList?.addEventListener("drop", (event) => {
        const item = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-prompt-item]")
            : null;
        if (!item) {
            return;
        }

        event.preventDefault();
        item.classList.remove("is-drop-target");
        const toIndex = Number.parseInt(item.getAttribute("data-prompt-index") || "-1", 10);
        const fromIndex = Number.parseInt(event.dataTransfer?.getData("text/plain") || String(draggedPromptIndex), 10);
        movePrompt(fromIndex, toIndex);
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

    tabButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const tab = button.getAttribute("data-showcase-tab");
            if (tab) {
                scrollToTab(tab);
            }
        });
    });

    window.addEventListener("scroll", syncActiveTabFromScroll, { passive: true });

    form.addEventListener("submit", (event) => {
        syncSelectedCategories();

        if (!validateDescription()) {
            event.preventDefault();
        }
    });

    iconInput?.addEventListener("input", () => {
        if (iconPreview) {
            iconPreview.className = iconInput.value.trim() || "fa-solid fa-sparkles";
        }
    });

    iconColorInput?.addEventListener("input", () => {
        if (iconPreview) {
            iconPreview.style.color = iconColorInput.value.trim() || "";
        }
    });

    primaryCategorySelect?.addEventListener("change", () => {
        selectedCategoryIds = [];
        renderCategoryPool();
    });

    categoryPoolList?.addEventListener("click", (event) => {
        const target = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-category-pill]")
            : null;

        if (!target) {
            return;
        }

        const categoryId = Number.parseInt(target.getAttribute("data-showcase-category-pill") || "0", 10);
        if (categoryId <= 0) {
            return;
        }

        const canRemove = selectedCategoryIds.length > 1;
        if (selectedCategoryIds.includes(categoryId) && canRemove) {
            selectedCategoryIds = selectedCategoryIds.filter((id) => id !== categoryId);
        } else if (!selectedCategoryIds.includes(categoryId)) {
            selectedCategoryIds.push(categoryId);
        }

        renderCategoryPool();
    });

    categoryPoolList?.addEventListener("dragstart", (event) => {
        const target = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-category-pill]")
            : null;

        if (!target) {
            return;
        }

        const categoryId = Number.parseInt(target.getAttribute("data-showcase-category-pill") || "0", 10);
        if (!selectedCategoryIds.includes(categoryId)) {
            event.preventDefault();
            return;
        }

        draggedCategoryId = categoryId;
        target.classList.add("is-dragging");
        event.dataTransfer?.setData("text/plain", String(categoryId));
    });

    categoryPoolList?.addEventListener("dragend", (event) => {
        const target = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-category-pill]")
            : null;
        target?.classList.remove("is-dragging");
        draggedCategoryId = 0;
    });

    categoryPoolList?.addEventListener("dragover", (event) => {
        const target = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-category-pill]")
            : null;

        if (!target) {
            return;
        }

        event.preventDefault();
        target.classList.add("is-drop-target");
    });

    categoryPoolList?.addEventListener("dragleave", (event) => {
        const target = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-category-pill]")
            : null;
        target?.classList.remove("is-drop-target");
    });

    categoryPoolList?.addEventListener("drop", (event) => {
        const target = event.target instanceof HTMLElement
            ? event.target.closest("[data-showcase-category-pill]")
            : null;

        if (!target) {
            return;
        }

        event.preventDefault();
        target.classList.remove("is-drop-target");
        const targetId = Number.parseInt(target.getAttribute("data-showcase-category-pill") || "0", 10);
        const sourceId = Number.parseInt(event.dataTransfer?.getData("text/plain") || String(draggedCategoryId), 10);
        moveSelectedCategory(sourceId, targetId);
        renderCategoryPool();
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
                window.alert("Bu �r�n zaten vitrinde var.");
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

    syncTabState(activeTab);
    syncActiveTabFromScroll();
    renderSlots();
});
