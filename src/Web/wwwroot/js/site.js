document.addEventListener('click', function (e) {
    const btn = e.target.closest('[data-action]');
    if (!btn) return;

    const listingId = btn.dataset.listingId;
    const action = btn.dataset.action;
    const token = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

    if (action === 'favourite') {
        fetch('/MatchAction?handler=Favourite', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `listingId=${listingId}&__RequestVerificationToken=${token}`
        })
            .then(r => r.json())
            .then(data => {
                btn.classList.toggle('active', data.isFavourite);
            });
    }

    if (action === 'dismiss') {
        fetch('/MatchAction?handler=Dismiss', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `listingId=${listingId}&__RequestVerificationToken=${token}`
        })
            .then(() => {
                btn.closest('.card').remove();
            });
    }
});