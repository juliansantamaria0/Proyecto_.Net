import { API } from '../api.js';
import { CONFIG, TIPO_ACCION_AUDITORIA } from '../config.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

let state = { page: 1, entidad: '', usuarioId: '' };

export async function renderAuditorias() {
    if (!document.getElementById('auditorias-timeline')?.dataset.bound) {
        bindEvents();
        document.getElementById('auditorias-timeline').dataset.bound = 'true';
    }
    await loadAuditorias();
}

function bindEvents() {
    document.getElementById('btn-filter-auditorias')?.addEventListener('click', () => {
        state.entidad = document.getElementById('filter-auditoria-entidad').value;
        state.usuarioId = document.getElementById('filter-auditoria-usuario').value;
        state.page = 1;
        loadAuditorias();
    });
}

async function loadAuditorias() {
    UI.setLoading(true);
    try {
        const { data, totalCount } = await API.getAuditorias(
            state.page, CONFIG.DEFAULT_PAGE_SIZE, state.entidad, state.usuarioId || null
        );
        const container = document.getElementById('auditorias-timeline');
        if (!data?.length) {
            container.innerHTML = '<p class="text-muted text-center">No hay registros de auditoría</p>';
        } else {
            container.innerHTML = data.map(a => `
                <div class="audit-item">
                    <div class="audit-icon audit-${a.tipoAccion}">
                        <i class="fa-solid ${getAuditIcon(a.tipoAccion)}"></i>
                    </div>
                    <div class="audit-content">
                        <p class="audit-title">
                            <strong>${Utils.escapeHtml(a.usuarioNombre)}</strong>
                            ${getAccionLabel(a.tipoAccion)} <em>${Utils.escapeHtml(a.entidad)}</em> #${a.entidadId}
                        </p>
                        ${a.detalle ? `<p class="audit-detail">${Utils.escapeHtml(a.detalle)}</p>` : ''}
                        <time class="audit-time">${Utils.formatDateTime(a.fechaAccion)}</time>
                    </div>
                </div>
            `).join('');
        }
        UI.renderPagination('auditorias-pagination', { page: state.page, pageSize: CONFIG.DEFAULT_PAGE_SIZE, totalCount }, p => {
            state.page = p; loadAuditorias();
        });
    } finally {
        UI.setLoading(false);
    }
}

function getAccionLabel(tipo) {
    return TIPO_ACCION_AUDITORIA[tipo] || 'Acción';
}

function getAuditIcon(tipo) {
    return { 0: 'fa-plus', 1: 'fa-pen', 2: 'fa-trash', 3: 'fa-eye' }[tipo] || 'fa-circle';
}
