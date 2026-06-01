/**
 * Genera js/env.js en el build de Netlify.
 * Variable en Netlify: API_BASE_URL = https://tu-api.up.railway.app
 * (con o sin /api al final)
 */
import { writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
let apiBase = (process.env.API_BASE_URL || '').trim().replace(/\/$/, '');

if (!apiBase) {
    console.warn(
        '[generate-env] API_BASE_URL no definida. Configure en Netlify → Environment variables.'
    );
} else if (!apiBase.endsWith('/api')) {
    apiBase = `${apiBase}/api`;
}

const outPath = join(__dirname, '..', 'js', 'env.js');
const content = `// Generado en build de Netlify — no editar en producción
window.__ATM_API_BASE__ = ${JSON.stringify(apiBase)};
`;

writeFileSync(outPath, content);
console.log('[generate-env] OK →', outPath, '→', apiBase || '(vacío)');
