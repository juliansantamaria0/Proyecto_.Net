import { API } from '../api.js';
import { CONFIG } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1, fechaDesde: '', fechaHasta: '' };
let facturasChart = null;

export async function renderFacturas() {
    if (!document.getElementById('facturas-table')?.dataset.bound) {
        bindFacturasEvents();
        document.getElementById('facturas-table').dataset.bound = 'true';
    }
    await loadFacturas();
}

function bindFacturasEvents() {
    document.getElementById('btn-filter-facturas')?.addEventListener('click', (e) => {
        state.fechaDesde = document.getElementById('filter-factura-fecha').value;
        state.fechaHasta = document.getElementById('filter-factura-fecha-hasta').value;
        state.page = 1;
        loadFacturas(e.currentTarget);
    });
    document.getElementById('facturas-table')?.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-action]');
        if (btn?.dataset.action === 'view') await showFacturaDetail(parseInt(btn.dataset.id, 10));
    });
}

async function loadFacturas(triggerEl = null) {
    UI.setLoading(true);
    try {
        const loadFn = async () => {
            const { data, totalCount } = await API.getFacturas(1, 500, { fechaDesde: state.fechaDesde });
            let items = data || [];
            if (state.fechaHasta) {
                const hasta = new Date(state.fechaHasta);
                hasta.setHours(23, 59, 59, 999);
                items = items.filter(f => new Date(f.fechaEmision) <= hasta);
            }

            renderFacturasChart(items);

            const pageSize = CONFIG.DEFAULT_PAGE_SIZE;
            const start = (state.page - 1) * pageSize;
            const paged = items.slice(start, start + pageSize);

            const tbody = document.getElementById('facturas-body');
            if (!paged.length) {
                tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No hay facturas en el período</td></tr>';
            } else {
                tbody.innerHTML = paged.map(f => `
                    <tr>
                        <td><strong>${Utils.escapeHtml(f.numeroFactura)}</strong></td>
                        <td>Orden #${f.ordenServicioId}</td>
                        <td>${Utils.escapeHtml(f.clienteNombre)}</td>
                        <td>${Utils.formatDateTime(f.fechaEmision)}</td>
                        <td><strong>${Utils.formatCurrency(f.montoTotal)}</strong></td>
                        <td class="actions">
                            <button class="btn-icon" data-action="view" data-id="${f.id}"><i class="fa-solid fa-file-invoice"></i></button>
                        </td>
                    </tr>
                `).join('');
            }

            UI.renderPagination('facturas-pagination', { page: state.page, pageSize, totalCount: items.length }, p => {
                state.page = p; loadFacturas();
            });
        };

        if (triggerEl) await API.withTrigger(triggerEl, loadFn);
        else await loadFn();
    } finally {
        UI.setLoading(false);
    }
}

function renderFacturasChart(facturas) {
    const ctx = document.getElementById('facturas-chart');
    if (!ctx || typeof Chart === 'undefined') return;

    const byDay = {};
    facturas.forEach(f => {
        const day = new Date(f.fechaEmision).toLocaleDateString('es-MX');
        byDay[day] = (byDay[day] || 0) + f.montoTotal;
    });
    const labels = Object.keys(byDay).sort((a, b) => new Date(a) - new Date(b));
    const values = labels.map(l => byDay[l]);

    if (facturasChart) facturasChart.destroy();
    facturasChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Ingresos ($)',
                data: values,
                backgroundColor: '#3b82f6',
                borderRadius: 6,
            }],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { 
                y: { 
                    beginAtZero: true,
                    grid: { color: 'rgba(255, 255, 255, 0.06)' },
                    ticks: { color: '#9ca3af', font: { family: "'Inter', system-ui, sans-serif" } }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: '#9ca3af', font: { family: "'Inter', system-ui, sans-serif" } }
                }
            },
        },
    });
}

async function showFacturaDetail(id) {
    UI.setLoading(true);
    try {
        const { data: f } = await API.getFactura(id);
        const { data: orden } = await API.getOrden(f.ordenServicioId);
        const repuestosHtml = (orden?.detalles || []).map(d => `
            <tr>
                <td>${Utils.escapeHtml(d.repuestoDescripcion)}</td>
                <td>${d.cantidad}</td>
                <td>${Utils.formatCurrency(d.costoUnitario)}</td>
                <td>${Utils.formatCurrency(d.subtotal)}</td>
            </tr>
        `).join('') || '<tr><td colspan="4" class="text-muted">Sin repuestos</td></tr>';

        UI.openModal(`Factura ${f.numeroFactura}`, `
            <div class="invoice-document">
                <div class="invoice-header">
                    <div><h3>AutoTallerManager</h3><p class="text-muted">Factura de servicio</p></div>
                    <div class="invoice-meta">
                        <p><strong>${Utils.escapeHtml(f.numeroFactura)}</strong></p>
                        <p>${Utils.formatDateTime(f.fechaEmision)}</p>
                    </div>
                </div>
                <div class="invoice-client">
                    <p><strong>Cliente:</strong> ${Utils.escapeHtml(f.clienteNombre)}</p>
                    <p><strong>Vehículo:</strong> ${Utils.escapeHtml(f.vehiculoDescripcion)}</p>
                    <p><strong>Orden:</strong> #${f.ordenServicioId}</p>
                </div>
                <table class="data-table">
                    <thead><tr><th>Concepto</th><th>Cant.</th><th>P. Unit.</th><th>Subtotal</th></tr></thead>
                    <tbody>
                        <tr>
                            <td>Mano de obra</td><td>1</td>
                            <td>${Utils.formatCurrency(f.montoManoObra)}</td>
                            <td>${Utils.formatCurrency(f.montoManoObra)}</td>
                        </tr>
                        ${repuestosHtml}
                    </tbody>
                </table>
                <div class="invoice-totals">
                    <div class="total-row"><span>Mano de obra:</span><span>${Utils.formatCurrency(f.montoManoObra)}</span></div>
                    <div class="total-row"><span>Repuestos:</span><span>${Utils.formatCurrency(f.montoRepuestos)}</span></div>
                    <div class="total-row total-final"><span>TOTAL:</span><span>${Utils.formatCurrency(f.montoTotal)}</span></div>
                </div>
            </div>
        `, `<button class="btn btn-outline" onclick="window.print()"><i class="fa-solid fa-print"></i> Imprimir</button>`);
    } finally {
        UI.setLoading(false);
    }
}
