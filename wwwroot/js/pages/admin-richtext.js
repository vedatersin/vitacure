(function () {
    const textareas = Array.from(document.querySelectorAll("[data-admin-richtext='true']"));

    const commands = [
        { command: "bold", icon: "fa-solid fa-bold", label: "KalIn" },
        { command: "italic", icon: "fa-solid fa-italic", label: "Italik" },
        { command: "underline", icon: "fa-solid fa-underline", label: "Alt cizgi" },
        { command: "insertUnorderedList", icon: "fa-solid fa-list-ul", label: "Madde listesi" },
        { command: "insertOrderedList", icon: "fa-solid fa-list-ol", label: "Numarali liste" }
    ];

    textareas.forEach((textarea) => {
        const wrapper = document.createElement("div");
        wrapper.className = "admin-richtext";

        const toolbar = document.createElement("div");
        toolbar.className = "admin-richtext-toolbar";

        const editor = document.createElement("div");
        editor.className = "admin-richtext-editor";
        editor.contentEditable = "true";
        editor.dataset.placeholder = textarea.getAttribute("data-richtext-placeholder") || "";
        editor.innerHTML = textarea.value.trim() || "<p><br></p>";

        const sync = () => {
            textarea.value = editor.innerHTML.trim() === "<br>" ? "" : editor.innerHTML.trim();
            editor.classList.toggle("is-empty", editor.textContent.trim().length === 0);
        };

        commands.forEach((item) => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "admin-richtext-button";
            button.innerHTML = `<i class="${item.icon}"></i>`;
            button.setAttribute("aria-label", item.label);
            button.addEventListener("click", () => {
                editor.focus();
                document.execCommand(item.command, false);
                sync();
            });
            toolbar.appendChild(button);
        });

        [
            { text: "P", label: "Paragraf", action: () => document.execCommand("formatBlock", false, "p") },
            { text: "H3", label: "Ara baslik", action: () => document.execCommand("formatBlock", false, "h3") }
        ].forEach((item) => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "admin-richtext-button";
            button.textContent = item.text;
            button.setAttribute("aria-label", item.label);
            button.addEventListener("click", () => {
                editor.focus();
                item.action();
                sync();
            });
            toolbar.appendChild(button);
        });

        const linkButton = document.createElement("button");
        linkButton.type = "button";
        linkButton.className = "admin-richtext-button";
        linkButton.innerHTML = `<i class="fa-solid fa-link"></i>`;
        linkButton.setAttribute("aria-label", "Baglanti ekle");
        linkButton.addEventListener("click", () => {
            const url = window.prompt("Baglanti adresi", "https://");
            if (!url) {
                return;
            }

            editor.focus();
            document.execCommand("createLink", false, url);
            sync();
        });
        toolbar.appendChild(linkButton);

        const clearButton = document.createElement("button");
        clearButton.type = "button";
        clearButton.className = "admin-richtext-button";
        clearButton.innerHTML = `<i class="fa-solid fa-eraser"></i>`;
        clearButton.setAttribute("aria-label", "Temizle");
        clearButton.addEventListener("click", () => {
            editor.focus();
            document.execCommand("removeFormat", false);
            sync();
        });
        toolbar.appendChild(clearButton);

        editor.addEventListener("input", sync);
        editor.addEventListener("blur", sync);

        textarea.classList.add("d-none");
        textarea.parentNode?.insertBefore(wrapper, textarea);
        wrapper.appendChild(toolbar);
        wrapper.appendChild(editor);
        wrapper.appendChild(textarea);

        textarea.form?.addEventListener("submit", sync);
        sync();
    });
})();
