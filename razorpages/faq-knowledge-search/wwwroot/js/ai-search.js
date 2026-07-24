document.addEventListener("DOMContentLoaded", () => {
    const input =
        document.querySelector("[data-ai-input]");

    const count =
        document.querySelector("[data-ai-count]");

    const exampleButtons =
        document.querySelectorAll("[data-ai-example]");

    const feedbackForm =
        document.querySelector("[data-ai-feedback-form]");

    const feedbackButtons =
        document.querySelectorAll(
            "[data-ai-feedback-button]");

    const feedbackMessage =
        document.querySelector(
            "[data-ai-feedback-message]");

    const updateCount = () => {
        if (!input || !count) {
            return;
        }

        count.textContent =
            `${input.value.length}/500`;
    };

    input?.addEventListener(
        "input",
        updateCount);

    exampleButtons.forEach(button => {
        button.addEventListener("click", () => {
            if (!input) {
                return;
            }

            input.value =
                button.dataset.aiExample ?? "";

            input.focus();
            updateCount();
        });
    });

    feedbackButtons.forEach(button => {
        button.addEventListener("click", async () => {
            if (!feedbackForm) {
                return;
            }

            const wasHelpful =
                button.dataset.aiFeedbackValue;

            if (!wasHelpful) {
                return;
            }

            setFeedbackButtonsDisabled(true);

            if (feedbackMessage) {
                feedbackMessage.textContent =
                    "登録しています…";

                feedbackMessage.classList.remove(
                    "error",
                    "success");
            }

            try {
                const formData =
                    new FormData(feedbackForm);

                formData.append(
                    "wasHelpful",
                    wasHelpful);

                const response = await fetch(
                    feedbackForm.action,
                    {
                        method: "POST",
                        body: formData,
                        credentials: "same-origin",
                        headers: {
                            "X-Requested-With":
                                "XMLHttpRequest"
                        }
                    });

                const payload =
                    await response.json();

                if (!response.ok ||
                    payload.success !== true) {
                    throw new Error(
                        payload.message ??
                        "登録に失敗しました。");
                }

                updateSelectedFeedback(
                    payload.wasHelpful === true);

                if (feedbackMessage) {
                    feedbackMessage.textContent =
                        payload.message;

                    feedbackMessage.classList.add(
                        "success");
                }
            } catch (error) {
                if (feedbackMessage) {
                    feedbackMessage.textContent =
                        error instanceof Error
                            ? error.message
                            : "フィードバックの登録に失敗しました。";

                    feedbackMessage.classList.add(
                        "error");
                }
            } finally {
                setFeedbackButtonsDisabled(false);
            }
        });
    });

    const setFeedbackButtonsDisabled = disabled => {
        feedbackButtons.forEach(button => {
            button.disabled = disabled;
        });
    };

    const updateSelectedFeedback = wasHelpful => {
        feedbackButtons.forEach(button => {
            const buttonValue =
                button.dataset.aiFeedbackValue ===
                "true";

            const isSelected =
                buttonValue === wasHelpful;

            button.classList.toggle(
                "is-selected",
                isSelected);

            button.setAttribute(
                "aria-pressed",
                isSelected.toString());
        });
    };

    updateCount();
});