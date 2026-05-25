(function () {
    const storageKeys = {
        viewMode: "vitacure-admin-products-view-mode",
        columns: "vitacure-admin-products-columns",
        sort: "vitacure-admin-products-sort"
    };

    const defaultColumns = ["product", "price", "old-price", "stock", "brand", "category", "tags", "created-at", "updated-at", "status"];
    const defaultPageSize = 20;
    const stateMap = new WeakMap();
    let transferMode = "import";

    const textFormatter = new Intl.Collator("tr-TR", { sensitivity: "base", numeric: true });
    const priceFormatter = new Intl.NumberFormat("tr-TR", {
        style: "currency",
        currency: "TRY",
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
    const dateFormatter = new Intl.DateTimeFormat("tr-TR", {
        day: "2-digit",
        month: "long",
        year: "numeric"
    });
    const timeFormatter = new Intl.DateTimeFormat("tr-TR", {
        hour: "2-digit",
        minute: "2-digit"
    });

    function initCatalog(root) {
        if (!root) {
            return;
        }

        const catalogDataElement = root.querySelector("[data-product-catalog-data]");
        if (!catalogDataElement) {
            return;
        }

        let payload = { products: [], filters: {}, savedFilters: [] };
        try {
            payload = JSON.parse(catalogDataElement.textContent || "{}");
        } catch {
            payload = { products: [], filters: {}, savedFilters: [] };
        }

        const state = {
            root,
            products: normalizeProducts(payload.products || []),
            filterOptions: normalizeFilterOptions(payload.filters || {}, payload.products || []),
            savedFilters: normalizeSavedFilters(payload.savedFilters || []),
            activeSavedFilterId: null,
            viewMode: loadViewMode(),
            columns: loadColumns(),
            sort: loadSort(),
            searchTerm: root.querySelector("[data-product-search-input]")?.value?.trim() || "",
            pageSize: defaultPageSize,
            currentPage: 1,
            selectedIds: new Set(),
            appliedFilters: [],
            draftFilters: [],
            filterPanelOpen: false,
            contextMenuOpen: false,
            activeValueMenu: null,
            activeSavedFilterMenuId: null,
            filterModal: null,
            filterModalFilterId: null,
            sortDraftOrder: []
        };

        stateMap.set(root, state);
        applyViewMode(root, state.viewMode);
        applyColumnVisibility(root, state.columns);
        syncSearchClearButton(state);
        syncFilterBuilder(state);
        render(state);
    }

    function normalizeProducts(products) {
        return products.map((product) => {
            const createdAt = new Date(product.createdAt);
            const updatedAt = new Date(product.updatedAt);

            return {
                id: Number(product.id),
                imageUrl: product.imageUrl || "",
                name: product.name || "",
                slug: product.slug || "",
                brandName: product.brandName || "-",
                categoryName: product.categoryName || "-",
                tagNames: Array.isArray(product.tagNames) ? product.tagNames.filter(Boolean) : [],
                salesChannels: Array.isArray(product.salesChannels) ? product.salesChannels.filter(Boolean) : [],
                price: Number(product.price || 0),
                oldPrice: product.oldPrice == null ? null : Number(product.oldPrice),
                stock: Number(product.stock || 0),
                stockSummary: product.stockSummary || "",
                isActive: Boolean(product.isActive),
                status: normalizeAdminStatus(product.status),
                createdAt,
                updatedAt,
                createdAtText: formatDateWithTime(createdAt),
                updatedAtText: formatDateWithTime(updatedAt),
                featureCount: Number(product.featureCount || 0),
                tagCount: Number(product.tagCount || 0),
                variantCount: Number(product.variantCount || 0),
                variantSummary: product.variantSummary || "-"
            };
        });
    }

    function normalizeSavedFilters(savedFilters) {
        return (Array.isArray(savedFilters) ? savedFilters : [])
            .map((filter, index) => ({
                id: Number(filter.id ?? filter.Id),
                name: String(filter.name ?? filter.Name ?? "İsimsiz Filtre").trim() || "İsimsiz Filtre",
                sortOrder: Number(filter.sortOrder ?? filter.SortOrder ?? index + 1),
                filters: sanitizeFilters(filter.filters ?? filter.Filters ?? [])
            }))
            .filter((filter) => Number.isFinite(filter.id) && filter.id > 0)
            .sort((left, right) => left.sortOrder - right.sortOrder || left.id - right.id);
    }

    function normalizeFilterOptions(filters, products) {
        const brandFallback = products.map((product) => product.brandName).filter(Boolean);
        const categoryFallback = products.map((product) => product.categoryName).filter(Boolean);
        const tagFallback = products.flatMap((product) => Array.isArray(product.tagNames) ? product.tagNames : []);
        const salesChannelFallback = products.flatMap((product) => Array.isArray(product.salesChannels) ? product.salesChannels : []);

        return {
            brands: sortTextValues((filters.brands || []).concat(brandFallback)),
            categories: sortTextValues((filters.categories || []).concat(categoryFallback)),
            tags: sortTextValues((filters.tags || []).concat(tagFallback)),
            salesChannels: sortTextValues((filters.salesChannels || []).concat(salesChannelFallback))
        };
    }

    function sortTextValues(values) {
        return [...new Set((Array.isArray(values) ? values : []).filter((value) => value && value !== "-"))]
            .sort((left, right) => textFormatter.compare(left, right));
    }

    function normalizeAdminStatus(value) {
        const normalized = String(value || "").trim().toLowerCase();
        switch (normalized) {
            case "publishedclosed":
            case "satisa kapali yayin":
                return "Satisa Kapali Yayin";
            case "publishedopen":
            case "satisa acik yayin":
            case "published":
                return "Satisa A�ik Yayin";
            case "archived":
            case "arsiv":
                return "Arsiv";
            case "draft":
            case "taslak":
                return "Taslak";
            default:
                return "Satisa A�ik Yayin";
        }
    }

    function normalizeStatus(value) {
        const normalized = String(value || "").trim().toLowerCase();
        switch (normalized) {
            case "archived":
            case "arşivlendi":
                return "Arşivlendi";
            case "draft":
            case "taslak":
                return "Taslak";
            default:
                return "Yayında";
        }
    }

    function loadViewMode() {
        const value = window.localStorage.getItem(storageKeys.viewMode);
        return value === "table" ? "table" : "grid";
    }

    function saveViewMode(value) {
        window.localStorage.setItem(storageKeys.viewMode, value);
    }

    function loadColumns() {
        try {
            const raw = window.localStorage.getItem(storageKeys.columns);
            const parsed = raw ? JSON.parse(raw) : defaultColumns;
            return Array.isArray(parsed) && parsed.length ? parsed : defaultColumns;
        } catch {
            return defaultColumns;
        }
    }

    function saveColumns(columns) {
        window.localStorage.setItem(storageKeys.columns, JSON.stringify(columns));
    }

    function loadSort() {
        const raw = window.localStorage.getItem(storageKeys.sort);
        if (!raw) {
            return { key: "updatedAt", direction: "desc" };
        }

        const [key, direction] = raw.split(":");
        return {
            key: key || "updatedAt",
            direction: direction === "asc" ? "asc" : "desc"
        };
    }

    function saveSort(sort) {
        window.localStorage.setItem(storageKeys.sort, `${sort.key}:${sort.direction}`);
    }

    function currentRoot(element) {
        return element.closest("[data-product-catalog='true']");
    }

    function applyViewMode(root, mode) {
        root.dataset.productViewMode = mode;

        root.querySelectorAll("[data-product-view-button]").forEach((button) => {
            button.classList.toggle("is-active", button.getAttribute("data-product-view-button") === mode);
        });

        root.querySelectorAll("[data-product-view-target]").forEach((section) => {
            section.hidden = section.getAttribute("data-product-view-target") !== mode;
        });

        syncContextPanel(root, mode);
    }

    function syncContextPanel(root, mode) {
        const icon = root.querySelector("[data-product-context-icon]");
        const title = root.querySelector("[data-product-context-title]");
        const copy = root.querySelector("[data-product-context-copy]");
        const button = root.querySelector("[data-product-context-toggle]");

        root.querySelectorAll("[data-product-context-panel]").forEach((panel) => {
            panel.hidden = panel.getAttribute("data-product-context-panel") !== (mode === "table" ? "columns" : "sort");
        });

        if (mode === "table") {
            if (icon) {
                icon.className = "fa-solid fa-sliders";
            }
            if (title) {
                title.textContent = "Kolonlar";
            }
            if (copy) {
                copy.textContent = "Liste görünümündeki sütunları açıp kapatın";
            }
            if (button) {
                button.setAttribute("aria-label", "Kolonlar");
            }
            return;
        }

        if (icon) {
            icon.className = "fa-solid fa-arrow-down-wide-short";
        }
        if (title) {
            title.textContent = "Sıralama";
        }
        if (copy) {
            copy.textContent = "Döşeme görünümünde gösterim sıralaması";
        }
        if (button) {
            button.setAttribute("aria-label", "Sıralama");
        }
    }

    function applyColumnVisibility(root, columns) {
        const normalized = new Set(columns);

        root.querySelectorAll("[data-product-column]").forEach((cell) => {
            const key = cell.getAttribute("data-product-column");
            cell.hidden = key ? !normalized.has(key) : false;
        });

        root.querySelectorAll("[data-product-column-option]").forEach((input) => {
            const key = input.getAttribute("data-product-column-option");
            input.checked = key ? normalized.has(key) : false;
        });
    }

    function syncSearchClearButton(state) {
        const clearButton = state.root.querySelector("[data-product-search-clear]");
        if (!clearButton) {
            return;
        }

        clearButton.hidden = !state.searchTerm;
    }

    function sanitizeFilters(filters) {
        return (Array.isArray(filters) ? filters : [])
            .map((filter) => {
                const field = String(filter.field ?? filter.Field ?? "").trim();
                const operator = String(filter.operator ?? filter.Operator ?? "").trim();
                const value = typeof (filter.value ?? filter.Value) === "string" ? String(filter.value ?? filter.Value).trim() : "";
                const valuesSource = filter.values ?? filter.Values;
                const values = Array.isArray(valuesSource)
                    ? [...new Set(valuesSource.filter(Boolean).map((entry) => String(entry).trim()))].sort((left, right) => textFormatter.compare(left, right))
                    : [];

                return {
                    field,
                    operator,
                    value,
                    values
                };
            })
            .filter((filter) => filter.field && filter.operator && (filter.values.length || filter.value));
    }

    function cloneFilters(filters) {
        return sanitizeFilters(filters).map((filter) => ({
            field: filter.field,
            operator: filter.operator,
            value: filter.value,
            values: [...filter.values]
        }));
    }

    function syncFilterBuilder(state) {
        const root = state.root;
        const rowsContainer = root.querySelector("[data-product-filter-rows]");
        const emptyState = root.querySelector("[data-product-filter-empty]");
        const applyButton = root.querySelector("[data-product-filter-apply]");
        const addButton = root.querySelector("[data-product-filter-add]");

        if (!rowsContainer || !emptyState) {
            return;
        }

        rowsContainer.innerHTML = "";

        if (!state.draftFilters.length) {
            emptyState.hidden = false;
        } else {
            emptyState.hidden = true;
            state.draftFilters.forEach((filter, index) => {
                const row = document.createElement("div");
                row.className = "admin-product-filter-row";
                row.dataset.filterIndex = String(index);
                row.innerHTML = buildFilterRowMarkup(filter, state, index);
                rowsContainer.appendChild(row);
            });
        }

        if (applyButton) {
            applyButton.disabled = !canApplyFilters(state);
        }

        if (addButton) {
            addButton.disabled = !canAddMoreFilters(state);
        }

        if (typeof state.activeValueMenu === "number") {
            const activeRow = rowsContainer.querySelector(`.admin-product-filter-row[data-filter-index='${state.activeValueMenu}']`);
            const activeMenu = activeRow?.querySelector("[data-filter-value-menu]");
            const activeButton = activeRow?.querySelector("[data-filter-value-toggle]");
            if (activeMenu) {
                activeMenu.hidden = false;
            }
            if (activeButton) {
                activeButton.setAttribute("aria-expanded", "true");
            }
        }
    }

    function canApplyFilters(state) {
        if (!state.draftFilters.length) {
            return false;
        }

        return state.draftFilters.every(isFilterValid);
    }

    function canAddMoreFilters(state) {
        return Boolean(getNextAvailableField(state));
    }

    function getNextAvailableField(state) {
        return getFieldDefinitions(state).find((field) => !field.disabled)?.key || null;
    }

    function buildFilterRowMarkup(filter, state, index) {
        const config = getFieldConfig(filter.field, state, index);
        const fieldOptions = buildFieldOptions(filter.field, state, index);
        const operatorOptions = buildOperatorOptions(config, filter.operator);
        const valueMarkup = buildValueControlMarkup(filter, config);

        return `
            <label class="admin-product-filter-cell">
                <select data-filter-field>
                    ${fieldOptions}
                </select>
            </label>
            <label class="admin-product-filter-cell">
                <select data-filter-operator>
                    ${operatorOptions}
                </select>
            </label>
            <div class="admin-product-filter-cell admin-product-filter-value-cell">
                ${valueMarkup}
            </div>
            <button type="button" class="admin-product-filter-remove" data-filter-remove aria-label="Filtreyi kaldır">
                <i class="fa-regular fa-trash-can"></i>
            </button>
        `;
    }

    function buildFieldOptions(selectedField, state, index) {
        return getFieldDefinitions(state, index).map((field) => {
            const disabled = field.disabled ? " disabled" : "";
            const selected = field.key === selectedField ? " selected" : "";
            return `<option value="${field.key}"${selected}${disabled}>${escapeHtml(field.label)}</option>`;
        }).join("");
    }

    function buildOperatorOptions(config, selectedOperator) {
        return config.operators.map((operator) => {
            const selected = operator.value === selectedOperator ? " selected" : "";
            return `<option value="${operator.value}"${selected}>${escapeHtml(operator.label)}</option>`;
        }).join("");
    }

    function buildValueControlMarkup(filter, config) {
        if (config.valueType === "date") {
            return `<input type="date" value="${escapeAttribute(filter.value || "")}" data-filter-date />`;
        }

        if (config.valueType === "multi") {
            const selectedValues = Array.isArray(filter.values) ? filter.values : [];
            const selectedLabels = selectedValues.length
                ? selectedValues.map((value) => `<span>${escapeHtml(value)}</span>`).join("")
                : `<span class="is-placeholder">Seçiniz</span>`;

            const optionItems = config.options.length
                ? config.options.map((option) => {
                    const checked = selectedValues.includes(option.value) ? " checked" : "";
                    const disabled = option.disabled ? " disabled" : "";
                    const className = option.disabled ? "admin-product-option-check is-disabled" : "admin-product-option-check";
                    return `
                        <label class="${className}">
                            <input type="checkbox" value="${escapeAttribute(option.value)}" data-filter-option${checked}${disabled} />
                            <span>${escapeHtml(option.label)}</span>
                        </label>
                    `;
                }).join("")
                : `<div class="admin-product-option-empty">${escapeHtml(config.emptyMessage || "Bu alan için seçenek bulunamadı.")}</div>`;

            return `
                <button type="button" class="admin-product-value-trigger" data-filter-value-toggle aria-expanded="false">
                    <span class="admin-product-value-pills">${selectedLabels}</span>
                    <i class="fa-solid fa-chevron-down"></i>
                </button>
                <div class="admin-product-value-menu" data-filter-value-menu hidden>
                    ${optionItems}
                </div>
            `;
        }

        return `<input type="text" value="${escapeAttribute(filter.value || "")}" data-filter-text />`;
    }

    function getFieldDefinitions(state, currentIndex) {
        const definitions = [
            { key: "brand", label: "Marka" },
            { key: "tag", label: "Etiket" },
            { key: "category", label: "Kategori" },
            { key: "salesChannel", label: "Satış Kanalı", disabled: state.filterOptions.salesChannels.length === 0 },
            { key: "status", label: "Durum" },
            { key: "stock", label: "Stok" },
            { key: "createdAt", label: "Oluşturulma Tarihi" },
            { key: "updatedAt", label: "Güncellenme Tarihi" }
        ];

        const usedFields = new Set(
            state.draftFilters
                .map((filter, index) => index === currentIndex ? null : filter.field)
                .filter(Boolean)
        );

        return definitions.map((field) => ({
            ...field,
            disabled: Boolean(field.disabled || usedFields.has(field.key))
        }));
    }

    function getFieldConfig(field, state, index) {
        switch (field) {
            case "brand":
                return {
                    valueType: "multi",
                    options: buildOptionEntries(getAvailableOptionValues(state, index, "brand"), state.draftFilters[index]?.values || []),
                    emptyMessage: "Kayıtlı marka bulunmuyor.",
                    operators: [
                        { value: "includes", label: "İçeren" },
                        { value: "excludes", label: "İçermeyen" }
                    ]
                };
            case "tag":
                return {
                    valueType: "multi",
                    options: buildOptionEntries(getAvailableOptionValues(state, index, "tag"), state.draftFilters[index]?.values || []),
                    emptyMessage: "Kayıtlı etiket bulunmuyor.",
                    operators: [
                        { value: "includes", label: "İçeren" },
                        { value: "excludes", label: "İçermeyen" }
                    ]
                };
            case "category":
                return {
                    valueType: "multi",
                    options: buildOptionEntries(getAvailableOptionValues(state, index, "category"), state.draftFilters[index]?.values || []),
                    emptyMessage: "Kayıtlı kategori bulunmuyor.",
                    operators: [
                        { value: "includes", label: "İçeren" },
                        { value: "excludes", label: "İçermeyen" }
                    ]
                };
            case "salesChannel":
                return {
                    valueType: "multi",
                    options: buildOptionEntries(getAvailableOptionValues(state, index, "salesChannel"), state.draftFilters[index]?.values || []),
                    emptyMessage: "Bu katalogda henüz satış kanalı verisi yok.",
                    operators: [
                        { value: "includes", label: "İçeren" },
                        { value: "excludes", label: "İçermeyen" }
                    ]
                };
            case "status":
                return {
                    valueType: "multi",
                    options: buildOptionEntries(getAvailableOptionValues(state, index, "status"), state.draftFilters[index]?.values || []),
                    operators: [
                        { value: "includes", label: "İçeren" },
                        { value: "excludes", label: "İçermeyen" }
                    ]
                };
            case "stock":
                return {
                    valueType: "multi",
                    options: buildOptionEntries(getAvailableOptionValues(state, index, "stock"), state.draftFilters[index]?.values || []),
                    operators: [
                        { value: "includes", label: "İçeren" },
                        { value: "excludes", label: "İçermeyen" }
                    ]
                };
            case "createdAt":
            case "updatedAt":
                return {
                    valueType: "date",
                    operators: [
                        { value: "on", label: "Tarihinde" },
                        { value: "before", label: "Öncesi" },
                        { value: "after", label: "Sonrası" }
                    ]
                };
            default:
                return {
                    valueType: "text",
                    operators: [
                        { value: "contains", label: "İçeren" }
                    ]
                };
        }
    }

    function buildOptionEntries(availableValues, selectedValues) {
        const selected = Array.isArray(selectedValues) ? selectedValues : [];
        const merged = [...new Set([...(availableValues || []), ...selected])];

        return merged.map((value) => ({
            value,
            label: value,
            disabled: false
        }));
    }

    function getAvailableOptionValues(state, index, field) {
        const otherFilters = state.draftFilters.filter((_, filterIndex) => filterIndex !== index && isFilterValid(state.draftFilters[filterIndex]));
        const scopedProducts = state.products.filter((product) => otherFilters.every((filter) => matchesFilter(product, filter)));

        switch (field) {
            case "brand":
                return sortTextValues(scopedProducts.map((product) => product.brandName));
            case "tag":
                return sortTextValues(scopedProducts.flatMap((product) => product.tagNames));
            case "category":
                return sortTextValues(scopedProducts.map((product) => product.categoryName));
            case "salesChannel":
                return sortTextValues(scopedProducts.flatMap((product) => product.salesChannels));
            case "status":
                return sortTextValues(scopedProducts.map((product) => product.status));
            case "stock":
                return sortTextValues(scopedProducts.map((product) => product.stock > 0 ? "Stokta var" : "Stok yok"));
            default:
                return [];
        }
    }

    function createEmptyFilter(state) {
        const nextField = getNextAvailableField(state);
        const config = getFieldConfig(nextField || "brand", state, state.draftFilters.length);

        return {
            field: nextField || "brand",
            operator: config.operators[0]?.value || "includes",
            values: config.valueType === "multi" ? [] : [],
            value: config.valueType === "date" ? "" : ""
        };
    }

    function isFilterValid(filter) {
        if (!filter || !filter.field || !filter.operator) {
            return false;
        }

        if (Array.isArray(filter.values) && filter.values.length) {
            return true;
        }

        return Boolean(String(filter.value || "").trim());
    }

    function filterProducts(state) {
        const search = state.searchTerm.trim().toLocaleLowerCase("tr-TR");

        return state.products.filter((product) => {
            if (search) {
                const haystack = [
                    product.name,
                    product.slug,
                    product.brandName,
                    product.categoryName,
                    product.tagNames.join(" "),
                    product.salesChannels.join(" ")
                ].join(" ").toLocaleLowerCase("tr-TR");

                if (!haystack.includes(search)) {
                    return false;
                }
            }

            return state.appliedFilters.every((filter) => matchesFilter(product, filter));
        });
    }

    function matchesFilter(product, filter) {
        if (!filter || !filter.field) {
            return true;
        }

        switch (filter.field) {
            case "brand":
                return matchMulti([product.brandName], filter);
            case "tag":
                return matchMulti(product.tagNames, filter);
            case "category":
                return matchMulti([product.categoryName], filter);
            case "salesChannel":
                return matchMulti(product.salesChannels, filter);
            case "status":
                return matchMulti([product.status], filter);
            case "stock":
                return matchMulti([product.stock > 0 ? "Stokta var" : "Stok yok"], filter);
            case "createdAt":
                return matchDate(product.createdAt, filter);
            case "updatedAt":
                return matchDate(product.updatedAt, filter);
            default:
                return true;
        }
    }

    function matchMulti(productValues, filter) {
        const selected = Array.isArray(filter.values) ? filter.values.filter(Boolean) : [];
        if (!selected.length) {
            return true;
        }

        const normalizedProductValues = productValues.map((value) => String(value).toLocaleLowerCase("tr-TR"));
        const hasMatch = selected.some((value) => normalizedProductValues.includes(String(value).toLocaleLowerCase("tr-TR")));

        return filter.operator === "excludes" ? !hasMatch : hasMatch;
    }

    function matchDate(productDate, filter) {
        if (!(productDate instanceof Date) || Number.isNaN(productDate.getTime()) || !filter.value) {
            return true;
        }

        const filterDate = new Date(filter.value);
        if (Number.isNaN(filterDate.getTime())) {
            return true;
        }

        const left = normalizeDay(productDate).getTime();
        const right = normalizeDay(filterDate).getTime();

        if (filter.operator === "before") {
            return left < right;
        }

        if (filter.operator === "after") {
            return left > right;
        }

        return left === right;
    }

    function normalizeDay(date) {
        return new Date(date.getFullYear(), date.getMonth(), date.getDate());
    }

    function sortProducts(products, sort) {
        const direction = sort.direction === "asc" ? 1 : -1;
        const items = [...products];

        items.sort((left, right) => {
            let comparison = 0;

            switch (sort.key) {
                case "name":
                    comparison = textFormatter.compare(left.name, right.name);
                    break;
                case "price":
                    comparison = left.price - right.price;
                    break;
                case "stock":
                    comparison = left.stock - right.stock;
                    break;
                case "createdAt":
                    comparison = left.createdAt.getTime() - right.createdAt.getTime();
                    break;
                case "updatedAt":
                    comparison = left.updatedAt.getTime() - right.updatedAt.getTime();
                    break;
                default:
                    comparison = left.updatedAt.getTime() - right.updatedAt.getTime();
                    break;
            }

            if (comparison === 0) {
                comparison = textFormatter.compare(left.name, right.name);
            }

            return comparison * direction;
        });

        return items;
    }

    function paginateProducts(products, currentPage, pageSize) {
        const totalPages = Math.max(1, Math.ceil(products.length / pageSize));
        const page = Math.min(Math.max(1, currentPage), totalPages);
        const start = (page - 1) * pageSize;
        const end = start + pageSize;

        return {
            page,
            totalPages,
            start,
            end,
            items: products.slice(start, end)
        };
    }

    function render(state) {
        const filtered = filterProducts(state);
        const sorted = sortProducts(filtered, state.sort);
        const pagination = paginateProducts(sorted, state.currentPage, state.pageSize);
        state.currentPage = pagination.page;

        renderSavedFilterTabs(state);
        renderAppliedFilters(state);
        renderGrid(state, pagination.items);
        renderTable(state, pagination.items);
        renderEmptyState(state, filtered.length === 0);
        renderPagination(state, filtered.length, pagination);
        renderSelection(state, pagination.items);
        syncSortUi(state);
        syncSearchClearButton(state);
        syncFilterBuilder(state);
        syncSaveButtons(state);
        syncSortDialog(state);
    }

    function renderSavedFilterTabs(state) {
        const container = state.root.querySelector("[data-product-saved-filter-tabs]");
        if (!container) {
            return;
        }

        if (!state.savedFilters.length) {
            container.hidden = true;
            container.innerHTML = "";
            return;
        }

        container.hidden = false;

        const savedMarkup = state.savedFilters.map((filter) => {
            const isActive = state.activeSavedFilterId === filter.id;
            const menuOpen = state.activeSavedFilterMenuId === filter.id;

            return `
                <div class="admin-product-saved-filter-item">
                    <div class="admin-product-saved-filter-tab${isActive ? " is-active" : ""}">
                        <button type="button" class="admin-product-saved-filter-button" data-product-saved-filter-select="${filter.id}">
                            ${escapeHtml(filter.name)}
                        </button>
                        <button type="button" class="admin-product-saved-filter-menu-toggle" data-product-saved-filter-menu-toggle="${filter.id}" aria-expanded="${menuOpen ? "true" : "false"}">
                            <i class="fa-solid fa-chevron-down"></i>
                        </button>
                    </div>
                    <div class="admin-product-saved-filter-menu" data-product-saved-filter-menu="${filter.id}"${menuOpen ? "" : " hidden"}>
                        <button type="button" data-product-saved-filter-rename="${filter.id}">Filtre İsmini Değiştir</button>
                        <button type="button" data-product-saved-filter-sort="${filter.id}">Filtre Sıralama</button>
                        <button type="button" class="is-danger" data-product-saved-filter-delete="${filter.id}">Filtreyi Sil</button>
                    </div>
                </div>
            `;
        }).join("");

        container.innerHTML = `
            <div class="admin-product-saved-filter-tab${state.activeSavedFilterId == null ? " is-active" : ""}">
                <button type="button" class="admin-product-saved-filter-button" data-product-saved-filter-all>Hepsi</button>
            </div>
            ${savedMarkup}
        `;
    }

    function renderAppliedFilters(state) {
        const container = state.root.querySelector("[data-product-applied-filters]");
        if (!container) {
            return;
        }

        if (!state.appliedFilters.length) {
            container.hidden = true;
            container.innerHTML = "";
            return;
        }

        container.hidden = false;
        container.innerHTML = `
            ${state.appliedFilters.map((filter, index) => `
                <div class="admin-product-filter-chip">
                    <span>${escapeHtml(buildFilterSummary(filter))}</span>
                    <button type="button" data-product-applied-filter-remove="${index}" aria-label="Filtreyi kaldır">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
            `).join("")}
            <button type="button" class="admin-product-applied-clear" data-product-applied-filters-clear>Temizle</button>
        `;
    }

    function syncSaveButtons(state) {
        const saveButton = state.root.querySelector("[data-product-filter-save]");
        const saveChangesButton = state.root.querySelector("[data-product-filter-save-changes]");
        const activeSaved = getActiveSavedFilter(state);
        const hasAppliedFilters = state.appliedFilters.length > 0;
        const isDirty = activeSaved ? !areFiltersEqual(activeSaved.filters, state.appliedFilters) : false;

        if (saveButton) {
            saveButton.hidden = !hasAppliedFilters || Boolean(activeSaved);
            saveButton.disabled = !hasAppliedFilters || Boolean(activeSaved);
        }

        if (saveChangesButton) {
            saveChangesButton.hidden = !activeSaved || !isDirty;
            saveChangesButton.disabled = !activeSaved || !isDirty;
        }
    }

    function getActiveSavedFilter(state) {
        return state.savedFilters.find((filter) => filter.id === state.activeSavedFilterId) || null;
    }

    function buildFilterSummary(filter) {
        const labels = {
            brand: "Marka",
            tag: "Etiket",
            category: "Kategori",
            salesChannel: "Satış Kanalı",
            status: "Durum",
            stock: "Stok",
            createdAt: "Oluşturulma Tarihi",
            updatedAt: "Güncellenme Tarihi"
        };

        const operators = {
            includes: "İçeren",
            excludes: "İçermeyen",
            on: "Tarihinde",
            before: "Öncesi",
            after: "Sonrası"
        };

        const valueText = Array.isArray(filter.values) && filter.values.length
            ? filter.values.join(", ")
            : filter.value;

        return `${labels[filter.field] || filter.field} (${operators[filter.operator] || filter.operator}) ${valueText || ""}`.trim();
    }

    function areFiltersEqual(left, right) {
        return JSON.stringify(cloneFilters(left)) === JSON.stringify(cloneFilters(right));
    }

    function renderGrid(state, products) {
        const container = state.root.querySelector("[data-product-view-target='grid']");
        if (!container) {
            return;
        }

        container.innerHTML = products.map((product) => {
            const oldPrice = product.oldPrice && product.oldPrice > product.price
                ? `<span>${escapeHtml(priceFormatter.format(product.oldPrice))}</span>`
                : "";
            const variantPill = product.variantCount > 0
                ? `<span class="admin-product-metric-pill">${product.variantCount} varyant</span>`
                : "";

            return `
                <article class="admin-product-card">
                    <div class="admin-product-card-topline">
                        ${variantPill}
                    </div>
                    <a href="/admin/products/edit/${product.id}" class="admin-product-card-media">
                        <img src="${escapeAttribute(product.imageUrl)}" alt="${escapeAttribute(product.name)}" loading="lazy" />
                    </a>
                    <div class="admin-product-card-body">
                        <div class="admin-product-card-copy">
                            <a href="/admin/products/edit/${product.id}" class="admin-product-card-title">${escapeHtml(product.name)}</a>
                            <div class="admin-product-card-meta">${escapeHtml(product.brandName)} <span>&bull;</span> ${escapeHtml(product.categoryName)}</div>
                        </div>
                        <div class="admin-product-card-price">
                            <strong>${escapeHtml(priceFormatter.format(product.price))}</strong>
                            ${oldPrice}
                        </div>
                        <div class="admin-product-card-footer">
                            <div>
                                <strong>${product.stock} adet</strong>
                                <span>${escapeHtml(product.stockSummary)}</span>
                            </div>
                            <a href="/admin/products/edit/${product.id}" class="admin-product-card-link">Düzenle</a>
                        </div>
                    </div>
                </article>
            `;
        }).join("");
    }

    function renderTable(state, products) {
        const tbody = state.root.querySelector("[data-product-table-body]");
        if (!tbody) {
            return;
        }

        tbody.innerHTML = products.map((product) => {
            const selected = state.selectedIds.has(product.id) ? " checked" : "";
            const tags = product.tagNames.length
                ? product.tagNames.map((tag) => `<span class="admin-product-inline-pill">${escapeHtml(tag)}</span>`).join("")
                : `<span class="text-muted">-</span>`;
            const oldPrice = product.oldPrice && product.oldPrice > product.price
                ? `<span class="admin-product-old-price">${escapeHtml(priceFormatter.format(product.oldPrice))}</span>`
                : `<span class="text-muted">-</span>`;

            return `
                <tr data-product-row="${product.id}">
                    <td class="admin-product-table-check">
                        <input type="checkbox" aria-label="${escapeAttribute(product.name)} seç" data-product-row-select="${product.id}"${selected} />
                    </td>
                    <td data-product-column="product">
                        <div class="admin-product-table-product">
                            <img src="${escapeAttribute(product.imageUrl)}" alt="${escapeAttribute(product.name)}" class="admin-product-thumb" loading="lazy" />
                            <div>
                                <strong>${escapeHtml(product.name)}</strong>
                                <span>${escapeHtml(product.slug)}</span>
                            </div>
                        </div>
                    </td>
                    <td data-product-column="price">
                        <strong>${escapeHtml(priceFormatter.format(product.price))}</strong>
                    </td>
                    <td data-product-column="old-price">${oldPrice}</td>
                    <td data-product-column="stock">
                        <strong>${product.stock} adet</strong>
                        <div class="text-muted small">${escapeHtml(product.stockSummary)}</div>
                    </td>
                    <td data-product-column="brand">${escapeHtml(product.brandName)}</td>
                    <td data-product-column="category">${escapeHtml(product.categoryName)}</td>
                    <td data-product-column="tags">
                        <div class="admin-product-inline-pills">${tags}</div>
                    </td>
                    <td data-product-column="created-at">
                        <strong>${escapeHtml(formatDate(product.createdAt))}</strong>
                        <div class="text-muted small">${escapeHtml(formatTime(product.createdAt))}</div>
                    </td>
                    <td data-product-column="updated-at">
                        <strong>${escapeHtml(formatDate(product.updatedAt))}</strong>
                        <div class="text-muted small">${escapeHtml(formatTime(product.updatedAt))}</div>
                    </td>
                    <td data-product-column="status">${escapeHtml(product.status)}</td>
                    <td class="text-end">
                        <a href="/admin/products/edit/${product.id}" class="btn auth-table-link">Düzenle</a>
                    </td>
                </tr>
            `;
        }).join("");

        applyColumnVisibility(state.root, state.columns);
    }

    function renderEmptyState(state, isEmpty) {
        const emptyState = state.root.querySelector("[data-product-empty-state]");
        const views = state.root.querySelector(".admin-product-view-stack");
        if (!emptyState || !views) {
            return;
        }

        emptyState.hidden = !isEmpty;
        views.hidden = isEmpty;
    }

    function renderPagination(state, totalItems, pagination) {
        const summary = state.root.querySelector("[data-product-page-summary]");
        const prevButton = state.root.querySelector("[data-product-page-prev]");
        const nextButton = state.root.querySelector("[data-product-page-next]");
        const numbers = state.root.querySelector("[data-product-page-numbers]");
        const select = state.root.querySelector("[data-product-page-size]");

        if (select) {
            select.value = String(state.pageSize);
        }

        const start = totalItems === 0 ? 0 : pagination.start + 1;
        const end = totalItems === 0 ? 0 : Math.min(pagination.end, totalItems);
        if (summary) {
            summary.textContent = `${start} - ${end} / ${totalItems} ürün`;
        }

        if (prevButton) {
            prevButton.disabled = pagination.page <= 1;
        }

        if (nextButton) {
            nextButton.disabled = pagination.page >= pagination.totalPages;
        }

        if (!numbers) {
            return;
        }

        numbers.innerHTML = buildPageNumberMarkup(pagination.page, pagination.totalPages);
    }

    function buildPageNumberMarkup(page, totalPages) {
        if (totalPages <= 1) {
            return `<button type="button" class="admin-product-page-number is-active" data-product-page-number="1">1</button>`;
        }

        const pages = [];
        const spread = 2;
        const start = Math.max(1, page - spread);
        const end = Math.min(totalPages, page + spread);

        if (start > 1) {
            pages.push(1);
        }
        if (start > 2) {
            pages.push("ellipsis-start");
        }
        for (let index = start; index <= end; index += 1) {
            pages.push(index);
        }
        if (end < totalPages - 1) {
            pages.push("ellipsis-end");
        }
        if (end < totalPages) {
            pages.push(totalPages);
        }

        return pages.map((entry) => {
            if (typeof entry !== "number") {
                return `<span class="admin-product-page-ellipsis">...</span>`;
            }

            const active = entry === page ? " is-active" : "";
            return `<button type="button" class="admin-product-page-number${active}" data-product-page-number="${entry}">${entry}</button>`;
        }).join("");
    }

    function renderSelection(state, currentItems) {
        const bulkBar = state.root.querySelector("[data-product-bulk-bar]");
        const selectionCount = state.root.querySelector("[data-product-selection-count]");
        const selectAll = state.root.querySelector("[data-product-select-all]");
        const editButton = state.root.querySelector("[data-product-bulk-edit]");
        const deleteButton = state.root.querySelector("[data-product-bulk-delete]");

        if (selectionCount) {
            selectionCount.textContent = `${state.selectedIds.size} ürün seçildi`;
        }

        if (bulkBar) {
            bulkBar.hidden = state.selectedIds.size === 0 || state.viewMode !== "table";
        }

        const selectableIds = currentItems.map((item) => item.id);
        if (selectAll) {
            const allSelected = selectableIds.length > 0 && selectableIds.every((id) => state.selectedIds.has(id));
            selectAll.checked = allSelected;
            selectAll.indeterminate = !allSelected && selectableIds.some((id) => state.selectedIds.has(id));
        }

        if (editButton) {
            editButton.disabled = state.selectedIds.size !== 1;
        }

        if (deleteButton) {
            deleteButton.disabled = state.selectedIds.size === 0;
        }
    }

    function syncSortUi(state) {
        state.root.querySelectorAll("[data-product-sort-option]").forEach((button) => {
            const [key, direction] = (button.getAttribute("data-product-sort-option") || ":").split(":");
            button.classList.toggle("is-active", key === state.sort.key && direction === state.sort.direction);
        });

        state.root.querySelectorAll("[data-product-sort-header]").forEach((button) => {
            const key = button.getAttribute("data-product-sort-header");
            const isActive = key === state.sort.key;
            button.classList.toggle("is-active", isActive);
            button.dataset.sortDirection = isActive ? state.sort.direction : "";
        });
    }

    function updateStateAndRender(root, updater) {
        const state = stateMap.get(root);
        if (!state) {
            return;
        }

        updater(state);
        render(state);
    }

    function toggleFilterPanel(root) {
        const state = stateMap.get(root);
        if (!state) {
            return;
        }

        const panel = root.querySelector("[data-product-filter-builder]");
        const button = root.querySelector("[data-product-filter-toggle]");
        if (!panel) {
            return;
        }

        if (!state.draftFilters.length && state.appliedFilters.length) {
            state.draftFilters = cloneFilters(state.appliedFilters);
        }

        state.filterPanelOpen = !state.filterPanelOpen;
        panel.hidden = !state.filterPanelOpen;
        if (button) {
            button.setAttribute("aria-expanded", state.filterPanelOpen ? "true" : "false");
        }
    }

    function toggleContextMenu(root) {
        const state = stateMap.get(root);
        if (!state) {
            return;
        }

        const menu = root.querySelector("[data-product-column-menu]");
        const button = root.querySelector("[data-product-context-toggle]");
        if (!menu) {
            return;
        }

        state.contextMenuOpen = !state.contextMenuOpen;
        menu.hidden = !state.contextMenuOpen;
        if (button) {
            button.setAttribute("aria-expanded", state.contextMenuOpen ? "true" : "false");
        }
    }

    function closePanels(root) {
        const state = stateMap.get(root);
        if (!state) {
            return;
        }

        state.contextMenuOpen = false;
        state.activeValueMenu = null;
        state.activeSavedFilterMenuId = null;

        const contextMenu = root.querySelector("[data-product-column-menu]");
        const contextButton = root.querySelector("[data-product-context-toggle]");
        if (contextMenu) {
            contextMenu.hidden = true;
        }
        if (contextButton) {
            contextButton.setAttribute("aria-expanded", "false");
        }

        root.querySelectorAll("[data-filter-value-menu]").forEach((menu) => {
            menu.hidden = true;
        });
        root.querySelectorAll("[data-filter-value-toggle]").forEach((button) => {
            button.setAttribute("aria-expanded", "false");
        });
        root.querySelectorAll("[data-product-saved-filter-menu]").forEach((menu) => {
            menu.hidden = true;
        });
        root.querySelectorAll("[data-product-saved-filter-menu-toggle]").forEach((button) => {
            button.setAttribute("aria-expanded", "false");
        });
    }

    function openValueMenu(index, root) {
        const state = stateMap.get(root);
        if (!state) {
            return;
        }

        const nextValue = state.activeValueMenu === index ? null : index;
        state.activeValueMenu = nextValue;

        root.querySelectorAll(".admin-product-filter-row").forEach((filterRow, rowIndex) => {
            const menu = filterRow.querySelector("[data-filter-value-menu]");
            const trigger = filterRow.querySelector("[data-filter-value-toggle]");
            const isCurrent = rowIndex === nextValue;
            if (menu) {
                menu.hidden = !isCurrent;
            }
            if (trigger) {
                trigger.setAttribute("aria-expanded", isCurrent ? "true" : "false");
            }
        });
    }

    function toggleSavedFilterMenu(root, filterId) {
        const state = stateMap.get(root);
        if (!state) {
            return;
        }

        state.activeSavedFilterMenuId = state.activeSavedFilterMenuId === filterId ? null : filterId;
        renderSavedFilterTabs(state);
    }

    function applyDraftFilters(state) {
        state.appliedFilters = cloneFilters(state.draftFilters);
        state.currentPage = 1;
        state.filterPanelOpen = false;
        state.activeValueMenu = null;
    }

    function syncDraftFiltersWithApplied(state) {
        state.draftFilters = cloneFilters(state.appliedFilters);
    }

    function selectSavedFilter(root, filterId) {
        updateStateAndRender(root, (state) => {
            const selected = state.savedFilters.find((filter) => filter.id === filterId);
            if (!selected) {
                return;
            }

            state.activeSavedFilterId = selected.id;
            state.appliedFilters = cloneFilters(selected.filters);
            state.draftFilters = cloneFilters(selected.filters);
            state.currentPage = 1;
            state.activeSavedFilterMenuId = null;
        });
    }

    function clearAllFilters(root) {
        updateStateAndRender(root, (state) => {
            state.activeSavedFilterId = null;
            state.appliedFilters = [];
            state.draftFilters = [];
            state.currentPage = 1;
        });
    }

    function removeAppliedFilter(root, index) {
        updateStateAndRender(root, (state) => {
            state.appliedFilters.splice(index, 1);
            syncDraftFiltersWithApplied(state);
            state.currentPage = 1;
            if (state.activeSavedFilterId != null && state.appliedFilters.length === 0) {
                state.activeSavedFilterId = null;
            }
        });
    }

    async function saveNewFilter(root) {
        const state = stateMap.get(root);
        if (!state || !state.appliedFilters.length) {
            return;
        }

        openFilterModal(root, "rename-create", null, "İsimsiz Filtre");
    }

    async function saveFilterChanges(root) {
        const state = stateMap.get(root);
        const activeSaved = state ? getActiveSavedFilter(state) : null;
        if (!state || !activeSaved) {
            return;
        }

        const response = await postJson(root, `/admin/products/filters/${activeSaved.id}/update`, {
            name: activeSaved.name,
            filters: cloneFilters(state.appliedFilters)
        });

        if (!response?.filter) {
            return;
        }

        updateStateAndRender(root, (nextState) => {
            upsertSavedFilter(nextState, response.filter);
            nextState.activeSavedFilterId = Number(response.filter.id);
        });
    }

    function upsertSavedFilter(state, incomingFilter) {
        const normalized = normalizeSavedFilters([incomingFilter])[0];
        const existingIndex = state.savedFilters.findIndex((filter) => filter.id === normalized.id);
        if (existingIndex >= 0) {
            state.savedFilters.splice(existingIndex, 1, normalized);
        } else {
            state.savedFilters.push(normalized);
        }

        state.savedFilters = normalizeSavedFilters(state.savedFilters);
    }

    function removeSavedFilterFromState(state, filterId) {
        state.savedFilters = state.savedFilters.filter((filter) => filter.id !== filterId);
        if (state.activeSavedFilterId === filterId) {
            state.activeSavedFilterId = null;
            state.appliedFilters = [];
            state.draftFilters = [];
        }
        state.activeSavedFilterMenuId = null;
    }

    function openFilterModal(root, modal, filterId, defaultName) {
        const state = stateMap.get(root);
        if (!state) {
            return;
        }

        state.filterModal = modal;
        state.filterModalFilterId = filterId;
        state.sortDraftOrder = state.savedFilters.map((filter) => filter.id);

        if (modal === "rename-create" || modal === "rename-edit") {
            const input = document.querySelector("[data-product-filter-rename-input]");
            if (input) {
                input.value = defaultName || "";
                window.setTimeout(() => input.focus(), 0);
            }
        }

        if (modal === "sort") {
            syncSortDialog(state);
        }

        document.querySelector("[data-product-filter-delete-modal]")?.toggleAttribute("hidden", modal !== "delete");
        document.querySelector("[data-product-filter-rename-modal]")?.toggleAttribute("hidden", modal !== "rename-create" && modal !== "rename-edit");
        document.querySelector("[data-product-filter-sort-modal]")?.toggleAttribute("hidden", modal !== "sort");
        document.body.classList.add("is-admin-dialog-open");
    }

    function closeFilterModals() {
        document.querySelector("[data-product-filter-delete-modal]")?.setAttribute("hidden", "hidden");
        document.querySelector("[data-product-filter-rename-modal]")?.setAttribute("hidden", "hidden");
        document.querySelector("[data-product-filter-sort-modal]")?.setAttribute("hidden", "hidden");
        document.querySelectorAll("[data-product-catalog='true']").forEach((root) => {
            const state = stateMap.get(root);
            if (state) {
                state.filterModal = null;
                state.filterModalFilterId = null;
                state.sortDraftOrder = [];
            }
        });

        if (!document.querySelector("[data-product-create-modal]:not([hidden]),[data-product-transfer-step-one]:not([hidden]),[data-product-transfer-step-two]:not([hidden])")) {
            document.body.classList.remove("is-admin-dialog-open");
        }
    }

    function syncSortDialog(state) {
        const list = document.querySelector("[data-product-filter-sort-list]");
        if (!list) {
            return;
        }

        const ids = state.sortDraftOrder.length ? state.sortDraftOrder : state.savedFilters.map((filter) => filter.id);
        list.innerHTML = ids.map((id) => {
            const filter = state.savedFilters.find((entry) => entry.id === id);
            if (!filter) {
                return "";
            }

            return `
                <div class="admin-product-sort-item" draggable="true" data-product-filter-sort-id="${filter.id}">
                    <i class="fa-solid fa-grip-vertical"></i>
                    <span>${escapeHtml(filter.name)}</span>
                </div>
            `;
        }).join("");
    }

    async function confirmFilterModal(root) {
        const state = stateMap.get(root);
        if (!state || !state.filterModal) {
            return;
        }

        if (state.filterModal === "delete" && state.filterModalFilterId != null) {
            const response = await postJson(root, `/admin/products/filters/${state.filterModalFilterId}/delete`, {});
            if (!response) {
                return;
            }

            updateStateAndRender(root, (nextState) => {
                removeSavedFilterFromState(nextState, state.filterModalFilterId);
            });
            closeFilterModals();
            return;
        }

        if ((state.filterModal === "rename-create" || state.filterModal === "rename-edit")) {
            const input = document.querySelector("[data-product-filter-rename-input]");
            const name = input?.value?.trim() || "İsimsiz Filtre";

            if (state.filterModal === "rename-create") {
                const response = await postJson(root, "/admin/products/filters", {
                    name,
                    filters: cloneFilters(state.appliedFilters)
                });

                if (!response?.filter) {
                    return;
                }

                updateStateAndRender(root, (nextState) => {
                    upsertSavedFilter(nextState, response.filter);
                    nextState.activeSavedFilterId = Number(response.filter.id);
                    nextState.appliedFilters = cloneFilters(response.filter.filters || nextState.appliedFilters);
                    nextState.draftFilters = cloneFilters(nextState.appliedFilters);
                });
            } else if (state.filterModalFilterId != null) {
                const response = await postJson(root, `/admin/products/filters/${state.filterModalFilterId}/rename`, { name });
                if (!response?.filter) {
                    return;
                }

                updateStateAndRender(root, (nextState) => {
                    upsertSavedFilter(nextState, response.filter);
                });
            }

            closeFilterModals();
            return;
        }

        if (state.filterModal === "sort") {
            const response = await postJson(root, "/admin/products/filters/reorder", {
                filterIds: state.sortDraftOrder
            });

            if (!response?.filters) {
                return;
            }

            updateStateAndRender(root, (nextState) => {
                nextState.savedFilters = normalizeSavedFilters(response.filters);
            });
            closeFilterModals();
        }
    }

    async function postJson(root, url, payload) {
        const token = root.querySelector("input[name='__RequestVerificationToken']")?.value;

        try {
            const response = await window.fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token || ""
                },
                body: JSON.stringify(payload || {})
            });

            if (!response.ok) {
                return null;
            }

            return await response.json();
        } catch {
            return null;
        }
    }

    function handleSortChange(root, key, direction) {
        updateStateAndRender(root, (state) => {
            state.sort = { key, direction };
            state.currentPage = 1;
            saveSort(state.sort);
        });
    }

    function handleHeaderSort(root, key) {
        updateStateAndRender(root, (state) => {
            const nextDirection = state.sort.key === key && state.sort.direction === "asc" ? "desc" : "asc";
            state.sort = { key, direction: nextDirection };
            state.currentPage = 1;
            saveSort(state.sort);
        });
    }

    function formatDate(date) {
        return dateFormatter.format(date);
    }

    function formatTime(date) {
        return timeFormatter.format(date);
    }

    function formatDateWithTime(date) {
        return `${formatDate(date)} ${formatTime(date)}`;
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function escapeAttribute(value) {
        return escapeHtml(value);
    }

    function openDialog(selector) {
        const dialog = document.querySelector(selector);
        if (!dialog) {
            return;
        }

        dialog.hidden = false;
        document.body.classList.add("is-admin-dialog-open");
    }

    function closeDialogs() {
        document.querySelectorAll("[data-product-create-modal],[data-product-transfer-step-one],[data-product-transfer-step-two]").forEach((dialog) => {
            dialog.hidden = true;
        });
        closeFilterModals();
        document.body.classList.remove("is-admin-dialog-open");
    }

    function configureTransferStepOne(mode) {
        transferMode = mode;

        const title = document.querySelector("[data-product-transfer-title]");
        if (title) {
            title.textContent = mode === "export" ? "Ürünleri Dışa Aktar" : "Ürünleri İçe Aktar";
        }
    }

    function configureTransferStepTwo() {
        const title = document.querySelector("[data-product-transfer-step-two-title]");
        const subtitle = document.querySelector("[data-product-transfer-step-two-subtitle]");
        const submit = document.querySelector("[data-transfer-submit-label]");
        const summaryTitle = document.querySelector("[data-product-transfer-summary-title]");
        const summaryCopy = document.querySelector("[data-product-transfer-summary-copy]");

        if (title) {
            title.textContent = transferMode === "export" ? "Ürünleri Dışa Aktar" : "Ürünleri İçe Aktar";
        }

        if (subtitle) {
            subtitle.textContent = transferMode === "export"
                ? "Dışa aktarılacak alanları seç"
                : "İçeri aktarım sırasında kullanılacak alanları seç";
        }

        if (submit) {
            submit.textContent = transferMode === "export" ? "Dışa Aktar" : "İçe Aktar";
        }

        if (summaryTitle) {
            summaryTitle.textContent = transferMode === "export" ? "Dışa aktarım alanları" : "İçe aktarım alanları";
        }

        if (summaryCopy) {
            summaryCopy.textContent = transferMode === "export"
                ? "Seçili alanlar dışa aktarım dosyasına eklenecek katalog sütunlarını belirler."
                : "Seçili alanlar ikinci adımda kullanılacak katalog sütunlarını belirler.";
        }
    }

    document.addEventListener("click", (event) => {
        const root = currentRoot(event.target);
        const modalRoot = root || document.querySelector("[data-product-catalog='true']");

        const viewButton = event.target.closest("[data-product-view-button]");
        if (viewButton && root) {
            const mode = viewButton.getAttribute("data-product-view-button");
            if (mode) {
                updateStateAndRender(root, (state) => {
                    state.viewMode = mode;
                    state.currentPage = 1;
                    saveViewMode(mode);
                    applyViewMode(root, mode);
                    closePanels(root);
                });
            }
            return;
        }

        const filterButton = event.target.closest("[data-product-filter-toggle]");
        if (filterButton && root) {
            toggleFilterPanel(root);
            closePanels(root);
            return;
        }

        const saveButton = event.target.closest("[data-product-filter-save]");
        if (saveButton && root) {
            saveNewFilter(root);
            return;
        }

        const saveChangesButton = event.target.closest("[data-product-filter-save-changes]");
        if (saveChangesButton && root) {
            saveFilterChanges(root);
            return;
        }

        const contextButton = event.target.closest("[data-product-context-toggle]");
        if (contextButton && root) {
            toggleContextMenu(root);
            return;
        }

        const sortOption = event.target.closest("[data-product-sort-option]");
        if (sortOption && root) {
            const [key, direction] = (sortOption.getAttribute("data-product-sort-option") || ":").split(":");
            handleSortChange(root, key, direction === "asc" ? "asc" : "desc");
            closePanels(root);
            return;
        }

        const sortHeader = event.target.closest("[data-product-sort-header]");
        if (sortHeader && root) {
            const key = sortHeader.getAttribute("data-product-sort-header");
            if (key) {
                handleHeaderSort(root, key);
            }
            return;
        }

        const allFiltersTab = event.target.closest("[data-product-saved-filter-all]");
        if (allFiltersTab && root) {
            clearAllFilters(root);
            return;
        }

        const selectSavedFilterButton = event.target.closest("[data-product-saved-filter-select]");
        if (selectSavedFilterButton && root) {
            selectSavedFilter(root, Number(selectSavedFilterButton.getAttribute("data-product-saved-filter-select")));
            return;
        }

        const toggleSavedFilterMenuButton = event.target.closest("[data-product-saved-filter-menu-toggle]");
        if (toggleSavedFilterMenuButton && root) {
            toggleSavedFilterMenu(root, Number(toggleSavedFilterMenuButton.getAttribute("data-product-saved-filter-menu-toggle")));
            return;
        }

        const renameSavedFilter = event.target.closest("[data-product-saved-filter-rename]");
        if (renameSavedFilter && root) {
            const id = Number(renameSavedFilter.getAttribute("data-product-saved-filter-rename"));
            const state = stateMap.get(root);
            const filter = state?.savedFilters.find((entry) => entry.id === id);
            if (filter) {
                openFilterModal(root, "rename-edit", id, filter.name);
            }
            return;
        }

        const deleteSavedFilter = event.target.closest("[data-product-saved-filter-delete]");
        if (deleteSavedFilter && root) {
            openFilterModal(root, "delete", Number(deleteSavedFilter.getAttribute("data-product-saved-filter-delete")), "");
            return;
        }

        const sortSavedFilters = event.target.closest("[data-product-saved-filter-sort]");
        if (sortSavedFilters && root) {
            openFilterModal(root, "sort", Number(sortSavedFilters.getAttribute("data-product-saved-filter-sort")), "");
            return;
        }

        const addFilter = event.target.closest("[data-product-filter-add]");
        if (addFilter && root) {
            updateStateAndRender(root, (state) => {
                const nextField = getNextAvailableField(state);
                if (!nextField) {
                    return;
                }

                state.draftFilters.push(createEmptyFilter(state));
            });
            return;
        }

        const applyFilters = event.target.closest("[data-product-filter-apply]");
        if (applyFilters && root) {
            updateStateAndRender(root, (state) => {
                if (!canApplyFilters(state)) {
                    return;
                }

                applyDraftFilters(state);
            });
            const panel = root.querySelector("[data-product-filter-builder]");
            const button = root.querySelector("[data-product-filter-toggle]");
            if (panel) {
                panel.hidden = true;
            }
            if (button) {
                button.setAttribute("aria-expanded", "false");
            }
            closePanels(root);
            return;
        }

        const removeFilter = event.target.closest("[data-filter-remove]");
        if (removeFilter && root) {
            const row = removeFilter.closest(".admin-product-filter-row");
            const index = row ? Number(row.dataset.filterIndex) : -1;
            if (index >= 0) {
                updateStateAndRender(root, (state) => {
                    state.draftFilters.splice(index, 1);
                    if (state.activeValueMenu === index) {
                        state.activeValueMenu = null;
                    }
                });
            }
            return;
        }

        const removeAppliedFilterButton = event.target.closest("[data-product-applied-filter-remove]");
        if (removeAppliedFilterButton && root) {
            removeAppliedFilter(root, Number(removeAppliedFilterButton.getAttribute("data-product-applied-filter-remove")));
            return;
        }

        const clearAppliedFiltersButton = event.target.closest("[data-product-applied-filters-clear]");
        if (clearAppliedFiltersButton && root) {
            clearAllFilters(root);
            return;
        }

        const valueToggle = event.target.closest("[data-filter-value-toggle]");
        if (valueToggle && root) {
            const row = valueToggle.closest(".admin-product-filter-row");
            const index = row ? Number(row.dataset.filterIndex) : -1;
            if (index >= 0) {
                openValueMenu(index, root);
            }
            return;
        }

        const clearSearch = event.target.closest("[data-product-search-clear]");
        if (clearSearch && root) {
            updateStateAndRender(root, (state) => {
                state.searchTerm = "";
                state.currentPage = 1;
                const input = root.querySelector("[data-product-search-input]");
                if (input) {
                    input.value = "";
                }
            });
            return;
        }

        const prevPage = event.target.closest("[data-product-page-prev]");
        if (prevPage && root) {
            updateStateAndRender(root, (state) => {
                state.currentPage = Math.max(1, state.currentPage - 1);
            });
            return;
        }

        const nextPage = event.target.closest("[data-product-page-next]");
        if (nextPage && root) {
            updateStateAndRender(root, (state) => {
                const filtered = filterProducts(state);
                const totalPages = Math.max(1, Math.ceil(filtered.length / state.pageSize));
                state.currentPage = Math.min(totalPages, state.currentPage + 1);
            });
            return;
        }

        const pageNumber = event.target.closest("[data-product-page-number]");
        if (pageNumber && root) {
            const page = Number(pageNumber.getAttribute("data-product-page-number") || "1");
            updateStateAndRender(root, (state) => {
                state.currentPage = page;
            });
            return;
        }

        const bulkEdit = event.target.closest("[data-product-bulk-edit]");
        if (bulkEdit && root) {
            const state = stateMap.get(root);
            if (state && state.selectedIds.size === 1) {
                const [id] = [...state.selectedIds];
                window.location.assign(`/admin/products/edit/${id}`);
            }
            return;
        }

        if (event.target.closest("[data-product-create-open]")) {
            openDialog("[data-product-create-modal]");
            return;
        }

        if (event.target.closest("[data-product-export-open]")) {
            configureTransferStepOne("export");
            openDialog("[data-product-transfer-step-one]");
            return;
        }

        if (event.target.closest("[data-product-import-open]")) {
            configureTransferStepOne("import");
            openDialog("[data-product-transfer-step-one]");
            return;
        }

        if (event.target.closest("[data-product-transfer-continue]")) {
            configureTransferStepTwo();
            document.querySelector("[data-product-transfer-step-one]")?.setAttribute("hidden", "hidden");
            openDialog("[data-product-transfer-step-two]");
            return;
        }

        if (event.target.closest("[data-product-transfer-clear]")) {
            document.querySelectorAll(".admin-product-transfer-check input[type='checkbox']").forEach((input) => {
                input.checked = false;
            });
            return;
        }

        if (event.target.closest("[data-product-transfer-submit]")) {
            closeDialogs();
            return;
        }

        if (event.target.closest("[data-product-dialog-close]")) {
            closeDialogs();
            return;
        }

        if (event.target.closest("[data-product-filter-modal-close]")) {
            closeFilterModals();
            return;
        }

        if (event.target.closest("[data-product-filter-delete-confirm]") && modalRoot) {
            confirmFilterModal(modalRoot);
            return;
        }

        if (event.target.closest("[data-product-filter-rename-confirm]") && modalRoot) {
            confirmFilterModal(modalRoot);
            return;
        }

        if (event.target.closest("[data-product-filter-sort-confirm]") && modalRoot) {
            confirmFilterModal(modalRoot);
            return;
        }

        document.querySelectorAll("[data-product-catalog='true']").forEach((catalogRoot) => {
            const insideContext = catalogRoot.querySelector("[data-product-column-menu]")?.contains(event.target);
            const onContextButton = event.target.closest("[data-product-context-toggle]");
            const insideValueMenu = event.target.closest("[data-filter-value-menu]");
            const onValueTrigger = event.target.closest("[data-filter-value-toggle]");
            const insideFilterPanel = catalogRoot.querySelector("[data-product-filter-builder]")?.contains(event.target);
            const onFilterButton = event.target.closest("[data-product-filter-toggle]");
            const insideSavedMenu = event.target.closest("[data-product-saved-filter-menu]");
            const onSavedMenuButton = event.target.closest("[data-product-saved-filter-menu-toggle]");

            if (!insideContext && !onContextButton && !insideSavedMenu && !onSavedMenuButton) {
                closePanels(catalogRoot);
            }

            if (!insideFilterPanel && !onFilterButton) {
                const state = stateMap.get(catalogRoot);
                const panel = catalogRoot.querySelector("[data-product-filter-builder]");
                const button = catalogRoot.querySelector("[data-product-filter-toggle]");
                if (state && !insideValueMenu && !onValueTrigger && panel && state.filterPanelOpen) {
                    panel.hidden = true;
                    state.filterPanelOpen = false;
                    if (button) {
                        button.setAttribute("aria-expanded", "false");
                    }
                }
            }
        });
    });

    document.addEventListener("input", (event) => {
        const searchInput = event.target.closest("[data-product-search-input]");
        if (searchInput) {
            const root = currentRoot(searchInput);
            if (root) {
                updateStateAndRender(root, (state) => {
                    state.searchTerm = searchInput.value.trim();
                    state.currentPage = 1;
                });
            }
            return;
        }

        const dateInput = event.target.closest("[data-filter-date]");
        if (dateInput) {
            const root = currentRoot(dateInput);
            const row = dateInput.closest(".admin-product-filter-row");
            const index = row ? Number(row.dataset.filterIndex) : -1;
            if (root && index >= 0) {
                updateStateAndRender(root, (state) => {
                    state.draftFilters[index].value = dateInput.value;
                });
            }
            return;
        }

        const textInput = event.target.closest("[data-filter-text]");
        if (textInput) {
            const root = currentRoot(textInput);
            const row = textInput.closest(".admin-product-filter-row");
            const index = row ? Number(row.dataset.filterIndex) : -1;
            if (root && index >= 0) {
                updateStateAndRender(root, (state) => {
                    state.draftFilters[index].value = textInput.value;
                });
            }
            return;
        }

        const renameInput = event.target.closest("[data-product-filter-rename-input]");
        if (renameInput) {
            return;
        }
    });

    document.addEventListener("change", (event) => {
        const columnInput = event.target.closest("[data-product-column-option]");
        if (columnInput) {
            const root = currentRoot(columnInput);
            if (!root) {
                return;
            }

            const columns = Array.from(root.querySelectorAll("[data-product-column-option]"))
                .filter((input) => input.checked)
                .map((input) => input.getAttribute("data-product-column-option"))
                .filter(Boolean);

            if (!columns.length) {
                columnInput.checked = true;
                return;
            }

            updateStateAndRender(root, (state) => {
                state.columns = columns;
                saveColumns(columns);
            });
            return;
        }

        const fieldSelect = event.target.closest("[data-filter-field]");
        if (fieldSelect) {
            const root = currentRoot(fieldSelect);
            const row = fieldSelect.closest(".admin-product-filter-row");
            const index = row ? Number(row.dataset.filterIndex) : -1;
            if (root && index >= 0) {
                updateStateAndRender(root, (state) => {
                    const nextField = fieldSelect.value;
                    const config = getFieldConfig(nextField, state, index);
                    state.draftFilters[index] = {
                        field: nextField,
                        operator: config.operators[0]?.value || "includes",
                        values: [],
                        value: ""
                    };
                    state.activeValueMenu = null;
                });
            }
            return;
        }

        const operatorSelect = event.target.closest("[data-filter-operator]");
        if (operatorSelect) {
            const root = currentRoot(operatorSelect);
            const row = operatorSelect.closest(".admin-product-filter-row");
            const index = row ? Number(row.dataset.filterIndex) : -1;
            if (root && index >= 0) {
                updateStateAndRender(root, (state) => {
                    state.draftFilters[index].operator = operatorSelect.value;
                });
            }
            return;
        }

        const optionInput = event.target.closest("[data-filter-option]");
        if (optionInput) {
            const root = currentRoot(optionInput);
            const row = optionInput.closest(".admin-product-filter-row");
            const index = row ? Number(row.dataset.filterIndex) : -1;
            if (root && index >= 0) {
                updateStateAndRender(root, (state) => {
                    const values = new Set(state.draftFilters[index].values || []);
                    if (optionInput.checked) {
                        values.add(optionInput.value);
                    } else {
                        values.delete(optionInput.value);
                    }
                    state.draftFilters[index].values = [...values].sort((left, right) => textFormatter.compare(left, right));
                });
            }
            return;
        }

        const rowSelect = event.target.closest("[data-product-row-select]");
        if (rowSelect) {
            const root = currentRoot(rowSelect);
            if (root) {
                updateStateAndRender(root, (state) => {
                    const id = Number(rowSelect.getAttribute("data-product-row-select"));
                    if (rowSelect.checked) {
                        state.selectedIds.add(id);
                    } else {
                        state.selectedIds.delete(id);
                    }
                });
            }
            return;
        }

        const selectAll = event.target.closest("[data-product-select-all]");
        if (selectAll) {
            const root = currentRoot(selectAll);
            if (root) {
                updateStateAndRender(root, (state) => {
                    const filtered = sortProducts(filterProducts(state), state.sort);
                    const pageItems = paginateProducts(filtered, state.currentPage, state.pageSize).items;
                    if (selectAll.checked) {
                        pageItems.forEach((item) => state.selectedIds.add(item.id));
                    } else {
                        pageItems.forEach((item) => state.selectedIds.delete(item.id));
                    }
                });
            }
            return;
        }

        const pageSizeSelect = event.target.closest("[data-product-page-size]");
        if (pageSizeSelect) {
            const root = currentRoot(pageSizeSelect);
            if (root) {
                updateStateAndRender(root, (state) => {
                    state.pageSize = Number(pageSizeSelect.value) === 50 ? 50 : defaultPageSize;
                    state.currentPage = 1;
                });
            }
        }
    });

    document.addEventListener("submit", (event) => {
        const form = event.target.closest("[data-admin-async-form='true']");
        if (!form) {
            return;
        }

        const root = currentRoot(form);
        if (root) {
            event.preventDefault();
        }
    });

    document.addEventListener("dragstart", (event) => {
        const item = event.target.closest("[data-product-filter-sort-id]");
        if (!item) {
            return;
        }

        event.dataTransfer?.setData("text/plain", item.getAttribute("data-product-filter-sort-id") || "");
        event.dataTransfer.effectAllowed = "move";
    });

    document.addEventListener("dragover", (event) => {
        const item = event.target.closest("[data-product-filter-sort-id]");
        if (!item) {
            return;
        }

        event.preventDefault();
        item.classList.add("is-drag-over");
    });

    document.addEventListener("dragleave", (event) => {
        const item = event.target.closest("[data-product-filter-sort-id]");
        if (item) {
            item.classList.remove("is-drag-over");
        }
    });

    document.addEventListener("drop", (event) => {
        const item = event.target.closest("[data-product-filter-sort-id]");
        if (!item) {
            return;
        }

        event.preventDefault();
        item.classList.remove("is-drag-over");

        const draggedId = Number(event.dataTransfer?.getData("text/plain") || "0");
        const targetId = Number(item.getAttribute("data-product-filter-sort-id") || "0");
        if (!draggedId || !targetId || draggedId === targetId) {
            return;
        }

        const root = document.querySelector("[data-product-catalog='true']");
        const state = root ? stateMap.get(root) : null;
        if (!state) {
            return;
        }

        const nextOrder = [...state.sortDraftOrder];
        const draggedIndex = nextOrder.indexOf(draggedId);
        const targetIndex = nextOrder.indexOf(targetId);
        if (draggedIndex < 0 || targetIndex < 0) {
            return;
        }

        nextOrder.splice(draggedIndex, 1);
        nextOrder.splice(targetIndex, 0, draggedId);
        state.sortDraftOrder = nextOrder;
        syncSortDialog(state);
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeDialogs();
            document.querySelectorAll("[data-product-catalog='true']").forEach((root) => {
                closePanels(root);

                const state = stateMap.get(root);
                const panel = root.querySelector("[data-product-filter-builder]");
                const button = root.querySelector("[data-product-filter-toggle]");
                if (state && panel) {
                    panel.hidden = true;
                    state.filterPanelOpen = false;
                    if (button) {
                        button.setAttribute("aria-expanded", "false");
                    }
                }
            });
        }
    });

    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll("[data-product-catalog='true']").forEach(initCatalog);
    });

    window.addEventListener("codex:admin-async-updated", (event) => {
        const root = event.detail?.root;
        if (!root) {
            return;
        }

        root.querySelectorAll("[data-product-catalog='true']").forEach(initCatalog);
        if (root.matches("[data-product-catalog='true']")) {
            initCatalog(root);
        }
    });
})();
