import { registerView, initRouter } from './router.js';
import { UI } from './ui.js';
import { renderLogin } from './views/login.js';
import { renderDashboard } from './views/dashboard.js';
import { renderDashboardCliente } from './views/dashboard-cliente.js';
import { renderClientes } from './views/clientes.js';
import { renderOrdenes } from './views/ordenes.js';
import { renderRepuestos } from './views/repuestos.js';
import { renderFacturas } from './views/facturas.js';
import { renderVehiculos } from './views/vehiculos.js';
import { renderPanelMecanico } from './views/panel-mecanico.js';
import { renderUsuarios } from './views/usuarios.js';
import { renderAuditorias } from './views/auditorias.js';
import { renderMiPerfil } from './views/mi-perfil.js';
import { renderMisVehiculos } from './views/mis-vehiculos.js';
import { renderMisOrdenes } from './views/mis-ordenes.js';
import { renderMisFacturas } from './views/mis-facturas.js';

registerView('dashboard', renderDashboard);
registerView('dashboard-cliente', renderDashboardCliente);
registerView('mi-perfil', renderMiPerfil);
registerView('mis-vehiculos', renderMisVehiculos);
registerView('mis-ordenes', renderMisOrdenes);
registerView('mis-facturas', renderMisFacturas);
registerView('clientes', renderClientes);
registerView('vehiculos', renderVehiculos);
registerView('ordenes', renderOrdenes);
registerView('panel-mecanico', renderPanelMecanico);
registerView('repuestos', renderRepuestos);
registerView('facturas', renderFacturas);
registerView('usuarios', renderUsuarios);
registerView('auditorias', renderAuditorias);

document.getElementById('sidebar-toggle')?.addEventListener('click', () => UI.toggleSidebar());
document.getElementById('sidebar-backdrop')?.addEventListener('click', () => UI.closeSidebar());

window.addEventListener('resize', () => {
    if (window.innerWidth > 768) UI.closeSidebar();
});

document.addEventListener('DOMContentLoaded', () => {
    initRouter();
});

export { renderLogin };
