(function () {
    const container = document.getElementById('preview-panel');
    if (!container) return;

    const previewUrl = container.dataset.previewUrl;

    const startInput = document.getElementById('StartTime');
    const endInput = document.getElementById('EndTime');
    const roomIdInput = document.querySelector('input[name="RoomId"]');
    const serviceCheckboxes = document.querySelectorAll('input[name="SelectedServiceIds"]');
    const previewHours = document.getElementById('preview-hours');
    const previewRoom = document.getElementById('preview-room');
    const previewServices = document.getElementById('preview-services');
    const previewTotal = document.getElementById('preview-total');
    const previewError = document.getElementById('preview-error');

    const currency = new Intl.NumberFormat('uk-UA', {
        style: 'currency',
        currency: 'UAH',
        currencyDisplay: 'narrowSymbol'
    });

    let debounceTimer;
    let currentController;

    function resetPreview() {
        previewHours.textContent = '—';
        previewRoom.textContent = '—';
        previewServices.textContent = '—';
        previewTotal.textContent = '—';
    }

    async function fetchPreview() {
        if (!startInput.value || !endInput.value) {
            resetPreview();
            previewError.textContent = '';
            return;
        }

        // Cancel previous request to avoid race conditions
        currentController?.abort();
        currentController = new AbortController();

        const payload = {
            roomId: roomIdInput.value,
            startTime: startInput.value,
            endTime: endInput.value,
            serviceIds: Array.from(serviceCheckboxes)
                .filter(cb => cb.checked)
                .map(cb => cb.value)
        };

        try {
            const res = await fetch(previewUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
                signal: currentController.signal
            });

            if (res.ok) {
                const data = await res.json();
                previewHours.textContent = data.billableHours;
                previewRoom.textContent = currency.format(data.roomTotal);
                previewServices.textContent = currency.format(data.servicesTotal);
                previewTotal.textContent = currency.format(data.grandTotal);
                previewError.textContent = '';
            } else if (res.status === 400) {
                resetPreview();
                const data = await res.json().catch(() => null);
                previewError.textContent = data?.errors?.[0] ?? 'Invalid input';
            } else {
                resetPreview();
                previewError.textContent = 'Preview unavailable';
            }
        } catch (err) {
            if (err.name !== 'AbortError') {
                resetPreview();
                previewError.textContent = 'Network error';
            }
        }
    }

    function schedulePreview() {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(fetchPreview, 300);
    }

    // Hidden inputs receive change events from datetime-picker.js when date/hour selects change.
    [startInput, endInput].forEach(el => el.addEventListener('change', schedulePreview));
    serviceCheckboxes.forEach(cb => cb.addEventListener('change', schedulePreview));

    // Initial calc if form pre-filled (e.g. after server-side validation error)
    fetchPreview();
})();
