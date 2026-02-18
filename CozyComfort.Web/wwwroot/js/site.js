// CozyComfort global UI enhancements
// - Soft page fade-in
// - Navbar shrink on scroll
// - Small hover/tap helpers

document.addEventListener("DOMContentLoaded", function () {
    // Page fade-in
    document.body.classList.add("page-ready");

    // Navbar shrink on scroll
    const navbar = document.querySelector(".navbar");
    if (navbar) {
        const onScroll = () => {
            if (window.scrollY > 10) {
                navbar.classList.add("navbar-scrolled");
            } else {
                navbar.classList.remove("navbar-scrolled");
            }
        };
        window.addEventListener("scroll", onScroll, { passive: true });
        onScroll();
    }

    // Add a subtle press feedback for all primary buttons
    document.querySelectorAll("button, .btn, .cta-btn").forEach(el => {
        el.addEventListener("mousedown", () => el.classList.add("is-pressed"));
        el.addEventListener("mouseup", () => el.classList.remove("is-pressed"));
        el.addEventListener("mouseleave", () => el.classList.remove("is-pressed"));
    });
});

