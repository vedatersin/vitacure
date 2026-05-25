(function () {
    const builder = document.querySelector("[data-bundle-builder]");
    if (!builder) {
        return;
    }

    const form = builder.closest("form");
    const productKindSelect = form?.querySelector("select[name='ProductKind']");
    const createModeInput = form?.querySelector("input[name='CreateMode']");
    const bundleModeInput = form?.querySelector("[data-bundle-mode-input]");
    const bundlePricingModeInput = form?.querySelector("[data-bundle-pricing-mode-input]");
    const bundleAdjustmentTypeInput = form?.querySelector("[data-bundle-adjustment-type-input]");
    const bundleAdjustmentTypeSelect = form?.querySelector("[data-bundle-adjustment-type-select]");
    const bundleModeButtons = Array.from(form?.querySelectorAll("[data-bundle-mode-option]") || []);
    const bundlePricingButtons = Array.from(form?.querySelectorAll("[data-bundle-pricing-mode]") || []);
    const bundleVariantTab = form?.querySelector("[data-bundle-variant-tab]");
    const bundleVariantPanel = form?.querySelector("[data-bundle-variant-panel]");
    const productsPayload = builder.querySelector("[data-bundle-products-payload]");
    const itemsPayload = builder.querySelector("[data-bundle-items-payload]");
    const hiddenInputs = builder.querySelector("[data-bundle-item-inputs]");
    const emptyState = builder.querySelector("[data-bundle-empty]");
    const content = builder.querySelector("[data-bundle-content]");
    const groupsHost = builder.querySelector("[data-bundle-groups]");
    const addButtons = Array.from(builder.querySelectorAll("[data-bundle-add]"));
    const modeModal = builder.querySelector("[data-bundle-mode-modal]");
    const modeCloseButtons = Array.from(builder.querySelectorAll("[data-bundle-mode-close]"));
    const entryModeButtons = Array.from(builder.querySelectorAll("[data-bundle-entry-mode]"));
    const pickerModal = builder.querySelector("[data-bundle-picker-modal]");
    const pickerCloseButtons = Array.from(builder.querySelectorAll("[data-bundle-picker-close]"));
    const pickerSearchInput = builder.querySelector("[data-bundle-search-input]");
    const pickerList = builder.querySelector("[data-bundle-picker-list]");
    const pickerSaveButton = builder.querySelector("[data-bundle-picker-save]");
    const settingsModal = builder.querySelector("[data-bundle-settings-modal]");
    const settingsCloseButtons = Array.from(builder.querySelectorAll("[data-bundle-settings-close]"));
    const settingsSaveButton = builder.querySelector("[data-bundle-settings-save]");
    const settingsMinInput = builder.querySelector("[data-bundle-settings-min]");
    const settingsMaxInput = builder.querySelector("[data-bundle-settings-max]");

    const parseJson = (raw, fallback) => {
        try {
            return JSON.parse(raw || "");
        } catch {
            return fallback;
        }
    };

    let nextTempId = -1;
    let variants = [];
    let items = parseJson(itemsPayload?.textContent, []).map((item, index) => normalizeItem(item, index));
    const products = parseJson(productsPayload?.textContent, []).map(normalizeProduct);
    let activeEntryMode = "product";
    let pickerContext = { parentVariantId: null };
    let selectedPickerKeys = new Set();
    let editingSettingsKey = null;

    items.forEach((item) => {
        if (typeof item.id === "number") {
            nextTempId = Math.min(nextTempId, item.id - 1);
        }
    });

    function createTempId() {
        const value = nextTempId;
        nextTempId -= 1;
        return value;
    }

    function normalizeProduct(product) {
        return {
            id: Number(product.id ?? product.Id ?? 0),
            name: String(product.name ?? product.Name ?? "").trim(),
            slug: String(product.slug ?? product.Slug ?? "").trim(),
            imageUrl: String(product.imageUrl ?? product.ImageUrl ?? "").trim(),
            price: toNumber(product.price ?? product.Price),
            stock: Number(product.stock ?? product.Stock ?? 0),
            hasVariants: Boolean(product.hasVariants ?? product.HasVariants),
            variants: (product.variants ?? product.Variants ?? []).map((variant) => ({
                id: Number(variant.id ?? variant.Id ?? 0),
                label: String(variant.label ?? variant.Label ?? "").trim(),
                price: toNumber(variant.price ?? variant.Price),
                stock: Number(variant.stock ?? variant.Stock ?? 0)
            }))
        };
    }

    function normalizeItem(item, index) {
        const id = item.id ?? item.Id;
        const parentVariantId = toNullableInt(item.parentVariantId ?? item.ParentVariantId);
        const productVariantId = toNullableInt(item.productVariantId ?? item.ProductVariantId);
        return {
            id: typeof id === "number" ? id : toNullableInt(id),
            clientKey: typeof id === "number" ? `saved-${id}` : `temp-${createTempId()}`,
            parentVariantId,
            productId: Number(item.productId ?? item.ProductId ?? 0),
            productVariantId,
            entryMode: normalizeEntryMode(item.entryMode ?? item.EntryMode),
            productName: String(item.productName ?? item.ProductName ?? "").trim(),
            productImageUrl: String(item.productImageUrl ?? item.ProductImageUrl ?? "").trim(),
            productVariantLabel: String(item.productVariantLabel ?? item.ProductVariantLabel ?? "").trim(),
            unitPrice: toNumber(item.unitPrice ?? item.UnitPrice),
            quantity: Math.max(0, Number(item.quantity ?? item.Quantity ?? 1)),
            minQuantity: toNullableInt(item.minQuantity ?? item.MinQuantity),
            maxQuantity: toNullableInt(item.maxQuantity ?? item.MaxQuantity),
            sortOrder: Number(item.sortOrder ?? item.SortOrder ?? index ?? 0)
        };
    }

    function normalizeEntryMode(value) {
        return String(value || "").trim().toLowerCase() === "assortment" ? "assortment" : "product";
    }

    function toNumber(value) {
        const parsed = Number.parseFloat(value ?? 0);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function toNullableInt(value) {
        if (value === null || value === undefined || value === "") {
            return null;
        }

        const parsed = Number.parseInt(value, 10);
        return Number.isFinite(parsed) ? parsed : null;
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    function formatMoney(value) {
        return `TL ${toNumber(value).toFixed(2)}`;
    }

    function readCurrentVariants() {
        const payload = [];
        const byIndex = new Map();
        const inputs = Array.from(form?.querySelectorAll("input[name^='Variants[']") || []);
        inputs.forEach((input) => {
            const match = input.name.match(/^Variants\[(\d+)\]\.(.+)$/);
            if (!match) {
                return;
            }

            const index = Number(match[1]);
            const key = match[2];
            const item = byIndex.get(index) || { optionIds: [] };
            if (key.startsWith("OptionIds[")) {
                item.optionIds.push(Number(input.value));
            } else {
                item[key] = input.value;
            }
            byIndex.set(index, item);
        });

        byIndex.forEach((item) => {
            payload.push({
                id: Number(item.Id ?? 0),
                displayName: String(item.DisplayName ?? item.OptionName ?? "").trim(),
                optionName: String(item.OptionName ?? "").trim()
            });
        });

        variants = payload.filter((variant) => variant.id !== 0);
        pruneOrphanVariantItems();
    }

    function pruneOrphanVariantItems() {
        if (currentBundleMode() !== "variant") {
            items = items.map((item, index) => ({
                ...item,
                parentVariantId: null,
                sortOrder: index
            }));
            return;
        }

        const validIds = new Set(variants.map((variant) => variant.id));
        items = items
            .filter((item) => item.parentVariantId === null || validIds.has(item.parentVariantId))
            .map((item) => item.parentVariantId === null
                ? {
                    ...item,
                    parentVariantId: variants[0]?.id ?? null
                }
                : item);

        variants.forEach((variant) => resequenceItems(variant.id));
    }

    function currentBundleMode() {
        return String(bundleModeInput?.value || "simple").trim().toLowerCase() === "variant" ? "variant" : "simple";
    }

    function currentPricingMode() {
        return String(bundlePricingModeInput?.value || "manual").trim().toLowerCase() === "sum" ? "sum" : "manual";
    }

    function setBundleMode(mode) {
        const normalized = mode === "variant" ? "variant" : "simple";
        if (bundleModeInput) {
            bundleModeInput.value = normalized;
        }
        if (createModeInput) {
            createModeInput.value = normalized === "variant" ? "bundle-variant" : "bundle";
        }
        if (productKindSelect) {
            productKindSelect.value = "2";
        }

        if (normalized === "variant" && variants.length === 0) {
            readCurrentVariants();
        }

        pruneOrphanVariantItems();
        renderModeButtons();
        renderVariantVisibility();
        render();
    }

    function setPricingMode(mode) {
        if (bundlePricingModeInput) {
            bundlePricingModeInput.value = mode === "sum" ? "sum" : "manual";
        }
        renderPricingButtons();
    }

    function renderModeButtons() {
        const mode = currentBundleMode();
        bundleModeButtons.forEach((button) => {
            button.classList.toggle("is-active", button.getAttribute("data-bundle-mode-option") === mode);
        });
    }

    function renderPricingButtons() {
        const mode = currentPricingMode();
        bundlePricingButtons.forEach((button) => {
            button.classList.toggle("is-active", button.getAttribute("data-bundle-pricing-mode") === mode);
        });
    }

    function renderVariantVisibility() {
        const isVariant = currentBundleMode() === "variant";
        if (bundleVariantTab) {
            bundleVariantTab.hidden = !isVariant;
        }
        if (bundleVariantPanel) {
            bundleVariantPanel.hidden = !isVariant;
        }
    }

    function openModal(modal) {
        if (!modal) {
            return;
        }

        modal.hidden = false;
        document.body.classList.add("is-modal-open");
    }

    function closeModal(modal) {
        if (!modal) {
            return;
        }

        modal.hidden = true;
        if (!document.querySelector(".admin-product-dialog-layer:not([hidden])")) {
            document.body.classList.remove("is-modal-open");
        }
    }

    function openModeModal(parentVariantId) {
        pickerContext = { parentVariantId: parentVariantId ?? null };
        openModal(modeModal);
    }

    function openPicker(mode) {
        activeEntryMode = normalizeEntryMode(mode);
        selectedPickerKeys = new Set();
        closeModal(modeModal);
        renderPickerList();
        if (pickerSearchInput) {
            pickerSearchInput.value = "";
        }
        openModal(pickerModal);
    }

    function productSelectionKey(productId) {
        return `product:${productId}`;
    }

    function variantSelectionKey(productId, variantId) {
        return `variant:${productId}:${variantId}`;
    }

    function renderPickerList() {
        if (!pickerList) {
            return;
        }

        const query = String(pickerSearchInput?.value || "").trim().toLocaleLowerCase("tr-TR");
        const filteredProducts = products.filter((product) => {
            if (!query) {
                return true;
            }

            const variantMatch = product.variants.some((variant) => variant.label.toLocaleLowerCase("tr-TR").includes(query));
            return product.name.toLocaleLowerCase("tr-TR").includes(query) || variantMatch;
        });

        if (filteredProducts.length === 0) {
            pickerList.innerHTML = `
                <div class="admin-bundle-picker-empty">
                    <strong>�r�n bulunamadi.</strong>
                    <span>Arama kriterinizi degistirip tekrar deneyin.</span>
                </div>
            `;
            return;
        }

        pickerList.innerHTML = filteredProducts.map((product) => {
            const baseKey = productSelectionKey(product.id);
            const productChecked = selectedPickerKeys.has(baseKey);
            const variantMarkup = product.variants.map((variant) => {
                const key = variantSelectionKey(product.id, variant.id);
                return `
                    <label class="admin-bundle-picker-variant">
                        <input type="checkbox" data-bundle-pick="${escapeHtml(key)}" ${selectedPickerKeys.has(key) ? "checked" : ""} />
                        <span>${escapeHtml(variant.label)}</span>
                        <small>${formatMoney(variant.price)} · ${variant.stock} stok</small>
                    </label>
                `;
            }).join("");

            return `
                <article class="admin-bundle-picker-item">
                    <label class="admin-bundle-picker-card">
                        <input type="checkbox" data-bundle-pick="${escapeHtml(baseKey)}" ${productChecked ? "checked" : ""} />
                        <div class="admin-bundle-picker-card-main">
                            <div class="admin-bundle-picker-thumb">
                                ${product.imageUrl ? `<img src="${escapeHtml(product.imageUrl)}" alt="${escapeHtml(product.name)}" />` : `<span>${escapeHtml(product.name.slice(0, 1) || "?")}</span>`}
                            </div>
                            <div class="admin-bundle-picker-copy">
                                <strong>${escapeHtml(product.name)}</strong>
                                <span>${formatMoney(product.price)} · ${product.stock} stok</span>
                            </div>
                        </div>
                    </label>
                    ${product.hasVariants && product.variants.length > 0 ? `<div class="admin-bundle-picker-variants">${variantMarkup}</div>` : ""}
                </article>
            `;
        }).join("");
    }

    function buildItem(product, variant, entryMode) {
        return {
            id: null,
            clientKey: `temp-${createTempId()}`,
            parentVariantId: currentBundleMode() === "variant" ? pickerContext.parentVariantId : null,
            productId: product.id,
            productVariantId: variant?.id ?? null,
            entryMode,
            productName: product.name,
            productImageUrl: product.imageUrl,
            productVariantLabel: variant?.label ?? "",
            unitPrice: variant?.price ?? product.price,
            quantity: 1,
            minQuantity: null,
            maxQuantity: null,
            sortOrder: nextSortOrder(currentBundleMode() === "variant" ? pickerContext.parentVariantId : null)
        };
    }

    function nextSortOrder(parentVariantId) {
        return items.filter((item) => item.parentVariantId === (parentVariantId ?? null)).length;
    }

    function upsertItem(candidate) {
        const existing = items.find((item) =>
            item.parentVariantId === candidate.parentVariantId &&
            item.productId === candidate.productId &&
            item.productVariantId === candidate.productVariantId &&
            item.entryMode === candidate.entryMode);

        if (existing) {
            existing.quantity += candidate.quantity;
            return;
        }

        items.push(candidate);
    }

    function addSelectedProducts() {
        const additions = [];
        products.forEach((product) => {
            const baseKey = productSelectionKey(product.id);
            if (selectedPickerKeys.has(baseKey)) {
                if (activeEntryMode === "assortment" && product.hasVariants && product.variants.length > 0) {
                    product.variants.forEach((variant) => additions.push(buildItem(product, variant, "assortment")));
                } else {
                    additions.push(buildItem(product, null, activeEntryMode));
                }
            }

            product.variants.forEach((variant) => {
                const key = variantSelectionKey(product.id, variant.id);
                if (selectedPickerKeys.has(key)) {
                    additions.push(buildItem(product, variant, activeEntryMode));
                }
            });
        });

        additions.forEach(upsertItem);
        resequenceItems(currentBundleMode() === "variant" ? pickerContext.parentVariantId : null);
        closeModal(pickerModal);
        render();
    }

    function resequenceItems(parentVariantId) {
        items
            .filter((item) => item.parentVariantId === (parentVariantId ?? null))
            .sort((left, right) => left.sortOrder - right.sortOrder)
            .forEach((item, index) => {
                item.sortOrder = index;
            });
    }

    function visibleGroups() {
        if (currentBundleMode() === "variant") {
            return variants.map((variant) => ({
                key: `variant-${variant.id}`,
                parentVariantId: variant.id,
                title: variant.displayName || variant.optionName || `Varyant ${variant.id}`
            }));
        }

        return [{
            key: "bundle",
            parentVariantId: null,
            title: "Paket Icerigi"
        }];
    }

    function itemCount() {
        return items.length;
    }

    function render() {
        const hasItems = itemCount() > 0;
        if (emptyState) {
            emptyState.hidden = hasItems;
        }
        if (content) {
            content.hidden = !hasItems;
        }
        renderGroups();
        syncHiddenInputs();
    }

    function renderGroups() {
        if (!groupsHost) {
            return;
        }

        const markup = visibleGroups().map((group) => {
            const groupItems = items
                .filter((item) => item.parentVariantId === group.parentVariantId)
                .sort((left, right) => left.sortOrder - right.sortOrder);

            const rows = groupItems.length === 0
                ? `
                    <div class="admin-bundle-group-empty">
                        <span>Bu alan icin henuz urun eklenmedi.</span>
                        <button type="button" class="admin-editor-secondary" data-bundle-add-row="${group.parentVariantId ?? ""}">�r�n Ekle</button>
                    </div>
                `
                : `
                    <div class="admin-bundle-table-wrap">
                        <table class="admin-bundle-table">
                            <thead>
                                <tr>
                                    <th>�r�nler</th>
                                    <th>Satis Fiyati</th>
                                    <th>Adet</th>
                                    <th>Limitler</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                ${groupItems.map(renderRow).join("")}
                            </tbody>
                        </table>
                    </div>
                `;

            return `
                <section class="admin-bundle-group" data-bundle-group="${group.key}">
                    <div class="admin-bundle-group-head">
                        <div>
                            <strong>${escapeHtml(group.title)}</strong>
                            <span>${groupItems.length} urun</span>
                        </div>
                        <button type="button" class="admin-editor-ghost" data-bundle-add-row="${group.parentVariantId ?? ""}">�r�n Ekle</button>
                    </div>
                    ${rows}
                </section>
            `;
        }).join("");

        groupsHost.innerHTML = markup;
    }

    function renderRow(item) {
        return `
            <tr data-bundle-item-row="${escapeHtml(item.clientKey)}">
                <td>
                    <div class="admin-bundle-product-cell">
                        <div class="admin-bundle-product-thumb">
                            ${item.productImageUrl ? `<img src="${escapeHtml(item.productImageUrl)}" alt="${escapeHtml(item.productName)}" />` : `<span>${escapeHtml(item.productName.slice(0, 1) || "?")}</span>`}
                        </div>
                        <div class="admin-bundle-product-copy">
                            <strong>${escapeHtml(item.productName)}</strong>
                            <small>${escapeHtml(item.productVariantLabel || (item.entryMode === "assortment" ? "Asorti" : "�r�n"))}</small>
                        </div>
                    </div>
                </td>
                <td>${formatMoney(item.unitPrice)}</td>
                <td>
                    <input class="form-control auth-input admin-bundle-qty-input" type="number" min="0" step="1" value="${item.quantity}" data-bundle-qty="${escapeHtml(item.clientKey)}" />
                </td>
                <td>
                    <div class="admin-bundle-limit-copy">
                        <span>Minimum: ${item.minQuantity ?? 0}</span>
                        <span>Maksimum: ${item.maxQuantity ?? 1}</span>
                    </div>
                </td>
                <td>
                    <div class="admin-bundle-row-actions">
                        <button type="button" class="admin-icon-button admin-bundle-row-icon" data-bundle-settings="${escapeHtml(item.clientKey)}" aria-label="Ayarlar">
                            <i class="fa-solid fa-sliders"></i>
                        </button>
                        <button type="button" class="admin-icon-button admin-bundle-row-icon is-danger" data-bundle-remove="${escapeHtml(item.clientKey)}" aria-label="Sil">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }

    function syncHiddenInputs() {
        if (!hiddenInputs) {
            return;
        }

        const sorted = [...items]
            .sort((left, right) => (left.parentVariantId ?? 0) - (right.parentVariantId ?? 0) || left.sortOrder - right.sortOrder);

        hiddenInputs.innerHTML = sorted.map((item, index) => {
            return [
                hiddenField(`BundleItems[${index}].Id`, item.id ?? ""),
                hiddenField(`BundleItems[${index}].ParentVariantId`, item.parentVariantId ?? ""),
                hiddenField(`BundleItems[${index}].ProductId`, item.productId),
                hiddenField(`BundleItems[${index}].ProductVariantId`, item.productVariantId ?? ""),
                hiddenField(`BundleItems[${index}].EntryMode`, item.entryMode),
                hiddenField(`BundleItems[${index}].ProductName`, item.productName),
                hiddenField(`BundleItems[${index}].ProductImageUrl`, item.productImageUrl),
                hiddenField(`BundleItems[${index}].ProductVariantLabel`, item.productVariantLabel),
                hiddenField(`BundleItems[${index}].UnitPrice`, item.unitPrice.toFixed(2)),
                hiddenField(`BundleItems[${index}].Quantity`, item.quantity),
                hiddenField(`BundleItems[${index}].MinQuantity`, item.minQuantity ?? ""),
                hiddenField(`BundleItems[${index}].MaxQuantity`, item.maxQuantity ?? ""),
                hiddenField(`BundleItems[${index}].SortOrder`, item.sortOrder)
            ].join("");
        }).join("");
    }

    function hiddenField(name, value) {
        return `<input type="hidden" name="${escapeHtml(name)}" value="${escapeHtml(String(value ?? ""))}" />`;
    }

    function findItem(clientKey) {
        return items.find((item) => item.clientKey === clientKey) || null;
    }

    function openSettings(clientKey) {
        const item = findItem(clientKey);
        if (!item) {
            return;
        }

        editingSettingsKey = clientKey;
        if (settingsMinInput) {
            settingsMinInput.value = item.minQuantity ?? "";
        }
        if (settingsMaxInput) {
            settingsMaxInput.value = item.maxQuantity ?? "";
        }
        openModal(settingsModal);
    }

    function saveSettings() {
        const item = findItem(editingSettingsKey);
        if (!item) {
            closeModal(settingsModal);
            return;
        }

        item.minQuantity = toNullableInt(settingsMinInput?.value);
        item.maxQuantity = toNullableInt(settingsMaxInput?.value);
        closeModal(settingsModal);
        render();
    }

    function removeItem(clientKey) {
        const item = findItem(clientKey);
        items = items.filter((entry) => entry.clientKey !== clientKey);
        resequenceItems(item?.parentVariantId ?? null);
        render();
    }

    bundleModeButtons.forEach((button) => {
        button.addEventListener("click", () => {
            setBundleMode(button.getAttribute("data-bundle-mode-option"));
        });
    });

    bundlePricingButtons.forEach((button) => {
        button.addEventListener("click", () => {
            setPricingMode(button.getAttribute("data-bundle-pricing-mode"));
        });
    });

    if (bundleAdjustmentTypeSelect && bundleAdjustmentTypeInput) {
        bundleAdjustmentTypeSelect.value = bundleAdjustmentTypeInput.value || bundleAdjustmentTypeSelect.value;
        bundleAdjustmentTypeSelect.addEventListener("change", () => {
            bundleAdjustmentTypeInput.value = bundleAdjustmentTypeSelect.value;
        });
    }

    addButtons.forEach((button) => {
        button.addEventListener("click", () => {
            openModeModal(null);
        });
    });

    entryModeButtons.forEach((button) => {
        button.addEventListener("click", () => {
            openPicker(button.getAttribute("data-bundle-entry-mode"));
        });
    });

    modeCloseButtons.forEach((button) => {
        button.addEventListener("click", () => closeModal(modeModal));
    });

    pickerCloseButtons.forEach((button) => {
        button.addEventListener("click", () => closeModal(pickerModal));
    });

    settingsCloseButtons.forEach((button) => {
        button.addEventListener("click", () => closeModal(settingsModal));
    });

    if (pickerSaveButton) {
        pickerSaveButton.addEventListener("click", addSelectedProducts);
    }

    if (settingsSaveButton) {
        settingsSaveButton.addEventListener("click", saveSettings);
    }

    if (pickerSearchInput) {
        pickerSearchInput.addEventListener("input", renderPickerList);
    }

    if (pickerList) {
        pickerList.addEventListener("change", (event) => {
            const input = event.target.closest("[data-bundle-pick]");
            if (!(input instanceof HTMLInputElement)) {
                return;
            }

            const key = input.getAttribute("data-bundle-pick");
            if (!key) {
                return;
            }

            if (input.checked) {
                selectedPickerKeys.add(key);
            } else {
                selectedPickerKeys.delete(key);
            }
        });
    }

    if (groupsHost) {
        groupsHost.addEventListener("click", (event) => {
            const addButton = event.target.closest("[data-bundle-add-row]");
            if (addButton) {
                const rawParent = addButton.getAttribute("data-bundle-add-row");
                openModeModal(toNullableInt(rawParent));
                return;
            }

            const settingsButton = event.target.closest("[data-bundle-settings]");
            if (settingsButton) {
                openSettings(settingsButton.getAttribute("data-bundle-settings"));
                return;
            }

            const removeButton = event.target.closest("[data-bundle-remove]");
            if (removeButton) {
                removeItem(removeButton.getAttribute("data-bundle-remove"));
            }
        });

        groupsHost.addEventListener("input", (event) => {
            const input = event.target.closest("[data-bundle-qty]");
            if (!(input instanceof HTMLInputElement)) {
                return;
            }

            const item = findItem(input.getAttribute("data-bundle-qty"));
            if (!item) {
                return;
            }

            item.quantity = Math.max(0, Number.parseInt(input.value || "0", 10) || 0);
            syncHiddenInputs();
        });
    }

    if (bundleVariantPanel) {
        bundleVariantPanel.addEventListener("admin:variants-changed", (event) => {
            variants = (event.detail?.variants || []).map((variant) => ({
                id: Number(variant.id),
                displayName: String(variant.displayName || variant.optionName || "").trim(),
                optionName: String(variant.optionName || "").trim()
            }));
            pruneOrphanVariantItems();
            renderVariantVisibility();
            render();
        });
    }

    readCurrentVariants();
    renderModeButtons();
    renderPricingButtons();
    renderVariantVisibility();
    render();
})();
