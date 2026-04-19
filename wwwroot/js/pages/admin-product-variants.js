(function () {
    const editor = document.querySelector("[data-variant-editor]");
    const list = editor?.querySelector("[data-variant-list]");
    const addButton = editor?.querySelector("[data-variant-add]");
    const template = document.querySelector("[data-variant-template]");

    if (!editor || !list || !addButton || !template) {
        return;
    }

    const toggleEmptyState = () => {
        const rows = list.querySelectorAll("[data-variant-row]");
        const empty = list.querySelector("[data-variant-empty]");
        if (empty) {
            empty.hidden = rows.length > 0;
        }
    };

    const syncIndexes = () => {
        const rows = Array.from(list.querySelectorAll("[data-variant-row]"));

        rows.forEach((row, index) => {
            const fields = row.querySelectorAll("input");
            fields.forEach((field) => {
                const currentName = field.getAttribute("name");
                if (!currentName) {
                    return;
                }

                field.setAttribute("name", currentName.replace(/Variants\[\d+\]/g, `Variants[${index}]`));
            });
        });
    };

    addButton.addEventListener("click", () => {
        const index = list.querySelectorAll("[data-variant-row]").length;
        const fragment = template.content.cloneNode(true);
        fragment.querySelectorAll("[name]").forEach((field) => {
            const currentName = field.getAttribute("name");
            if (!currentName) {
                return;
            }

            field.setAttribute("name", currentName.replace(/__index__/g, index.toString()));
        });

        list.appendChild(fragment);
        toggleEmptyState();
    });

    list.addEventListener("click", (event) => {
        const removeButton = event.target instanceof Element
            ? event.target.closest("[data-variant-remove]")
            : null;

        if (!removeButton) {
            return;
        }

        const row = removeButton.closest("[data-variant-row]");
        row?.remove();
        syncIndexes();
        toggleEmptyState();
    });

    toggleEmptyState();
})();
