import { API } from '../api.js';
import { CONFIG } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1, search: '' };
let repuestosCache = [];

export async function renderRepuestos() {
    if (!document.getElementById('repuestos-table')?.dataset.bound) {
        bindRepuestosEvents();
        document.getElementById('repuestos-table').dataset.bound = 'true';
    }
    await loadRepuestos();
}

function bindRepuestosEvents() {
    document.getElementById('repuestos-search')?.addEventListener('input',
        Utils.debounce((e) => { state.search = e.target.value; state.page = 1; loadRepuestos(); })
    );
    document.getElementById('btn-nuevo-repuesto')?.addEventListener('click', () => openRepuestoModal());
    document.getElementById('repuestos-table')?.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-action]');
        if (!btn) return;
        const id = parseInt(btn.dataset.id, 10);
        if (btn.dataset.action === 'edit') openRepuestoModal(id);
        if (btn.dataset.action === 'stock') openQuickStockModal(id);
        if (btn.dataset.action === 'delete') deleteRepuesto(id);
    });
}

function rowClass(r) {
    const level = Utils.getStockLevel(r);
    if (level === 'critical' || level === 'low') return 'row-alert';
    if (level === 'warning') return 'row-warning';
    return '';
}

async function loadRepuestos() {
    UI.setLoading(true);
    try {
        const { data, totalCount } = await API.getRepuestos(state.page, CONFIG.DEFAULT_PAGE_SIZE, {
            descripcion: state.search,
        });
        repuestosCache = data || [];

        const tbody = document.getElementById('repuestos-body');
        if (!data?.length) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No hay repuestos</td></tr>';
        } else {
            tbody.innerHTML = data.map(r => {
                const min = Utils.getStockMinimo(r);
                return `
                <tr class="${rowClass(r)} ${!r.activo ? 'row-inactive' : ''}">
                    <td><code>${Utils.escapeHtml(r.codigo)}</code></td>
                    <td>${Utils.escapeHtml(r.descripcion)}</td>
                    <td>${Utils.escapeHtml(r.categoria)}</td>
                    <td><strong>${r.cantidadStock}</strong> <small class="text-muted">/ mín ${min}</small></td>
                    <td>${Utils.formatCurrency(r.precioUnitario)}</td>
                    <td><span class="badge ${r.activo ? 'badge-done' : 'badge-cancel'}">${r.activo ? 'Activo' : 'Baja'}</span></td>
                    <td class="actions">
                        <button class="btn-icon" data-action="stock" data-id="${r.id}" title="Ajustar stock"><i class="fa-solid fa-boxes-packing"></i></button>
                        <button class="btn-icon" data-action="edit" data-id="${r.id}"><i class="fa-solid fa-pen"></i></button>
                        <button class="btn-icon btn-icon-danger" data-action="delete" data-id="${r.id}"><i class="fa-solid fa-trash"></i></button>
                    </td>
                </tr>`;
            }).join('');
        }

        UI.renderPagination('repuestos-pagination', { page: state.page, pageSize: CONFIG.DEFAULT_PAGE_SIZE, totalCount }, p => {
            state.page = p; loadRepuestos();
        });
    } finally {
        UI.setLoading(false);
    }
}

function openQuickStockModal(id) {
    const r = repuestosCache.find(x => x.id === id);
    if (!r) return;
    UI.openModal(`Ajustar stock — ${r.codigo}`, `
        <p>Stock actual: <strong>${r.cantidadStock}</strong> (mínimo: ${Utils.getStockMinimo(r)})</p>
        <form id="form-quick-stock">
            <div class="form-group">
                <label>Nuevo stock total</label>
                <input name="cantidadStock" type="number" min="0" value="${r.cantidadStock}" required class="input-large">
            </div>
            <div class="quick-stock-actions">
                <button type="button" class="btn btn-sm btn-outline" data-add="5">+5</button>
                <button type="button" class="btn btn-sm btn-outline" data-add="10">+10</button>
                <button type="button" class="btn btn-sm btn-outline" data-add="25">+25</button>
            </div>
        </form>
    `, `<button class="btn btn-primary" id="btn-save-stock" data-rate-sensitive>Actualizar stock</button>`);

    const input = document.querySelector('#form-quick-stock input');
    document.querySelectorAll('[data-add]').forEach(btn => {
        btn.addEventListener('click', () => {
            input.value = parseInt(input.value, 10) + parseInt(btn.dataset.add, 10);
        });
    });

    document.getElementById('btn-save-stock').addEventListener('click', async (e) => {
        const val = parseInt(input.value, 10);
        try {
            await API.withTrigger(e.currentTarget, () => API.updateRepuestoStock(id, val));
            UI.toast('Stock actualizado', 'success');
            UI.closeModal();
            loadRepuestos();
        } catch { /* handled */ }
    });
}

async function openRepuestoModal(id = null) {
    let r = { codigo: '', descripcion: '', categoria: '', cantidadStock: 0, stockMinimo: 10, precioUnitario: 0, activo: true };
    if (id) r = repuestosCache.find(x => x.id === id) || r;
    const isEdit = !!id;

    UI.openModal(isEdit ? 'Editar repuesto' : 'Nuevo repuesto', `
        <form id="form-repuesto">
            ${!isEdit ? `<div class="form-group"><label>Código *</label><input name="codigo" required></div>` : ''}
            <div class="form-grid">
                <div class="form-group"><label>Descripción *</label><input name="descripcion" value="${Utils.escapeHtml(r.descripcion)}" required></div>
                <div class="form-group"><label>Categoría *</label><input name="categoria" value="${Utils.escapeHtml(r.categoria)}" required></div>
                <div class="form-group"><label>Stock actual *</label><input name="cantidadStock" type="number" min="0" value="${r.cantidadStock}" required></div>
                <div class="form-group"><label>Stock mínimo *</label><input name="stockMinimo" type="number" min="0" value="${Utils.getStockMinimo(r)}" required></div>
                <div class="form-group"><label>Precio unitario *</label><input name="precioUnitario" type="number" step="0.01" min="0" value="${r.precioUnitario}" required></div>
                ${isEdit ? `<div class="form-group"><label><input type="checkbox" name="activo" ${r.activo ? 'checked' : ''}> Activo</label></div>` : ''}
            </div>
        </form>
    `, `<button class="btn btn-primary" id="btn-save-repuesto" data-rate-sensitive>Guardar</button>`);

    document.getElementById('btn-save-repuesto').addEventListener('click', async (e) => {
        const form = document.getElementById('form-repuesto');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        const payload = {
            descripcion: form.descripcion.value,
            categoria: form.categoria.value,
            cantidadStock: parseInt(form.cantidadStock.value, 10),
            stockMinimo: parseInt(form.stockMinimo.value, 10),
            precioUnitario: parseFloat(form.precioUnitario.value),
        };
        try {
            await API.withTrigger(e.currentTarget, async () => {
                if (isEdit) {
                    await API.updateRepuesto(id, { ...payload, activo: form.activo.checked });
                } else {
                    await API.createRepuesto({ ...payload, codigo: form.codigo.value });
                }
            });
            UI.toast('Repuesto guardado', 'success');
            UI.closeModal();
            loadRepuestos();
        } catch { /* handled */ }
    });
}

async function deleteRepuesto(id) {
    if (!confirm('¿Dar de baja este repuesto?')) return;
    try {
        await API.deleteRepuesto(id);
        UI.toast('Repuesto dado de baja', 'success');
        loadRepuestos();
    } catch { /* handled */ }
}
