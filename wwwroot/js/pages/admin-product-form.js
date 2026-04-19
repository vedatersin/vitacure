(function () {
    const forms = Array.from(document.querySelectorAll("[data-admin-product-form='true']"));

    forms.forEach((form) => {
        const categoryInput = form.querySelector("[data-product-category-input]");
        const parentSelect = form.querySelector("[data-product-parent-category]");
        const childSelect = form.querySelector("[data-product-child-category]");
        const selectedPath = form.querySelector("[data-product-category-path]");
        const categoryParentChoices = Array.from(form.querySelectorAll("[data-category-parent-choice]"));
        const categoryChildChoices = Array.from(form.querySelectorAll("[data-category-child-choice]"));
        const statusInput = form.querySelector("[data-product-status-input]");
        const statusSelect = form.querySelector("[data-product-status-select]");
        const statusBadge = form.querySelector("[data-product-status-badge]");
        const brandSelect = form.querySelector("select[name='BrandId']");
        const brandSummary = form.querySelector("[data-product-brand-summary]");
        const tagInputs = Array.from(form.querySelectorAll("input[name='SelectedTagIds']"));
        const tagSummary = form.querySelector("[data-product-tag-summary]");
        const featureSummary = form.querySelector("[data-product-feature-summary]");
        const featureGroups = Array.from(form.querySelectorAll("[data-feature-option-group]"));

        const syncCategoryState = () => {
            if (!categoryInput || !parentSelect || !childSelect) {
                return;
            }

            const parentId = parentSelect.value || "";
            const currentValue = childSelect.value || parentId || "0";

            Array.from(childSelect.options).forEach((option) => {
                if (!option.value) {
                    option.hidden = false;
                    return;
                }

                const optionParentId = option.getAttribute("data-parent-id") || "";
                option.hidden = parentId.length > 0 && optionParentId !== parentId;
            });

            const hasVisibleChild = Array.from(childSelect.options).some((option) => option.value && !option.hidden);
            childSelect.disabled = !parentId || !hasVisibleChild;

            if (childSelect.disabled) {
                childSelect.value = "";
            } else if (childSelect.value) {
                const selectedOption = childSelect.selectedOptions[0];
                if (!selectedOption || selectedOption.hidden) {
                    childSelect.value = "";
                }
            }

            categoryInput.value = childSelect.value || parentId || "0";

            if (selectedPath) {
                const parentLabel = parentSelect.selectedOptions[0]?.text?.trim() || "";
                const childLabel = childSelect.value ? childSelect.selectedOptions[0]?.text?.trim() || "" : "";
                selectedPath.textContent = childLabel ? `${parentLabel} / ${childLabel}` : (parentLabel || "Kategori secilmedi");
            }

            categoryParentChoices.forEach((button) => {
                button.closest(".admin-product-category-node")?.classList.toggle("is-selected", button.getAttribute("data-category-parent-choice") === parentSelect.value);
            });

            categoryChildChoices.forEach((button) => {
                button.classList.toggle("is-selected", button.getAttribute("data-category-child-choice") === childSelect.value);
            });
        };

        const syncStatusState = () => {
            if (!statusInput || !statusSelect) {
                return;
            }

            const isActive = statusSelect.value === "true";
            statusInput.value = isActive ? "true" : "false";

            if (statusBadge) {
                statusBadge.textContent = isActive ? "Yayinda" : "Taslak";
                statusBadge.classList.toggle("is-active", isActive);
            }
        };

        const syncBrandState = () => {
            if (!brandSummary || !brandSelect) {
                return;
            }

            brandSummary.textContent = brandSelect.selectedOptions[0]?.text?.trim() || "Marka secilmedi";
        };

        const syncTagState = () => {
            if (!tagSummary) {
                return;
            }

            const checkedCount = tagInputs.filter((input) => input.checked).length;
            tagSummary.textContent = `${checkedCount} secili`;

            tagInputs.forEach((input) => {
                const pill = input.closest(".admin-product-choice-pill, .admin-product-merchandising-toggle");
                pill?.classList.toggle("is-checked", input.checked);
            });
        };

        const syncFeatureSummary = () => {
            if (!featureSummary) {
                return;
            }

            const checkedCount = featureGroups.filter((group) => {
                const checkbox = group.querySelector("[data-feature-checkbox]");
                return checkbox?.checked;
            }).length;

            featureSummary.textContent = `${checkedCount} secili`;
        };

        featureGroups.forEach((group) => {
            const checkbox = group.querySelector("[data-feature-checkbox]");
            const optionWrap = group.querySelector("[data-feature-select]");
            const optionInputs = Array.from(group.querySelectorAll("[data-feature-value-option]"));

            const syncFeatureState = () => {
                if (!checkbox) {
                    return;
                }

                group.classList.toggle("is-selected", checkbox.checked);
                optionWrap?.classList.toggle("is-disabled", !checkbox.checked);
                optionInputs.forEach((input) => {
                    input.disabled = !checkbox.checked;
                });

                if (!checkbox.checked) {
                    optionInputs.forEach((input) => {
                        input.checked = false;
                    });
                }

                syncFeatureSummary();
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

        categoryParentChoices.forEach((button) => {
            button.addEventListener("click", () => {
                if (!parentSelect) {
                    return;
                }

                parentSelect.value = button.getAttribute("data-category-parent-choice") || "";
                if (childSelect) {
                    childSelect.value = "";
                }
                syncCategoryState();
            });
        });

        categoryChildChoices.forEach((button) => {
            button.addEventListener("click", () => {
                if (!parentSelect || !childSelect) {
                    return;
                }

                const parentId = button.getAttribute("data-category-parent-link") || "";
                const childId = button.getAttribute("data-category-child-choice") || "";

                parentSelect.value = parentId;
                childSelect.value = childId;
                syncCategoryState();
            });
        });

        brandSelect?.addEventListener("change", syncBrandState);
        tagInputs.forEach((input) => input.addEventListener("change", syncTagState));
        parentSelect?.addEventListener("change", syncCategoryState);
        childSelect?.addEventListener("change", syncCategoryState);
        statusSelect?.addEventListener("change", syncStatusState);

        syncCategoryState();
        syncStatusState();
        syncBrandState();
        syncTagState();
        syncFeatureSummary();
    });
})();
