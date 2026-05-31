import { API } from '../api.js';
import { CONFIG, ROL_LABELS } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1 };

export async function renderUsuarios() {
    if (!document.getElementById('usuarios-table')?.dataset.bound) {
        bindEvents();
        document.getElementById('usuarios-table').dataset.bound = 'true';
    }
    await loadUsuarios();
}

function bindEvents() {
    document.getElementById('btn-nuevo-usuario')?.addEventListener('click', () => openUsuarioModal());
    document.getElementById('usuarios-table')?.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-action]');
        if (!btn) return;
        const id = parseInt(btn.dataset.id, 10);
        if (btn.dataset.action === 'edit') openUsuarioModal(id);
        if (btn.dataset.action === 'delete') deleteUsuario(id);
    });
}

async function loadUsuarios() {
    UI.setLoading(true);
    try {
        const { data, totalCount } = await API.getUsuarios(state.page, CONFIG.DEFAULT_PAGE_SIZE);
        const tbody = document.getElementById('usuarios-body');
        if (!data?.length) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Sin usuarios</td></tr>';
        } else {
            tbody.innerHTML = data.map(u => `
                <tr class="${!u.activo ? 'row-inactive' : ''}">
                    <td>${u.id}</td>
                    <td>${Utils.escapeHtml(u.nombre)}</td>
                    <td>${Utils.escapeHtml(u.correo)}</td>
                    <td><span class="badge badge-info">${ROL_LABELS[u.rol] || Utils.normalizeRol(u.rol)}</span></td>
                    <td><span class="badge ${u.activo ? 'badge-done' : 'badge-cancel'}">${u.activo ? 'Activo' : 'Inactivo'}</span></td>
                    <td class="actions">
                        <button class="btn-icon" data-action="edit" data-id="${u.id}"><i class="fa-solid fa-pen"></i></button>
                        <button class="btn-icon btn-icon-danger" data-action="delete" data-id="${u.id}"><i class="fa-solid fa-user-slash"></i></button>
                    </td>
                </tr>
            `).join('');
        }
        UI.renderPagination('usuarios-pagination', { page: state.page, pageSize: CONFIG.DEFAULT_PAGE_SIZE, totalCount }, p => {
            state.page = p; loadUsuarios();
        });
    } finally {
        UI.setLoading(false);
    }
}

async function openUsuarioModal(id = null) {
    let u = { nombre: '', correo: '', rol: 2, activo: true };
    if (id) {
        const res = await API.getUsuarios(1, 100);
        u = (res.data || []).find(x => x.id === id) || u;
    }
    const isEdit = !!id;
    UI.openModal(isEdit ? 'Editar usuario' : 'Nuevo usuario', `
        <form id="form-usuario">
            <div class="form-grid">
                <div class="form-group"><label>Nombre *</label><input name="nombre" value="${Utils.escapeHtml(u.nombre)}" required></div>
                <div class="form-group"><label>Correo *</label><input name="correo" type="email" value="${Utils.escapeHtml(u.correo)}" required ${isEdit ? 'readonly' : ''}></div>
                ${!isEdit ? `<div class="form-group"><label>Contraseña *</label><input name="password" type="password" required minlength="6"></div>` : ''}
                <div class="form-group"><label>Rol *</label>
                    <select name="rol" required>
                        <option value="0" ${u.rol === 0 || u.rol === 'Admin' ? 'selected' : ''}>Administrador</option>
                        <option value="1" ${u.rol === 1 || u.rol === 'Mecanico' ? 'selected' : ''}>Mecánico</option>
                        <option value="2" ${u.rol === 2 || u.rol === 'Recepcionista' ? 'selected' : ''}>Recepcionista</option>
                    </select>
                </div>
                ${isEdit ? `<div class="form-group"><label><input type="checkbox" name="activo" ${u.activo ? 'checked' : ''}> Usuario activo</label></div>` : ''}
            </div>
        </form>
    `, `<button class="btn btn-primary" id="btn-save-usuario" data-rate-sensitive>Guardar</button>`);

    document.getElementById('btn-save-usuario').addEventListener('click', async (e) => {
        const form = document.getElementById('form-usuario');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        try {
            await API.withTrigger(e.currentTarget, async () => {
                if (isEdit) {
                    await API.updateUsuario(id, {
                        nombre: form.nombre.value,
                        correo: form.correo.value,
                        rol: parseInt(form.rol.value, 10),
                        activo: form.activo.checked,
                    });
                } else {
                    await API.createUsuario({
                        nombre: form.nombre.value,
                        correo: form.correo.value,
                        password: form.password.value,
                        rol: parseInt(form.rol.value, 10),
                    });
                }
            });
            UI.toast('Usuario guardado', 'success');
            UI.closeModal();
            loadUsuarios();
        } catch { /* handled */ }
    });
}

async function deleteUsuario(id) {
    UI.openModal('Desactivar usuario', '<p>¿Desactivar este usuario? No podrá iniciar sesión.</p>',
        `<button class="btn btn-danger" id="btn-confirm-del-user">Confirmar</button>`);
    document.getElementById('btn-confirm-del-user').addEventListener('click', async () => {
        try {
            await API.deleteUsuario(id);
            UI.toast('Usuario desactivado', 'success');
            UI.closeModal();
            loadUsuarios();
        } catch { /* handled */ }
    });
}
