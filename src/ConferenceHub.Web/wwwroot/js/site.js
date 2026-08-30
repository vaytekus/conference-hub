document.querySelectorAll('[data-toggle-password]').forEach(btn => {
    btn.addEventListener('click', () => {
        const input = btn.closest('.input-group').querySelector('input');
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        btn.innerHTML = show ? '<i class="bi bi-eye-slash"></i>' : '<i class="bi bi-eye"></i>';
    });
});
