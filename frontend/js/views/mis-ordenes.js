import { API } from '../api.js';
import { Auth } from '../auth.js';
import { CONFIG, ESTADO_ORDEN, TIPO_SERVICIO } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1, estado: '' };

export async function renderMisOrdenes() {
    if (!document.getElementById('mis-ordenes-table')?.dataset.bound) {
        document.getElementById('filter-mis-orden-estado')?.addEventListener('change', (e) => {
            state.estado = e.target.value;
            state.page = 1;
            loadOrdenes();
        });
        document.getElementById('mis-ordenes-table').dataset.bound = 'true';
        document.getElementById('mis-ordenes-table').addEventListener('click', async (e) => {
            const btn = e.target.closest('[data-action="detail"]');
            if (btn) await showDetalle(parseInt(btn.dataset.id, 10));
        });
    }
    await loadOrdenes();
}

async function loadOrdenes() {
    UI.setLoading(true);
    try {
        const params = {
            page: state.page,
            pageSize: CONFIG.DEFAULT_PAGE_SIZE,
            clienteId: Auth.getClienteId(),
        };
        if (state.estado !== '') params.estado = parseInt(state.estado, 10);

        const { data, totalCount } = await API.getOrdenes(params);
        const tbody = document.getElementById('mis-ordenes-body');
        if (!data?.length) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No hay órdenes</td></tr>';
        } else {
            tbody.innerHTML = data.map(o => {
                const est = ESTADO_ORDEN[o.estado] || { label: o.estado, class: '' };
                return `<tr>
                    <td>#${o.id}</td>
                    <td>${Utils.escapeHtml(o.vehiculoDescripcion)}</td>
                    <td>${TIPO_SERVICIO[o.tipoServicio] || o.tipoServicio}</td>
                    <td><span class="badge ${est.class}">${est.label}</span></td>
                    <td>${Utils.formatDate(o.fechaEstimadaEntrega)}</td>
                    <td class="actions">
                        <button class="btn-icon" data-action="detail" data-id="${o.id}"><i class="fa-solid fa-eye"></i></button>
                    </td>
                </tr>`;
            }).join('');
        }
        UI.renderPagination('mis-ordenes-pagination', {
            page: state.page, pageSize: CONFIG.DEFAULT_PAGE_SIZE, totalCount,
        }, p => { state.page = p; loadOrdenes(); });
    } finally {
        UI.setLoading(false);
    }
}

async function showDetalle(id) {
    const { data: o } = await API.getOrden(id);
    const est = ESTADO_ORDEN[o.estado] || { label: o.estado, class: '' };
    const steps = ['Pendiente', 'En proceso', 'Completada'];
    const stepIdx = o.estado === 3 ? -1 : Math.min(o.estado, 2);

    UI.openModal(`Seguimiento — Orden #${o.id}`, `
        <div class="order-tracker">
            ${steps.map((label, i) => `
                <div class="tracker-step ${i <= stepIdx ? 'done' : ''} ${i === stepIdx ? 'active' : ''}">
                    <div class="tracker-dot"></div>
                    <span>${label}</span>
                </div>
            `).join('')}
        </div>
        <div class="detail-grid" style="margin-top:1rem">
            <div><strong>Vehículo:</strong> ${Utils.escapeHtml(o.vehiculoDescripcion)}</div>
            <div><strong>Servicio:</strong> ${TIPO_SERVICIO[o.tipoServicio]}</div>
            <div><strong>Estado:</strong> <span class="badge ${est.class}">${est.label}</span></div>
            <div><strong>Entrega estimada:</strong> ${Utils.formatDate(o.fechaEstimadaEntrega)}</div>
            <div><strong>Ingreso:</strong> ${Utils.formatDateTime(o.fechaIngreso)}</div>
            <div><strong>Mano de obra:</strong> ${Utils.formatCurrency(o.costoManoObra)}</div>
        </div>
        ${o.descripcion ? `<p><strong>Descripción:</strong> ${Utils.escapeHtml(o.descripcion)}</p>` : ''}
        ${o.trabajoRealizado ? `<p><strong>Trabajo realizado:</strong> ${Utils.escapeHtml(o.trabajoRealizado)}</p>` : ''}
        ${o.detalles?.length ? `
            <h4>Repuestos utilizados</h4>
            <table class="data-table"><thead><tr><th>Repuesto</th><th>Cant.</th><th>Subtotal</th></tr></thead>
            <tbody>${o.detalles.map(d => `<tr>
                <td>${Utils.escapeHtml(d.repuestoDescripcion)}</td>
                <td>${d.cantidad}</td>
                <td>${Utils.formatCurrency(d.subtotal)}</td>
            </tr>`).join('')}</tbody></table>
        ` : ''}
    `);
}
