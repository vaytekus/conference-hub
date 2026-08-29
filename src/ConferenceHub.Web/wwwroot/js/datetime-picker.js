(function () {
    const pickersByForm = new Map();

    document.querySelectorAll('[data-datetime-picker]').forEach(container => {
        const dateEl = container.querySelector('input[type="date"]');
        const hourEl = container.querySelector('select');
        const hiddenEl = container.querySelector('input[type="hidden"]');

        if (!dateEl || !hourEl || !hiddenEl) return;

        if (hiddenEl.value) {
            const [datePart, timePart] = hiddenEl.value.split('T');
            if (datePart && timePart) {
                dateEl.value = datePart;
                hourEl.value = parseInt(timePart.substring(0, 2), 10).toString();
            }
        }

        const errorEl = document.createElement('span');
        errorEl.className = 'text-danger small d-none';
        container.after(errorEl);

        function compose() {
            if (dateEl.value && hourEl.value) {
                const hh = String(hourEl.value).padStart(2, '0');
                hiddenEl.value = `${dateEl.value}T${hh}:00`;
            } else {
                hiddenEl.value = '';
            }
            errorEl.classList.add('d-none');
            errorEl.textContent = '';
            hiddenEl.dispatchEvent(new Event('change', { bubbles: true }));
        }

        dateEl.addEventListener('change', compose);
        hourEl.addEventListener('change', compose);

        const form = container.closest('form');
        if (form) {
            if (!pickersByForm.has(form)) pickersByForm.set(form, []);
            pickersByForm.get(form).push({ dateEl, hourEl, errorEl });
        }
    });

    pickersByForm.forEach((pickers, form) => {
        form.addEventListener('submit', e => {
            let hasError = false;
            pickers.forEach(({ dateEl, hourEl, errorEl }) => {
                const hasDate = !!dateEl.value;
                const hasHour = !!hourEl.value;
                if (hasDate !== hasHour) {
                    errorEl.textContent = 'Please select both date and hour.';
                    errorEl.classList.remove('d-none');
                    hasError = true;
                } else {
                    errorEl.classList.add('d-none');
                    errorEl.textContent = '';
                }
            });
            if (hasError) e.preventDefault();
        });
    });
})();
