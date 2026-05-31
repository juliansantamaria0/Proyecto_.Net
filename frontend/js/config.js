/** Configuración global de la aplicación */
export const CONFIG = {
    API_BASE_URL: `${window.location.origin}/api`,
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
