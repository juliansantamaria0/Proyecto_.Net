const CACHE_NAME = 'autotaller-v2';

const ASSETS_TO_CACHE = [
  './',
  './index.html',
  './manifest.json',
  './css/styles.css',
  './js/app.js',
  './js/router.js',
  './js/auth.js',
  './js/config.js',
  './js/api.js',
  './js/ui.js',
  './js/utils.js',
  './js/views/login.js',
  './js/views/register.js',
  './js/views/dashboard.js',
  './js/views/dashboard-cliente.js',
  './js/views/clientes.js',
  './js/views/ordenes.js',
  './js/views/repuestos.js',
  './js/views/facturas.js',
  './js/views/vehiculos.js',
  './js/views/panel-mecanico.js',
  './js/views/usuarios.js',
  './js/views/auditorias.js',
  './js/views/mi-perfil.js',
  './js/views/mis-vehiculos.js',
  './js/views/mis-ordenes.js',
  './js/views/mis-facturas.js',
  './icons/icon-192.png',
  './icons/icon-512.png',
];

function cacheKey(request) {
  const url = new URL(request.url);
  url.search = '';
  return url.href;
}

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => cache.addAll(ASSETS_TO_CACHE))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((names) => Promise.all(
        names.filter((name) => name !== CACHE_NAME).map((name) => caches.delete(name))
      ))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const requestUrl = new URL(event.request.url);

  if (requestUrl.pathname.includes('/api/') || requestUrl.origin !== self.location.origin) {
    return;
  }

  event.respondWith(
    fetch(event.request)
      .then((response) => {
        if (response && response.status === 200 && response.type === 'basic') {
          const responseToCache = response.clone();
          caches.open(CACHE_NAME).then((cache) => {
            cache.put(cacheKey(event.request), responseToCache);
          });
        }
        return response;
      })
      .catch(() => caches.match(cacheKey(event.request)).then((cached) => {
        if (cached) return cached;
        if (event.request.mode === 'navigate') {
          return caches.match('./index.html');
        }
      }))
  );
});
