import { CONFIG, ROLES } from './config.js';

const ROLE_FROM_CLAIM = {
    Admin: ROLES.ADMIN,
    Mecanico: ROLES.MECANICO,
    Recepcionista: ROLES.RECEPCIONISTA,
    Cliente: ROLES.CLIENTE,
    0: ROLES.ADMIN,
    1: ROLES.MECANICO,
    2: ROLES.RECEPCIONISTA,
    3: ROLES.CLIENTE,
};

function decodeJwt(token) {
    try {
        const payload = token.split('.')[1];
        return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    } catch {
        return null;
    }
}

function getRememberPreference() {
    return localStorage.getItem(CONFIG.REMEMBER_KEY) !== 'false';
}

function getStorage() {
    return getRememberPreference() ? localStorage : sessionStorage;
}

function normalizeRole(roleClaim) {
    if (roleClaim == null || roleClaim === '') return null;
    if (typeof roleClaim === 'string') return ROLE_FROM_CLAIM[roleClaim] ?? roleClaim;
    return ROLE_FROM_CLAIM[roleClaim] ?? String(roleClaim);
}

export const Auth = {
    getToken() {
        return getStorage().getItem(CONFIG.TOKEN_KEY)
            || localStorage.getItem(CONFIG.TOKEN_KEY)
            || sessionStorage.getItem(CONFIG.TOKEN_KEY);
    },

    setSession(token, user, remember = true) {
        Auth.clearSession();
        localStorage.setItem(CONFIG.REMEMBER_KEY, remember ? 'true' : 'false');
        const storage = remember ? localStorage : sessionStorage;

        const payload = decodeJwt(token);
        if (payload?.sub) user.id = parseInt(payload.sub, 10);
        if (payload?.email && !user.correo) user.correo = payload.email;

        const roleClaim = payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? payload?.role;
        const normalized = normalizeRole(roleClaim);
        if (normalized) user.rol = normalized;

        const clienteClaim = payload?.ClienteId;
        if (clienteClaim) user.clienteId = parseInt(clienteClaim, 10);
        if (user.clienteId == null && user.ClienteId != null) user.clienteId = user.ClienteId;

        storage.setItem(CONFIG.TOKEN_KEY, token);
        storage.setItem(CONFIG.USER_KEY, JSON.stringify(user));
    },

    clearSession() {
        [localStorage, sessionStorage].forEach(s => {
            s.removeItem(CONFIG.TOKEN_KEY);
            s.removeItem(CONFIG.USER_KEY);
        });
        localStorage.removeItem(CONFIG.REMEMBER_KEY);
    },

    isAuthenticated() {
        const token = this.getToken();
        if (!token) return false;
        const payload = decodeJwt(token);
        if (!payload) return false;
        if (payload.exp && payload.exp * 1000 < Date.now()) {
            this.clearSession();
            return false;
        }
        return true;
    },

    getUser() {
        try {
            const raw = getStorage().getItem(CONFIG.USER_KEY)
                || localStorage.getItem(CONFIG.USER_KEY)
                || sessionStorage.getItem(CONFIG.USER_KEY);
            return JSON.parse(raw || 'null');
        } catch {
            return null;
        }
    },

    getRole() { return this.getUser()?.rol || null; },
    getUserId() { return this.getUser()?.id || null; },
    getClienteId() { return this.getUser()?.clienteId || null; },

    hasRole(...roles) {
        const role = this.getRole();
        return role && roles.includes(role);
    },
    isAdmin() { return this.getRole() === ROLES.ADMIN; },
    isMecanico() { return this.getRole() === ROLES.MECANICO; },
    isRecepcionista() { return this.getRole() === ROLES.RECEPCIONISTA; },
    isCliente() { return this.getRole() === ROLES.CLIENTE; },
};
