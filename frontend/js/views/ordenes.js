import { API } from '../api.js';
import { Auth } from '../auth.js';
import { CONFIG, TIPO_SERVICIO } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';
import { navigateTo } from '../router.js';

let state = { page: 1, filters: {} };
let mecanicosCache = [];
let repuestosCache = [];
let vehiculosCache = [];

export async function renderOrdenes() {
    if (!document.getElementById('ordenes-table')?.dataset.bound) {
        bindOrdenesEvents();
        document.getElementById('ordenes-table').dataset.bound = 'true';
    }
    await loadOrdenes();
}

function bindOrdenesEvents() {
    document.getElementById('btn-nueva-orden')?.addEventListener('click', openCreateOrdenModal);
    document.getElementById('btn-filter-ordenes')?.addEventListener('click', applyFilters);
    document.getElementById('btn-clear-filters')?.addEventListener('click', () => {
        document.getElementById('filter-estado').value = '';
        document.getElementById('filter-fecha-desde').value = '';
        document.getElementById('filter-fecha-hasta').value = '';
        document.getElementById('filter-vin').value = '';
        state.filters = {};
        state.page = 1;
        loadOrdenes();
    });

    document.getElementById('ordenes-table')?.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-action]');
        if (!btn) return;
        const id = parseInt(btn.dataset.id, 10);
        if (btn.dataset.action === 'progress') await openProgressModal(id);
        if (btn.dataset.action === 'cancel') await cancelOrden(id);
        if (btn.dataset.action === 'factura') await generarFactura(id);
        if (btn.dataset.action === 'detail') await showOrdenDetail(id);
    });
}

function applyFilters() {
    state.filters = {
        estado: document.getElementById('filter-estado').value,
        fechaDesde: document.getElementById('filter-fecha-desde').value,
        fechaHasta: document.getElementById('filter-fecha-hasta').value,
        vin: document.getElementById('filter-vin')?.value?.trim().toLowerCase() || '',
    };
    state.page = 1;
    loadOrdenes();
}

async function loadOrdenes() {
    UI.setLoading(true);
    try {
        const vinFilter = state.filters.vin || '';
        const useLocalVinFilter = !!vinFilter;
        const pageSize = useLocalVinFilter ? 500 : CONFIG.DEFAULT_PAGE_SIZE;
        const page = useLocalVinFilter ? 1 : state.page;

        const params = {
            page,
            pageSize,
            estado: state.filters.estado,
            fechaDesde: state.filters.fechaDesde,
            fechaHasta: state.filters.fechaHasta,
        };
        if (Auth.isMecanico()) params.mecanicoId = Auth.getUserId();

        const { data, totalCount } = await API.getOrdenes(params);

        let items = data || [];
        if (vinFilter) {
            items = items.filter(o => o.vehiculoDescripcion?.toLowerCase().includes(vinFilter));
        }

        const displayPageSize = CONFIG.DEFAULT_PAGE_SIZE;
        let paged = items;
        let paginationTotal = totalCount;

        if (useLocalVinFilter) {
            paginationTotal = items.length;
            const start = (state.page - 1) * displayPageSize;
            paged = items.slice(start, start + displayPageSize);
        }

        const tbody = document.getElementById('ordenes-body');
        if (!paged.length) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">No hay órdenes</td></tr>';
        } else {
            tbody.innerHTML = paged.map(o => {
                const est = Utils.getEstadoOrden(o.estado);
                return `
                <tr>
                    <td>#${o.id}</td>
                    <td>${Utils.escapeHtml(o.clienteNombre)}</td>
                    <td>${Utils.escapeHtml(o.vehiculoDescripcion)}</td>
                    <td>${TIPO_SERVICIO[o.tipoServicio] || o.tipoServicio}</td>
                    <td>${Utils.escapeHtml(o.mecanicoNombre || '—')}</td>
                    <td><span class="badge ${est.class}">${est.label}</span></td>
                    <td>${Utils.formatDate(o.fechaEstimadaEntrega)}</td>
                    <td class="actions">
                        <button class="btn-icon" data-action="detail" data-id="${o.id}" title="Detalle"><i class="fa-solid fa-eye"></i></button>
                        ${canUpdateProgress(o) ? `<button class="btn-icon" data-action="progress" data-id="${o.id}" title="Actualizar progreso"><i class="fa-solid fa-wrench"></i></button>` : ''}
                        ${canCancel(o) ? `<button class="btn-icon btn-icon-danger" data-action="cancel" data-id="${o.id}" title="Cancelar"><i class="fa-solid fa-ban"></i></button>` : ''}
                        ${canFacturar(o) ? `<button class="btn-icon btn-icon-success" data-action="factura" data-id="${o.id}" title="Generar factura"><i class="fa-solid fa-file-invoice-dollar"></i></button>` : ''}
                    </td>
                </tr>`;
            }).join('');
        }

        UI.renderPagination('ordenes-pagination', {
            page: state.page,
            pageSize: displayPageSize,
            totalCount: paginationTotal,
        }, (p) => { state.page = p; loadOrdenes(); });
    } finally {
        UI.setLoading(false);
    }
}

