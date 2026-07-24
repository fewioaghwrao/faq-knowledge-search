(() => {
    const overlay =
        document.getElementById("globalLoadingOverlay");

    const messageElement =
        document.getElementById("globalLoadingMessage");

    if (!overlay || !messageElement) {
        return;
    }

    const show = (message = "処理しています…") => {
        messageElement.textContent = message;

        overlay.hidden = false;
        overlay.setAttribute("aria-hidden", "false");

        document.body.classList.add(
            "is-global-loading");
    };

    const hide = () => {
        overlay.hidden = true;
        overlay.setAttribute("aria-hidden", "true");

        document.body.classList.remove(
            "is-global-loading");
    };

    const isFormValid = form => {
        if (!form.checkValidity()) {
            return false;
        }

        // jQuery Validationが有効な画面にも対応
        if (window.jQuery) {
            const $form = window.jQuery(form);

            if (typeof $form.valid === "function" &&
                !$form.valid()) {
                return false;
            }
        }

        return true;
    };

    document.addEventListener("submit", event => {
        const form = event.target;

        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        // AJAX送信など、共通ローディングを使わないフォーム
        if (form.dataset.noGlobalLoading === "true") {
            return;
        }

        if (event.defaultPrevented ||
            !isFormValid(form)) {
            return;
        }

        // 二重送信防止
        if (form.dataset.loadingSubmitted === "true") {
            event.preventDefault();
            return;
        }

        form.dataset.loadingSubmitted = "true";

        const message =
            form.dataset.loadingMessage ??
            "処理しています…";

        show(message);
    });

    // 戻る操作・ブラウザーキャッシュ復元時に解除
    window.addEventListener("pageshow", () => {
        document
            .querySelectorAll(
                "form[data-loading-submitted='true']")
            .forEach(form => {
                delete form.dataset.loadingSubmitted;
            });

        hide();
    });

    // fetch処理などからも利用可能
    window.globalLoading = {
        show,
        hide
    };
})();

(() => {
    const toggleButton =
        document.querySelector("[data-site-menu-toggle]");

    const navigation =
        document.querySelector("[data-site-navigation]");

    const label =
        document.querySelector("[data-site-menu-label]");

    if (!toggleButton ||
        !navigation ||
        !label) {
        return;
    }

    const mobileMedia =
        window.matchMedia("(max-width: 768px)");

    const setMenuOpen = open => {
        navigation.classList.toggle(
            "is-open",
            open);

        toggleButton.setAttribute(
            "aria-expanded",
            open.toString());

        label.textContent =
            open ? "閉じる" : "メニュー";
    };

    toggleButton.addEventListener("click", () => {
        const isOpen =
            toggleButton.getAttribute(
                "aria-expanded") === "true";

        setMenuOpen(!isOpen);
    });

    navigation.addEventListener("click", event => {
        if (!mobileMedia.matches) {
            return;
        }

        const target =
            event.target.closest(
                "a, button[data-bs-target]");

        if (target) {
            setMenuOpen(false);
        }
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape") {
            setMenuOpen(false);
        }
    });

    mobileMedia.addEventListener("change", event => {
        if (!event.matches) {
            setMenuOpen(false);
        }
    });

    setMenuOpen(false);
})();