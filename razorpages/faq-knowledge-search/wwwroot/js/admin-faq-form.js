document.addEventListener("DOMContentLoaded", () => {
    const titleInput = document.querySelector("[data-title-input]");
    const titleCount = document.querySelector("[data-title-count]");

    const bodyInput = document.querySelector("[data-body-input]");
    const bodyCount = document.querySelector("[data-body-count]");

    const updateTitleCount = () => {
        if (!titleInput || !titleCount) {
            return;
        }

        titleCount.textContent =
            `${titleInput.value.length}/100`;
    };

    const updateBodyCount = () => {
        if (!bodyInput || !bodyCount) {
            return;
        }

        bodyCount.textContent =
            `${bodyInput.value.length}文字`;
    };

    titleInput?.addEventListener(
        "input",
        updateTitleCount);

    bodyInput?.addEventListener(
        "input",
        updateBodyCount);

    updateTitleCount();
    updateBodyCount();
});