function canUpdateProgress(o) {
    return (Auth.isMecanico() || Auth.isAdmin()) && o.estado !== 2 && o.estado !== 3;
}
function canCancel(o) {
    return Auth.isAdmin() && o.estado !== 2 && o.estado !== 3;
}
function canFacturar(o) {
    return (Auth.isMecanico() || Auth.isAdmin()) && o.estado === 2;
}

async function loadAuxData() {
    const [vehiculos, repuestos, usuarios] = await Promise.allSettled([
        API.getVehiculos(1, 200),
        API.getRepuestos(1, 200),
        Auth.isAdmin() ? API.getUsuarios(1, 50) : Promise.resolve({ data: [] }),
    ]);
    vehiculosCache = vehiculos.status === 'fulfilled' ? vehiculos.value.data || [] : [];
    repuestosCache = repuestos.status === 'fulfilled' ? repuestos.value.data || [] : [];
    mecanicosCache = usuarios.status === 'fulfilled'
        ? (usuarios.value.data || []).filter(u => u.rol === 1 || u.rol === 'Mecanico')
        : [{ id: 2, nombre: 'Juan Mecánico' }];
}

async function openCreateOrdenModal() {
    UI.setLoading(true);
    try {
        await loadAuxData();
    } finally {
        UI.setLoading(false);
    }

    const vehiculoOptions = vehiculosCache.map(v =>
        `<option value="${v.id}">${Utils.escapeHtml(v.marca)} ${Utils.escapeHtml(v.modelo)} — ${Utils.escapeHtml(v.vin)} (${Utils.escapeHtml(v.clienteNombre)})</option>`
    ).join('');

    const mecanicoOptions = mecanicosCache.map(m =>
        `<option value="${m.id}">${Utils.escapeHtml(m.nombre)}</option>`
    ).join('');

    const repuestoOptions = repuestosCache.filter(r => r.activo).map(r =>
        `<option value="${r.id}">${Utils.escapeHtml(r.codigo)} — ${Utils.escapeHtml(r.descripcion)} (Stock: ${r.cantidadStock})</option>`
    ).join('');

    UI.openModal('Nueva Orden de Servicio', `
        <form id="form-nueva-orden">
            <div class="form-grid">
                <div class="form-group full-width">
                    <label>Vehículo *</label>
                    <select name="vehiculoId" required><option value="">Seleccione...</option>${vehiculoOptions}</select>
                </div>
                <div class="form-group">
                    <label>Tipo de servicio *</label>
                    <select name="tipoServicio" id="orden-tipo-servicio" required>
                        <option value="0">Mantenimiento preventivo</option>
                        <option value="1">Reparación</option>
                        <option value="2">Diagnóstico</option>
                    </select>
                </div>
                <div class="form-group">
                    <label>Complejidad</label>
                    <select name="complejidad" id="orden-complejidad">
                        <option value="0">Baja</option>
                        <option value="1" selected>Media</option>
                        <option value="2">Alta (+50% tiempo)</option>
                    </select>
                </div>
                <div class="form-group">
                    <label>Fecha estimada de entrega</label>
                    <p id="fecha-estimada-preview" class="fecha-preview">—</p>
                </div>
                <div class="form-group">
                    <label>Mecánico</label>
                    <select name="mecanicoId"><option value="">Sin asignar</option>${mecanicoOptions}</select>
                </div>
                <div class="form-group">
                    <label>Costo mano de obra *</label>
                    <input name="costoManoObra" type="number" step="0.01" min="0" required>
                </div>
                <div class="form-group full-width">
                    <label>Descripción</label>
                    <textarea name="descripcion" rows="2"></textarea>
                </div>
            </div>
            <h4 class="form-section-title">Repuestos (opcional)</h4>
            <div id="repuestos-orden-container">
                <div class="repuesto-row form-grid">
                    <div class="form-group"><label>Repuesto</label><select class="repuesto-select"><option value="">—</option>${repuestoOptions}</select></div>
                    <div class="form-group"><label>Cantidad</label><input class="repuesto-cantidad" type="number" min="1" value="1"></div>
                </div>
            </div>
            <button type="button" class="btn btn-sm btn-outline" id="btn-add-repuesto-orden"><i class="fa-solid fa-plus"></i> Agregar repuesto</button>
        </form>
    `, `<button class="btn btn-primary" id="btn-save-orden" data-rate-sensitive><i class="fa-solid fa-save"></i> Crear orden</button>`);

    const tipoSelect = document.getElementById('orden-tipo-servicio');
    const complejidadSelect = document.getElementById('orden-complejidad');
    const updateFechaPreview = () => {
        const tipo = parseInt(tipoSelect.value, 10);
        const complejidad = parseInt(complejidadSelect?.value ?? '1', 10);
        const fecha = Utils.calcularFechaEstimadaEntrega(tipo, new Date(), complejidad);
        document.getElementById('fecha-estimada-preview').textContent = Utils.formatDate(fecha.toISOString());
    };
    tipoSelect.addEventListener('change', updateFechaPreview);
    complejidadSelect?.addEventListener('change', updateFechaPreview);
    updateFechaPreview();

    document.getElementById('repuestos-orden-container')?.addEventListener('change', (e) => {
        if (e.target.classList.contains('repuesto-select') || e.target.classList.contains('repuesto-cantidad')) {
            const select = e.target.closest('.repuesto-row')?.querySelector('.repuesto-select');
            if (select) highlightStockWarning(select);
        }
    });

    document.getElementById('btn-add-repuesto-orden').addEventListener('click', () => {
        document.getElementById('repuestos-orden-container').insertAdjacentHTML('beforeend', `
            <div class="repuesto-row form-grid">
                <div class="form-group"><label>Repuesto</label><select class="repuesto-select"><option value="">—</option>${repuestoOptions}</select></div>
                <div class="form-group"><label>Cantidad</label><input class="repuesto-cantidad" type="number" min="1" value="1"></div>
            </div>
        `);
    });

    document.getElementById('btn-save-orden').addEventListener('click', async (e) => {
        const form = document.getElementById('form-nueva-orden');
        if (!form.checkValidity()) { form.reportValidity(); return; }

        const repuestos = collectRepuestosFromForm(form);
        if (!validateRepuestosStock(repuestos)) return;

        try {
            await API.withTrigger(e.currentTarget, () => API.createOrden({
                vehiculoId: parseInt(form.vehiculoId.value, 10),
                tipoServicio: parseInt(form.tipoServicio.value, 10),
                complejidad: parseInt(form.complejidad?.value ?? '1', 10),
                mecanicoId: form.mecanicoId.value ? parseInt(form.mecanicoId.value, 10) : null,
                costoManoObra: parseFloat(form.costoManoObra.value),
                descripcion: form.descripcion.value || null,
                repuestos,
            }));
            UI.toast('Orden creada correctamente', 'success');
            UI.closeModal();
            loadOrdenes();
        } catch { /* handled */ }
    });
}

