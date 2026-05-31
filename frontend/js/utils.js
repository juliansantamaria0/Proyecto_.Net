import { CONFIG, DIAS_POR_TIPO_SERVICIO, ESTADO_ORDEN } from './config.js';

export const Utils = {
    formatCurrency(value) {
        return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(value ?? 0);
    },

    formatDate(dateStr) {
        if (!dateStr) return '—';
        return new Date(dateStr).toLocaleDateString('es-MX', {
            year: 'numeric', month: 'short', day: 'numeric',
        });
    },

    formatDateTime(dateStr) {
        if (!dateStr) return '—';
        return new Date(dateStr).toLocaleString('es-MX', {
            year: 'numeric', month: 'short', day: 'numeric',
            hour: '2-digit', minute: '2-digit',
        });
    },

    todayISO() {
        return new Date().toISOString().split('T')[0];
    },

    escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str ?? '';
        return div.innerHTML;
    },

    debounce(fn, delay = 350) {
        let timer;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => fn(...args), delay);
        };
    },

    isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    },

    calcularFechaEstimadaEntrega(tipoServicio, fechaIngreso = new Date(), complejidad = 1) {
        const diasBase = DIAS_POR_TIPO_SERVICIO[tipoServicio] ?? 2;
        const factor = complejidad === 2 ? 1.5 : 1;
        const dias = Math.max(1, Math.ceil(diasBase * factor));
        const fecha = new Date(fechaIngreso);
        fecha.setDate(fecha.getDate() + dias);
        return fecha;
    },

    getStockMinimo(repuesto) {
        return repuesto.stockMinimo ?? repuesto.StockMinimo ?? CONFIG.LOW_STOCK_THRESHOLD;
    },

    getStockLevel(repuesto) {
        const min = this.getStockMinimo(repuesto);
        const stock = repuesto.cantidadStock ?? 0;
        if (stock <= 0) return 'critical';
        if (stock <= min) return 'low';
        if (stock <= min * 1.5) return 'warning';
        return 'ok';
    },

    normalizeRol(rol) {
        const map = { 0: 'Admin', 1: 'Mecanico', 2: 'Recepcionista', 3: 'Cliente' };
        return map[rol] ?? rol;
    },

    getEstadoOrden(estado) {
        return ESTADO_ORDEN[estado] || { label: estado, class: '' };
    },
};
