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

const addBtn = document.getElementById('addWishlistBtn');
if (addBtn) {
    addBtn.addEventListener('click', function() {
        const title = document.getElementById('newTitle').value.trim();
        const platform = document.getElementById('newPlatform').value;
        if (!title) return;

        const token = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

        fetch('/Actions/WishlistActions?handler=Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `title=${encodeURIComponent(title)}&platform=${platform}&__RequestVerificationToken=${token}`
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    // Add card to the list visually
                    const list = document.querySelector('.matches-panel') ?? document.querySelector('.wishlist-list');
                    document.getElementById('newTitle').value = '';
                    // Reload to show new item with proper tab counts
                    window.location.reload();
                }
            });
    });

    const addBtn = document.getElementById('addWishlistBtn');
    if (addBtn) {
        addBtn.addEventListener('click', function() {
            const title = document.getElementById('newTitle').value.trim();
            const platform = document.getElementById('newPlatform').value;
            if (!title) return;

            const token = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

            fetch('/Actions/WishlistActions?handler=Add', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `title=${encodeURIComponent(title)}&platform=${platform}&__RequestVerificationToken=${token}`
            })
                .then(r => r.json())
                .then(data => {
                    if (data.success) {
                        document.getElementById('newTitle').value = '';
                        window.location.reload();
                    }
                });
        });
    }
}

const checkStatusBtn = document.getElementById('checkStatusBtn');
if (checkStatusBtn) {
    checkStatusBtn.addEventListener('click', function() {
        const token = document.querySelector('meta[name="csrf-token"]').getAttribute('content');
        checkStatusBtn.disabled = true;
        checkStatusBtn.textContent = 'Checking...';
        checkStatusBtn.style.opacity = '0.7';

        fetch('/Actions/MatchActions?handler=CheckStatus', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `__RequestVerificationToken=${token}`
        })
            .then(r => r.json())
            .then(data => {
                data.matches.forEach(m => {
                    const card = document.querySelector(`[data-listing-id="${m.listingId}"]`)?.closest('.card');
                    if (!card) return;
                    const badge = card.querySelector('.price-badge, .price-badge-sold, .price-badge-removed');
                    if (!badge) return;

                    if (m.status === 'Sold') {
                        badge.className = 'price-badge price-badge-sold';
                        badge.textContent = 'SOLD';
                    } else if (m.status === 'Removed') {
                        badge.className = 'price-badge price-badge-removed';
                        badge.textContent = 'REMOVED';
                    }
                });

                checkStatusBtn.disabled = false;
                checkStatusBtn.textContent = 'Check status';
                checkStatusBtn.style.opacity = '';
            });
    });
}

