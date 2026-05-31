import { API } from '../api.js';
import { Auth } from '../auth.js';
import { CONFIG } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1, search: '', selectedId: null };

export async function renderClientes() {
    if (!document.getElementById('clientes-table')?.dataset.bound) {
        bindClientesEvents();
        document.getElementById('clientes-table').dataset.bound = 'true';
    }
    await loadClientes();
}

function bindClientesEvents() {
    document.getElementById('clientes-search')?.addEventListener('input',
        Utils.debounce((e) => { state.search = e.target.value; state.page = 1; loadClientes(); })
    );

    document.getElementById('btn-nuevo-cliente')?.addEventListener('click', openRegistroModal);

    document.getElementById('clientes-table')?.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-action]');
        if (!btn) return;
        const id = parseInt(btn.dataset.id, 10);
        if (btn.dataset.action === 'view') await showClienteDetail(id);
        if (btn.dataset.action === 'edit') openEditModal(id);
        if (btn.dataset.action === 'delete') await deleteCliente(id);
    });
}

async function loadClientes() {
    UI.setLoading(true);
    try {
        const term = state.search.trim().toLowerCase();
        const useApiNombre = term && !term.includes('@');
        const { data, totalCount } = await API.getClientes(
            state.page,
            term.includes('@') ? 100 : CONFIG.DEFAULT_PAGE_SIZE,
            useApiNombre ? state.search : ''
        );
        let items = data || [];
        if (term) {
            items = items.filter(c =>
                c.nombre?.toLowerCase().includes(term) ||
                c.correo?.toLowerCase().includes(term) ||
                c.telefono?.includes(term)
            );
        }
        const displayTotal = term.includes('@') ? items.length : totalCount;
        const tbody = document.getElementById('clientes-body');
        if (!items.length) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No hay clientes registrados</td></tr>';
        } else {
            tbody.innerHTML = items.map(c => `
                <tr class="${state.selectedId === c.id ? 'row-selected' : ''}">
                    <td>${c.id}</td>
                    <td><strong>${Utils.escapeHtml(c.nombre)}</strong></td>
                    <td>${Utils.escapeHtml(c.correo)}</td>
                    <td>${Utils.escapeHtml(c.telefono)}</td>
                    <td><span class="badge badge-info">${c.cantidadVehiculos} vehículo(s)</span></td>
                    <td class="actions">
                        <button class="btn-icon" data-action="view" data-id="${c.id}" title="Ver detalle"><i class="fa-solid fa-eye"></i></button>
                        <button class="btn-icon" data-action="edit" data-id="${c.id}" title="Editar"><i class="fa-solid fa-pen"></i></button>
                        ${Auth.isAdmin() ? `<button class="btn-icon btn-icon-danger" data-action="delete" data-id="${c.id}" title="Eliminar"><i class="fa-solid fa-trash"></i></button>` : ''}
                    </td>
                </tr>
            `).join('');
        }
        UI.renderPagination('clientes-pagination', {
            page: state.page,
            pageSize: CONFIG.DEFAULT_PAGE_SIZE,
            totalCount: displayTotal,
        }, (p) => { state.page = p; loadClientes(); });
    } finally {
        UI.setLoading(false);
    }
}

async function showClienteDetail(id) {
    state.selectedId = id;
    UI.setLoading(true);
    try {
        const [{ data: cliente }, { data: vehiculos }] = await Promise.all([
            API.getCliente(id),
            API.getVehiculos(1, 50, id),
        ]);

        const panel = document.getElementById('cliente-detail-panel');
        panel.classList.remove('hidden');
        document.getElementById('cliente-detail-title').textContent = cliente.nombre;
        document.getElementById('cliente-detail-info').innerHTML = `
            <p><i class="fa-solid fa-envelope"></i> ${Utils.escapeHtml(cliente.correo)}</p>
            <p><i class="fa-solid fa-phone"></i> ${Utils.escapeHtml(cliente.telefono)}</p>
        `;

        const vBody = document.getElementById('cliente-vehiculos-body');
        if (!vehiculos?.length) {
            vBody.innerHTML = '<tr><td colspan="5" class="text-muted">Sin vehículos registrados</td></tr>';
        } else {
            vBody.innerHTML = vehiculos.map(v => `
                <tr>
                    <td>${Utils.escapeHtml(v.marca)} ${Utils.escapeHtml(v.modelo)}</td>
                    <td>${v.anio}</td>
                    <td><code>${Utils.escapeHtml(v.vin)}</code></td>
                    <td>${v.kilometraje.toLocaleString()} km</td>
                </tr>
            `).join('');
        }
        loadClientes();
    } finally {
        UI.setLoading(false);
    }
}