function collectRepuestosFromForm(form, selector = '.repuesto-row') {
    const repuestos = [];
    form.querySelectorAll(selector).forEach(row => {
        const id = row.querySelector('.repuesto-select')?.value;
        const cant = parseInt(row.querySelector('.repuesto-cantidad')?.value, 10);
        if (id && cant > 0) repuestos.push({ repuestoId: parseInt(id, 10), cantidad: cant });
    });
    return repuestos;
}

function validateRepuestosStock(repuestosList) {
    for (const { repuestoId, cantidad } of repuestosList) {
        const r = repuestosCache.find(x => x.id === repuestoId);
        if (!r) continue;
        if (r.cantidadStock < cantidad) {
            UI.toast(
                `Stock insuficiente: ${r.descripcion} (disponible: ${r.cantidadStock}, solicitado: ${cantidad})`,
                'warning',
                6000
            );
            return false;
        }
    }
    return true;
}

function highlightStockWarning(selectEl) {
    const row = selectEl.closest('.repuesto-row');
    const cantInput = row?.querySelector('.repuesto-cantidad');
    const id = parseInt(selectEl.value, 10);
    const cant = parseInt(cantInput?.value, 10) || 0;
    const r = repuestosCache.find(x => x.id === id);
    row?.classList.toggle('row-alert', !!(r && id && cant > r.cantidadStock));
}

