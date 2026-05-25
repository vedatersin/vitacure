(function () {
    const forms = Array.from(document.querySelectorAll("[data-admin-product-form='true']"));

    forms.forEach((form) => {
        const initialTab = form.getAttribute("data-product-initial-tab") || "basic";
        const shell = form.querySelector("[data-product-page-shell]");
        const intro = form.querySelector("[data-product-page-intro]");
        const stickyBar = form.querySelector("[data-product-sticky-bar]");
        const categoryInput = form.querySelector("[data-product-category-input]");
        const categoryPayload = form.querySelector("[data-product-category-options]");
        const categorySelectedContainer = form.querySelector("[data-product-selected-category-inputs]");
        const categoryListContainer = form.querySelector("[data-product-selected-category-list]");
        const categoryEmpty = form.querySelector("[data-product-category-empty]");
        const categorySelected = form.querySelector("[data-product-category-selected]");
        const selectorModal = document.querySelector("[data-category-selector-modal]");
        const selectorList = selectorModal?.querySelector("[data-category-selector-list]");
        const selectorSearchInput = selectorModal?.querySelector("[data-category-search-input]");
        const createModal = document.querySelector("[data-category-create-modal]");
        const featureModal = form.querySelector("[data-feature-selector-modal]");
        const featureEmpty = form.querySelector("[data-product-feature-empty]");
        const featureSelected = form.querySelector("[data-product-feature-selected]");
        const featureSelectedList = form.querySelector("[data-product-selected-feature-list]");
        const customFieldPayload = form.querySelector("[data-product-custom-field-options]");
        const customFieldModal = form.querySelector("[data-custom-field-selector-modal]");
        const customFieldList = form.querySelector("[data-custom-field-selector-list]");
        const customFieldSearchInput = form.querySelector("[data-custom-field-search-input]");
        const customFieldEmpty = form.querySelector("[data-product-custom-field-empty]");
        const customFieldSelected = form.querySelector("[data-product-custom-field-selected]");
        const customFieldSelectedList = form.querySelector("[data-product-selected-custom-field-list]");
        const customFieldInputs = form.querySelector("[data-product-selected-custom-field-inputs]");
        const customFieldCreateModal = document.querySelector("[data-custom-field-create-modal]");
        const personalizationPayload = form.querySelector("[data-product-personalization-options]");
        const personalizationModal = form.querySelector("[data-personalization-selector-modal]");
        const personalizationList = form.querySelector("[data-personalization-selector-list]");
        const personalizationSearchInput = form.querySelector("[data-personalization-search-input]");
        const personalizationEmpty = form.querySelector("[data-product-personalization-empty]");
        const personalizationSelected = form.querySelector("[data-product-personalization-selected]");
        const personalizationSelectedList = form.querySelector("[data-product-selected-personalization-list]");
        const personalizationInputs = form.querySelector("[data-product-selected-personalization-inputs]");
        const personalizationCreateModal = document.querySelector("[data-personalization-create-modal]");
        const statusInput = form.querySelector("[data-product-status-input]");
        const statusSelect = form.querySelector("[data-product-status-select]");
        const brandSelect = form.querySelector("select[name='BrandId']");
        const brandSummary = form.querySelector("[data-product-brand-summary]");
        const tagPayload = form.querySelector("[data-product-tag-options]");
        const tagSelector = form.querySelector("[data-product-tag-selector]");
        const tagSelectorControl = form.querySelector("[data-tag-selector-control]");
        const tagSelectorInput = form.querySelector("[data-tag-selector-input]");
        const tagSelectorChips = form.querySelector("[data-tag-selector-chips]");
        const tagSelectorMenu = form.querySelector("[data-tag-selector-menu]");
        const tagSelectorList = form.querySelector("[data-tag-selector-list]");
        const tagSelectorEmpty = form.querySelector("[data-tag-selector-empty]");
        const tagSelectorEmptyTitle = form.querySelector("[data-tag-selector-empty-title]");
        const tagSelectorEmptyCopy = form.querySelector("[data-tag-selector-empty-copy]");
        const tagSelectorClear = form.querySelector("[data-tag-selector-clear]");
        const selectedTagInputsContainer = form.querySelector("[data-product-selected-tag-inputs]");
        const tagSummary = form.querySelector("[data-product-tag-summary]");
        const infoSections = Array.from(form.querySelectorAll("[data-inline-info-wrap]"));
        const unitPriceToggle = form.querySelector("[data-unit-price-toggle]");
        const unitPricePanel = form.querySelector("[data-unit-price-panel]");
        const unitContentAmount = form.querySelector("[data-unit-content-amount]");
        const unitContentType = form.querySelector("[data-unit-content-type]");
        const unitComparisonAmount = form.querySelector("[data-unit-comparison-amount]");
        const unitComparisonType = form.querySelector("[data-unit-comparison-type]");
        const unitPriceResult = form.querySelector("[data-unit-price-result]");
        const featureGroups = Array.from(form.querySelectorAll("[data-feature-option-group]"));
        const tabButtons = Array.from(form.querySelectorAll("[data-product-tab]"));
        const tabPanels = Array.from(form.querySelectorAll("[data-product-tab-panel]"));
        const tabNav = form.querySelector("[data-product-tab-nav]");
        const tabShell = form.querySelector("[data-product-tab-shell]");
        const tabArrowLeft = form.querySelector("[data-product-tab-arrow='left']");
        const tabArrowRight = form.querySelector("[data-product-tab-arrow='right']");
        let allCategories = parseCategories(categoryPayload?.textContent || "[]");
        const categoryMap = new Map(allCategories.map((item) => [item.id, item]));
        let allCustomFields = parseCustomFields(customFieldPayload?.textContent || "[]");
        const customFieldMap = new Map(allCustomFields.map((item) => [item.id, item]));
        let allPersonalizations = parsePersonalizations(personalizationPayload?.textContent || "[]");
        const personalizationMap = new Map(allPersonalizations.map((item) => [item.id, item]));
        let allTags = parseTags(tagPayload?.textContent || "[]");
        const tagMap = new Map(allTags.map((item) => [item.id, item]));
        let savedSelectedIds = getInitialSelectedIds(categorySelectedContainer, "SelectedCategoryIds");
        let primaryCategoryId = Number(categoryInput?.value || "") || Array.from(savedSelectedIds)[0] || null;
        let selectedCustomFieldIds = getInitialSelectedIds(customFieldInputs, "SelectedCustomFieldIds");
        let draftCustomFieldIds = new Set(selectedCustomFieldIds);
        let selectedPersonalizationIds = getInitialSelectedIds(personalizationInputs, "SelectedPersonalizationIds");
        let draftPersonalizationIds = new Set(selectedPersonalizationIds);
        let selectedTagIds = getInitialSelectedIds(selectedTagInputsContainer, "SelectedTagIds");
        let draftSelectedIds = new Set(savedSelectedIds);
        let activeTab = initialTab;
        let featureSnapshot = null;
        let highlightedCategoryId = null;
        let highlightedFeatureId = null;
        let highlightedCustomFieldId = null;
        let highlightedPersonalizationId = null;

        const getStatusLabel = (value) => {
            switch (value) {
                case "PublishedClosed":
                    return "Satisa Kapali Yayin";
                case "PublishedOpen":
                    return "Satisa A?ik Yayin";
                case "Archived":
                    return "Arsiv";
                default:
                    return "Taslak";
            }
        };

        function parseCategories(raw) {
            try {
                return JSON.parse(raw).map((item) => ({
                    id: Number(item.id ?? item.Id),
                    name: String(item.name ?? item.Name ?? "").trim(),
                    parentId: item.parentId ?? item.ParentId ?? null
                })).filter((item) => item.id > 0 && item.name);
            } catch {
                return [];
            }
        }

        function parseTags(raw) {
            try {
                return JSON.parse(raw).map((item) => ({
                    id: Number(item.id ?? item.Id),
                    name: String(item.name ?? item.Name ?? "").trim(),
                    slug: String(item.slug ?? item.Slug ?? "").trim()
                })).filter((item) => item.id > 0 && item.name);
            } catch {
                return [];
            }
        }

        function parseCustomFields(raw) {
            try {
                return JSON.parse(raw).map((item) => ({
                    id: Number(item.id ?? item.Id),
                    name: String(item.name ?? item.Name ?? "").trim(),
                    slug: String(item.slug ?? item.Slug ?? "").trim(),
                    fieldType: String(item.fieldType ?? item.FieldType ?? "Text").trim(),
                    isFilterable: Boolean(item.isFilterable ?? item.IsFilterable)
                })).filter((item) => item.id > 0 && item.name);
            } catch {
                return [];
            }
        }

        function parsePersonalizations(raw) {
            try {
                return JSON.parse(raw).map((item) => ({
                    id: Number(item.id ?? item.Id),
                    name: String(item.name ?? item.Name ?? "").trim(),
                    slug: String(item.slug ?? item.Slug ?? "").trim(),
                    inputType: String(item.inputType ?? item.InputType ?? "Text").trim()
                })).filter((item) => item.id > 0 && item.name);
            } catch {
                return [];
            }
        }

        function getInitialSelectedIds(container, inputName) {
            return new Set(Array.from(container?.querySelectorAll(`input[name='${inputName}']:checked`) || [])
                .map((input) => Number(input.value))
                .filter((value) => value > 0));
        }

        function getChildren(parentId) {
            return allCategories
                .filter((item) => item.parentId === parentId)
                .sort((left, right) => left.name.localeCompare(right.name, "tr"));
        }

        function collectDescendants(categoryId, bucket = new Set()) {
            bucket.add(categoryId);
            getChildren(categoryId).forEach((child) => collectDescendants(child.id, bucket));
            return bucket;
        }

        function getAncestorLabels(categoryId) {
            const labels = [];
            let current = categoryMap.get(categoryId) || null;

            while (current) {
                labels.unshift(current.name);
                current = current.parentId ? categoryMap.get(current.parentId) || null : null;
            }

            return labels;
        }

        function renderSelectedCategories() {
            const primaryId = savedSelectedIds.has(primaryCategoryId) ? primaryCategoryId : (Array.from(savedSelectedIds)[0] || null);
            primaryCategoryId = primaryId;
            if (categoryInput) {
                categoryInput.value = primaryId ? String(primaryId) : "";
            }

            if (categorySelectedContainer) {
                categorySelectedContainer.innerHTML = Array.from(savedSelectedIds)
                    .sort((left, right) => left - right)
                    .map((id) => `<input type="checkbox" name="SelectedCategoryIds" value="${id}" checked />`)
                    .join("");
            }

            if (categoryListContainer) {
                const selectedItems = Array.from(savedSelectedIds)
                    .map((id) => categoryMap.get(id))
                    .filter(Boolean)
                    .sort((left, right) => {
                        if (left.id === primaryId) {
                            return -1;
                        }
                        if (right.id === primaryId) {
                            return 1;
                        }
                        return getAncestorLabels(left.id).join(" / ").localeCompare(getAncestorLabels(right.id).join(" / "), "tr");
                    });

                categoryListContainer.innerHTML = selectedItems.map((item) => `
                    <div class="admin-product-selected-row" data-category-row="${item.id}">
                        <div class="admin-product-selected-copy">
                            <strong>${escapeHtml(getAncestorLabels(item.id).join(" / "))}</strong>
                            ${item.id === primaryId ? `<span class="admin-product-primary-badge">Ana Kategori</span>` : ""}
                        </div>
                        <details class="admin-product-item-menu">
                            <summary><i class="fa-solid fa-ellipsis"></i></summary>
                            <div class="admin-product-item-menu-panel">
                                <button type="button" data-category-action="edit" data-category-id="${item.id}">D?zenle</button>
                                ${item.id === primaryId ? "" : `<button type="button" data-category-action="make-primary" data-category-id="${item.id}">Ana Kategori Yap</button>`}
                                <button type="button" class="is-danger" data-category-action="remove" data-category-id="${item.id}">Kaldir</button>
                            </div>
                        </details>
                    </div>
                `).join("");
            }

            if (categoryEmpty) {
                categoryEmpty.hidden = savedSelectedIds.size > 0;
            }
            if (categorySelected) {
                categorySelected.hidden = savedSelectedIds.size === 0;
            }
        }

        function renderSelectorList() {
            if (!selectorList) {
                return;
            }

            const search = selectorSearchInput?.value?.trim().toLocaleLowerCase("tr-TR") || "";
            const roots = allCategories
                .filter((item) => item.parentId == null)
                .sort((left, right) => left.name.localeCompare(right.name, "tr"));

            const rows = [];

            const renderNode = (node, depth = 0) => {
                const children = getChildren(node.id);
                const subtree = collectDescendants(node.id);
                const isVisible = !search || getAncestorLabels(node.id).join(" ").toLocaleLowerCase("tr-TR").includes(search);
                const childMarkup = children.map((child) => renderNode(child, depth + 1)).join("");

                if (!isVisible && !childMarkup) {
                    return "";
                }

                const isChecked = draftSelectedIds.has(node.id);
                rows.push(`
                    <label class="admin-category-selector-row ${highlightedCategoryId === node.id ? "is-highlighted" : ""}" style="--depth:${depth}" data-category-selector-row="${node.id}">
                        <input type="checkbox" data-category-selector-check="${node.id}" ${isChecked ? "checked" : ""} />
                        <span>${escapeHtml(node.name)}</span>
                    </label>
                `);

                return childMarkup;
            };

            roots.forEach((root) => {
                renderNode(root);
            });

            selectorList.innerHTML = rows.join("") || `<div class="admin-product-placeholder-state"><div><strong>Sonuc bulunamadi</strong><p>Arama kriterinizi degistirin.</p></div></div>`;
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
            if (document.querySelectorAll(".admin-product-dialog-layer:not([hidden])").length === 0) {
                document.body.classList.remove("is-modal-open");
            }
        }

        function revealHighlighted(selector) {
            const target = document.querySelector(selector);
            if (!target) {
                return;
            }

            target.scrollIntoView({ behavior: "smooth", block: "center" });
        }

        function syncTabState(tab) {
            activeTab = tab;
            tabButtons.forEach((button) => {
                button.classList.toggle("is-active", button.getAttribute("data-product-tab") === tab);
            });
            ensureActiveTabVisible(false);
            syncTabOverflowState();
        }

        function getTabTargets(tab) {
            return tabPanels.filter((panel) => panel.getAttribute("data-product-tab-panel") === tab);
        }

        function scrollToTab(tab, smooth = true) {
            const target = getTabTargets(tab)[0];
            if (!target) {
                return;
            }

            target.scrollIntoView({ behavior: smooth ? "smooth" : "auto", block: "start" });
            syncTabState(tab);
        }

        function getVisibleTabButtons() {
            return tabButtons.filter((button) => !button.hidden && button.offsetParent !== null);
        }

        function syncTabOverflowState() {
            if (!tabNav || !tabShell || !tabArrowLeft || !tabArrowRight) {
                return;
            }

            const hasOverflow = tabNav.scrollWidth - tabNav.clientWidth > 2;
            const canScrollLeft = hasOverflow && tabNav.scrollLeft > 2;
            const canScrollRight = hasOverflow && tabNav.scrollLeft + tabNav.clientWidth < tabNav.scrollWidth - 2;

            tabArrowLeft.hidden = !hasOverflow;
            tabArrowRight.hidden = !hasOverflow;
            tabArrowLeft.disabled = !canScrollLeft;
            tabArrowRight.disabled = !canScrollRight;
            tabShell.classList.toggle("can-scroll-left", canScrollLeft);
            tabShell.classList.toggle("can-scroll-right", canScrollRight);
        }

        function ensureTabButtonVisible(button, smooth = true) {
            if (!tabNav || !button || button.hidden || button.offsetParent === null) {
                syncTabOverflowState();
                return;
            }

            const buttonLeft = button.offsetLeft;
            const buttonRight = buttonLeft + button.offsetWidth;
            const viewLeft = tabNav.scrollLeft;
            const viewRight = viewLeft + tabNav.clientWidth;
            let nextScrollLeft = null;

            if (buttonLeft < viewLeft) {
                nextScrollLeft = Math.max(0, buttonLeft - 12);
            } else if (buttonRight > viewRight) {
                nextScrollLeft = Math.max(0, buttonRight - tabNav.clientWidth + 12);
            }

            if (nextScrollLeft !== null) {
                tabNav.scrollTo({ left: nextScrollLeft, behavior: smooth ? "smooth" : "auto" });
            }

            syncTabOverflowState();
        }

        function ensureActiveTabVisible(smooth = true) {
            const activeButton = tabButtons.find((button) => button.getAttribute("data-product-tab") === activeTab);
            ensureTabButtonVisible(activeButton, smooth);
        }

        function stepTabNavigation(direction) {
            if (!tabNav) {
                return;
            }

            const visibleButtons = getVisibleTabButtons();
            if (!visibleButtons.length) {
                return;
            }

            const delta = Math.max(220, Math.round(tabNav.clientWidth * 0.55)) * direction;
            tabNav.scrollBy({ left: delta, behavior: "smooth" });
        }

        function syncHeaderState() {
            if (!shell || !intro || !stickyBar) {
                return;
            }

            const threshold = intro.offsetHeight - stickyBar.offsetHeight * 0.35;
            shell.classList.toggle("is-condensed", window.scrollY > Math.max(32, threshold));
        }

        function parseAmount(input) {
            const value = Number.parseFloat(input?.value || "");
            return Number.isFinite(value) && value > 0 ? value : null;
        }

        function getUnitDefinition(unit) {
            const definitions = {
                ml: { group: "volume", factor: 1 },
                cl: { group: "volume", factor: 10 },
                l: { group: "volume", factor: 1000 },
                m3: { group: "volume", factor: 1000000 },
                mg: { group: "weight", factor: 1 },
                gr: { group: "weight", factor: 1000 },
                kg: { group: "weight", factor: 1000000 },
                mm: { group: "length", factor: 1 },
                cm: { group: "length", factor: 10 },
                m: { group: "length", factor: 1000 },
                m2: { group: "area", factor: 1 }
            };

            return definitions[unit] || null;
        }

        function formatAmount(value) {
            if (!Number.isFinite(value)) {
                return "";
            }

            return value % 1 === 0
                ? value.toLocaleString("tr-TR", { maximumFractionDigits: 0 })
                : value.toLocaleString("tr-TR", { minimumFractionDigits: 0, maximumFractionDigits: 4 });
        }

        function syncUnitPriceState() {
            if (unitPricePanel && unitPriceToggle) {
                unitPricePanel.hidden = !unitPriceToggle.checked;
            }

            if (!unitPriceResult) {
                return;
            }

            if (!unitPriceToggle?.checked) {
                unitPriceResult.textContent = "-";
                return;
            }

            const price = parseAmount(form.querySelector("input[name='Price']"));
            const contentAmount = parseAmount(unitContentAmount);
            const comparisonAmount = parseAmount(unitComparisonAmount);
            const contentUnit = unitContentType?.value || "";
            const comparisonUnit = unitComparisonType?.value || "";
            const contentDefinition = getUnitDefinition(contentUnit);
            const comparisonDefinition = getUnitDefinition(comparisonUnit);

            if (!price || !contentAmount || !comparisonAmount || !contentDefinition || !comparisonDefinition) {
                unitPriceResult.textContent = "-";
                return;
            }

            if (contentDefinition.group !== comparisonDefinition.group) {
                unitPriceResult.textContent = "Uyumsuz birim";
                return;
            }

            const contentBaseValue = contentAmount * contentDefinition.factor;
            const comparisonBaseValue = comparisonAmount * comparisonDefinition.factor;

            if (contentBaseValue <= 0 || comparisonBaseValue <= 0) {
                unitPriceResult.textContent = "-";
                return;
            }

            const unitPrice = (price / contentBaseValue) * comparisonBaseValue;
            unitPriceResult.textContent = `? ${unitPrice.toLocaleString("tr-TR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} / ${formatAmount(comparisonAmount)} ${comparisonUnit}`;
        }

        function syncActiveTabFromScroll() {
            const stickyHeight = stickyBar?.getBoundingClientRect().height ?? 0;
            const offset = stickyHeight + 120;
            const visiblePanels = tabPanels
                .filter((panel) => !panel.hidden && panel.offsetParent !== null)
                .map((panel) => ({
                    key: panel.getAttribute("data-product-tab-panel"),
                    top: panel.getBoundingClientRect().top,
                    bottom: panel.getBoundingClientRect().bottom
                }))
                .filter((item) => item.key);

            if (!visiblePanels.length) {
                return;
            }

            const passedPanels = visiblePanels.filter((panel) => panel.top <= offset + 8);
            let nextTab = null;

            if (passedPanels.length > 0) {
                const currentPanel = passedPanels.reduce((winner, panel) => (
                    panel.top > winner.top ? panel : winner
                ));
                nextTab = currentPanel.key;
            } else {
                const upcomingPanel = visiblePanels.reduce((winner, panel) => (
                    panel.top < winner.top ? panel : winner
                ));
                nextTab = upcomingPanel.key;
            }

            if (nextTab && nextTab !== activeTab) {
                syncTabState(nextTab);
            } else {
                ensureActiveTabVisible(false);
            }
        }

        function syncStatusState() {
            if (!statusInput || !statusSelect) {
                return;
            }

            const nextStatus = statusSelect.value || "Draft";
            statusInput.value = nextStatus;
        }

        function syncBrandState() {
            if (!brandSummary || !brandSelect) {
                return;
            }

            brandSummary.textContent = brandSelect.selectedOptions[0]?.text?.trim() || "Marka secilmedi";
        }

        function renderSelectedTags() {
            if (selectedTagInputsContainer) {
                selectedTagInputsContainer.innerHTML = Array.from(selectedTagIds)
                    .sort((left, right) => left - right)
                    .map((id) => `<input type="checkbox" name="SelectedTagIds" value="${id}" checked />`)
                    .join("");
            }

            if (tagSelectorChips) {
                const chipsMarkup = Array.from(selectedTagIds)
                    .map((id) => tagMap.get(id))
                    .filter(Boolean)
                    .sort((left, right) => left.name.localeCompare(right.name, "tr"))
                    .map((tag) => `
                        <span class="admin-product-tag-chip" data-tag-chip="${tag.id}">
                            <span>${escapeHtml(tag.name)}</span>
                            <button type="button" data-tag-remove="${tag.id}" aria-label="${escapeHtml(`${tag.name} etiketini kaldir`)}">
                                <i class="fa-solid fa-xmark"></i>
                            </button>
                        </span>
                    `)
                    .join("");
                tagSelectorChips.innerHTML = chipsMarkup;
            }

            if (tagSelectorClear) {
                tagSelectorClear.hidden = selectedTagIds.size === 0 && !(tagSelectorInput?.value?.trim());
            }

            if (tagSummary) {
                tagSummary.textContent = `${selectedTagIds.size} secili`;
            }
        }

        function renderTagResults() {
            if (!tagSelectorList || !tagSelectorMenu || !tagSelectorInput) {
                return;
            }

            const search = tagSelectorInput.value.trim();
            const normalizedSearch = search.toLocaleLowerCase("tr-TR");
            const results = allTags
                .filter((tag) => !normalizedSearch || tag.name.toLocaleLowerCase("tr-TR").includes(normalizedSearch) || tag.slug.toLocaleLowerCase("tr-TR").includes(normalizedSearch))
                .sort((left, right) => left.name.localeCompare(right.name, "tr"));

            tagSelectorList.innerHTML = results.map((tag) => `
                <button type="button" class="admin-product-tag-option ${selectedTagIds.has(tag.id) ? "is-selected" : ""}" data-tag-option="${tag.id}">
                    <span>${escapeHtml(tag.name)}</span>
                    <i class="fa-solid fa-check"></i>
                </button>
            `).join("");

            const hasResults = results.length > 0;
            if (tagSelectorEmpty) {
                tagSelectorEmpty.hidden = hasResults || !search;
            }
            if (tagSelectorEmptyTitle) {
                tagSelectorEmptyTitle.textContent = search ? `${search} Bulunamadi` : "Sonuc bulunamadi";
            }
            if (tagSelectorEmptyCopy) {
                tagSelectorEmptyCopy.textContent = "Yeni deger eklemek icin isim yazip ENTER tusuna basin";
            }

            tagSelectorMenu.hidden = false;
            tagSelector?.classList.add("is-open");
            if (tagSelectorClear) {
                tagSelectorClear.hidden = selectedTagIds.size === 0 && !search;
            }
        }

        function openTagMenu() {
            renderTagResults();
        }

        function closeTagMenu() {
            if (!tagSelectorMenu) {
                return;
            }

            tagSelectorMenu.hidden = true;
            tagSelector?.classList.remove("is-open");
        }

        function toggleTagSelection(tagId) {
            if (!tagMap.has(tagId)) {
                return;
            }

            if (selectedTagIds.has(tagId)) {
                selectedTagIds.delete(tagId);
            } else {
                selectedTagIds.add(tagId);
            }

            renderSelectedTags();
            renderTagResults();
        }

        async function createTagFromInput() {
            if (!tagSelectorInput) {
                return;
            }

            const name = tagSelectorInput.value.trim();
            if (!name) {
                return;
            }

            const existing = allTags.find((tag) => tag.name.localeCompare(name, "tr", { sensitivity: "accent" }) === 0);
            if (existing) {
                selectedTagIds.add(existing.id);
                tagSelectorInput.value = "";
                renderSelectedTags();
                renderTagResults();
                return;
            }

            const token = form.querySelector("input[name='__RequestVerificationToken']")?.value || "";
            const response = await fetch("/admin/tags/quick-create", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                credentials: "same-origin",
                body: JSON.stringify({ name })
            });

            const payload = await response.json();
            if (!response.ok || !payload?.tag?.id) {
                return;
            }

            const nextTag = {
                id: Number(payload.tag.id),
                name: payload.tag.name,
                slug: payload.tag.slug || ""
            };

            allTags.push(nextTag);
            tagMap.set(nextTag.id, nextTag);
            selectedTagIds.add(nextTag.id);
            tagSelectorInput.value = "";
            renderSelectedTags();
            renderTagResults();
        }

        function renderDefinitionList(listElement, source, selectedIds, search, typeLabel) {
            if (!listElement) {
                return;
            }

            const normalizedSearch = (search || "").trim().toLocaleLowerCase("tr-TR");
            const items = source
                .filter((item) => !normalizedSearch || item.name.toLocaleLowerCase("tr-TR").includes(normalizedSearch) || item.slug.toLocaleLowerCase("tr-TR").includes(normalizedSearch))
                .sort((left, right) => left.name.localeCompare(right.name, "tr"));

            listElement.innerHTML = items.length === 0
                ? `<div class="admin-product-placeholder-state"><div><strong>Sonuc bulunamadi</strong><p>Yeni kayit eklemek icin sagdaki butonu kullanin.</p></div></div>`
                : items.map((item) => `
                    <label class="admin-product-definition-row ${highlightedCustomFieldId === item.id || highlightedPersonalizationId === item.id ? "is-highlighted" : ""}" data-definition-row="${item.id}">
                        <input type="checkbox" data-definition-check="${item.id}" ${selectedIds.has(item.id) ? "checked" : ""} />
                        <span class="admin-product-definition-copy">
                            <strong>${escapeHtml(item.name)}</strong>
                            <small>${escapeHtml(typeLabel(item))}</small>
                        </span>
                    </label>
                `).join("");
        }

        function renderSelectedCustomFields() {
            if (customFieldInputs) {
                customFieldInputs.innerHTML = Array.from(selectedCustomFieldIds)
                    .sort((left, right) => left - right)
                    .map((id) => `<input type="checkbox" name="SelectedCustomFieldIds" value="${id}" checked />`)
                    .join("");
            }

            if (customFieldSelectedList) {
                customFieldSelectedList.innerHTML = Array.from(selectedCustomFieldIds)
                    .map((id) => customFieldMap.get(id))
                    .filter(Boolean)
                    .sort((left, right) => left.name.localeCompare(right.name, "tr"))
                    .map((item) => `
                        <div class="admin-product-selected-row" data-custom-field-row="${item.id}">
                            <div class="admin-product-selected-copy">
                                <strong>${escapeHtml(item.name)}</strong>
                                <small>${escapeHtml(item.fieldType)}${item.isFilterable ? " � Filtrelenebilir" : ""}</small>
                            </div>
                            <details class="admin-product-item-menu">
                                <summary><i class="fa-solid fa-ellipsis"></i></summary>
                                <div class="admin-product-item-menu-panel">
                                    <button type="button" data-custom-field-action="edit" data-custom-field-id="${item.id}">D?zenle</button>
                                    <button type="button" class="is-danger" data-custom-field-action="remove" data-custom-field-id="${item.id}">Kaldir</button>
                                </div>
                            </details>
                        </div>
                    `).join("");
            }

            if (customFieldEmpty) {
                customFieldEmpty.hidden = selectedCustomFieldIds.size > 0;
            }
            if (customFieldSelected) {
                customFieldSelected.hidden = selectedCustomFieldIds.size === 0;
            }
        }

        function renderSelectedPersonalizations() {
            if (personalizationInputs) {
                personalizationInputs.innerHTML = Array.from(selectedPersonalizationIds)
                    .sort((left, right) => left - right)
                    .map((id) => `<input type="checkbox" name="SelectedPersonalizationIds" value="${id}" checked />`)
                    .join("");
            }

            if (personalizationSelectedList) {
                personalizationSelectedList.innerHTML = Array.from(selectedPersonalizationIds)
                    .map((id) => personalizationMap.get(id))
                    .filter(Boolean)
                    .sort((left, right) => left.name.localeCompare(right.name, "tr"))
                    .map((item) => `
                        <div class="admin-product-selected-row" data-personalization-row="${item.id}">
                            <div class="admin-product-selected-copy">
                                <strong>${escapeHtml(item.name)}</strong>
                                <small>${escapeHtml(item.inputType)}</small>
                            </div>
                            <details class="admin-product-item-menu">
                                <summary><i class="fa-solid fa-ellipsis"></i></summary>
                                <div class="admin-product-item-menu-panel">
                                    <button type="button" data-personalization-action="edit" data-personalization-id="${item.id}">D?zenle</button>
                                    <button type="button" class="is-danger" data-personalization-action="remove" data-personalization-id="${item.id}">Kaldir</button>
                                </div>
                            </details>
                        </div>
                    `).join("");
            }

            if (personalizationEmpty) {
                personalizationEmpty.hidden = selectedPersonalizationIds.size > 0;
            }
            if (personalizationSelected) {
                personalizationSelected.hidden = selectedPersonalizationIds.size === 0;
            }
        }

        function renderSelectedFeatures() {
            const selectedGroups = featureGroups
                .filter((group) => group.querySelector("[data-feature-checkbox]")?.checked)
                .map((group) => {
                    const featureId = Number(group.getAttribute("data-feature-id"));
                    const value = group.querySelector("[data-feature-value-option]:checked")?.value || "";
                    return {
                        featureId,
                        groupName: group.getAttribute("data-feature-group-name") || "?zellik",
                        featureName: group.getAttribute("data-feature-name") || "Alan",
                        value
                    };
                });

            if (featureSelectedList) {
                featureSelectedList.innerHTML = selectedGroups.map((item) => `
                    <div class="admin-product-selected-row" data-feature-row="${item.featureId}">
                        <div class="admin-product-selected-copy">
                            <strong>${escapeHtml(item.featureName)}</strong>
                            <small>${escapeHtml(item.value || item.groupName)}</small>
                        </div>
                        <details class="admin-product-item-menu">
                            <summary><i class="fa-solid fa-ellipsis"></i></summary>
                            <div class="admin-product-item-menu-panel">
                                <button type="button" data-feature-action="edit" data-feature-id="${item.featureId}">D?zenle</button>
                                <button type="button" class="is-danger" data-feature-action="remove" data-feature-id="${item.featureId}">Kaldir</button>
                            </div>
                        </details>
                    </div>
                `).join("");
            }

            if (featureEmpty) {
                featureEmpty.hidden = selectedGroups.length > 0;
            }
            if (featureSelected) {
                featureSelected.hidden = selectedGroups.length === 0;
            }
        }

        function createFeatureSnapshot() {
            return featureGroups.map((group) => ({
                checkbox: group.querySelector("[data-feature-checkbox]")?.checked ?? false,
                selectedValue: group.querySelector("[data-feature-value-option]:checked")?.value ?? ""
            }));
        }

        function restoreFeatureSnapshot(snapshot) {
            if (!Array.isArray(snapshot)) {
                return;
            }

            featureGroups.forEach((group, index) => {
                const state = snapshot[index];
                const checkbox = group.querySelector("[data-feature-checkbox]");
                const optionInputs = Array.from(group.querySelectorAll("[data-feature-value-option]"));
                if (!state || !checkbox) {
                    return;
                }

                checkbox.checked = state.checkbox;
                optionInputs.forEach((input) => {
                    input.checked = !!state.selectedValue && input.value === state.selectedValue;
                    input.disabled = !state.checkbox;
                });
                if (!state.checkbox) {
                    optionInputs.forEach((input) => {
                        input.checked = false;
                    });
                }
                group.classList.toggle("is-selected", state.checkbox);
                group.classList.toggle("is-highlighted", highlightedFeatureId === Number(group.getAttribute("data-feature-id")));
                group.querySelector("[data-feature-select]")?.classList.toggle("is-disabled", !state.checkbox);
            });
        }

        function escapeHtml(value) {
            return String(value)
                .replaceAll("&", "&amp;")
                .replaceAll("<", "&lt;")
                .replaceAll(">", "&gt;")
                .replaceAll("\"", "&quot;")
                .replaceAll("'", "&#39;");
        }

        featureGroups.forEach((group) => {
            const checkbox = group.querySelector("[data-feature-checkbox]");
            const optionWrap = group.querySelector("[data-feature-select]");
            const optionInputs = Array.from(group.querySelectorAll("[data-feature-value-option]"));

            const syncFeatureState = () => {
                if (!checkbox) {
                    return;
                }

                group.classList.toggle("is-selected", checkbox.checked);
                group.classList.toggle("is-highlighted", highlightedFeatureId === Number(group.getAttribute("data-feature-id")));
                optionWrap?.classList.toggle("is-disabled", !checkbox.checked);
                optionInputs.forEach((input) => {
                    input.disabled = !checkbox.checked;
                });

                if (!checkbox.checked) {
                    optionInputs.forEach((input) => {
                        input.checked = false;
                    });
                }

                renderSelectedFeatures();
            };

            optionInputs.forEach((input) => {
                input.addEventListener("change", () => {
                    if (input.checked && checkbox) {
                        checkbox.checked = true;
                        syncFeatureState();
                    }
                });
            });

            checkbox?.addEventListener("change", syncFeatureState);
            syncFeatureState();
        });

        form.querySelectorAll("[data-category-selector-open]").forEach((button) => {
            button.addEventListener("click", () => {
                highlightedCategoryId = null;
                draftSelectedIds = new Set(savedSelectedIds);
                renderSelectorList();
                openModal(selectorModal);
            });
        });

        selectorSearchInput?.addEventListener("input", renderSelectorList);

        selectorList?.addEventListener("change", (event) => {
            const target = event.target.closest("[data-category-selector-check]");
            if (!(target instanceof HTMLInputElement)) {
                return;
            }

            const categoryId = Number(target.getAttribute("data-category-selector-check"));
            const affected = collectDescendants(categoryId);
            if (target.checked) {
                affected.forEach((id) => draftSelectedIds.add(id));
            } else {
                affected.forEach((id) => draftSelectedIds.delete(id));
            }
            renderSelectorList();
        });

        selectorModal?.querySelector("[data-category-selector-save]")?.addEventListener("click", () => {
            savedSelectedIds = new Set(draftSelectedIds);
            if (!savedSelectedIds.has(primaryCategoryId)) {
                primaryCategoryId = Array.from(savedSelectedIds)[0] || null;
            }
            renderSelectedCategories();
            closeModal(selectorModal);
        });

        categorySelected?.addEventListener("click", (event) => {
            const actionButton = event.target.closest("[data-category-action]");
            if (!(actionButton instanceof HTMLElement)) {
                return;
            }

            const categoryId = Number(actionButton.getAttribute("data-category-id"));
            const action = actionButton.getAttribute("data-category-action");
            if (!(categoryId > 0) || !action) {
                return;
            }

            if (action === "make-primary") {
                primaryCategoryId = categoryId;
            }

            if (action === "edit") {
                highlightedCategoryId = categoryId;
                draftSelectedIds = new Set(savedSelectedIds);
                renderSelectorList();
                openModal(selectorModal);
                requestAnimationFrame(() => revealHighlighted(`[data-category-selector-row="${categoryId}"]`));
            }

            if (action === "remove") {
                savedSelectedIds.delete(categoryId);
                if (primaryCategoryId === categoryId) {
                    primaryCategoryId = Array.from(savedSelectedIds)[0] || null;
                }
            }

            renderSelectedCategories();
            actionButton.closest("details")?.removeAttribute("open");
        });

        document.querySelectorAll("[data-category-modal-close]").forEach((button) => {
            button.addEventListener("click", () => closeModal(selectorModal));
        });

        document.querySelectorAll("[data-category-create-open]").forEach((button) => {
            button.addEventListener("click", () => openModal(createModal));
        });

        document.querySelectorAll("[data-category-create-close]").forEach((button) => {
            button.addEventListener("click", () => closeModal(createModal));
        });

        createModal?.querySelector("[data-category-create-save]")?.addEventListener("click", async () => {
            const token = form.querySelector("input[name='__RequestVerificationToken']")?.value || "";
            const body = {
                name: createModal.querySelector("[data-category-create-name]")?.value || "",
                parentId: Number(createModal.querySelector("[data-category-create-parent]")?.value || "") || null,
                description: createModal.querySelector("[data-category-create-description]")?.value || "",
                imageUrl: createModal.querySelector("[data-category-create-image]")?.value || "",
                productSortType: createModal.querySelector("[data-category-create-sort]")?.value || "",
                slug: createModal.querySelector("[data-category-create-slug]")?.value || "",
                seoTitle: createModal.querySelector("[data-category-create-seo-title]")?.value || "",
                metaDescription: createModal.querySelector("[data-category-create-meta-description]")?.value || "",
                isActive: Boolean(createModal.querySelector("[data-category-create-active]")?.checked)
            };

            createModal.querySelectorAll("[data-category-create-error]").forEach((element) => {
                element.textContent = "";
            });

            const response = await fetch("/admin/categories/quick-create", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                credentials: "same-origin",
                body: JSON.stringify(body)
            });

            const payload = await response.json();
            if (!response.ok) {
                Object.entries(payload?.errors || {}).forEach(([key, messages]) => {
                    const normalizedKey = key.split(".").pop();
                    const target = createModal.querySelector(`[data-category-create-error='${normalizedKey}']`);
                    if (target) {
                        target.textContent = Array.isArray(messages) ? messages[0] : "Kayit yapilamadi.";
                    }
                });
                return;
            }

            if (payload?.category?.id) {
                allCategories.push({
                    id: Number(payload.category.id),
                    name: payload.category.name,
                    parentId: payload.category.parentId
                });
                categoryMap.set(Number(payload.category.id), {
                    id: Number(payload.category.id),
                    name: payload.category.name,
                    parentId: payload.category.parentId
                });
                draftSelectedIds.add(Number(payload.category.id));
                savedSelectedIds.add(Number(payload.category.id));
                if (!primaryCategoryId) {
                    primaryCategoryId = Number(payload.category.id);
                }
                renderSelectedCategories();
                renderSelectorList();
            }

            closeModal(createModal);
        });

        form.querySelectorAll("[data-custom-field-selector-open]").forEach((button) => {
            button.addEventListener("click", () => {
                highlightedCustomFieldId = null;
                draftCustomFieldIds = new Set(selectedCustomFieldIds);
                renderDefinitionList(customFieldList, allCustomFields, draftCustomFieldIds, customFieldSearchInput?.value || "", (item) => `${item.fieldType}${item.isFilterable ? " � Filtrelenebilir" : ""}`);
                openModal(customFieldModal);
            });
        });

        customFieldSearchInput?.addEventListener("input", () => {
            renderDefinitionList(customFieldList, allCustomFields, draftCustomFieldIds, customFieldSearchInput.value, (item) => `${item.fieldType}${item.isFilterable ? " � Filtrelenebilir" : ""}`);
        });

        customFieldList?.addEventListener("change", (event) => {
            const target = event.target.closest("[data-definition-check]");
            if (!(target instanceof HTMLInputElement)) {
                return;
            }

            const definitionId = Number(target.getAttribute("data-definition-check"));
            if (!(definitionId > 0)) {
                return;
            }

            if (target.checked) {
                draftCustomFieldIds.add(definitionId);
            } else {
                draftCustomFieldIds.delete(definitionId);
            }
        });

        form.querySelector("[data-custom-field-modal-save]")?.addEventListener("click", () => {
            selectedCustomFieldIds = new Set(draftCustomFieldIds);
            renderSelectedCustomFields();
            highlightedCustomFieldId = null;
            closeModal(customFieldModal);
        });

        form.querySelectorAll("[data-custom-field-modal-close]").forEach((button) => {
            button.addEventListener("click", () => {
                draftCustomFieldIds = new Set(selectedCustomFieldIds);
                highlightedCustomFieldId = null;
                closeModal(customFieldModal);
            });
        });

        customFieldSelectedList?.addEventListener("click", (event) => {
            const actionButton = event.target.closest("[data-custom-field-action]");
            if (!(actionButton instanceof HTMLElement)) {
                return;
            }

            const fieldId = Number(actionButton.getAttribute("data-custom-field-id"));
            const action = actionButton.getAttribute("data-custom-field-action");
            if (!(fieldId > 0) || !action) {
                return;
            }

            if (action === "edit") {
                highlightedCustomFieldId = fieldId;
                draftCustomFieldIds = new Set(selectedCustomFieldIds);
                renderDefinitionList(customFieldList, allCustomFields, draftCustomFieldIds, customFieldSearchInput?.value || "", (item) => `${item.fieldType}${item.isFilterable ? " � Filtrelenebilir" : ""}`);
                openModal(customFieldModal);
                requestAnimationFrame(() => revealHighlighted(`[data-definition-row="${fieldId}"]`));
            }

            if (action === "remove") {
                selectedCustomFieldIds.delete(fieldId);
                draftCustomFieldIds.delete(fieldId);
                renderSelectedCustomFields();
            }

            actionButton.closest("details")?.removeAttribute("open");
        });

        document.querySelectorAll("[data-custom-field-create-open]").forEach((button) => {
            button.addEventListener("click", () => openModal(customFieldCreateModal));
        });

        document.querySelectorAll("[data-custom-field-create-close]").forEach((button) => {
            button.addEventListener("click", () => closeModal(customFieldCreateModal));
        });

        customFieldCreateModal?.querySelector("[data-custom-field-create-save]")?.addEventListener("click", async () => {
            const token = form.querySelector("input[name='__RequestVerificationToken']")?.value || "";
            const body = {
                name: customFieldCreateModal.querySelector("[data-custom-field-create-name]")?.value || "",
                fieldType: customFieldCreateModal.querySelector("[data-custom-field-create-type]")?.value || "",
                isFilterable: Boolean(customFieldCreateModal.querySelector("[data-custom-field-create-filterable]")?.checked)
            };

            customFieldCreateModal.querySelectorAll("[data-custom-field-create-error]").forEach((element) => {
                element.textContent = "";
            });

            const response = await fetch("/admin/products/custom-fields/quick-create", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                credentials: "same-origin",
                body: JSON.stringify(body)
            });

            const payload = await response.json();
            if (!response.ok) {
                Object.entries(payload?.errors || {}).forEach(([key, messages]) => {
                    const normalizedKey = key.split(".").pop();
                    const target = customFieldCreateModal.querySelector(`[data-custom-field-create-error='${normalizedKey}']`);
                    if (target) {
                        target.textContent = Array.isArray(messages) ? messages[0] : "Kayit yapilamadi.";
                    }
                });
                return;
            }

            if (payload?.customField?.id) {
                const nextField = {
                    id: Number(payload.customField.id),
                    name: payload.customField.name,
                    slug: payload.customField.slug || "",
                    fieldType: payload.customField.fieldType || "Text",
                    isFilterable: Boolean(payload.customField.isFilterable)
                };

                allCustomFields.push(nextField);
                customFieldMap.set(nextField.id, nextField);
                draftCustomFieldIds.add(nextField.id);
                selectedCustomFieldIds.add(nextField.id);
                renderSelectedCustomFields();
                renderDefinitionList(customFieldList, allCustomFields, draftCustomFieldIds, customFieldSearchInput?.value || "", (item) => `${item.fieldType}${item.isFilterable ? " � Filtrelenebilir" : ""}`);
            }

            closeModal(customFieldCreateModal);
        });

        form.querySelectorAll("[data-personalization-selector-open]").forEach((button) => {
            button.addEventListener("click", () => {
                highlightedPersonalizationId = null;
                draftPersonalizationIds = new Set(selectedPersonalizationIds);
                renderDefinitionList(personalizationList, allPersonalizations, draftPersonalizationIds, personalizationSearchInput?.value || "", (item) => item.inputType);
                openModal(personalizationModal);
            });
        });

        personalizationSearchInput?.addEventListener("input", () => {
            renderDefinitionList(personalizationList, allPersonalizations, draftPersonalizationIds, personalizationSearchInput.value, (item) => item.inputType);
        });

        personalizationList?.addEventListener("change", (event) => {
            const target = event.target.closest("[data-definition-check]");
            if (!(target instanceof HTMLInputElement)) {
                return;
            }

            const definitionId = Number(target.getAttribute("data-definition-check"));
            if (!(definitionId > 0)) {
                return;
            }

            if (target.checked) {
                draftPersonalizationIds.add(definitionId);
            } else {
                draftPersonalizationIds.delete(definitionId);
            }
        });

        form.querySelector("[data-personalization-modal-save]")?.addEventListener("click", () => {
            selectedPersonalizationIds = new Set(draftPersonalizationIds);
            renderSelectedPersonalizations();
            highlightedPersonalizationId = null;
            closeModal(personalizationModal);
        });

        form.querySelectorAll("[data-personalization-modal-close]").forEach((button) => {
            button.addEventListener("click", () => {
                draftPersonalizationIds = new Set(selectedPersonalizationIds);
                highlightedPersonalizationId = null;
                closeModal(personalizationModal);
            });
        });

        personalizationSelectedList?.addEventListener("click", (event) => {
            const actionButton = event.target.closest("[data-personalization-action]");
            if (!(actionButton instanceof HTMLElement)) {
                return;
            }

            const definitionId = Number(actionButton.getAttribute("data-personalization-id"));
            const action = actionButton.getAttribute("data-personalization-action");
            if (!(definitionId > 0) || !action) {
                return;
            }

            if (action === "edit") {
                highlightedPersonalizationId = definitionId;
                draftPersonalizationIds = new Set(selectedPersonalizationIds);
                renderDefinitionList(personalizationList, allPersonalizations, draftPersonalizationIds, personalizationSearchInput?.value || "", (item) => item.inputType);
                openModal(personalizationModal);
                requestAnimationFrame(() => revealHighlighted(`[data-definition-row="${definitionId}"]`));
            }

            if (action === "remove") {
                selectedPersonalizationIds.delete(definitionId);
                draftPersonalizationIds.delete(definitionId);
                renderSelectedPersonalizations();
            }

            actionButton.closest("details")?.removeAttribute("open");
        });

        document.querySelectorAll("[data-personalization-create-open]").forEach((button) => {
            button.addEventListener("click", () => openModal(personalizationCreateModal));
        });

        document.querySelectorAll("[data-personalization-create-close]").forEach((button) => {
            button.addEventListener("click", () => closeModal(personalizationCreateModal));
        });

        personalizationCreateModal?.querySelector("[data-personalization-create-save]")?.addEventListener("click", async () => {
            const token = form.querySelector("input[name='__RequestVerificationToken']")?.value || "";
            const body = {
                name: personalizationCreateModal.querySelector("[data-personalization-create-name]")?.value || "",
                inputType: personalizationCreateModal.querySelector("[data-personalization-create-type]")?.value || ""
            };

            personalizationCreateModal.querySelectorAll("[data-personalization-create-error]").forEach((element) => {
                element.textContent = "";
            });

            const response = await fetch("/admin/products/personalizations/quick-create", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                credentials: "same-origin",
                body: JSON.stringify(body)
            });

            const payload = await response.json();
            if (!response.ok) {
                Object.entries(payload?.errors || {}).forEach(([key, messages]) => {
                    const normalizedKey = key.split(".").pop();
                    const target = personalizationCreateModal.querySelector(`[data-personalization-create-error='${normalizedKey}']`);
                    if (target) {
                        target.textContent = Array.isArray(messages) ? messages[0] : "Kayit yapilamadi.";
                    }
                });
                return;
            }

            if (payload?.personalization?.id) {
                const nextItem = {
                    id: Number(payload.personalization.id),
                    name: payload.personalization.name,
                    slug: payload.personalization.slug || "",
                    inputType: payload.personalization.inputType || "Text"
                };

                allPersonalizations.push(nextItem);
                personalizationMap.set(nextItem.id, nextItem);
                draftPersonalizationIds.add(nextItem.id);
                selectedPersonalizationIds.add(nextItem.id);
                renderSelectedPersonalizations();
                renderDefinitionList(personalizationList, allPersonalizations, draftPersonalizationIds, personalizationSearchInput?.value || "", (item) => item.inputType);
            }

            closeModal(personalizationCreateModal);
        });

        form.querySelectorAll("[data-feature-selector-open]").forEach((button) => {
            button.addEventListener("click", () => {
                highlightedFeatureId = null;
                featureSnapshot = createFeatureSnapshot();
                openModal(featureModal);
            });
        });

        form.querySelectorAll("[data-feature-modal-close]").forEach((button) => {
            button.addEventListener("click", () => {
                restoreFeatureSnapshot(featureSnapshot);
                renderSelectedFeatures();
                closeModal(featureModal);
            });
        });

        form.querySelector("[data-feature-modal-save]")?.addEventListener("click", () => {
            renderSelectedFeatures();
            featureSnapshot = null;
            highlightedFeatureId = null;
            closeModal(featureModal);
        });

        form.querySelector("[data-feature-modal-cancel]")?.addEventListener("click", () => {
            restoreFeatureSnapshot(featureSnapshot);
            renderSelectedFeatures();
            highlightedFeatureId = null;
            closeModal(featureModal);
        });

        featureSelectedList?.addEventListener("click", (event) => {
            const actionButton = event.target.closest("[data-feature-action]");
            if (!(actionButton instanceof HTMLElement)) {
                return;
            }

            const featureId = Number(actionButton.getAttribute("data-feature-id"));
            const action = actionButton.getAttribute("data-feature-action");
            if (!(featureId > 0) || !action) {
                return;
            }

            const featureGroup = featureGroups.find((group) => Number(group.getAttribute("data-feature-id")) === featureId);
            const checkbox = featureGroup?.querySelector("[data-feature-checkbox]");
            const optionInputs = Array.from(featureGroup?.querySelectorAll("[data-feature-value-option]") || []);

            if (action === "edit") {
                highlightedFeatureId = featureId;
                featureSnapshot = createFeatureSnapshot();
                featureGroups.forEach((group) => {
                    group.classList.toggle("is-highlighted", highlightedFeatureId === Number(group.getAttribute("data-feature-id")));
                });
                openModal(featureModal);
                requestAnimationFrame(() => revealHighlighted(`[data-feature-id="${featureId}"]`));
            }

            if (action === "remove" && checkbox) {
                checkbox.checked = false;
                optionInputs.forEach((input) => {
                    input.checked = false;
                });
                checkbox.dispatchEvent(new Event("change", { bubbles: true }));
                renderSelectedFeatures();
            }

            actionButton.closest("details")?.removeAttribute("open");
        });

        brandSelect?.addEventListener("change", syncBrandState);
        statusSelect?.addEventListener("change", syncStatusState);
        unitPriceToggle?.addEventListener("change", syncUnitPriceState);
        [unitContentAmount, unitContentType, unitComparisonAmount, unitComparisonType, form.querySelector("input[name='Price']")]
            .filter(Boolean)
            .forEach((element) => element.addEventListener("input", syncUnitPriceState));
        [unitContentType, unitComparisonType]
            .filter(Boolean)
            .forEach((element) => element.addEventListener("change", syncUnitPriceState));

        tagSelectorControl?.addEventListener("click", () => {
            openTagMenu();
            tagSelectorInput?.focus();
        });

        tagSelectorInput?.addEventListener("focus", openTagMenu);
        tagSelectorInput?.addEventListener("input", renderTagResults);
        tagSelectorInput?.addEventListener("keydown", async (event) => {
            if (event.key === "Enter") {
                event.preventDefault();
                await createTagFromInput();
            }

            if (event.key === "Backspace" && !tagSelectorInput.value.trim() && selectedTagIds.size > 0) {
                const lastSelectedId = Array.from(selectedTagIds).at(-1);
                if (lastSelectedId) {
                    selectedTagIds.delete(lastSelectedId);
                    renderSelectedTags();
                    renderTagResults();
                }
            }

            if (event.key === "Escape") {
                closeTagMenu();
            }
        });

        tagSelectorList?.addEventListener("click", (event) => {
            const target = event.target.closest("[data-tag-option]");
            if (!(target instanceof HTMLElement)) {
                return;
            }

            const tagId = Number(target.getAttribute("data-tag-option"));
            if (tagId > 0) {
                toggleTagSelection(tagId);
            }
        });

        tagSelectorChips?.addEventListener("click", (event) => {
            const removeButton = event.target.closest("[data-tag-remove]");
            if (!(removeButton instanceof HTMLElement)) {
                return;
            }

            const tagId = Number(removeButton.getAttribute("data-tag-remove"));
            if (tagId > 0) {
                selectedTagIds.delete(tagId);
                renderSelectedTags();
                renderTagResults();
                tagSelectorInput?.focus();
            }
        });

        tagSelectorClear?.addEventListener("click", (event) => {
            event.stopPropagation();
            if (tagSelectorInput) {
                tagSelectorInput.value = "";
            }
            renderSelectedTags();
            renderTagResults();
            tagSelectorInput?.focus();
        });

        infoSections.forEach((wrap) => {
            const toggle = wrap.querySelector("[data-inline-info-toggle]");
            const tooltip = wrap.querySelector("[data-inline-info-tooltip]");
            let isPinned = false;

            const syncInfoState = (forceOpen = false) => {
                if (!tooltip) {
                    return;
                }

                const isHovered = wrap.matches(":hover");
                const shouldOpen = forceOpen || isPinned || isHovered;
                wrap.classList.toggle("is-hovered", isHovered && !isPinned);
                wrap.classList.toggle("is-pinned", isPinned);
                tooltip.hidden = !shouldOpen;
            };

            wrap.addEventListener("mouseenter", () => syncInfoState(true));
            wrap.addEventListener("mouseleave", () => {
                if (!isPinned) {
                    syncInfoState(false);
                }
            });

            toggle?.addEventListener("click", (event) => {
                event.stopPropagation();
                isPinned = !isPinned;
                syncInfoState(isPinned);
            });

            document.addEventListener("click", (event) => {
                if (!isPinned || wrap.contains(event.target)) {
                    return;
                }

                isPinned = false;
                syncInfoState(false);
            });

            syncInfoState(false);
        });

        document.addEventListener("click", (event) => {
            if (!tagSelector || tagSelector.contains(event.target)) {
                return;
            }

            closeTagMenu();
        });

        tabButtons.forEach((button) => {
            button.addEventListener("click", () => {
                const tab = button.getAttribute("data-product-tab");
                if (tab) {
                    scrollToTab(tab);
                }
            });
        });

        tabArrowLeft?.addEventListener("click", () => {
            stepTabNavigation(-1);
        });

        tabArrowRight?.addEventListener("click", () => {
            stepTabNavigation(1);
        });

        tabNav?.addEventListener("scroll", () => {
            syncTabOverflowState();
        });

        if (tabNav && "MutationObserver" in window) {
            const tabObserver = new MutationObserver(() => {
                requestAnimationFrame(() => {
                    ensureActiveTabVisible(false);
                    syncTabOverflowState();
                });
            });
            tabObserver.observe(tabNav, { attributes: true, subtree: true, attributeFilter: ["hidden", "class"] });
        }

        window.addEventListener("scroll", () => {
            syncHeaderState();
            syncActiveTabFromScroll();
        }, { passive: true });

        window.addEventListener("resize", () => {
            ensureActiveTabVisible(false);
            syncTabOverflowState();
        });

        syncStatusState();
        syncBrandState();
        renderSelectedTags();
        closeTagMenu();
        renderSelectedCustomFields();
        renderSelectedPersonalizations();
        syncUnitPriceState();
        syncTabState(initialTab);
        syncHeaderState();
        renderSelectedCategories();
        syncTabOverflowState();
        ensureActiveTabVisible(false);

        if (initialTab !== "basic") {
            requestAnimationFrame(() => scrollToTab(initialTab, false));
        } else {
            syncActiveTabFromScroll();
        }
    });
})();
