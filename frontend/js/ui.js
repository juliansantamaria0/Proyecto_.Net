import { Utils } from './utils.js';
import { CONFIG } from './config.js';

let rateLimitUntil = 0;

export const UI = {
    toast(message, type = 'info', duration = 4000) {
        const container = document.getElementById('toast-container');
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        const icon = { error: 'fa-circle-xmark', warning: 'fa-triangle-exclamation', success: 'fa-circle-check' }[type] || 'fa-circle-info';
        toast.innerHTML = `<i class="fa-solid ${icon}"></i><span>${Utils.escapeHtml(message)}</span>`;
        container.appendChild(toast);
        requestAnimationFrame(() => toast.classList.add('show'));
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, duration);
    },

    isRateLimited() {
        return Date.now() < rateLimitUntil;
    },

    applyRateLimitCooldown(triggerEl = null) {
        rateLimitUntil = Date.now() + CONFIG.RATE_LIMIT_COOLDOWN_MS;
        this.toast('Demasiadas solicitudes. Por favor, espera unos segundos e intenta de nuevo.', 'warning', 6000);
        if (triggerEl) this.freezeButton(triggerEl, CONFIG.RATE_LIMIT_COOLDOWN_MS);
        document.querySelectorAll('[data-rate-sensitive]').forEach(btn => {
            if (!btn.disabled) this.freezeButton(btn, CONFIG.RATE_LIMIT_COOLDOWN_MS);
        });
    },

    freezeButton(btn, ms = CONFIG.RATE_LIMIT_COOLDOWN_MS) {
        if (!btn) return;
        const original = btn.innerHTML;
        btn.disabled = true;
        btn.classList.add('btn-frozen');
        const remaining = Math.ceil(ms / 1000);
        btn.dataset.frozenOriginal = original;
        btn.innerHTML = `<i class="fa-solid fa-hourglass-half"></i> Espere ${remaining}s...`;
        let left = remaining;
        const interval = setInterval(() => {
            left -= 1;
            if (left <= 0) {
                clearInterval(interval);
                btn.disabled = false;
                btn.classList.remove('btn-frozen');
                btn.innerHTML = btn.dataset.frozenOriginal || original;
                delete btn.dataset.frozenOriginal;
            } else {
                btn.innerHTML = `<i class="fa-solid fa-hourglass-half"></i> Espere ${left}s...`;
            }
        }, 1000);
    },

    openModal(title, bodyHtml, footerHtml = '') {
        document.getElementById('modal-title').textContent = title;
        document.getElementById('modal-body').innerHTML = bodyHtml;
        document.getElementById('modal-footer').innerHTML = footerHtml;
        document.getElementById('modal-overlay').classList.add('active');
        document.body.style.overflow = 'hidden';
    },

    closeModal() {
        document.getElementById('modal-overlay').classList.remove('active');
        document.body.style.overflow = '';
    },

    renderPagination(containerId, { page, pageSize, totalCount }, onPageChange) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
        const start = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
        const end = Math.min(page * pageSize, totalCount);

        const maxVisible = 5;
        let startPage = Math.max(1, page - Math.floor(maxVisible / 2));
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);
        startPage = Math.max(1, endPage - maxVisible + 1);

        let pageButtons = '';
        for (let p = startPage; p <= endPage; p++) {
            pageButtons += `<button class="btn btn-sm ${p === page ? 'btn-primary' : 'btn-outline'}" data-page-num="${p}">${p}</button>`;
        }

        container.innerHTML = `
            <div class="pagination-info">Mostrando ${start}–${end} de ${totalCount}</div>
            <div class="pagination-controls">
                <button class="btn btn-sm btn-outline" data-page="prev" ${page <= 1 ? 'disabled' : ''}>
                    <i class="fa-solid fa-chevron-left"></i> Anterior
                </button>
                <div class="pagination-numbers">${pageButtons}</div>
                <button class="btn btn-sm btn-outline" data-page="next" ${page >= totalPages ? 'disabled' : ''}>
                    Siguiente <i class="fa-solid fa-chevron-right"></i>
                </button>
            </div>
        `;

        container.querySelector('[data-page="prev"]')?.addEventListener('click', () => onPageChange(page - 1));
        container.querySelector('[data-page="next"]')?.addEventListener('click', () => onPageChange(page + 1));
        container.querySelectorAll('[data-page-num]').forEach(btn => {
            btn.addEventListener('click', () => onPageChange(parseInt(btn.dataset.pageNum, 10)));
        });
    },

    setLoading(show) {
        document.getElementById('loading-overlay')?.classList.toggle('active', show);
    },

    applyRoleGuard(role) {
        document.querySelectorAll('[data-roles]').forEach(el => {
            const allowed = el.dataset.roles.split(',').map(r => r.trim());
            el.style.display = allowed.includes(role) ? '' : 'none';
        });
    },

    closeSidebar() {
        document.getElementById('sidebar')?.classList.remove('open');
        document.getElementById('sidebar-backdrop')?.classList.remove('active');
        document.body.classList.remove('sidebar-open');
    },

    toggleSidebar() {
        const willOpen = !document.getElementById('sidebar')?.classList.contains('open');
        document.getElementById('sidebar')?.classList.toggle('open', willOpen);
        document.getElementById('sidebar-backdrop')?.classList.toggle('active', willOpen);
        document.body.classList.toggle('sidebar-open', willOpen);
    },
};

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('modal-overlay')?.addEventListener('click', (e) => {
        if (e.target.id === 'modal-overlay') UI.closeModal();
    });
    document.getElementById('modal-close')?.addEventListener('click', () => UI.closeModal());
});
