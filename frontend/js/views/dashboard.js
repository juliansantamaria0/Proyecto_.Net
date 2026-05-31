import { API } from '../api.js';
import { Auth } from '../auth.js';
import { CONFIG } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

export async function renderDashboard() {
    const user = Auth.getUser();
    document.getElementById('user-name').textContent = user?.nombre || '';
    document.getElementById('user-role').textContent = user?.rol || '';

    UI.setLoading(true);
    try {
        const [ordenesPend, ordenesProc, repuestos, facturas] = await Promise.all([
            API.getOrdenes({ page: 1, pageSize: 1, estado: 0 }),
            API.getOrdenes({ page: 1, pageSize: 1, estado: 1 }),
            API.getRepuestos(1, 100),
            API.getFacturas(1, 100, { fechaDesde: Utils.todayISO() }),
        ]);

        const activas = (ordenesPend.totalCount || 0) + (ordenesProc.totalCount || 0);
        document.getElementById('metric-ordenes').textContent = activas;

        const bajoStock = (repuestos.data || []).filter(r => r.cantidadStock < CONFIG.LOW_STOCK_THRESHOLD).length;
        document.getElementById('metric-stock').textContent = bajoStock;

        const factHoy = (facturas.data || []).reduce((sum, f) => sum + f.montoTotal, 0);
        document.getElementById('metric-facturacion').textContent = Utils.formatCurrency(factHoy);

        renderChart(ordenesPend.totalCount, ordenesProc.totalCount, repuestos.data);
        renderRecentOrdenes();
    } catch {
        document.getElementById('metric-ordenes').textContent = '—';
        document.getElementById('metric-stock').textContent = '—';
        document.getElementById('metric-facturacion').textContent = '—';
    } finally {
        UI.setLoading(false);
    }
}

async function renderRecentOrdenes() {
    const params = { page: 1, pageSize: 5 };
    if (Auth.isMecanico()) params.mecanicoId = Auth.getUserId();

    try {
        const { data } = await API.getOrdenes(params);
        const tbody = document.getElementById('dashboard-ordenes-body');
        if (!data?.length) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin órdenes recientes</td></tr>';
            return;
        }
        tbody.innerHTML = data.map(o => {
            const est = Utils.getEstadoOrden(o.estado);
            return `
            <tr>
                <td>#${o.id}</td>
                <td>${Utils.escapeHtml(o.clienteNombre)}</td>
                <td>${Utils.escapeHtml(o.vehiculoDescripcion)}</td>
                <td><span class="badge ${est.class}">${est.label}</span></td>
                <td>${Utils.formatDate(o.fechaIngreso)}</td>
            </tr>`;
        }).join('');
    } catch {
        document.getElementById('dashboard-ordenes-body').innerHTML =
            '<tr><td colspan="5" class="text-center text-muted">Error al cargar</td></tr>';
    }
}

let chartInstance = null;
function renderChart(pendientes, enProceso, repuestos) {
    const ctx = document.getElementById('dashboard-chart');
    if (!ctx || typeof Chart === 'undefined') return;

    const bajo = (repuestos || []).filter(r => r.cantidadStock < CONFIG.LOW_STOCK_THRESHOLD).length;
    const ok = (repuestos || []).length - bajo;

    if (chartInstance) chartInstance.destroy();
    chartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Pendientes', 'En proceso', 'Stock OK', 'Stock bajo'],
            datasets: [{
                data: [pendientes, enProceso, Math.max(ok, 0), bajo],
                backgroundColor: ['#fbbf24', '#3b82f6', '#10b981', '#ef4444'],
                borderWidth: 2,
                borderColor: '#111827', // Matches card background
            }],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { 
                legend: { 
                    position: 'bottom',
                    labels: {
                        color: '#9ca3af',
                        font: {
                            family: "'Inter', system-ui, sans-serif",
                            size: 11
                        },
                        padding: 15
                    }
                } 
            },
        },
    });
}
