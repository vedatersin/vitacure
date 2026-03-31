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
        const variant = widget.dataset.variant || "home";
        const mainPlaceholder = widget.dataset.mainPlaceholder || "";
        const searchPlaceholder = widget.dataset.searchPlaceholder || "";
        const searchPlaceholderLocked = widget.dataset.searchPlaceholderLocked || "";
        let promptIndex = 0;
        let categoryIndex = 0;
        let currentMode = "chat";
        let lockedCategory = widget.dataset.categorySlug || "";
        let promptTimer = null;

        function resizeInput() {
            input.style.height = "auto";
            input.style.height = `${Math.min(input.scrollHeight, currentMode === "search" ? 40 : 180)}px`;
        }

        function setPlaceholder() {
            if (currentMode === "search") {
                input.placeholder = lockedCategory ? searchPlaceholderLocked : searchPlaceholder;
                if (lockedCategory) {
                    searchFilter.classList.remove("hidden");
                } else {
                    searchFilter.classList.add("hidden");
                }
                return;
            }

            const category = categories[categoryIndex];
            const categorySlug = category?.slug || category?.Slug;
            const categoryName = category?.name || category?.Name;
            const prompts = categorySlug ? promptsByCategory[categorySlug] || [] : [];
            if (!prompts.length) {
                input.placeholder = mainPlaceholder;
                return;
            }

            input.placeholder = prompts[promptIndex % prompts.length];
            if (activeCategory && categoryName) {
                activeCategory.textContent = categoryName;
            }
            promptIndex += 1;
            categoryIndex = (categoryIndex + 1) % Math.max(categories.length, 1);
        }

        function startPromptRotation() {
            if (variant !== "home") {
                return;
            }

            stopPromptRotation();
            setPlaceholder();
            promptTimer = window.setInterval(setPlaceholder, 3000);
        }

        function stopPromptRotation() {
            if (promptTimer) {
                window.clearInterval(promptTimer);
                promptTimer = null;
            }
        }

        function setMode(mode) {
            currentMode = mode;
            panel.classList.toggle("search-mode-active", mode === "search");
            modeButtons.forEach((button) => {
                button.classList.toggle("active", button.dataset.chatMode === mode);
            });

            if (mode === "search") {
                stopPromptRotation();
            } else if (!input.value.trim()) {
                startPromptRotation();
            }

            setPlaceholder();
            resizeInput();
        }

        function updateSendState() {
            sendButton.classList.toggle("active", input.value.trim().length > 0 || !!input.placeholder);
        }

        function handleFiles(files) {
            Array.from(files).forEach((file) => {
                const item = document.createElement("div");
                item.className = "ag-file-item";
                item.innerHTML = `<i class="${file.type.startsWith("image/") ? "fa-regular fa-image" : "fa-solid fa-file-pdf"}"></i><span>${file.name}</span>`;
                fileList.appendChild(item);
            });
        }

        input.addEventListener("input", function () {
            resizeInput();
            updateSendState();
            if (this.value.trim()) {
                stopPromptRotation();
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
            setMode("search");
            setMode("chat");
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-chat-widget]").forEach(initChatWidget);
    });
})();
