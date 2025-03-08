document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".copy-btn").forEach(button => {
        button.addEventListener("click", function () {
            const textToCopy = this.getAttribute("data-copy-text");
            navigator.clipboard.writeText(textToCopy).then(() => {
                this.innerHTML = '<i class="bi bi-check-lg text-success"></i>'; // Change icon to checkmark
                setTimeout(() => {
                    this.innerHTML = '<i class="bi bi-clipboard"></i>'; // Revert back after 2s
                }, 2000);
            }).catch(err => console.error("Copy failed:", err));
        });
    });
});