async function openProgressModal(id) {
    UI.setLoading(true);
    try {
        const [{ data: orden }, repuestosRes] = await Promise.all([
            API.getOrden(id),
            API.getRepuestos(1, 200),
        ]);
        repuestosCache = repuestosRes.data || [];

        const repuestoOptions = repuestosCache.filter(r => r.activo).map(r =>
            `<option value="${r.id}">${Utils.escapeHtml(r.codigo)} — Stock: ${r.cantidadStock}</option>`
        ).join('');

        UI.openModal(`Actualizar Orden #${id}`, `
            <form id="form-progress">
                <div class="form-grid">
                    <div class="form-group">
                        <label>Estado *</label>
                        <select name="estado" required>
                            <option value="0" ${orden.estado === 0 ? 'selected' : ''}>Pendiente</option>
                            <option value="1" ${orden.estado === 1 ? 'selected' : ''}>En proceso</option>
                            <option value="2" ${orden.estado === 2 ? 'selected' : ''}>Completada</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>Costo mano de obra</label>
                        <input name="costoManoObra" type="number" step="0.01" value="${orden.costoManoObra}">
                    </div>
                    <div class="form-group full-width">
                        <label>Trabajo realizado</label>
                        <textarea name="trabajoRealizado" rows="3">${Utils.escapeHtml(orden.trabajoRealizado || '')}</textarea>
                    </div>
                </div>
                ${orden.detalles?.length ? `
                    <h4 class="form-section-title">Repuestos asignados</h4>
                    <ul class="detail-list">${orden.detalles.map(d =>
                        `<li>${Utils.escapeHtml(d.repuestoDescripcion)} × ${d.cantidad} — ${Utils.formatCurrency(d.subtotal)}</li>`
                    ).join('')}</ul>
                ` : ''}
                <h4 class="form-section-title">Repuestos adicionales</h4>
                <div id="rep-adicionales">
                    <div class="repuesto-row form-grid">
                        <div class="form-group"><select class="repuesto-select"><option value="">—</option>${repuestoOptions}</select></div>
                        <div class="form-group"><input class="repuesto-cantidad" type="number" min="1" value="1" placeholder="Cantidad"></div>
                    </div>
                </div>
            </form>
        `, `<button class="btn btn-primary" id="btn-save-progress" data-rate-sensitive><i class="fa-solid fa-check"></i> Guardar</button>`);

        document.getElementById('rep-adicionales')?.addEventListener('change', (e) => {
            if (e.target.classList.contains('repuesto-select') || e.target.classList.contains('repuesto-cantidad')) {
                const select = e.target.closest('.repuesto-row')?.querySelector('.repuesto-select');
                if (select) highlightStockWarning(select);
            }
        });

        document.getElementById('btn-save-progress').addEventListener('click', async (e) => {
            const form = document.getElementById('form-progress');
            const adicionales = collectRepuestosFromForm(form, '#rep-adicionales .repuesto-row');
            if (!validateRepuestosStock(adicionales)) return;

            try {
                await API.withTrigger(e.currentTarget, () => API.updateOrdenTrabajo(id, {
                    estado: parseInt(form.estado.value, 10),
                    trabajoRealizado: form.trabajoRealizado.value,
                    costoManoObra: parseFloat(form.costoManoObra.value),
                    repuestosAdicionales: adicionales.length ? adicionales : null,
                }));
                UI.toast('Orden actualizada', 'success');
                UI.closeModal();
                loadOrdenes();
            } catch { /* handled */ }
        });
    } finally {
        UI.setLoading(false);
    }
}

