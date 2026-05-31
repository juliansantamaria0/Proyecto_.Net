import { API } from '../api.js';
import { Auth } from '../auth.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';
import { ESTADO_ORDEN } from '../config.js';

export async function renderDashboardCliente() {
    const user = Auth.getUser();
    document.getElementById('user-name').textContent = user?.nombre || '';
    document.getElementById('user-role').textContent = 'Cliente';

    UI.setLoading(true);
    try {
        const clienteId = Auth.getClienteId();
        const [vehiculos, pendientes, proceso, completadas, facturas] = await Promise.all([
            API.getVehiculos(1, 1, clienteId),
            API.getOrdenes({ page: 1, pageSize: 1, estado: 0, clienteId }),
            API.getOrdenes({ page: 1, pageSize: 1, estado: 1, clienteId }),
            API.getOrdenes({ page: 1, pageSize: 1, estado: 2, clienteId }),
            API.getFacturas(1, 5, { clienteId }),
        ]);

        document.getElementById('cli-metric-vehiculos').textContent = vehiculos.totalCount || 0;
        const activas = (pendientes.totalCount || 0) + (proceso.totalCount || 0);
        document.getElementById('cli-metric-ordenes').textContent = activas;
        document.getElementById('cli-metric-completadas').textContent = completadas.totalCount || 0;

        const totalFacturado = (facturas.data || []).reduce((s, f) => s + f.montoTotal, 0);
        document.getElementById('cli-metric-facturas').textContent = Utils.formatCurrency(totalFacturado);

        const tbody = document.getElementById('cli-ordenes-recientes');
        const { data: recientes } = await API.getOrdenes({ page: 1, pageSize: 5, clienteId });
        if (!recientes?.length) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Sin órdenes registradas</td></tr>';
        } else {
            tbody.innerHTML = recientes.map(o => {
                const est = ESTADO_ORDEN[o.estado] || { label: o.estado, class: '' };
                return `<tr>
                    <td>#${o.id}</td>
                    <td>${Utils.escapeHtml(o.vehiculoDescripcion)}</td>
                    <td><span class="badge ${est.class}">${est.label}</span></td>
                    <td>${Utils.formatDate(o.fechaEstimadaEntrega)}</td>
                </tr>`;
            }).join('');
        }
    } catch {
        ['cli-metric-vehiculos', 'cli-metric-ordenes', 'cli-metric-completadas'].forEach(id => {
            document.getElementById(id).textContent = '—';
        });
    } finally {
        UI.setLoading(false);
    }
}
