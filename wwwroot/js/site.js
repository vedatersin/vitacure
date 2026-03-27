
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.cat-icon-card, .nav-link').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href && href.startsWith('#')) {
                e.preventDefault();
                const targetId = href.substring(1);
                const targetSection = document.getElementById(targetId);
                
                if (targetSection) {
                    const scrollContainer = document.querySelector('.scroll-container');
                    if (scrollContainer) {
                        scrollContainer.scrollTo({
                            top: targetSection.offsetTop,
                            behavior: 'smooth'
                        });
                    } else {
                        targetSection.scrollIntoView({ behavior: 'smooth' });
                    }
                }
            }
        });
    });
});
