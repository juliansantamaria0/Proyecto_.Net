import { API } from '../api.js';
import { Auth } from '../auth.js';
import { CONFIG, TIPO_SERVICIO } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';
import { navigateTo } from '../router.js';

let repuestosCache = [];

export async function renderPanelMecanico() {
    const view = document.getElementById('view-panel-mecanico');
    if (!view?.dataset.bound) {
        view.dataset.bound = 'true';
        document.getElementById('panel-refresh')?.addEventListener('click', (e) => {
            API.withTrigger(e.currentTarget, () => loadPanel());
        });
    }
    await loadPanel();
}

async function loadPanel() {
    UI.setLoading(true);
    try {
        const mecanicoId = Auth.isMecanico() ? Auth.getUserId() : null;
        const [pendientes, enProceso, completadas, repuestosRes] = await Promise.all([
            API.getOrdenes({ page: 1, pageSize: 50, estado: 0, mecanicoId }),
            API.getOrdenes({ page: 1, pageSize: 50, estado: 1, mecanicoId }),
            API.getOrdenes({ page: 1, pageSize: 50, estado: 2, mecanicoId }),
            API.getRepuestos(1, 200),
        ]);
        repuestosCache = repuestosRes.data || [];

        renderColumn('panel-pendientes', pendientes.data || [], { showProgress: true, showComplete: false, showFactura: false });
        renderColumn('panel-proceso', enProceso.data || [], { showProgress: true, showComplete: true, showFactura: false });
        renderColumn('panel-completadas', completadas.data || [], { showProgress: false, showComplete: false, showFactura: true });

        document.getElementById('panel-count-pendientes').textContent = pendientes.totalCount || 0;
        document.getElementById('panel-count-proceso').textContent = enProceso.totalCount || 0;
        document.getElementById('panel-count-completadas').textContent = completadas.totalCount || 0;
    } finally {
        UI.setLoading(false);
    }
}

function renderColumn(containerId, ordenes, { showProgress, showComplete, showFactura }) {
    const container = document.getElementById(containerId);
    if (!ordenes.length) {
        container.innerHTML = '<p class="text-muted text-center">Sin órdenes en esta columna</p>';
        return;
    }
    container.innerHTML = ordenes.map(o => {
        const est = Utils.getEstadoOrden(o.estado);
        return `
        <div class="work-card" data-id="${o.id}">
            <div class="work-card-header">
                <span class="badge ${est.class}">${est.label}</span>
                <strong>#${o.id}</strong>
            </div>
            <p><i class="fa-solid fa-user"></i> ${Utils.escapeHtml(o.clienteNombre)}</p>
            <p><i class="fa-solid fa-car"></i> ${Utils.escapeHtml(o.vehiculoDescripcion)}</p>
            <p><i class="fa-solid fa-wrench"></i> ${TIPO_SERVICIO[o.tipoServicio]}</p>
            <p class="text-sm text-muted">Entrega: ${Utils.formatDate(o.fechaEstimadaEntrega)}</p>
            <div class="work-card-actions">
                ${showProgress ? `<button class="btn btn-sm btn-primary btn-panel-progress" data-id="${o.id}" data-rate-sensitive>
                    <i class="fa-solid fa-play"></i> Actualizar trabajo
                </button>` : ''}
                ${showComplete ? `<button class="btn btn-sm btn-success btn-panel-complete" data-id="${o.id}" data-rate-sensitive>
                    <i class="fa-solid fa-check"></i> Completar
                </button>` : ''}
                ${showFactura ? `<button class="btn btn-sm btn-primary btn-panel-factura" data-id="${o.id}" data-rate-sensitive>
                    <i class="fa-solid fa-file-invoice-dollar"></i> Generar factura
                </button>` : ''}
            </div>
        </div>`;
    }).join('');

    container.querySelectorAll('.btn-panel-progress').forEach(btn => {
        btn.addEventListener('click', (e) => openProgressModal(parseInt(btn.dataset.id, 10), e.currentTarget));
    });
    container.querySelectorAll('.btn-panel-complete').forEach(btn => {
        btn.addEventListener('click', (e) => completeOrder(parseInt(btn.dataset.id, 10), e.currentTarget));
    });
    container.querySelectorAll('.btn-panel-factura').forEach(btn => {
        btn.addEventListener('click', (e) => generarFactura(parseInt(btn.dataset.id, 10), e.currentTarget));
    });
}

