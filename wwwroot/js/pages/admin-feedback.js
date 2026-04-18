document.addEventListener("DOMContentLoaded", () => {
    const toasts = Array.from(document.querySelectorAll("[data-admin-toast]"));
    if (toasts.length === 0) {
        return;
    }

    toasts.forEach((toast) => {
        const closeButton = toast.querySelector("[data-admin-toast-close]");
        const isSticky = toast.dataset.sticky === "true";

        function dismissToast() {
            toast.classList.add("is-leaving");
            window.setTimeout(() => {
                const stack = toast.closest("[data-admin-toast-stack]");
                toast.remove();
                if (stack && stack.children.length === 0) {
                    stack.remove();
                }
            }, 220);
        }

        closeButton?.addEventListener("click", dismissToast);

        if (!isSticky) {
            window.setTimeout(dismissToast, 4200);
        }
    });
});
