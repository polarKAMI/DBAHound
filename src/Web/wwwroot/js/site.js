document.addEventListener('click', function (e) {
    const btn = e.target.closest('[data-action]');
    if (!btn) return;

    const action = btn.dataset.action;
    const listingId = btn.dataset.listingId;
    const token = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

    if (action === 'favourite') {
        fetch('/Actions/MatchActions?handler=Favourite', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `listingId=${listingId}&__RequestVerificationToken=${token}`
        })
            .then(r => r.json())
            .then(data => {
                btn.classList.toggle('active', data.isFavourite);
                const countEl = document.querySelector('.favourite-count');
                if (countEl) {
                    let count = parseInt(countEl.textContent);
                    countEl.textContent = data.isFavourite ? count + 1 : count - 1;
                }
            });
    }

    if (action === 'dismiss') {
        fetch('/Actions/MatchActions?handler=Dismiss', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `listingId=${listingId}&__RequestVerificationToken=${token}`
        })
            .then(() => {
                btn.closest('.card').remove();
                const totalEl = document.querySelector('.tab-count');
                if (totalEl) {
                    let count = parseInt(totalEl.textContent);
                    totalEl.textContent = count - 1;
                }
            });
    }

    if (action === 'wishlist-remove') {
        const title = btn.dataset.wishlistTitle;
        fetch('/Actions/WishlistActions?handler=Remove', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `title=${encodeURIComponent(title)}&__RequestVerificationToken=${token}`
        })
            .then(() => {
                btn.closest('.card').remove();
            });
    }
    
    if (action === 'restore') {
        fetch('/Actions/MatchActions?handler=Restore', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `listingId=${listingId}&__RequestVerificationToken=${token}`
        })
            .then(() => {
                btn.closest('.card').remove();
                const totalEl = document.querySelector('.tab-count');
                if (totalEl) {
                    let count = parseInt(totalEl.textContent);
                    totalEl.textContent = count + 1;
                }
            });
    }
});

// Scrape button loading state
const scrapeForm = document.getElementById('scrapeForm');
if (scrapeForm) {
    scrapeForm.addEventListener('submit', function () {
        const btn = document.getElementById('scrapeBtn');
        btn.disabled = true;
        btn.textContent = 'Scraping...';
        btn.style.opacity = '0.7';
    });
}

// Scroll fade indicator
const panel = document.getElementById('matchesPanel');
if (panel) {
    panel.addEventListener('scroll', () => {
        const atEnd = panel.scrollHeight - panel.scrollTop <= panel.clientHeight + 8;
        panel.closest('.matches-panel-wrap').classList.toggle('scrolled-to-end', atEnd);
    });
}