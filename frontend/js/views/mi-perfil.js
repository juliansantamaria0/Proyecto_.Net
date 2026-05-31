import { API } from '../api.js';
import { Auth } from '../auth.js';
import { UI } from '../ui.js';
import { Utils } from '../utils.js';

export async function renderMiPerfil() {
    UI.setLoading(true);
    try {
        const { data: perfil } = await API.getMiPerfil();
        document.getElementById('perfil-nombre').textContent = perfil.nombre;
        document.getElementById('perfil-correo').textContent = perfil.correo;
        document.getElementById('perfil-telefono').textContent = perfil.telefono;
        document.getElementById('perfil-vehiculos-count').textContent = perfil.cantidadVehiculos ?? 0;

        const user = Auth.getUser();
        document.getElementById('user-name').textContent = user?.nombre || perfil.nombre;
        document.getElementById('user-role').textContent = 'Cliente';
    } catch {
        UI.toast('No se pudo cargar su perfil', 'error');
    } finally {
        UI.setLoading(false);
    }
}
