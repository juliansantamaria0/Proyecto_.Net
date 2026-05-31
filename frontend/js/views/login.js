import { API } from '../api.js';
import { Auth } from '../auth.js';
import { UI } from '../ui.js';
import { navigateTo } from '../router.js';
import { Utils } from '../utils.js';

export function renderLogin() {
    const form = document.getElementById('login-form');
    if (!form || form.dataset.bound) return;
    form.dataset.bound = 'true';

    document.getElementById('link-to-register')?.addEventListener('click', (e) => {
        e.preventDefault();
        navigateTo('register');
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const correo = form.correo.value.trim();
        const password = form.password.value;

        document.getElementById('login-correo-error').textContent = '';
        document.getElementById('login-password-error').textContent = '';

        let valid = true;
        if (!correo) {
            document.getElementById('login-correo-error').textContent = 'El correo es obligatorio';
            valid = false;
        } else if (!Utils.isValidEmail(correo)) {
            document.getElementById('login-correo-error').textContent = 'Correo inválido';
            valid = false;
        }
        if (!password) {
            document.getElementById('login-password-error').textContent = 'La contraseña es obligatoria';
            valid = false;
        }
        if (!valid) return;

        const btn = form.querySelector('button[type="submit"]');
        btn.disabled = true;
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Ingresando...';

        try {
            const remember = document.getElementById('login-remember')?.checked ?? true;
            const { data } = await API.login(correo, password);
            const roleMap = {
                0: 'Admin', 1: 'Mecanico', 2: 'Recepcionista', 3: 'Cliente',
                Admin: 'Admin', Mecanico: 'Mecanico', Recepcionista: 'Recepcionista', Cliente: 'Cliente',
            };
            const rol = roleMap[data.usuario.rol] ?? String(data.usuario.rol);
            Auth.setSession(data.token, {
                id: data.usuario.id,
                nombre: data.usuario.nombre,
                correo: data.usuario.correo,
                rol,
                clienteId: data.usuario.clienteId ?? data.usuario.ClienteId,
            }, remember);
            UI.toast(`Bienvenido, ${data.usuario.nombre}`, 'success');
            if (rol === 'Mecanico') navigateTo('panel-mecanico');
            else if (rol === 'Cliente') navigateTo('dashboard-cliente');
            else navigateTo('dashboard');
        } catch {
            // errores manejados en API
        } finally {
            btn.disabled = false;
            btn.innerHTML = '<i class="fa-solid fa-right-to-bracket"></i> Iniciar sesión';
        }
    });
}
