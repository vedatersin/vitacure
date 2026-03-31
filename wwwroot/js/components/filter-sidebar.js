document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".filter-block").forEach((block) => {
        const heading = block.querySelector(".d-flex");
        if (!heading) {
            return;
        }

        heading.addEventListener("click", function () {
            block.classList.toggle("collapsed");
        });
    });
});
