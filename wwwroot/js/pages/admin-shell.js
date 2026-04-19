document.addEventListener("DOMContentLoaded", () => {
    const shell = document.querySelector("[data-admin-shell]");
    if (!shell) {
        return;
    }

    const openButtons = Array.from(document.querySelectorAll("[data-admin-shell-open]"));
    const closeButtons = Array.from(document.querySelectorAll("[data-admin-shell-close]"));
    const desktopBreakpoint = 1080;

    const openShell = () => shell.classList.add("is-nav-open");
    const closeShell = () => shell.classList.remove("is-nav-open");

    openButtons.forEach((button) => {
        button.addEventListener("click", openShell);
    });

    closeButtons.forEach((button) => {
        button.addEventListener("click", closeShell);
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeShell();
        }
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth > desktopBreakpoint) {
            closeShell();
        }
    });
});
