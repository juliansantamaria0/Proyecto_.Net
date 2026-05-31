import { Auth } from './auth.js';
import { ROUTE_PERMISSIONS, ROLES } from './config.js';
import { UI } from './ui.js';

const views = {};

export function registerView(name, renderFn) {
    views[name] = renderFn;
}

export function navigateTo(route) {
    window.location.hash = `#${route}`;
}

export function getCurrentRoute() {
    return window.location.hash.replace('#', '') || '';
}

const PAGE_TITLES = {
    dashboard: 'Dashboard',
    'dashboard-cliente': 'Mi Panel',
    clientes: 'Clientes y Vehículos',
    'mi-perfil': 'Mi Perfil',
    vehiculos: 'Búsqueda de Vehículos',
    'mis-vehiculos': 'Mis Vehículos',
    ordenes: 'Órdenes de Servicio',
    'mis-ordenes': 'Mis Órdenes',
    'panel-mecanico': 'Mi Panel de Trabajo',
    repuestos: 'Inventario',
    facturas: 'Facturación',
    'mis-facturas': 'Mis Facturas',
    usuarios: 'Gestión de Usuarios',
    auditorias: 'Auditoría del Sistema',
    register: 'Registro',
};

function getDefaultRoute() {
    if (Auth.isMecanico()) return 'panel-mecanico';
    if (Auth.isCliente()) return 'dashboard-cliente';
    return 'dashboard';
}

function hideAllViews() {
    document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
    document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
}

function showLayout(show) {
    document.getElementById('app-layout').classList.toggle('hidden', !show);
    document.getElementById('login-view').classList.toggle('active', !show);
    document.getElementById('register-view')?.classList.toggle('active', false);
}

export async function router() {
    let route = getCurrentRoute();

    if (route === 'register') {
        showLayout(false);
        document.getElementById('login-view')?.classList.remove('active');
        document.getElementById('register-view')?.classList.add('active');
        const { renderRegister } = await import('./views/register.js');
        renderRegister();
        return;
    }

    if (!Auth.isAuthenticated()) {
        showLayout(false);
        document.getElementById('register-view')?.classList.remove('active');
        document.getElementById('login-view').classList.add('active');
        const { renderLogin } = await import('./views/login.js');
        renderLogin();
        return;
    }

    if (!route || route === 'login') {
        navigateTo(getDefaultRoute());
        return;
    }

    showLayout(true);
    document.getElementById('register-view')?.classList.remove('active');

    const role = Auth.getRole();
    const allowed = ROUTE_PERMISSIONS[route];
    if (allowed && !allowed.includes(role)) {
        UI.toast('No tiene permiso para acceder a esta sección.', 'warning');
        navigateTo(getDefaultRoute());
        return;
    }

    hideAllViews();

    const viewEl = document.getElementById(`view-${route}`);
    if (!viewEl) {
        navigateTo(getDefaultRoute());
        return;
    }

    viewEl.classList.add('active');
    document.querySelector(`.nav-link[data-route="${route}"]`)?.classList.add('active');
    document.getElementById('page-title').textContent = PAGE_TITLES[route] || 'AutoTallerManager';
    document.title = `${PAGE_TITLES[route] || 'AutoTaller'} — AutoTallerManager`;

    UI.applyRoleGuard(role);

    const renderFn = views[route];
    if (renderFn) await renderFn();
}

export function initRouter() {
    window.addEventListener('hashchange', router);
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            UI.closeSidebar();
            navigateTo(link.dataset.route);
        });
    });
    document.getElementById('btn-logout')?.addEventListener('click', () => {
        Auth.clearSession();
        navigateTo('login');
    });
    router();
}
