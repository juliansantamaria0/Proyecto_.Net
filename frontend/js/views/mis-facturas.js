import { API } from '../api.js';
import { Auth } from '../auth.js';
import { CONFIG } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1 };

export async function renderMisFacturas() {
    if (!document.getElementById('mis-facturas-table')?.dataset.bound) {
        document.getElementById('mis-facturas-table').dataset.bound = 'true';
        document.getElementById('mis-facturas-table').addEventListener('click', async (e) => {
            const btn = e.target.closest('[data-action="view"]');
            if (btn) await showFactura(parseInt(btn.dataset.id, 10));
        });
    }
    await loadFacturas();
}

async function loadFacturas() {
    UI.setLoading(true);
    try {
        const { data, totalCount } = await API.getFacturas(state.page, CONFIG.DEFAULT_PAGE_SIZE, {
            clienteId: Auth.getClienteId(),
        });
        const tbody = document.getElementById('mis-facturas-body');
        if (!data?.length) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No tiene facturas</td></tr>';
        } else {
            tbody.innerHTML = data.map(f => `
                <tr>
                    <td><strong>${Utils.escapeHtml(f.numeroFactura)}</strong></td>
                    <td>Orden #${f.ordenServicioId}</td>
                    <td>${Utils.escapeHtml(f.vehiculoDescripcion)}</td>
                    <td>${Utils.formatDateTime(f.fechaEmision)}</td>
                    <td><strong>${Utils.formatCurrency(f.montoTotal)}</strong></td>
                    <td class="actions">
                        <button class="btn-icon" data-action="view" data-id="${f.id}"><i class="fa-solid fa-file-invoice"></i></button>
                    </td>
                </tr>
            `).join('');
        }
        UI.renderPagination('mis-facturas-pagination', {
            page: state.page, pageSize: CONFIG.DEFAULT_PAGE_SIZE, totalCount,
        }, p => { state.page = p; loadFacturas(); });
    } finally {
        UI.setLoading(false);
    }
}

async function showFactura(id) {
    UI.setLoading(true);
    try {
        const { data: f } = await API.getFactura(id);
        const { data: orden } = await API.getOrden(f.ordenServicioId);
        const repuestosHtml = (orden?.detalles || []).map(d => `
            <tr><td>${Utils.escapeHtml(d.repuestoDescripcion)}</td><td>${d.cantidad}</td>
            <td>${Utils.formatCurrency(d.subtotal)}</td></tr>
        `).join('') || '<tr><td colspan="3" class="text-muted">Sin repuestos</td></tr>';

        UI.openModal(`Factura ${f.numeroFactura}`, `
            <div class="invoice-document">
                <div class="invoice-header">
                    <div><h3>AutoTallerManager</h3><p class="text-muted">Comprobante de servicio</p></div>
                    <div class="invoice-meta">
                        <p><strong>${Utils.escapeHtml(f.numeroFactura)}</strong></p>
                        <p>${Utils.formatDateTime(f.fechaEmision)}</p>
                    </div>
                </div>
                <div class="invoice-client">
                    <p><strong>Vehículo:</strong> ${Utils.escapeHtml(f.vehiculoDescripcion)}</p>
                    <p><strong>Orden:</strong> #${f.ordenServicioId}</p>
                </div>
                <table class="data-table">
                    <thead><tr><th>Concepto</th><th>Cant.</th><th>Subtotal</th></tr></thead>
                    <tbody>
                        <tr><td>Mano de obra</td><td>1</td><td>${Utils.formatCurrency(f.montoManoObra)}</td></tr>
                        ${repuestosHtml}
                    </tbody>
                </table>
                <div class="invoice-totals">
                    <div class="total-row"><span>Repuestos:</span><span>${Utils.formatCurrency(f.montoRepuestos)}</span></div>
                    <div class="total-row total-final"><span>TOTAL:</span><span>${Utils.formatCurrency(f.montoTotal)}</span></div>
                </div>
            </div>
        `);
    } finally {
        UI.setLoading(false);
    }
}
