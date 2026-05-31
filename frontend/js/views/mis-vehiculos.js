import { API } from '../api.js';
import { Auth } from '../auth.js';
import { CONFIG } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1 };

export async function renderMisVehiculos() {
    if (!document.getElementById('mis-vehiculos-table')?.dataset.bound) {
        bindEvents();
        document.getElementById('mis-vehiculos-table').dataset.bound = 'true';
    }
    await loadVehiculos();
}

function bindEvents() {
    document.getElementById('btn-agregar-vehiculo')?.addEventListener('click', openAddVehiculoModal);
}

async function loadVehiculos() {
    UI.setLoading(true);
    try {
        const { data, totalCount } = await API.getVehiculos(state.page, CONFIG.DEFAULT_PAGE_SIZE, Auth.getClienteId());
        const tbody = document.getElementById('mis-vehiculos-body');
        if (!data?.length) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No tiene vehículos registrados</td></tr>';
        } else {
            tbody.innerHTML = data.map(v => `
                <tr>
                    <td><strong>${Utils.escapeHtml(v.marca)} ${Utils.escapeHtml(v.modelo)}</strong></td>
                    <td>${v.anio}</td>
                    <td><code>${Utils.escapeHtml(v.vin)}</code></td>
                    <td>${v.kilometraje.toLocaleString('es-MX')} km</td>
                </tr>
            `).join('');
        }
        UI.renderPagination('mis-vehiculos-pagination', {
            page: state.page, pageSize: CONFIG.DEFAULT_PAGE_SIZE, totalCount,
        }, p => { state.page = p; loadVehiculos(); });
    } finally {
        UI.setLoading(false);
    }
}

function openAddVehiculoModal() {
    UI.openModal('Agregar vehículo', `
        <form id="form-nuevo-vehiculo-cliente">
            <div class="form-grid">
                <div class="form-group"><label>Marca *</label><input name="marca" required></div>
                <div class="form-group"><label>Modelo *</label><input name="modelo" required></div>
                <div class="form-group"><label>Año *</label><input name="anio" type="number" min="1980" max="2030" required></div>
                <div class="form-group"><label>VIN *</label><input name="vin" maxlength="17" required></div>
                <div class="form-group"><label>Kilometraje *</label><input name="kilometraje" type="number" min="0" required></div>
            </div>
        </form>
    `, `<button class="btn btn-primary" id="btn-save-vehiculo-cli" data-rate-sensitive>Registrar vehículo</button>`);

    document.getElementById('btn-save-vehiculo-cli').addEventListener('click', async (e) => {
        const form = document.getElementById('form-nuevo-vehiculo-cliente');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        try {
            await API.withTrigger(e.currentTarget, () => API.createVehiculo({
                clienteId: Auth.getClienteId(),
                marca: form.marca.value.trim(),
                modelo: form.modelo.value.trim(),
                anio: parseInt(form.anio.value, 10),
                vin: form.vin.value.trim().toUpperCase(),
                kilometraje: parseInt(form.kilometraje.value, 10),
            }));
            UI.toast('Vehículo registrado', 'success');
            UI.closeModal();
            loadVehiculos();
        } catch { /* handled */ }
    });
}
