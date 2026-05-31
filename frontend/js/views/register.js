import { API } from '../api.js';
import { Auth } from '../auth.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';
import { navigateTo } from '../router.js';

export function renderRegister() {
    const form = document.getElementById('register-form');
    if (!form || form.dataset.bound) return;
    form.dataset.bound = 'true';

    document.getElementById('link-to-login')?.addEventListener('click', (e) => {
        e.preventDefault();
        navigateTo('login');
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const nombre = form.nombre.value.trim();
        const telefono = form.telefono.value.trim();
        const correo = form.correo.value.trim();
        const password = form.password.value;
        const confirm = form.passwordConfirm.value;

        clearErrors();
        let valid = true;
        if (!nombre) { showError('reg-nombre-error', 'El nombre es obligatorio'); valid = false; }
        if (!telefono) { showError('reg-telefono-error', 'El teléfono es obligatorio'); valid = false; }
        if (!correo || !Utils.isValidEmail(correo)) { showError('reg-correo-error', 'Correo inválido'); valid = false; }
        if (password.length < 6) { showError('reg-password-error', 'Mínimo 6 caracteres'); valid = false; }
        if (password !== confirm) { showError('reg-confirm-error', 'Las contraseñas no coinciden'); valid = false; }
        if (!valid) return;

        const btn = form.querySelector('button[type="submit"]');
        btn.disabled = true;
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Creando cuenta...';
        try {
            const { data } = await API.register({ nombre, telefono, correo, password });
            const u = data.usuario;
            Auth.setSession(data.token, {
                id: u.id,
                nombre: u.nombre,
                correo: u.correo,
                rol: 'Cliente',
                clienteId: u.clienteId ?? u.ClienteId,
            });
            UI.toast('¡Cuenta creada! Bienvenido.', 'success');
            navigateTo('dashboard-cliente');
        } catch { /* api handles */ }
        finally {
            btn.disabled = false;
            btn.innerHTML = '<i class="fa-solid fa-user-check"></i> Crear cuenta';
        }
    });
}

function showError(id, msg) {
    const el = document.getElementById(id);
    if (el) el.textContent = msg;
}

function clearErrors() {
    ['reg-nombre-error', 'reg-telefono-error', 'reg-correo-error', 'reg-password-error', 'reg-confirm-error']
        .forEach(id => { const el = document.getElementById(id); if (el) el.textContent = ''; });
}
