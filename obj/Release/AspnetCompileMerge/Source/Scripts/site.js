function showSpinner(message = "Please wait...") {
    const overlay = document.getElementById("globalSpinnerOverlay");
    const text = document.getElementById("progressText");

    if (text) text.textContent = message;

    if (overlay) {
        overlay.classList.remove("d-none");     // ✅ remove display:none
        overlay.classList.add("show");          // ✅ trigger CSS display:flex
    }
}

// Attach spinner to forms that don't have the .no-spinner class
document.addEventListener("DOMContentLoaded", function () {
    // Spinner for forms
    document.querySelectorAll('form:not(.no-spinner)').forEach(form => {
        form.addEventListener('submit', function () {
            showSpinner("Processing...");
        });
    });
});

// Spinner for dashboard cards
document.querySelectorAll('.dashboard-card').forEach(card => {
    card.addEventListener('click', function () {
        if (card.classList.contains('no-spinner')) return;
        showSpinner("Loading...");
    });
});

// Spinner for sidebar links (optional)
document.querySelectorAll('#sidebarMenu a[href]').forEach(link => {
    link.addEventListener('click', function () {
        if (this.href !== window.location.href) {
            showSpinner("Navigating...");
        }
    });
});

function hideSpinner() {
    const overlay = document.getElementById("globalSpinnerOverlay");
    if (overlay) {
        overlay.classList.remove("show");
        overlay.classList.add("d-none");
    }
}

function showToast(message, type = "success") {
    const toastEl = document.getElementById("toastMessage");
    const toastBody = document.getElementById("toastBody");
    const toastIcon = document.getElementById("toastIcon");
    const toastText = document.getElementById("toastText");

    // Defensive fallback
    type = type || "success";

    toastEl.classList.remove("bg-success", "bg-danger", "bg-info");
    toastEl.classList.add("bg-" + type);

    // Choose icon
    let iconClass = "fas fa-info-circle";
    if (type === "success") iconClass = "fas fa-check-circle";
    else if (type === "danger") iconClass = "fas fa-exclamation-circle";
    else if (type === "info") iconClass = "fas fa-info-circle";

    if (toastIcon) toastIcon.className = iconClass;
    if (toastText) toastText.innerHTML = message;

    const toast = bootstrap.Toast.getOrCreateInstance(toastEl);
    toast.show();
}