async function showOrdenDetail(id) {
    const { data: o } = await API.getOrden(id);
    const est = Utils.getEstadoOrden(o.estado);
    UI.openModal(`Orden #${o.id}`, `
        <div class="detail-grid">
            <div><strong>Cliente:</strong> ${Utils.escapeHtml(o.clienteNombre)}</div>
            <div><strong>Vehículo:</strong> ${Utils.escapeHtml(o.vehiculoDescripcion)}</div>
            <div><strong>Tipo:</strong> ${TIPO_SERVICIO[o.tipoServicio]}</div>
            <div><strong>Mecánico:</strong> ${Utils.escapeHtml(o.mecanicoNombre || '—')}</div>
            <div><strong>Estado:</strong> <span class="badge ${est.class}">${est.label}</span></div>
            <div><strong>Ingreso:</strong> ${Utils.formatDateTime(o.fechaIngreso)}</div>
            <div><strong>Entrega est.:</strong> ${Utils.formatDate(o.fechaEstimadaEntrega)}</div>
            <div><strong>Mano de obra:</strong> ${Utils.formatCurrency(o.costoManoObra)}</div>
        </div>
        ${o.descripcion ? `<p><strong>Descripción:</strong> ${Utils.escapeHtml(o.descripcion)}</p>` : ''}
        ${o.trabajoRealizado ? `<p><strong>Trabajo realizado:</strong> ${Utils.escapeHtml(o.trabajoRealizado)}</p>` : ''}
        ${o.detalles?.length ? `
            <h4>Repuestos</h4>
            <table class="data-table"><thead><tr><th>Repuesto</th><th>Cant.</th><th>Unit.</th><th>Subtotal</th></tr></thead>
            <tbody>${o.detalles.map(d => `<tr>
                <td>${Utils.escapeHtml(d.repuestoDescripcion)}</td>
                <td>${d.cantidad}</td>
                <td>${Utils.formatCurrency(d.costoUnitario)}</td>
                <td>${Utils.formatCurrency(d.subtotal)}</td>
            </tr>`).join('')}</tbody></table>
        ` : ''}
    `);
}

async function cancelOrden(id) {
    if (!confirm('¿Cancelar esta orden de servicio?')) return;
    try {
        await API.cancelarOrden(id);
        UI.toast('Orden cancelada', 'success');
        loadOrdenes();
    } catch { /* handled */ }
}

async function generarFactura(ordenId) {
    try {
        await API.generarFactura(ordenId);
        UI.toast('Factura generada correctamente', 'success');
        navigateTo('facturas');
    } catch { /* handled */ }
}
