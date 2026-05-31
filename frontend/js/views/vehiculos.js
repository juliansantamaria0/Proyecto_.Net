import { API } from '../api.js';
import { CONFIG } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1, vin: '', marca: '' };

export async function renderVehiculos() {
    if (!document.getElementById('vehiculos-table')?.dataset.bound) {
        bindEvents();
        document.getElementById('vehiculos-table').dataset.bound = 'true';
    }
    await loadVehiculos();
}

function bindEvents() {
    document.getElementById('btn-buscar-vehiculos')?.addEventListener('click', () => {
        state.vin = document.getElementById('vehiculos-vin').value.trim();
        state.marca = document.getElementById('vehiculos-marca').value.trim().toLowerCase();
        state.page = 1;
        loadVehiculos();
    });
    document.getElementById('vehiculos-vin')?.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') document.getElementById('btn-buscar-vehiculos').click();
    });
}

async function loadVehiculos() {
    UI.setLoading(true);
    try {
        const useLocalMarcaFilter = !!state.marca;
        const fetchSize = useLocalMarcaFilter ? 500 : 200;
        const { data } = await API.getVehiculos(1, fetchSize, null, state.vin);
        let items = data || [];
        if (state.marca) {
            items = items.filter(v =>
                v.marca?.toLowerCase().includes(state.marca) ||
                v.modelo?.toLowerCase().includes(state.marca)
            );
        }

        const pageSize = CONFIG.DEFAULT_PAGE_SIZE;
        const start = (state.page - 1) * pageSize;
        const paged = items.slice(start, start + pageSize);
        const filteredTotal = items.length;

        const tbody = document.getElementById('vehiculos-body');
        if (!paged.length) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No se encontraron vehículos</td></tr>';
        } else {
            tbody.innerHTML = paged.map(v => `
                <tr>
                    <td><code>${Utils.escapeHtml(v.vin)}</code></td>
                    <td>${Utils.escapeHtml(v.marca)} ${Utils.escapeHtml(v.modelo)}</td>
                    <td>${v.anio}</td>
                    <td>${v.kilometraje.toLocaleString()} km</td>
                    <td>${Utils.escapeHtml(v.clienteNombre)}</td>
                    <td>#${v.clienteId}</td>
                </tr>
            `).join('');
        }

        UI.renderPagination('vehiculos-pagination', { page: state.page, pageSize, totalCount: filteredTotal }, p => {
            state.page = p; loadVehiculos();
        });
    } finally {
        UI.setLoading(false);
    }
}
