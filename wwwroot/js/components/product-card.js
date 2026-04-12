document.addEventListener("DOMContentLoaded", function () {
    const cartResetTimers = new WeakMap();
    const cartLoadingTimers = new WeakMap();
    const body = document.body;
    const favoriteSeed = body?.dataset.favoriteSlugs ? JSON.parse(body.dataset.favoriteSlugs) : [];
    const isAuthenticatedCustomer = body?.dataset.customerAuthenticated === "true";
    const cartSeedCount = Number.parseInt(body?.dataset.cartItemCount || "0", 10);
    const favoriteStore = window.__vitacureFavoriteStore || (window.__vitacureFavoriteStore = new Set(favoriteSeed));
    const antiForgeryToken = document.querySelector('meta[name="request-verification-token"]')?.getAttribute("content") || "";
    const cartState = window.__vitacureCartState || (window.__vitacureCartState = { count: Number.isFinite(cartSeedCount) ? cartSeedCount : 0 });

    function buildReturnUrl() {
        return window.location.pathname + window.location.search + window.location.hash;
    }

    function redirectToLogin() {
        window.location.href = `/login?returnUrl=${encodeURIComponent(buildReturnUrl())}`;
    }

    function updateCartBadge(count) {
        const nextCount = Math.max(0, Number.parseInt(String(count), 10) || 0);
        cartState.count = nextCount;

        if (body) {
            body.dataset.cartItemCount = String(nextCount);
        }

        document.querySelectorAll(".cart-badge").forEach((badge) => {
            badge.textContent = String(nextCount);
        });
    }

    function animateCartButton(button) {
        const label = button.querySelector(".pc-add-btn-text, .coverflow-action-btn-text");
        const defaultLabel = button.getAttribute("data-default-label") || "Sepete Ekle";
        const addedLabel = button.getAttribute("data-added-label") || "Eklendi";

        const existingResetTimer = cartResetTimers.get(button);
        if (existingResetTimer) {
            window.clearTimeout(existingResetTimer);
        }

        const existingLoadingTimer = cartLoadingTimers.get(button);
        if (existingLoadingTimer) {
            window.clearTimeout(existingLoadingTimer);
        }

        button.classList.remove("is-added");
        button.classList.add("is-loading");
        button.setAttribute("aria-pressed", "false");

        if (label) {
            label.textContent = "";
        }

        const loadingTimerId = window.setTimeout(() => {
            button.classList.remove("is-loading");
            button.classList.remove("is-added");
            void button.offsetWidth;
            button.classList.add("is-added");
            button.setAttribute("aria-pressed", "true");

            if (label) {
                label.textContent = addedLabel;
            }

            const resetTimerId = window.setTimeout(() => {
                button.classList.remove("is-added");
                button.setAttribute("aria-pressed", "false");

                if (label) {
                    label.textContent = defaultLabel;
                }
            }, 1000);

            cartResetTimers.set(button, resetTimerId);
        }, 500);

        cartLoadingTimers.set(button, loadingTimerId);
    }

    async function syncCart(productSlug, quantity) {
        if (!isAuthenticatedCustomer) {
            redirectToLogin();
            return null;
        }

        const response = await fetch("/cart/items", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": antiForgeryToken
            },
            body: JSON.stringify({ productSlug, quantity })
        });

        if (response.status === 401) {
            redirectToLogin();
            return null;
        }

        const payload = await response.json().catch(() => null);
        if (!response.ok || !payload?.isSuccess) {
            return null;
        }

        updateCartBadge(payload.cartItemCount);
        return payload;
    }

    async function syncFavorite(productId, isActive, button) {
        if (!isAuthenticatedCustomer) {
            redirectToLogin();
            return false;
        }

        const response = await fetch("/account/favorites/toggle", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": antiForgeryToken
            },
            body: JSON.stringify({ productSlug: productId })
        });

        if (response.status === 401) {
            redirectToLogin();
            return false;
        }

        if (!response.ok) {
            button.classList.toggle("is-active", !isActive);
            button.setAttribute("aria-pressed", !isActive ? "true" : "false");
            return false;
        }

        return true;
    }

    window.VitacureCart = {
        addItem: async function (productSlug, button, quantity = 1) {
            const result = await syncCart(productSlug, quantity);
            if (!result) {
                return false;
            }

            if (button) {
                animateCartButton(button);
            }

            return true;
        },
        updateBadge: updateCartBadge,
        getCount: function () {
            return cartState.count;
        }
    };

    document.querySelectorAll("[data-favorite-button]").forEach((button) => {
        const productId = button.getAttribute("data-favorite-product-id") || "";

        if (productId && favoriteStore.has(productId)) {
            button.classList.add("is-active");
            button.setAttribute("aria-pressed", "true");
            const initialIcon = button.querySelector("i");
            if (initialIcon) {
                initialIcon.classList.remove("fa-regular");
                initialIcon.classList.add("fa-solid");
            }
        }

        button.addEventListener("click", async function (event) {
            event.preventDefault();
            event.stopPropagation();

            const isActive = button.classList.toggle("is-active");
            button.setAttribute("aria-pressed", isActive ? "true" : "false");

            if (productId) {
                if (isActive) {
                    favoriteStore.add(productId);
                } else {
                    favoriteStore.delete(productId);
                }
            }

            const icon = button.querySelector("i");
            if (icon) {
                icon.classList.toggle("fa-regular", !isActive);
                icon.classList.toggle("fa-solid", isActive);
            }

            const synced = await syncFavorite(productId, isActive, button);
            if (!synced && icon) {
                const revertedState = button.classList.contains("is-active");
                icon.classList.toggle("fa-regular", !revertedState);
                icon.classList.toggle("fa-solid", revertedState);
            }
        });
    });

    document.querySelectorAll("[data-cart-button]").forEach((button) => {
        button.addEventListener("click", async function (event) {
            event.preventDefault();
            event.stopPropagation();

            const productSlug = button.getAttribute("data-cart-product-id") || "";
            if (!productSlug) {
                return;
            }

            await window.VitacureCart.addItem(productSlug, button, 1);
        });
    });
});
