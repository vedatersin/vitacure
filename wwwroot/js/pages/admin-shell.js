document.addEventListener("DOMContentLoaded", () => {
    const shell = document.querySelector("[data-admin-shell]");
    if (!shell) {
        return;
    }

    const openButtons = Array.from(document.querySelectorAll("[data-admin-shell-open]"));
    const closeButtons = Array.from(document.querySelectorAll("[data-admin-shell-close]"));
    const primaryRail = shell.querySelector("[data-admin-primary-rail]");
    const secondaryRail = shell.querySelector("[data-admin-secondary-rail]");
    const notificationWrap = shell.querySelector("[data-admin-rail-notification]");
    const notificationToggle = shell.querySelector("[data-admin-rail-notification-toggle]");
    const notificationPanel = shell.querySelector("[data-admin-notification-panel]");
    const profileMenu = shell.querySelector("[data-admin-rail-profile-menu]");
    const profileToggle = shell.querySelector("[data-admin-rail-profile-toggle]");
    const desktopBreakpoint = 1080;
    let notificationSummaryLoaded = false;

    const openShell = () => shell.classList.add("is-nav-open");
    const closeShell = () => shell.classList.remove("is-nav-open");
    const closeNotificationPanel = () => notificationWrap?.classList.remove("is-open");
    const closeProfileMenu = () => profileMenu?.classList.remove("is-open");
    const isDesktop = () => window.innerWidth > desktopBreakpoint;
    const activateSecondaryFocus = () => {
        if (isDesktop()) {
            shell.classList.add("is-secondary-focus");
        }
    };
    const deactivateSecondaryFocus = () => {
        if (isDesktop()) {
            shell.classList.remove("is-secondary-focus");
        }
    };

    const formatOccurredAt = (value) => {
        if (!value) {
            return "";
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return "";
        }

        return new Intl.DateTimeFormat("tr-TR", {
            day: "2-digit",
            month: "short",
            hour: "2-digit",
            minute: "2-digit"
        }).format(date);
    };

    const renderNotificationSummary = (summary) => {
        const badges = Array.from(shell.querySelectorAll("[data-admin-notification-badge], [data-admin-notification-badge-clone]"));
        const list = notificationPanel?.querySelector("[data-admin-notification-list]");
        if (!notificationPanel || badges.length === 0 || !list) {
            return;
        }

        const unreadCount = Number(summary?.unreadCount ?? 0);
        badges.forEach((badge) => {
            badge.textContent = String(unreadCount);
            badge.classList.toggle("has-unread", unreadCount > 0);
            badge.classList.toggle("is-empty", unreadCount === 0);
        });

        const items = Array.isArray(summary?.items) ? summary.items : [];
        list.innerHTML = "";

        if (items.length === 0) {
            const empty = document.createElement("div");
            empty.className = "admin-rail-notification-empty";
            empty.innerHTML = "<strong>Bildirim yok</strong><span>Yeni olaylar burada görünecek.</span>";
            list.appendChild(empty);
            return;
        }

        items.forEach((item) => {
            const link = document.createElement("a");
            link.href = item.url || "/admin/notifications";
            link.className = "admin-rail-notification-item";

            const icon = document.createElement("span");
            icon.className = `admin-rail-notification-icon ${item.accentClass || ""}`.trim();
            icon.innerHTML = `<i class="${item.iconClass || "fa-regular fa-bell"}"></i>`;

            const copy = document.createElement("span");
            copy.className = "admin-rail-notification-copy";
            copy.innerHTML = `<strong>${item.title || ""}</strong><small>${item.categoryLabel || ""} - ${formatOccurredAt(item.occurredAt)}</small>`;

            link.appendChild(icon);
            link.appendChild(copy);

            if (item.isUnread) {
                const dot = document.createElement("span");
                dot.className = "admin-rail-notification-dot";
                link.appendChild(dot);
            }

            list.appendChild(link);
        });
    };

    const loadNotificationSummary = async () => {
        if (!notificationPanel || notificationSummaryLoaded) {
            return;
        }

        const endpoint = notificationPanel.getAttribute("data-admin-notification-endpoint");
        if (!endpoint) {
            return;
        }

        try {
            const response = await window.fetch(endpoint, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error(`Notification summary failed: ${response.status}`);
            }

            const summary = await response.json();
            renderNotificationSummary(summary);
            notificationSummaryLoaded = true;
        } catch {
            const list = notificationPanel.querySelector("[data-admin-notification-list]");
            if (list) {
                list.innerHTML = "<div class=\"admin-rail-notification-empty\"><strong>Bildirimler yuklenemedi</strong><span>Daha sonra tekrar deneyin.</span></div>";
            }
        }
    };

    openButtons.forEach((button) => {
        button.addEventListener("click", openShell);
    });

    closeButtons.forEach((button) => {
        button.addEventListener("click", closeShell);
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeShell();
            closeNotificationPanel();
            closeProfileMenu();
        }
    });

    notificationToggle?.addEventListener("click", async (event) => {
        event.stopPropagation();
        const willOpen = !notificationWrap?.classList.contains("is-open");
        notificationWrap?.classList.toggle("is-open", willOpen);
        closeProfileMenu();
        if (willOpen) {
            await loadNotificationSummary();
        }
    });

    profileToggle?.addEventListener("click", (event) => {
        event.stopPropagation();
        const willOpen = !profileMenu?.classList.contains("is-open");
        profileMenu?.classList.toggle("is-open", willOpen);
        closeNotificationPanel();
    });

    document.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof Node)) {
            return;
        }

        if (notificationWrap && !notificationWrap.contains(target)) {
            closeNotificationPanel();
        }

        if (profileMenu && !profileMenu.contains(target)) {
            closeProfileMenu();
        }
    });

    document.addEventListener("pointermove", (event) => {
        const target = event.target;
        if (!(target instanceof Node)) {
            return;
        }

        const isInsideSidebar = (primaryRail?.contains(target) ?? falsecondaryRail?.contains(target) ?? false);

        if (notificationWrap?.classList.contains("is-open") && !isInsideSidebar && !(notificationPanel?.contains(target) ?? false)) {
            closeNotificationPanel();
        }

        if (profileMenu?.classList.contains("is-open") && !isInsideSidebar && !(profileMenu?.contains(target) ?? false)) {
            closeProfileMenu();
        }
    });

    if (primaryRail && secondaryRail) {
        primaryRail.addEventListener("mouseenter", deactivateSecondaryFocus);
        secondaryRail.addEventListener("mouseenter", activateSecondaryFocus);

        primaryRail.addEventListener("focusin", deactivateSecondaryFocus);
        secondaryRail.addEventListener("focusin", activateSecondaryFocus);

        const contextLinks = Array.from(secondaryRail.querySelectorAll(".admin-context-link:not(.is-muted)"));
        if (contextLinks.length > 0 && !contextLinks.some((link) => link.classList.contains("is-active"))) {
            contextLinks[0].classList.add("is-active");
        }
    }

    window.addEventListener("resize", () => {
        if (isDesktop()) {
            closeShell();
        } else {
            deactivateSecondaryFocus();
        }
    });
});
