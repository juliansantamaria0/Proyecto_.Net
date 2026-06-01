/** Resuelve la URL de la API: local (mismo host o :5192) vs producción (Netlify → Railway). */
function resolveApiBaseUrl() {
    const injected = typeof window.__ATM_API_BASE__ === 'string' ? window.__ATM_API_BASE__.trim() : '';
    if (injected) return injected.replace(/\/$/, '');

    const { hostname, port, protocol, origin } = window.location;
    const isLocal = hostname === 'localhost' || hostname === '127.0.0.1';

    if (isLocal) {
        // SPA servida por la API en 5192/7197 → mismo origen
        if (port === '5192' || port === '7197') return `${origin}/api`;
        // Front en otro puerto (Live Server, etc.) → API local por defecto
        return 'http://localhost:5192/api';
    }

    console.warn(
        '[AutoTallerManager] API_BASE_URL no configurada. Defina API_BASE_URL en Netlify o window.__ATM_API_BASE__.'
    );
    return `${origin}/api`;
}

/** Configuración global de la aplicación */
export const CONFIG = {
    API_BASE_URL: resolveApiBaseUrl(),
    DEFAULT_PAGE_SIZE: 10,
    LOW_STOCK_THRESHOLD: 10,
    TOKEN_KEY: 'atm_token',
    USER_KEY: 'atm_user',
    REMEMBER_KEY: 'atm_remember',
    RATE_LIMIT_COOLDOWN_MS: 8000,
};

export const ROLES = {
    ADMIN: 'Admin',
    MECANICO: 'Mecanico',
    RECEPCIONISTA: 'Recepcionista',
    CLIENTE: 'Cliente',
};

export const ROUTE_PERMISSIONS = {
    dashboard: [ROLES.ADMIN, ROLES.MECANICO, ROLES.RECEPCIONISTA],
    'dashboard-cliente': [ROLES.CLIENTE],
    clientes: [ROLES.ADMIN, ROLES.RECEPCIONISTA],
    'mi-perfil': [ROLES.CLIENTE],
    vehiculos: [ROLES.ADMIN, ROLES.RECEPCIONISTA, ROLES.MECANICO],
    'mis-vehiculos': [ROLES.CLIENTE],
    ordenes: [ROLES.ADMIN, ROLES.RECEPCIONISTA],
    'mis-ordenes': [ROLES.CLIENTE],
    'panel-mecanico': [ROLES.ADMIN, ROLES.MECANICO],
    repuestos: [ROLES.ADMIN],
    facturas: [ROLES.ADMIN, ROLES.MECANICO],
    'mis-facturas': [ROLES.CLIENTE],
    usuarios: [ROLES.ADMIN],
    auditorias: [ROLES.ADMIN],
};

export const DIAS_POR_TIPO_SERVICIO = { 0: 1, 1: 3, 2: 2 };

export const ESTADO_ORDEN = {
    0: { label: 'Pendiente', class: 'badge-pending' },
    1: { label: 'En proceso', class: 'badge-progress' },
    2: { label: 'Completada', class: 'badge-done' },
    3: { label: 'Cancelada', class: 'badge-cancel' },
};

export const TIPO_SERVICIO = {
    0: 'Mantenimiento preventivo',
    1: 'Reparación',
    2: 'Diagnóstico',
};

export const TIPO_ACCION_AUDITORIA = {
    0: 'Creación', 1: 'Modificación', 2: 'Eliminación', 3: 'Consulta',
};

export const ROL_LABELS = {
    Admin: 'Administrador',
    Mecanico: 'Mecánico',
    Recepcionista: 'Recepcionista',
    Cliente: 'Cliente',
    0: 'Administrador',
    1: 'Mecánico',
    2: 'Recepcionista',
    3: 'Cliente',
};