function openRegistroModal() {
    UI.openModal('Registrar Cliente con Vehículo(s)', `
        <form id="form-registro-cliente">
            <h4 class="form-section-title">Datos del cliente</h4>
            <div class="form-grid">
                <div class="form-group"><label>Nombre *</label><input name="nombre" required></div>
                <div class="form-group"><label>Correo *</label><input name="correo" type="email" required></div>
                <div class="form-group"><label>Teléfono *</label><input name="telefono" required></div>
            </div>
            <h4 class="form-section-title">Vehículo(s)</h4>
            <div id="vehiculos-container">
                ${vehiculoFieldsHtml(0)}
            </div>
            <button type="button" class="btn btn-sm btn-outline" id="btn-add-vehiculo"><i class="fa-solid fa-plus"></i> Agregar vehículo</button>
        </form>
    `, `<button class="btn btn-primary" id="btn-save-registro"><i class="fa-solid fa-save"></i> Registrar</button>`);

    let vehiculoCount = 1;
    document.getElementById('btn-add-vehiculo').addEventListener('click', () => {
        document.getElementById('vehiculos-container').insertAdjacentHTML('beforeend', vehiculoFieldsHtml(vehiculoCount++));
    });

    document.getElementById('btn-save-registro').addEventListener('click', async () => {
        const form = document.getElementById('form-registro-cliente');
        if (!form.checkValidity()) { form.reportValidity(); return; }

        const vehiculos = [];
        form.querySelectorAll('.vehiculo-block').forEach(block => {
            vehiculos.push({
                marca: block.querySelector('[name="marca"]').value,
                modelo: block.querySelector('[name="modelo"]').value,
                anio: parseInt(block.querySelector('[name="anio"]').value, 10),
                vin: block.querySelector('[name="vin"]').value,
                kilometraje: parseInt(block.querySelector('[name="kilometraje"]').value, 10),
            });
        });

        try {
            await API.registrarClienteConVehiculos({
                cliente: {
                    nombre: form.nombre.value,
                    correo: form.correo.value,
                    telefono: form.telefono.value,
                },
                vehiculos,
            });
            UI.toast('Cliente registrado correctamente', 'success');
            UI.closeModal();
            loadClientes();
        } catch { /* handled */ }
    });
}

function vehiculoFieldsHtml(index) {
    return `
        <div class="vehiculo-block form-grid" data-index="${index}">
            <div class="form-group"><label>Marca *</label><input name="marca" required></div>
            <div class="form-group"><label>Modelo *</label><input name="modelo" required></div>
            <div class="form-group"><label>Año *</label><input name="anio" type="number" min="1980" max="2030" required></div>
            <div class="form-group"><label>VIN *</label><input name="vin" maxlength="17" required></div>
            <div class="form-group"><label>Kilometraje *</label><input name="kilometraje" type="number" min="0" required></div>
        </div>
    `;
}

async function openEditModal(id) {
    const { data: c } = await API.getCliente(id);
    UI.openModal('Editar Cliente', `
        <form id="form-edit-cliente">
            <div class="form-grid">
                <div class="form-group"><label>Nombre</label><input name="nombre" value="${Utils.escapeHtml(c.nombre)}" required></div>
                <div class="form-group"><label>Correo</label><input name="correo" type="email" value="${Utils.escapeHtml(c.correo)}" required></div>
                <div class="form-group"><label>Teléfono</label><input name="telefono" value="${Utils.escapeHtml(c.telefono)}" required></div>
            </div>
        </form>
    `, `<button class="btn btn-primary" id="btn-save-edit">Guardar</button>`);

    document.getElementById('btn-save-edit').addEventListener('click', async () => {
        const form = document.getElementById('form-edit-cliente');
        try {
            await API.updateCliente(id, {
                nombre: form.nombre.value,
                correo: form.correo.value,
                telefono: form.telefono.value,
            });
            UI.toast('Cliente actualizado', 'success');
            UI.closeModal();
            loadClientes();
        } catch { /* handled */ }
    });
}

async function deleteCliente(id) {
    UI.openModal('Confirmar eliminación', `
        <p>¿Está seguro de eliminar este cliente?</p>
        <p class="text-muted text-sm">No se puede eliminar si tiene órdenes de servicio activas.</p>
    `, `
        <button class="btn btn-danger" id="btn-confirm-delete">Eliminar</button>
        <button class="btn btn-outline" id="btn-cancel-delete">Cancelar</button>
    `);
    document.getElementById('btn-cancel-delete')?.addEventListener('click', () => UI.closeModal());
    document.getElementById('btn-confirm-delete').addEventListener('click', async () => {
        try {
            await API.deleteCliente(id);
            UI.toast('Cliente eliminado', 'success');
            UI.closeModal();
            document.getElementById('cliente-detail-panel')?.classList.add('hidden');
            loadClientes();
        } catch (err) {
            if (err.message === 'Forbidden') return;
            const msg = err.message?.toLowerCase() ?? '';
            if (msg.includes('órdenes') || msg.includes('activas') || msg.includes('factura') || msg.includes('restrict')) {
                UI.toast('No se puede eliminar: el cliente tiene órdenes o facturas asociadas.', 'warning', 6000);
            }
        }
    });
}