async function openProgressModal(id, triggerEl) {
    UI.setLoading(true);
    try {
        const { data: orden } = await API.getOrden(id);
        const repuestoOptions = repuestosCache.filter(r => r.activo !== false).map(r => {
            const level = Utils.getStockLevel(r);
            return `<option value="${r.id}" data-stock="${r.cantidadStock}">${Utils.escapeHtml(r.codigo)} — Stock: ${r.cantidadStock} ${level === 'low' || level === 'critical' ? '⚠' : ''}</option>`;
        }).join('');

        UI.openModal(`Orden #${id} — Trabajo`, `
            <form id="form-panel-progress">
                <div class="form-group">
                    <label>Estado</label>
                    <select name="estado">
                        <option value="0" ${orden.estado === 0 ? 'selected' : ''}>Pendiente</option>
                        <option value="1" ${orden.estado === 1 ? 'selected' : ''}>En proceso</option>
                        <option value="2" ${orden.estado === 2 ? 'selected' : ''}>Completada</option>
                    </select>
                </div>
                <div class="form-group">
                    <label>Trabajo realizado</label>
                    <textarea name="trabajoRealizado" rows="4" class="input-large">${Utils.escapeHtml(orden.trabajoRealizado || '')}</textarea>
                </div>
                <div class="form-group">
                    <label>Agregar repuesto</label>
                    <div class="form-grid">
                        <select id="panel-rep-select" class="repuesto-select"><option value="">—</option>${repuestoOptions}</select>
                        <input id="panel-rep-cant" type="number" min="1" value="1" class="repuesto-cantidad">
                    </div>
                    <p id="panel-stock-warning" class="field-error"></p>
                </div>
            </form>
        `, `<button class="btn btn-primary" id="btn-panel-save" data-rate-sensitive>Guardar avance</button>`);

        const updateWarning = () => {
            const sel = document.getElementById('panel-rep-select');
            const cant = parseInt(document.getElementById('panel-rep-cant').value, 10) || 0;
            const stock = parseInt(sel.selectedOptions[0]?.dataset.stock || '0', 10);
            const warn = document.getElementById('panel-stock-warning');
            if (sel.value && cant > stock) {
                warn.textContent = `Stock insuficiente: solo hay ${stock} unidades disponibles.`;
            } else warn.textContent = '';
        };
        document.getElementById('panel-rep-select')?.addEventListener('change', updateWarning);
        document.getElementById('panel-rep-cant')?.addEventListener('input', updateWarning);

        document.getElementById('btn-panel-save').addEventListener('click', async (e) => {
            const form = document.getElementById('form-panel-progress');
            const sel = document.getElementById('panel-rep-select');
            const cant = parseInt(document.getElementById('panel-rep-cant').value, 10) || 0;
            const repuestosAdicionales = [];
            if (sel.value) {
                const rep = repuestosCache.find(r => r.id === parseInt(sel.value, 10));
                if (rep && cant > rep.cantidadStock) {
                    UI.toast(`Stock insuficiente para ${rep.codigo}. Disponible: ${rep.cantidadStock}`, 'error');
                    return;
                }
                repuestosAdicionales.push({ repuestoId: parseInt(sel.value, 10), cantidad: cant });
            }

            const nuevoEstado = parseInt(form.estado.value, 10);
            try {
                await API.withTrigger(e.currentTarget, () => API.updateOrdenTrabajo(id, {
                    estado: nuevoEstado,
                    trabajoRealizado: form.trabajoRealizado.value,
                    repuestosAdicionales: repuestosAdicionales.length ? repuestosAdicionales : null,
                }));
                UI.toast('Avance guardado', 'success');
                UI.closeModal();
                loadPanel();
                if (nuevoEstado === 2) {
                    UI.toast('Orden completada. Puede generar la factura en la columna Completadas.', 'info', 6000);
                }
            } catch { /* handled */ }
        });
    } finally {
        UI.setLoading(false);
    }
}

async function completeOrder(id, triggerEl) {
    try {
        const { data: orden } = await API.getOrden(id);
        await API.withTrigger(triggerEl, () => API.updateOrdenTrabajo(id, {
            estado: 2,
            trabajoRealizado: orden.trabajoRealizado?.trim() || 'Trabajo completado.',
        }));
        UI.toast('Orden completada. Genere la factura en la columna Completadas.', 'success', 6000);
        loadPanel();
    } catch { /* handled */ }
}

async function generarFactura(ordenId, triggerEl) {
    try {
        await API.withTrigger(triggerEl, () => API.generarFactura(ordenId));
        UI.toast('Factura generada correctamente', 'success');
        navigateTo('facturas');
    } catch { /* handled */ }
}
