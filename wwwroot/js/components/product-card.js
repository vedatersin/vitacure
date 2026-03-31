document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".pc-heart-btn, .pc-add-btn").forEach((button) => {
        button.addEventListener("click", function (event) {
            event.preventDefault();
            event.stopPropagation();
        });
    });
});
