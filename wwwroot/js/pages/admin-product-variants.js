(function () {
    const editor = document.querySelector("[data-variant-editor]");
    if (!editor) {
        return;
    }

    const groupPayload = editor.querySelector("[data-variant-groups-payload]");
    const rowPayload = editor.querySelector("[data-variant-rows-payload]");
    const presetPayload = editor.querySelector("[data-variant-presets-payload]");
    const visibilityPayload = editor.querySelector("[data-variant-visibility-payload]");
    const groupInputs = editor.querySelector("[data-variant-group-inputs]");
    const rowInputs = editor.querySelector("[data-variant-row-inputs]");
    const visibilityInputs = editor.querySelector("[data-variant-visibility-inputs]");
    const emptyState = editor.querySelector("[data-variant-empty]");
    const content = editor.querySelector("[data-variant-content]");
    const groupList = editor.querySelector("[data-variant-group-list]");
    const tableHead = editor.querySelector("[data-variant-table-head]");
    const tableBody = editor.querySelector("[data-variant-table-body]");
    const searchInput = editor.querySelector("[data-variant-search-input]");
    const countLabel = editor.querySelector("[data-variant-count]");
    const groupModal = editor.querySelector("[data-variant-group-modal]");
    const groupNameInput = editor.querySelector("[data-variant-group-name]");
    const groupOptionEntryInput = editor.querySelector("[data-variant-group-option-entry]");
    const groupOptionList = editor.querySelector("[data-variant-option-list]");
    const groupOptionSubmit = editor.querySelector("[data-variant-option-submit]");
    const groupOptionClearAll = editor.querySelector("[data-variant-option-clear-all]");
    const groupPresetList = editor.querySelector("[data-variant-preset-list]");
    const groupShowOnCard = editor.querySelector("[data-variant-group-show-on-card]");
    const groupPrimary = editor.querySelector("[data-variant-group-primary]");
    const groupModalTitle = editor.querySelector("[data-variant-group-modal-title]");
    const styleButtons = Array.from(editor.querySelectorAll("[data-variant-style-option]"));
    const rowModal = editor.querySelector("[data-variant-row-modal]");
    const rowModalTitle = editor.querySelector("[data-variant-row-modal-title]");
    const rowModalSubtitle = editor.querySelector("[data-variant-row-modal-subtitle]");
    const rowTabButtons = Array.from(editor.querySelectorAll("[data-variant-row-tab]"));
    const rowPanels = Array.from(editor.querySelectorAll("[data-variant-row-panel]"));
    const rowImageInput = editor.querySelector("[data-variant-row-image]");
    const rowPriceInput = editor.querySelector("[data-variant-row-price]");
    const rowOldPriceInput = editor.querySelector("[data-variant-row-old-price]");
    const rowPurchasePriceInput = editor.querySelector("[data-variant-row-purchase-price]");
    const rowSkuInput = editor.querySelector("[data-variant-row-sku]");
    const rowBarcodeInput = editor.querySelector("[data-variant-row-barcode]");
    const rowStockInput = editor.querySelector("[data-variant-row-stock]");
    const rowDesiInput = editor.querySelector("[data-variant-row-desi]");
    const rowHsCodeInput = editor.querySelector("[data-variant-row-hscode]");
    const rowActiveInput = editor.querySelector("[data-variant-row-active]");
    const rowDefaultInput = editor.querySelector("[data-variant-row-default]");
    const columnToggles = Array.from(editor.querySelectorAll("[data-variant-column-toggle]"));
    const form = editor.closest("form");

    const parseJson = (raw, fallback) => {
        try {
            return JSON.parse(raw || "");
        } catch {
            return fallback;
        }
    };

    const basePriceInput = form?.querySelector("input[name='Price']");
    const baseOldPriceInput = form?.querySelector("input[name='OldPrice']");
    const basePurchasePriceInput = form?.querySelector("input[name='PurchasePrice']");

    const initialGroupsRaw = parseJson(groupPayload?.textContent, []);
    const initialVariantsRaw = parseJson(rowPayload?.textContent, []);
    const presets = parseJson(presetPayload?.textContent, []).map(normalizePreset);
    let fieldVisibility = normalizeVisibility(parseJson(visibilityPayload?.textContent, {}));
    let activeGroupStyle = "list";
    let editingGroupId = null;
    let editingOptionId = null;
    let editingRowId = null;
    let activeRowTab = "media";
    let nextTempId = -1;
    let draftOptions = [];
    let groups = initialGroupsRaw.map(normalizeGroup);
    let variants = initialVariantsRaw.map(normalizeVariant);
    nextTempId = getInitialTempId(groups, variants);

    function getInitialTempId(initialGroups, initialVariants) {
        const ids = [];
        initialGroups.forEach((group) => {
            ids.push(group.id);
            group.options.forEach((option) => ids.push(option.id));
        });
        initialVariants.forEach((variant) => ids.push(variant.id));
        const minId = ids.reduce((min, value) => Math.min(min, Number(value) || 0), 0);
        return minId <= 0 ? minId - 1 : -1;
    }

    function createTempId() {
        const value = nextTempId;
        nextTempId -= 1;
        return value;
    }

    function normalizeGroup(group, index) {
        return {
            id: Number(group.id ?? group.Id ?? createTempId()),
            name: String(group.name ?? group.Name ?? "").trim(),
            selectionStyle: normalizeStyle(group.selectionStyle ?? group.SelectionStyle),
            showOnCard: Boolean(group.showOnCard ?? group.ShowOnCard),
            isPrimary: Boolean(group.isPrimary ?? group.IsPrimary),
            sortOrder: Number(group.sortOrder ?? group.SortOrder ?? index ?? 0),
            options: (group.options ?? group.Options ?? []).map((option, optionIndex) => normalizeOption(option, optionIndex))
        };
    }

    function normalizeOption(option, index) {
        return {
            id: Number(option.id ?? option.Id ?? createTempId()),
            name: String(option.name ?? option.Name ?? "").trim(),
            colorHex: normalizeText(option.colorHex ?? option.ColorHex),
            swatchImageUrl: normalizeText(option.swatchImageUrl ?? option.SwatchImageUrl),
            sortOrder: Number(option.sortOrder ?? option.SortOrder ?? index ?? 0)
        };
    }

    function normalizeVariant(variant, index) {
        const optionIds = (variant.optionIds ?? variant.OptionIds ?? [])
            .map((value) => Number(value))
            .filter((value) => value !== 0);

        return {
            id: Number(variant.id ?? variant.Id ?? createTempId()),
            displayName: String(variant.displayName ?? variant.DisplayName ?? variant.optionName ?? variant.OptionName ?? "").trim(),
            groupName: String(variant.groupName ?? variant.GroupName ?? "").trim(),
            optionName: String(variant.optionName ?? variant.OptionName ?? "").trim(),
            imageUrl: normalizeText(variant.imageUrl ?? variant.ImageUrl),
            sku: normalizeText(variant.sku ?? variant.Sku),
            barcode: normalizeText(variant.barcode ?? variant.Barcode),
            price: toNumber(variant.price ?? variant.Price),
            oldPrice: toNullableNumber(variant.oldPrice ?? variant.OldPrice),
            purchasePrice: toNullableNumber(variant.purchasePrice ?? variant.PurchasePrice),
            stock: Number(variant.stock ?? variant.Stock ?? 0),
            desi: toNullableNumber(variant.desi ?? variant.Desi),
            hsCode: normalizeText(variant.hsCode ?? variant.HsCode),
            sortOrder: Number(variant.sortOrder ?? variant.SortOrder ?? index ?? 0),
            isDefault: Boolean(variant.isDefault ?? variant.IsDefault),
            isActive: Boolean(variant.isActive ?? variant.IsActive ?? true),
            optionIds
        };
    }

    function normalizePreset(preset) {
        return {
            name: String(preset.name ?? preset.Name ?? "").trim(),
            selectionStyle: normalizeStyle(preset.selectionStyle ?? preset.SelectionStyle),
            options: (preset.options ?? preset.Options ?? []).map((option) => ({
                name: String(option.name ?? option.Name ?? "").trim(),
                colorHex: normalizeText(option.colorHex ?? option.ColorHex),
                swatchImageUrl: normalizeText(option.swatchImageUrl ?? option.SwatchImageUrl)
            }))
        };
    }

    function normalizeVisibility(visibility) {
        return {
            showImage: visibility.showImage ?? visibility.ShowImage ?? true,
            showBarcode: visibility.showBarcode ?? visibility.ShowBarcode ?? true,
            showPurchasePrice: visibility.showPurchasePrice ?? visibility.ShowPurchasePrice ?? true,
            showDesi: visibility.showDesi ?? visibility.ShowDesi ?? false,
            showHsCode: visibility.showHsCode ?? visibility.ShowHsCode ?? false
        };
    }

    function normalizeStyle(style) {
        return String(style || "").trim().toLowerCase() === "visual" ? "visual" : "list";
    }

    function normalizeText(value) {
        const normalized = String(value ?? "").trim();
        return normalized ? normalized : "";
    }

    function toNumber(value) {
        const parsed = Number.parseFloat(value ?? 0);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function toNullableNumber(value) {
        if (value === null || value === undefined || value === "") {
            return null;
        }

        const parsed = Number.parseFloat(value);
        return Number.isFinite(parsed) ? parsed : null;
    }

    function getOptionById(optionId) {
        for (const group of groups) {
            const match = group.options.find((option) => option.id === optionId);
            if (match) {
                return { group, option: match };
            }
        }

        return null;
    }

    function optionIdsKey(optionIds) {
        return [...optionIds].sort((left, right) => left - right).join(":");
    }

    function ensurePrimaryGroup() {
        if (!groups.length) {
            return;
        }

        const primary = groups.find((group) => group.isPrimary) || groups[0];
        groups = groups.map((group) => ({
            ...group,
            isPrimary: group.id === primary.id
        }));
    }

    function ensurePrimaryVariant() {
        if (!variants.length) {
            return;
        }

        const primary = variants.find((variant) => variant.isDefault) || variants[0];
        variants = variants.map((variant) => ({
            ...variant,
            isDefault: variant.id === primary.id
        }));
    }

    function readBasePrice(input) {
        return input ? toNumber(input.value) : 0;
    }

    function buildVariantLabel(optionIds) {
        return optionIds
            .map((optionId) => getOptionById(optionId)?.option?.name || "")
            .filter(Boolean)
            .join(" / ");
    }

    function buildVariantGroupLabel(optionIds) {
        return optionIds
            .map((optionId) => getOptionById(optionId)?.group?.name || "")
            .filter(Boolean)
            .filter((value, index, array) => array.indexOf(value) === index)
            .join(" / ");
    }

    function generateCartesian(sourceGroups) {
        if (!sourceGroups.length || sourceGroups.some((group) => group.options.length === 0)) {
            return [];
        }

        let combinations = [[]];
        sourceGroups.forEach((group) => {
            const next = [];
            combinations.forEach((combination) => {
                group.options.forEach((option) => {
                    next.push([...combination, option.id]);
                });
            });
            combinations = next;
        });

        return combinations;
    }

    function regenerateVariants() {
        ensurePrimaryGroup();
        const sortedGroups = [...groups].sort((left, right) => left.sortOrder - right.sortOrder);
        const combinations = generateCartesian(sortedGroups);
        const existingByKey = new Map(variants.map((variant) => [optionIdsKey(variant.optionIds), variant]));

        variants = combinations.map((optionIds, index) => {
            const key = optionIdsKey(optionIds);
            const existing = existingByKey.get(key);
            const displayName = buildVariantLabel(optionIds);

            return {
                id: existing?.id ?? createTempId(),
                displayName,
                groupName: buildVariantGroupLabel(optionIds),
                optionName: displayName,
                imageUrl: existing?.imageUrl ?? "",
                sku: existing?.sku ?? "",
                barcode: existing?.barcode ?? "",
                price: existing?.price ?? readBasePrice(basePriceInput),
                oldPrice: existing?.oldPrice ?? toNullableNumber(baseOldPriceInput?.value),
                purchasePrice: existing?.purchasePrice ?? toNullableNumber(basePurchasePriceInput?.value),
                stock: existing?.stock ?? 0,
                desi: existing?.desi ?? null,
                hsCode: existing?.hsCode ?? "",
                sortOrder: index,
                isDefault: existing?.isDefault ?? index === 0,
                isActive: existing?.isActive ?? true,
                optionIds
            };
        });

        ensurePrimaryVariant();
        render();
    }

    function render() {
        renderGroups();
        renderTable();
        syncHiddenInputs();
        editor.dispatchEvent(new CustomEvent("admin:variants-changed", {
            detail: {
                groups,
                variants
            }
        }));
        const hasContent = groups.length > 0 && variants.length > 0;
        if (emptyState) {
            emptyState.hidden = hasContent;
        }
        if (content) {
            content.hidden = !hasContent;
        }
    }

    function renderGroups() {
        if (!groupList) {
            return;
        }

        const sortedGroups = [...groups].sort((left, right) => left.sortOrder - right.sortOrder);
        groupList.innerHTML = sortedGroups.map((group) => {
            const optionMarkup = group.options.map((option) => {
                const swatch = group.selectionStyle === "visual"
                    ? `<span class="admin-variant-swatch" style="${option.colorHex ? `--swatch:${escapeAttribute(option.colorHex)}` : ""}">${escapeHtml(option.name.slice(0, 1) || "?")}</span>`
                    : "";
                return `<span class="admin-variant-chip">${swatch}<span>${escapeHtml(option.name)}</span></span>`;
            }).join("");

            return `
                <div class="admin-product-selected-row" data-variant-group-row="${group.id}">
                    <div class="admin-product-selected-copy">
                        <strong>${escapeHtml(group.name)}</strong>
                        ${group.isPrimary ? `<span class="admin-product-primary-badge">Ana Se�enek</span>` : ""}
                        ${optionMarkup ? `<div class="admin-variant-chip-list">${optionMarkup}</div>` : ""}
                    </div>
                    <details class="admin-product-item-menu">
                        <summary><i class="fa-solid fa-ellipsis"></i></summary>
                        <div class="admin-product-item-menu-panel">
                            <button type="button" data-variant-group-action="edit" data-variant-group-id="${group.id}">D�zenle</button>
                            ${group.isPrimary ? "" : `<button type="button" data-variant-group-action="primary" data-variant-group-id="${group.id}">Ana Se�enek Yap</button>`}
                            <button type="button" class="is-danger" data-variant-group-action="remove" data-variant-group-id="${group.id}">Kaldir</button>
                        </div>
                    </details>
                </div>
            `;
        }).join("");
    }

    function renderTable() {
        if (!tableHead || !tableBody) {
            return;
        }

        columnToggles.forEach((toggle) => {
            const key = toggle.getAttribute("data-variant-column-toggle");
            if (key) {
                toggle.checked = Boolean(fieldVisibility[key]);
            }
        });

        tableHead.innerHTML = `
            <tr>
                ${fieldVisibility.showImage ? "<th>�r�n G�rseli</th>" : ""}
                <th>Varyantlar</th>
                <th>Satis Fiyati</th>
                <th>Indirimli Fiyat</th>
                ${fieldVisibility.showPurchasePrice ? "<th>Alis Fiyati</th>" : ""}
                <th>SKU</th>
                ${fieldVisibility.showBarcode ? "<th>Barkod</th>" : ""}
                <th>Stok</th>
                ${fieldVisibility.showDesi ? "<th>Desi</th>" : ""}
                ${fieldVisibility.showHsCode ? "<th>HS Kodu</th>" : ""}
                <th></th>
            </tr>
        `;

        const search = String(searchInput?.value || "").trim().toLocaleLowerCase("tr-TR");
        const filteredVariants = variants.filter((variant) => !search || variant.displayName.toLocaleLowerCase("tr-TR").includes(search));

        tableBody.innerHTML = filteredVariants.map((variant) => `
            <tr data-variant-row-id="${variant.id}">
                ${fieldVisibility.showImage ? `<td><button type="button" class="admin-variant-image-trigger" data-variant-row-edit="${variant.id}">${variant.imageUrl ? `<img src="${escapeAttribute(variant.imageUrl)}" alt="${escapeAttribute(variant.displayName)}" />` : `<span>+</span>`}</button></td>` : ""}
                <td>
                    <div class="admin-variant-row-copy">
                        <strong>${escapeHtml(variant.displayName)}</strong>
                        <small>${escapeHtml(variant.groupName)}</small>
                    </div>
                </td>
                <td><input class="form-control auth-input" type="number" min="0" step="0.01" data-variant-inline="price" data-variant-id="${variant.id}" value="${formatNumber(variant.price)}" /></td>
                <td><input class="form-control auth-input" type="number" min="0" step="0.01" data-variant-inline="oldPrice" data-variant-id="${variant.id}" value="${variant.oldPrice === null ? "" : formatNumber(variant.oldPrice)}" /></td>
                ${fieldVisibility.showPurchasePrice ? `<td><input class="form-control auth-input" type="number" min="0" step="0.01" data-variant-inline="purchasePrice" data-variant-id="${variant.id}" value="${variant.purchasePrice === null ? "" : formatNumber(variant.purchasePrice)}" /></td>` : ""}
                <td><input class="form-control auth-input" type="text" data-variant-inline="sku" data-variant-id="${variant.id}" value="${escapeAttribute(variant.sku)}" /></td>
                ${fieldVisibility.showBarcode ? `<td><input class="form-control auth-input" type="text" data-variant-inline="barcode" data-variant-id="${variant.id}" value="${escapeAttribute(variant.barcode)}" /></td>` : ""}
                <td><input class="form-control auth-input" type="number" min="0" step="1" data-variant-inline="stock" data-variant-id="${variant.id}" value="${variant.stock}" /></td>
                ${fieldVisibility.showDesi ? `<td><input class="form-control auth-input" type="number" min="0" step="0.01" data-variant-inline="desi" data-variant-id="${variant.id}" value="${variant.desi === null ? "" : formatNumber(variant.desi)}" /></td>` : ""}
                ${fieldVisibility.showHsCode ? `<td><input class="form-control auth-input" type="text" data-variant-inline="hsCode" data-variant-id="${variant.id}" value="${escapeAttribute(variant.hsCode)}" /></td>` : ""}
                <td>
                    <button type="button" class="admin-editor-ghost admin-variant-edit-button" data-variant-row-edit="${variant.id}">Satirlari D�zenle</button>
                </td>
            </tr>
        `).join("");

        if (countLabel) {
            countLabel.textContent = `${filteredVariants.length} / ${variants.length} varyant`;
        }
    }

    function syncHiddenInputs() {
        if (groupInputs) {
            groupInputs.innerHTML = groups
                .sort((left, right) => left.sortOrder - right.sortOrder)
                .map((group, groupIndex) => {
                    const fields = [
                        hiddenField(`VariantGroups[${groupIndex}].Id`, group.id),
                        hiddenField(`VariantGroups[${groupIndex}].Name`, group.name),
                        hiddenField(`VariantGroups[${groupIndex}].SelectionStyle`, group.selectionStyle),
                        hiddenField(`VariantGroups[${groupIndex}].ShowOnCard`, String(group.showOnCard)),
                        hiddenField(`VariantGroups[${groupIndex}].IsPrimary`, String(group.isPrimary)),
                        hiddenField(`VariantGroups[${groupIndex}].SortOrder`, group.sortOrder)
                    ];

                    group.options
                        .sort((left, right) => left.sortOrder - right.sortOrder)
                        .forEach((option, optionIndex) => {
                            fields.push(hiddenField(`VariantGroups[${groupIndex}].Options[${optionIndex}].Id`, option.id));
                            fields.push(hiddenField(`VariantGroups[${groupIndex}].Options[${optionIndex}].Name`, option.name));
                            fields.push(hiddenField(`VariantGroups[${groupIndex}].Options[${optionIndex}].ColorHex`, option.colorHex));
                            fields.push(hiddenField(`VariantGroups[${groupIndex}].Options[${optionIndex}].SwatchImageUrl`, option.swatchImageUrl));
                            fields.push(hiddenField(`VariantGroups[${groupIndex}].Options[${optionIndex}].SortOrder`, option.sortOrder));
                        });

                    return fields.join("");
                })
                .join("");
        }

        if (rowInputs) {
            rowInputs.innerHTML = variants
                .sort((left, right) => left.sortOrder - right.sortOrder)
                .map((variant, variantIndex) => {
                    const fields = [
                        hiddenField(`Variants[${variantIndex}].Id`, variant.id),
                        hiddenField(`Variants[${variantIndex}].DisplayName`, variant.displayName),
                        hiddenField(`Variants[${variantIndex}].GroupName`, variant.groupName),
                        hiddenField(`Variants[${variantIndex}].OptionName`, variant.optionName),
                        hiddenField(`Variants[${variantIndex}].ImageUrl`, variant.imageUrl),
                        hiddenField(`Variants[${variantIndex}].Sku`, variant.sku),
                        hiddenField(`Variants[${variantIndex}].Barcode`, variant.barcode),
                        hiddenField(`Variants[${variantIndex}].Price`, formatNumber(variant.price)),
                        hiddenField(`Variants[${variantIndex}].OldPrice`, variant.oldPrice === null ? "" : formatNumber(variant.oldPrice)),
                        hiddenField(`Variants[${variantIndex}].PurchasePrice`, variant.purchasePrice === null ? "" : formatNumber(variant.purchasePrice)),
                        hiddenField(`Variants[${variantIndex}].Stock`, variant.stock),
                        hiddenField(`Variants[${variantIndex}].Desi`, variant.desi === null ? "" : formatNumber(variant.desi)),
                        hiddenField(`Variants[${variantIndex}].HsCode`, variant.hsCode),
                        hiddenField(`Variants[${variantIndex}].SortOrder`, variant.sortOrder),
                        hiddenField(`Variants[${variantIndex}].IsDefault`, String(variant.isDefault)),
                        hiddenField(`Variants[${variantIndex}].IsActive`, String(variant.isActive))
                    ];

                    variant.optionIds.forEach((optionId, optionIndex) => {
                        fields.push(hiddenField(`Variants[${variantIndex}].OptionIds[${optionIndex}]`, optionId));
                    });

                    return fields.join("");
                })
                .join("");
        }

        if (visibilityInputs) {
            visibilityInputs.innerHTML = [
                hiddenField("VariantFieldVisibility.ShowImage", String(fieldVisibility.showImage)),
                hiddenField("VariantFieldVisibility.ShowBarcode", String(fieldVisibility.showBarcode)),
                hiddenField("VariantFieldVisibility.ShowPurchasePrice", String(fieldVisibility.showPurchasePrice)),
                hiddenField("VariantFieldVisibility.ShowDesi", String(fieldVisibility.showDesi)),
                hiddenField("VariantFieldVisibility.ShowHsCode", String(fieldVisibility.showHsCode))
            ].join("");
        }
    }

    function hiddenField(name, value) {
        return `<input type="hidden" name="${escapeAttribute(name)}" value="${escapeAttribute(String(value ?? ""))}" />`;
    }

    function formatNumber(value) {
        return Number(value || 0).toString().replace(",", ".");
    }

    function openGroupModal(groupId) {
        editingGroupId = groupId;
        const group = groups.find((item) => item.id === groupId) || null;
        if (groupModalTitle) {
            groupModalTitle.textContent = group ? "Varyanti D�zenle" : "Varyant Ekle";
        }
        if (groupNameInput) {
            groupNameInput.value = group?.name || "";
        }
        draftOptions = group ? group.options.map((option) => ({ ...option })) : [];
        editingOptionId = null;
        renderOptionDrafts();
        if (groupShowOnCard) {
            groupShowOnCard.checked = Boolean(group?.showOnCard);
        }
        if (groupPrimary) {
            groupPrimary.checked = Boolean(group?.isPrimary || (!group && groups.length === 0));
        }

        activeGroupStyle = group?.selectionStyle || "list";
        syncStyleButtons();
        renderPresetSuggestions();
        toggleDialog(groupModal, true);
        groupNameInput?.focus();
    }

    function closeGroupModal() {
        toggleDialog(groupModal, false);
        editingGroupId = null;
        editingOptionId = null;
        draftOptions = [];
        if (groupOptionEntryInput) {
            groupOptionEntryInput.value = "";
        }
        renderOptionDrafts();
    }

    function saveGroup() {
        const rawName = normalizeText(groupNameInput?.value);
        flushOptionEntry();
        if (!rawName || draftOptions.length === 0) {
            return;
        }

        const preset = presets.find((item) => item.name.localeCompare(rawName, "tr", { sensitivity: "base" }) === 0);
        const options = draftOptions.map((option, index) => {
            const presetOption = preset?.options.find((item) => item.name.localeCompare(option.name, "tr", { sensitivity: "base" }) === 0);
            return {
                ...option,
                colorHex: presetOption?.colorHex || option.colorHex || "",
                swatchImageUrl: presetOption?.swatchImageUrl || option.swatchImageUrl || "",
                sortOrder: index
            };
        });

        const nextGroup = {
            id: editingGroupId ?? createTempId(),
            name: rawName,
            selectionStyle: activeGroupStyle,
            showOnCard: Boolean(groupShowOnCard?.checked),
            isPrimary: Boolean(groupPrimary?.checked),
            sortOrder: groups.find((group) => group.id === editingGroupId)?.sortOrder ?? groups.length,
            options
        };

        if (editingGroupId === null) {
            groups.push(nextGroup);
        } else {
            groups = groups.map((group) => group.id === editingGroupId ? nextGroup : group);
        }

        if (nextGroup.isPrimary) {
            groups = groups.map((group) => ({
                ...group,
                isPrimary: group.id === nextGroup.id
            }));
        }

        closeGroupModal();
        regenerateVariants();
    }

    function parseOptionNames(rawValue) {
        return rawValue
            .split(/\r?\n|,/g)
            .map((value) => value.trim())
            .filter(Boolean)
            .filter((value, index, array) => array.findIndex((item) => item.localeCompare(value, "tr", { sensitivity: "base" }) === 0) === index);
    }

    function renderOptionDrafts() {
        if (!groupOptionList || !groupOptionEntryInput || !groupOptionSubmit || !groupOptionClearAll) {
            return;
        }

        const hasValue = normalizeText(groupOptionEntryInput.value).length > 0;
        groupOptionSubmit.hidden = !hasValue;
        groupOptionList.hidden = draftOptions.length === 0;
        groupOptionClearAll.hidden = draftOptions.length === 0;

        groupOptionList.innerHTML = draftOptions.map((option, index) => `
            <div class="admin-variant-option-row" data-variant-option-id="${option.id}">
                <span class="admin-variant-option-drag"><i class="fa-solid fa-grip-vertical"></i></span>
                <span class="admin-variant-option-name">${escapeHtml(option.name)}</span>
                <div class="admin-variant-option-actions">
                    <button type="button" class="admin-variant-option-icon" data-variant-option-action="edit" data-variant-option-id="${option.id}" aria-label="D�zenle">
                        <i class="fa-solid fa-pen"></i>
                    </button>
                    <button type="button" class="admin-variant-option-icon" data-variant-option-action="remove" data-variant-option-id="${option.id}" aria-label="Sil">
                        <i class="fa-regular fa-trash-can"></i>
                    </button>
                </div>
            </div>
        `).join("");
    }

    function flushOptionEntry() {
        if (!groupOptionEntryInput) {
            return;
        }

        addDraftOptions(groupOptionEntryInput.value);
    }

    function addDraftOptions(rawValue) {
        const names = parseOptionNames(rawValue);
        if (names.length === 0) {
            return;
        }

        if (editingOptionId !== null && names.length === 1) {
            draftOptions = draftOptions.map((option) => option.id === editingOptionId
                ? { ...option, name: names[0] }
                : option);
            editingOptionId = null;
        } else {
            names.forEach((name) => {
                const exists = draftOptions.some((option) => option.name.localeCompare(name, "tr", { sensitivity: "base" }) === 0);
                if (!exists) {
                    draftOptions.push({
                        id: createTempId(),
                        name,
                        colorHex: "",
                        swatchImageUrl: "",
                        sortOrder: draftOptions.length
                    });
                }
            });
        }

        draftOptions = draftOptions.map((option, index) => ({ ...option, sortOrder: index }));
        groupOptionEntryInput.value = "";
        renderOptionDrafts();
    }

    function editDraftOption(optionId) {
        const option = draftOptions.find((item) => item.id === optionId);
        if (!option || !groupOptionEntryInput) {
            return;
        }

        editingOptionId = optionId;
        groupOptionEntryInput.value = option.name;
        groupOptionEntryInput.focus();
        renderOptionDrafts();
    }

    function removeDraftOption(optionId) {
        draftOptions = draftOptions
            .filter((option) => option.id !== optionId)
            .map((option, index) => ({ ...option, sortOrder: index }));

        if (editingOptionId === optionId) {
            editingOptionId = null;
            if (groupOptionEntryInput) {
                groupOptionEntryInput.value = "";
            }
        }

        renderOptionDrafts();
    }

    function clearDraftOptions() {
        draftOptions = [];
        editingOptionId = null;
        if (groupOptionEntryInput) {
            groupOptionEntryInput.value = "";
        }
        renderOptionDrafts();
    }

    function renderPresetSuggestions() {
        if (!groupPresetList || !groupNameInput) {
            return;
        }

        const query = groupNameInput.value.trim().toLocaleLowerCase("tr-TR");
        const matches = presets.filter((preset) => query && preset.name.toLocaleLowerCase("tr-TR").includes(query));
        groupPresetList.hidden = matches.length === 0;
        groupPresetList.innerHTML = matches.map((preset) => `
            <button type="button" data-variant-preset="${escapeAttribute(preset.name)}">
                <strong>${escapeHtml(preset.name)}</strong>
                <span>${escapeHtml(preset.options.map((option) => option.name).join(", "))}</span>
            </button>
        `).join("");
    }

    function applyPreset(presetName) {
        const preset = presets.find((item) => item.name === presetName);
        if (!preset) {
            return;
        }

        if (groupNameInput) {
            groupNameInput.value = preset.name;
        }
        draftOptions = preset.options.map((option, index) => ({
            id: createTempId(),
            name: option.name,
            colorHex: option.colorHex || "",
            swatchImageUrl: option.swatchImageUrl || "",
            sortOrder: index
        }));
        editingOptionId = null;
        if (groupOptionEntryInput) {
            groupOptionEntryInput.value = "";
        }
        activeGroupStyle = preset.selectionStyle;
        syncStyleButtons();
        renderPresetSuggestions();
        renderOptionDrafts();
    }

    function syncStyleButtons() {
        styleButtons.forEach((button) => {
            button.classList.toggle("is-active", button.getAttribute("data-variant-style-option") === activeGroupStyle);
        });
    }

    function openRowModal(variantId) {
        editingRowId = variantId;
        const variant = variants.find((item) => item.id === variantId);
        if (!variant) {
            return;
        }

        if (rowModalTitle) {
            rowModalTitle.textContent = "�r�n Varyanti";
        }
        if (rowModalSubtitle) {
            rowModalSubtitle.textContent = variant.displayName;
        }

        setInputValue(rowImageInput, variant.imageUrl);
        setInputValue(rowPriceInput, variant.price);
        setInputValue(rowOldPriceInput, variant.oldPrice);
        setInputValue(rowPurchasePriceInput, variant.purchasePrice);
        setInputValue(rowSkuInput, variant.sku);
        setInputValue(rowBarcodeInput, variant.barcode);
        setInputValue(rowStockInput, variant.stock);
        setInputValue(rowDesiInput, variant.desi);
        setInputValue(rowHsCodeInput, variant.hsCode);
        if (rowActiveInput) {
            rowActiveInput.checked = variant.isActive;
        }
        if (rowDefaultInput) {
            rowDefaultInput.checked = variant.isDefault;
        }

        activateRowTab(activeRowTab);
        toggleDialog(rowModal, true);
    }

    function closeRowModal() {
        toggleDialog(rowModal, false);
        editingRowId = null;
    }

    function saveRowModal() {
        if (editingRowId === null) {
            return;
        }

        variants = variants.map((variant) => {
            if (variant.id !== editingRowId) {
                return variant;
            }

            return {
                ...variant,
                imageUrl: normalizeText(rowImageInput?.value),
                price: toNumber(rowPriceInput?.value),
                oldPrice: toNullableNumber(rowOldPriceInput?.value),
                purchasePrice: toNullableNumber(rowPurchasePriceInput?.value),
                sku: normalizeText(rowSkuInput?.value),
                barcode: normalizeText(rowBarcodeInput?.value),
                stock: Number(rowStockInput?.value || 0),
                desi: toNullableNumber(rowDesiInput?.value),
                hsCode: normalizeText(rowHsCodeInput?.value),
                isActive: Boolean(rowActiveInput?.checked),
                isDefault: Boolean(rowDefaultInput?.checked)
            };
        });

        ensurePrimaryVariant();
        closeRowModal();
        render();
    }

    function activateRowTab(tab) {
        activeRowTab = tab;
        rowTabButtons.forEach((button) => {
            button.classList.toggle("is-active", button.getAttribute("data-variant-row-tab") === tab);
        });
        rowPanels.forEach((panel) => {
            panel.hidden = panel.getAttribute("data-variant-row-panel") !== tab;
        });
    }

    function setInputValue(input, value) {
        if (!input) {
            return;
        }

        input.value = value === null || value === undefined ? "" : String(value);
    }

    function toggleDialog(dialog, isOpen) {
        if (!dialog) {
            return;
        }

        dialog.hidden = !isOpen;
        document.body.classList.toggle("is-modal-open", Boolean(document.querySelector(".admin-product-dialog-layer:not([hidden])")));
    }

    function updateInlineField(variantId, field, rawValue) {
        variants = variants.map((variant) => {
            if (variant.id !== variantId) {
                return variant;
            }

            const next = { ...variant };
            if (["price", "oldPrice", "purchasePrice", "desi"].includes(field)) {
                next[field] = field === "price" ? toNumber(rawValue) : toNullableNumber(rawValue);
            } else if (field === "stock") {
                next[field] = Number(rawValue || 0);
            } else {
                next[field] = normalizeText(rawValue);
            }
            return next;
        });

        syncHiddenInputs();
    }

    function removeGroup(groupId) {
        groups = groups.filter((group) => group.id !== groupId);
        regenerateVariants();
    }

    function setPrimaryGroup(groupId) {
        groups = groups.map((group) => ({
            ...group,
            isPrimary: group.id === groupId
        }));
        render();
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#39;");
    }

    function escapeAttribute(value) {
        return escapeHtml(value);
    }

    editor.querySelectorAll("[data-variant-add]").forEach((button) => {
        button.addEventListener("click", () => openGroupModal(null));
    });

    editor.querySelectorAll("[data-variant-group-close]").forEach((button) => {
        button.addEventListener("click", closeGroupModal);
    });

    editor.querySelector("[data-variant-group-save]")?.addEventListener("click", saveGroup);
    editor.querySelector("[data-variant-row-save]")?.addEventListener("click", saveRowModal);
    editor.querySelectorAll("[data-variant-row-close]").forEach((button) => {
        button.addEventListener("click", closeRowModal);
    });

    styleButtons.forEach((button) => {
        button.addEventListener("click", () => {
            activeGroupStyle = button.getAttribute("data-variant-style-option") || "list";
            syncStyleButtons();
        });
    });

    groupNameInput?.addEventListener("input", renderPresetSuggestions);
    groupOptionEntryInput?.addEventListener("input", renderOptionDrafts);
    groupOptionEntryInput?.addEventListener("keydown", (event) => {
        if (event.key !== "Enter") {
            return;
        }

        event.preventDefault();
        addDraftOptions(groupOptionEntryInput.value);
    });
    groupOptionSubmit?.addEventListener("click", () => addDraftOptions(groupOptionEntryInput?.value || ""));
    groupOptionClearAll?.addEventListener("click", clearDraftOptions);
    groupPresetList?.addEventListener("click", (event) => {
        const trigger = event.target instanceof Element
            ? event.target.closest("[data-variant-preset]")
            : null;
        if (!(trigger instanceof HTMLElement)) {
            return;
        }

        applyPreset(trigger.getAttribute("data-variant-preset") || "");
    });

    groupOptionList?.addEventListener("click", (event) => {
        const actionButton = event.target instanceof Element
            ? event.target.closest("[data-variant-option-action]")
            : null;
        if (!(actionButton instanceof HTMLElement)) {
            return;
        }

        const optionId = Number(actionButton.getAttribute("data-variant-option-id"));
        const action = actionButton.getAttribute("data-variant-option-action");
        if (!optionId || !action) {
            return;
        }

        if (action === "edit") {
            editDraftOption(optionId);
        }

        if (action === "remove") {
            removeDraftOption(optionId);
        }
    });

    groupList?.addEventListener("click", (event) => {
        const actionButton = event.target instanceof Element
            ? event.target.closest("[data-variant-group-action]")
            : null;
        if (!(actionButton instanceof HTMLElement)) {
            return;
        }

        const groupId = Number(actionButton.getAttribute("data-variant-group-id"));
        const action = actionButton.getAttribute("data-variant-group-action");
        if (!groupId || !action) {
            return;
        }

        if (action === "edit") {
            openGroupModal(groupId);
        }

        if (action === "remove") {
            removeGroup(groupId);
        }

        if (action === "primary") {
            setPrimaryGroup(groupId);
        }

        actionButton.closest("details")?.removeAttribute("open");
    });

    tableBody?.addEventListener("click", (event) => {
        const editButton = event.target instanceof Element
            ? event.target.closest("[data-variant-row-edit]")
            : null;
        if (!(editButton instanceof HTMLElement)) {
            return;
        }

        const variantId = Number(editButton.getAttribute("data-variant-row-edit"));
        if (variantId) {
            openRowModal(variantId);
        }
    });

    tableBody?.addEventListener("input", (event) => {
        const input = event.target instanceof Element
            ? event.target.closest("[data-variant-inline]")
            : null;
        if (!(input instanceof HTMLInputElement)) {
            return;
        }

        const variantId = Number(input.getAttribute("data-variant-id"));
        const field = input.getAttribute("data-variant-inline");
        if (variantId && field) {
            updateInlineField(variantId, field, input.value);
        }
    });

    searchInput?.addEventListener("input", renderTable);

    columnToggles.forEach((toggle) => {
        toggle.addEventListener("change", () => {
            const key = toggle.getAttribute("data-variant-column-toggle");
            if (!key) {
                return;
            }

            fieldVisibility = {
                ...fieldVisibility,
                [key]: toggle.checked
            };
            renderTable();
            syncHiddenInputs();
        });
    });

    rowTabButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const tab = button.getAttribute("data-variant-row-tab");
            if (tab) {
                activateRowTab(tab);
            }
        });
    });

    form?.addEventListener("submit", syncHiddenInputs);

    ensurePrimaryGroup();
    ensurePrimaryVariant();
    regenerateVariants();
    renderOptionDrafts();
})();
