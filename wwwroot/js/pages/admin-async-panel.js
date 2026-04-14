(function () {
    const roots = Array.from(document.querySelectorAll("[data-admin-async-root='true']"));

    if (!roots.length) {
        return;
    }

    const rootMap = new Map();

    roots.forEach((root) => {
        const endpoint = root.getAttribute("data-admin-async-endpoint");

        if (!endpoint) {
            return;
        }

        rootMap.set(endpoint, root);

        root.addEventListener("click", async (event) => {
            const link = event.target.closest("[data-admin-async-link='true']");

            if (!link || !root.contains(link)) {
                return;
            }

            const href = link.getAttribute("href");

            if (!href) {
                return;
            }

            event.preventDefault();
            await loadIntoRoot(root, href, true);
        });

        root.addEventListener("submit", async (event) => {
            const form = event.target.closest("[data-admin-async-form='true']");

            if (!form || !root.contains(form)) {
                return;
            }

            event.preventDefault();

            const action = form.getAttribute("action") || endpoint;
            const formData = new FormData(form);
            const params = new URLSearchParams();

            formData.forEach((value, key) => {
                if (typeof value === "string" && value.trim() !== "") {
                    params.set(key, value);
                }
            });

            const requestUrl = params.toString() ? `${action}?${params.toString()}` : action;
            await loadIntoRoot(root, requestUrl, true);
        });
    });

    window.addEventListener("popstate", async () => {
        const currentUrl = new URL(window.location.href);

        for (const [endpoint, root] of rootMap.entries()) {
            if (currentUrl.pathname === endpoint) {
                await loadIntoRoot(root, currentUrl.toString(), false);
                break;
            }
        }
    });

    async function loadIntoRoot(root, url, pushState) {
        root.classList.add("is-loading");

        try {
            const response = await fetch(url, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                window.location.assign(url);
                return;
            }

            const html = await response.text();
            root.innerHTML = html;

            if (pushState) {
                window.history.pushState({}, "", url);
            }
        } catch (error) {
            window.location.assign(url);
        } finally {
            root.classList.remove("is-loading");
        }
    }
})();
