(function () {
    function parseJson(element, key) {
        try {
            return JSON.parse(element.dataset[key] || "{}");
        } catch {
            return {};
        }
    }

    function initChatWidget(widget) {
        const panel = widget.querySelector("[data-chat-panel]");
        const input = widget.querySelector("[data-chat-input]");
        const fullscreenButton = widget.querySelector("[data-chat-fullscreen]");
        const sendButton = widget.querySelector("[data-chat-send]");
        const modeButtons = widget.querySelectorAll("[data-chat-mode]");
        const addButton = widget.querySelector("[data-chat-add]");
        const fileMenu = widget.querySelector("[data-chat-file-menu]");
        const fileList = widget.querySelector("[data-chat-files]");
        const searchFilter = widget.querySelector("[data-chat-search-filter]");
        const searchCategory = widget.querySelector("[data-chat-search-category]");
        const clearFilter = widget.querySelector("[data-chat-clear-filter]");
        const activeCategory = widget.querySelector("[data-chat-active-category]");
        const promptsByCategory = parseJson(widget, "prompts");
        const categories = parseJson(widget, "categories");
        const categoryPills = widget.querySelectorAll("[data-category-pill]");
        const variant = widget.dataset.variant || "home";
        const mainPlaceholder = widget.dataset.mainPlaceholder || "";
        const searchPlaceholder = widget.dataset.searchPlaceholder || "";
        const searchPlaceholderLocked = widget.dataset.searchPlaceholderLocked || "";
        let categoryIndex = 0;
        let currentMode = "chat";
        let lockedCategory = widget.dataset.categorySlug || "";
        let promptTimer = null;
        let typeTimer = null;
        let eraseTimer = null;
        let currentAnimatedText = "";
        let isPlaceholderTyping = false;
        let isPlaceholderErasing = false;
        let isPlaceholderReadyToSend = false;
        const lineThreshold = 3;
        const promptQueues = buildPromptQueues();

        function normalizeCategory(category) {
            return {
                slug: category?.slug || category?.Slug || "",
                name: category?.name || category?.Name || ""
            };
        }

        function buildPromptQueues() {
            const queues = {};
            categories.map(normalizeCategory).forEach((category) => {
                const pool = Array.isArray(promptsByCategory[category.slug]) ? [...promptsByCategory[category.slug]] : [];
                queues[category.slug] = shuffle(pool);
            });

            return queues;
        }

        function shuffle(items) {
            const cloned = [...items];
            for (let i = cloned.length - 1; i > 0; i -= 1) {
                const j = Math.floor(Math.random() * (i + 1));
                [cloned[i], cloned[j]] = [cloned[j], cloned[i]];
            }

            return cloned;
        }

        function resizeInput() {
            input.style.height = "auto";
            const baseHeight = parseFloat(getComputedStyle(input).getPropertyValue("--chat-input-base-height")) || 84;
            input.style.height = `${baseHeight}px`;
            input.classList.toggle("has-scrollbar", currentMode === "chat" && input.scrollHeight > input.clientHeight);
            updateFullscreenVisibility();
        }

        function updateFullscreenVisibility() {
            const shouldShow = currentMode === "chat" && input.classList.contains("has-scrollbar");
            fullscreenButton?.classList.toggle("visible", shouldShow);
        }

        function clearActivePills() {
            categoryPills.forEach((pill) => pill.classList.remove("active"));
        }

        function setActiveCategoryPill(slug) {
            clearActivePills();
            categoryPills.forEach((pill) => {
                if (pill.dataset.categorySlug === slug) {
                    pill.classList.add("active");
                }
            });
        }

        function getNextPrompt() {
            const orderedCategories = categories.map(normalizeCategory).filter((category) => category.slug);
            if (!orderedCategories.length) {
                return null;
            }

            const category = orderedCategories[categoryIndex % orderedCategories.length];
            if (!promptQueues[category.slug] || promptQueues[category.slug].length === 0) {
                promptQueues[category.slug] = shuffle(Array.isArray(promptsByCategory[category.slug]) ? promptsByCategory[category.slug] : []);
            }

            const prompt = promptQueues[category.slug].shift();
            categoryIndex = (categoryIndex + 1) % orderedCategories.length;

            return prompt ? { prompt, category } : null;
        }

        function typePlaceholder(text, index, callback) {
            isPlaceholderTyping = true;
            isPlaceholderErasing = false;
            isPlaceholderReadyToSend = false;
            updateSendState();

            if (index === 0) {
                input.placeholder = "";
            }

            input.placeholder = text.slice(0, index + 1);
            if (index + 1 < text.length) {
                typeTimer = window.setTimeout(() => typePlaceholder(text, index + 1, callback), 26);
                return;
            }

            isPlaceholderTyping = false;
            isPlaceholderReadyToSend = true;
            updateSendState();
            callback?.();
        }

        function erasePlaceholder(text, index, callback) {
            isPlaceholderTyping = false;
            isPlaceholderErasing = true;
            isPlaceholderReadyToSend = false;
            updateSendState();

            input.placeholder = text.slice(0, index);
            if (index > 0) {
                eraseTimer = window.setTimeout(() => erasePlaceholder(text, index - 1, callback), 12);
                return;
            }

            isPlaceholderErasing = false;
            isPlaceholderReadyToSend = false;
            updateSendState();
            callback?.();
        }

        function animateNextPrompt() {
            const next = getNextPrompt();
            if (!next) {
                input.placeholder = mainPlaceholder;
                return;
            }

            currentAnimatedText = next.prompt;
            if (activeCategory) {
                activeCategory.textContent = next.category.name;
            }
            setActiveCategoryPill(next.category.slug);

            const previousText = input.placeholder || "";
            if (previousText) {
                erasePlaceholder(previousText, previousText.length, function () {
                    typePlaceholder(next.prompt, 0);
                });
                return;
            }

            typePlaceholder(next.prompt, 0);
        }

        function setPlaceholder() {
            if (currentMode === "search") {
                input.placeholder = lockedCategory ? searchPlaceholderLocked : searchPlaceholder;
                if (lockedCategory) {
                    searchFilter.classList.remove("hidden");
                } else {
                    searchFilter.classList.add("hidden");
                }
                clearActivePills();
                return;
            }

            animateNextPrompt();
        }

        function startPromptRotation() {
            if (variant !== "home") {
                return;
            }

            stopPromptRotation();
            setPlaceholder();
            promptTimer = window.setInterval(setPlaceholder, 4200);
        }

        function stopPromptRotation() {
            if (promptTimer) {
                window.clearInterval(promptTimer);
                promptTimer = null;
            }
            if (typeTimer) {
                window.clearTimeout(typeTimer);
                typeTimer = null;
            }
            if (eraseTimer) {
                window.clearTimeout(eraseTimer);
                eraseTimer = null;
            }
            currentAnimatedText = "";
            isPlaceholderTyping = false;
            isPlaceholderErasing = false;
            isPlaceholderReadyToSend = false;
            updateSendState();
        }

        function setMode(mode) {
            currentMode = mode;
            panel.classList.toggle("search-mode-active", mode === "search");
            modeButtons.forEach((button) => {
                button.classList.toggle("active", button.dataset.chatMode === mode);
            });

            if (mode === "search") {
                stopPromptRotation();
                currentAnimatedText = "";
                isPlaceholderReadyToSend = false;
            } else if (!input.value.trim()) {
                startPromptRotation();
            }

            setPlaceholder();
            resizeInput();
        }

        function updateSendState() {
            const hasManualInput = input.value.trim().length > 0;
            const canSendAnimatedPrompt =
                !!currentAnimatedText &&
                isPlaceholderReadyToSend &&
                !isPlaceholderTyping &&
                !isPlaceholderErasing;

            const isActive = hasManualInput || canSendAnimatedPrompt;
            sendButton.classList.toggle("active", isActive);
            sendButton.disabled = !isActive;
        }

        function handleFiles(files) {
            Array.from(files).forEach((file) => {
                const item = document.createElement("div");
                item.className = "ag-file-item";
                item.innerHTML = `<i class="${file.type.startsWith("image/") ? "fa-regular fa-image" : "fa-solid fa-file-pdf"}"></i><span>${file.name}</span><button type="button" class="ag-file-remove" aria-label="Dosyayı kaldır"><i class="fa-solid fa-xmark"></i></button>`;
                item.querySelector(".ag-file-remove")?.addEventListener("click", function (event) {
                    event.preventDefault();
                    event.stopPropagation();
                    item.remove();
                });
                fileList.appendChild(item);
            });
        }

        input.addEventListener("input", function () {
            resizeInput();
            updateSendState();
            if (this.value.trim()) {
                stopPromptRotation();
                currentAnimatedText = "";
                isPlaceholderReadyToSend = false;
                clearActivePills();
            } else if (currentMode === "chat") {
                startPromptRotation();
            }
        });

        fullscreenButton?.addEventListener("click", function (event) {
            event.preventDefault();
            panel.classList.toggle("fullscreen-mode");
        });

        modeButtons.forEach((button) => {
            button.addEventListener("click", function (event) {
                event.preventDefault();
                setMode(button.dataset.chatMode || "chat");
            });
        });

        addButton?.addEventListener("click", function (event) {
            event.preventDefault();
            fileMenu.classList.toggle("show");
        });

        document.addEventListener("click", function (event) {
            if (!widget.contains(event.target)) {
                fileMenu.classList.remove("show");
            }
        });

        widget.querySelectorAll("[data-chat-file-trigger]").forEach((trigger) => {
            trigger.addEventListener("click", function () {
                const target = widget.querySelector(`[data-chat-file-input="${trigger.dataset.chatFileTrigger}"]`);
                target?.click();
                fileMenu.classList.remove("show");
            });
        });

        widget.querySelectorAll("[data-chat-file-input]").forEach((inputFile) => {
            inputFile.addEventListener("change", function () {
                handleFiles(this.files || []);
                this.value = "";
            });
        });

        clearFilter?.addEventListener("click", function (event) {
            event.preventDefault();
            if (variant === "category") {
                return;
            }

            lockedCategory = "";
            searchCategory.textContent = "Kategori";
            setMode("search");
        });

        if (lockedCategory && searchCategory) {
            searchCategory.textContent = widget.dataset.categoryName || "Kategori";
        }

        input.placeholder = mainPlaceholder;
        resizeInput();
        updateSendState();
        if (variant === "home") {
            startPromptRotation();
        } else {
            clearActivePills();
            if (activeCategory) {
                activeCategory.textContent = widget.dataset.categoryName || "";
            }
            input.placeholder = mainPlaceholder;
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-chat-widget]").forEach(initChatWidget);
    });
})();
