import { CONFIG } from './config.js';
import { Auth } from './auth.js';
import { UI } from './ui.js';

export const API = {
    lastTrigger: null,

    async request(endpoint, options = {}) {
        if (UI.isRateLimited()) {
            UI.applyRateLimitCooldown(this.lastTrigger);
            throw new Error('Rate limit cooldown');
        }

        const url = `${CONFIG.API_BASE_URL}${endpoint}`;
        const headers = { 'Content-Type': 'application/json', ...options.headers };
        const token = Auth.getToken();
        if (token) headers['Authorization'] = `Bearer ${token}`;

        let response;
        try {
            response = await fetch(url, { ...options, headers });
        } catch {
            UI.toast('No se pudo conectar con el servidor.', 'error');
            throw new Error('Network error');
        }

        if (response.status === 429) {
            UI.applyRateLimitCooldown(this.lastTrigger);
            throw new Error('Rate limit exceeded');
        }

        if (response.status === 401) {
            Auth.clearSession();
            UI.toast('Sesión expirada. Inicie sesión nuevamente.', 'warning');
            window.location.hash = '#login';
            throw new Error('Unauthorized');
        }

        if (response.status === 403) {
            const contentType = response.headers.get('content-type');
            let forbiddenData = null;
            if (contentType?.includes('application/json')) {
                try { forbiddenData = await response.json(); } catch { /* empty */ }
            }
            const msg = forbiddenData?.error || 'No tiene permiso para acceder a este recurso.';
            UI.toast(msg, 'warning');
            throw new Error('Forbidden');
        }

        if (response.status === 204) return { data: null, totalCount: 0 };

        const contentType = response.headers.get('content-type');
        let data = null;
        if (contentType?.includes('application/json')) data = await response.json();

        if (!response.ok) {
            const message = data?.error || data?.title || `Error ${response.status}`;
            if (response.status === 404) UI.toast('Recurso no encontrado.', 'error');
            else if (response.status >= 500) UI.toast('Error interno del servidor.', 'error');
            else UI.toast(message, 'error');
            throw new Error(message);
        }

        return {
            data,
            totalCount: parseInt(response.headers.get('X-Total-Count') || '0', 10),
        };
    },

    withTrigger(el, promiseFactory) {
        this.lastTrigger = el;
        return promiseFactory();
    },

    get(endpoint, params = {}) {
        const qs = new URLSearchParams();
        Object.entries(params).forEach(([k, v]) => {
            if (v !== null && v !== undefined && v !== '') qs.append(k, v);
        });
        const query = qs.toString();
        return this.request(query ? `${endpoint}?${query}` : endpoint);
    },

    post(endpoint, body) { return this.request(endpoint, { method: 'POST', body: JSON.stringify(body) }); },
    put(endpoint, body) { return this.request(endpoint, { method: 'PUT', body: JSON.stringify(body) }); },
    patch(endpoint, body) { return this.request(endpoint, { method: 'PATCH', body: JSON.stringify(body) }); },
    delete(endpoint) { return this.request(endpoint, { method: 'DELETE' }); },

    login(correo, password) { return this.post('/auth/login', { correo, password }); },
    register(payload) { return this.post('/usuarios/registrar', payload); },
    registerAuth(payload) { return this.post('/auth/register', payload); },

    getMiPerfil() { return this.request('/clientes/mi-perfil'); },
    createVehiculo(payload) { return this.post('/vehiculos', payload); },

    getClientes(page, pageSize, nombre) {
        return this.get('/clientes', { pageNumber: page, pageSize, nombre });
    },
    getCliente(id) { return this.request(`/clientes/${id}`); },
    registrarClienteConVehiculos(payload) { return this.post('/clientes/registrar-con-vehiculos', payload); },
    updateCliente(id, payload) { return this.put(`/clientes/${id}`, payload); },
    deleteCliente(id) { return this.delete(`/clientes/${id}`); },

    getVehiculos(page, pageSize, clienteId, vin, marca) {
        return this.get('/vehiculos', { pageNumber: page, pageSize, clienteId: clienteId ?? '', vin: vin ?? '', });
    },

    getOrdenes(params = {}) {
        return this.get('/ordenesservicio', {
            pageNumber: params.page || 1,
            pageSize: params.pageSize || CONFIG.DEFAULT_PAGE_SIZE,
            estado: params.estado ?? '',
            mecanicoId: params.mecanicoId ?? '',
            clienteId: params.clienteId ?? '',
            fechaDesde: params.fechaDesde ?? '',
            fechaHasta: params.fechaHasta ?? '',
        });
    },
    getOrden(id) { return this.request(`/ordenesservicio/${id}`); },
    createOrden(payload) { return this.post('/ordenesservicio', payload); },
    updateOrdenTrabajo(id, payload) { return this.put(`/ordenesservicio/${id}/trabajo`, payload); },
    cancelarOrden(id) { return this.put(`/ordenesservicio/${id}/cancelar`, {}); },

    getRepuestos(page, pageSize, filters = {}) {
        return this.get('/repuestos', {
            pageNumber: page, pageSize,
            categoria: filters.categoria || '',
            descripcion: filters.descripcion || '',
            stockMinimo: filters.stockMinimo ?? '',
        });
    },
    createRepuesto(payload) { return this.post('/repuestos', payload); },
    updateRepuesto(id, payload) { return this.put(`/repuestos/${id}`, payload); },
    updateRepuestoStock(id, cantidadStock) { return this.patch(`/repuestos/${id}/stock`, { cantidadStock }); },
    deleteRepuesto(id) { return this.delete(`/repuestos/${id}`); },

    getFacturas(page, pageSize, filters = {}) {
        return this.get('/facturas', {
            pageNumber: page, pageSize,
            clienteId: filters.clienteId ?? '',
            ordenId: filters.ordenId ?? '',
            fechaDesde: filters.fechaDesde ?? '',
        });
    },
    getFactura(id) { return this.request(`/facturas/${id}`); },
    generarFactura(ordenServicioId) { return this.post('/facturas/generar', { ordenServicioId }); },

    getUsuarios(page, pageSize) { return this.get('/usuarios', { pageNumber: page, pageSize }); },
    createUsuario(payload) { return this.post('/usuarios', payload); },
    updateUsuario(id, payload) { return this.put(`/usuarios/${id}`, payload); },
    deleteUsuario(id) { return this.delete(`/usuarios/${id}`); },

    getAuditorias(page, pageSize, entidad, usuarioId) {
        return this.get('/auditorias', { pageNumber: page, pageSize, entidad: entidad ?? '', usuarioId: usuarioId ?? '' });
    },
};
