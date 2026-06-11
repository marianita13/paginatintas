// ════════════════════════════════════════
//  CONFIGURACIÓN
// ════════════════════════════════════════
const API_URL = 'http://localhost:5115/api';

// ════════════════════════════════════════
//  ESTADO GLOBAL
// ════════════════════════════════════════
let token       = localStorage.getItem('token') || null;
let currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
let allColors   = []; // cache para filtrado local

// Diccionario Pantone → HEX (se carga una vez al inicio)
// Fuente: brettapeters/pantones (coated + uncoated)
let pantoneMap = {}; // { "100 C": "#f6d155", "100 U": "#f4d961", ... }

async function cargarPantoneMap() {
  if (Object.keys(pantoneMap).length > 0) return; // Ya cargado
  
  try {
    const [resC, resU] = await Promise.all([
      fetch('https://raw.githubusercontent.com/brettapeters/pantones/master/pantone-coated.json'),
      fetch('https://raw.githubusercontent.com/brettapeters/pantones/master/pantone-uncoated.json')
    ]);

    const coated   = resC.ok   ? await resC.json()   : {};
    const uncoated = resU.ok   ? await resU.json()   : {};

    const todosLosColores = { ...coated, ...uncoated };

    Object.keys(todosLosColores).forEach(nombrePantone => {
      const elColor = todosLosColores[nombrePantone];

      // Verificamos que el objeto exista y tenga la propiedad 'hex'
      if (elColor && elColor.hex) {
        // Forzamos a que sea string y limpiamos espacios
        const hexString = String(elColor.hex).trim();
        
        // Formateamos correctamente el HEX con su '#'
        const hexFinal = hexString.startsWith('#') ? hexString : '#' + hexString;
        
        // Guardamos en el mapa con la clave normalizada
        pantoneMap[normalizarPantone(nombrePantone)] = hexFinal;
      }
    });

    console.log(`[Pantone] Mapa cargado exitosamente: ${Object.keys(pantoneMap).length} colores.`);
  } catch (e) {
    console.warn('[Pantone] No se pudo cargar el mapa de colores:', e.message);
  }
}

// Normaliza "PANTONE 100 C", "Pantone 100C", "100 C" → "100 C"
function normalizarPantone(nombre) {
  return nombre
    .replace(/pantone\s*/i, '')
    .trim()
    .toUpperCase();
}

// Busca el hex en el mapa Pantone dado el nombre de la fórmula
function hexDesdePantone(nombreColor) {
  if (!nombreColor) return null;
  const key = normalizarPantone(nombreColor);
  // Intento exacto
  if (pantoneMap[key]) return pantoneMap[key];
  // Sin sufijo C/U: "100 C" → "100"
  const sinSufijo = key.replace(/\s+[CU]$/, '').trim();
  if (pantoneMap[sinSufijo + ' C']) return pantoneMap[sinSufijo + ' C'];
  if (pantoneMap[sinSufijo + ' U']) return pantoneMap[sinSufijo + ' U'];
  // Búsqueda parcial como último recurso
  const encontrado = Object.keys(pantoneMap).find(k => k.startsWith(sinSufijo));
  return encontrado ? pantoneMap[encontrado] : null;
}

// ════════════════════════════════════════
//  UTILIDADES HTTP
// ════════════════════════════════════════
async function apiFetch(endpoint, method = 'GET', body = null) {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const opts = { method, headers };
  if (body) opts.body = JSON.stringify(body);

  const res = await fetch(`${API_URL}/${endpoint}`, opts);

  if (res.status === 401) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      headers['Authorization'] = `Bearer ${token}`;
      const retry = await fetch(`${API_URL}/${endpoint}`, { method, headers, body: opts.body });
      return retry;
    } else {
      handleLogout();
      throw new Error('Sesión expirada');
    }
  }
  return res;
}

async function tryRefreshToken() {
  const refreshToken = localStorage.getItem('refreshToken');
  if (!refreshToken) return false;
  try {
    const res = await fetch(`${API_URL}/Usuario/refresh-token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(refreshToken)
    });
    if (!res.ok) return false;
    const data = await res.json();
    token = data.token;
    localStorage.setItem('token', token);
    localStorage.setItem('refreshToken', data.refreshToken);
    return true;
  } catch { return false; }
}

// ════════════════════════════════════════
//  INICIO DE APP
// ════════════════════════════════════════
window.addEventListener('load', () => {
  setTimeout(() => document.getElementById('loading').classList.add('hidden'), 600);

  // Aplicar tema guardado
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'dark') applyTheme('dark');

  if (token && currentUser) showMainPage();
});

document.addEventListener('keydown', (e) => {
  const loginPage = document.getElementById('login-page');
  if (e.key === 'Enter' && loginPage && getComputedStyle(loginPage).display !== 'none') {
    handleLogin();
  }
});

// ════════════════════════════════════════
//  TEMA (modo oscuro / claro)
// ════════════════════════════════════════
function toggleTheme() {
  const isDark = document.body.classList.contains('dark');
  applyTheme(isDark ? 'light' : 'dark');
}

function applyTheme(mode) {
  if (mode === 'dark') {
    document.body.classList.add('dark');
    document.getElementById('theme-label').textContent = 'Modo claro';
    document.querySelector('.theme-icon-light').style.display = 'none';
    document.querySelector('.theme-icon-dark').style.display  = 'flex';
    localStorage.setItem('theme', 'dark');
  } else {
    document.body.classList.remove('dark');
    document.getElementById('theme-label').textContent = 'Modo oscuro';
    document.querySelector('.theme-icon-light').style.display = 'flex';
    document.querySelector('.theme-icon-dark').style.display  = 'none';
    localStorage.setItem('theme', 'light');
  }
}

// ════════════════════════════════════════
//  SIDEBAR TOGGLE
// ════════════════════════════════════════
document.addEventListener('DOMContentLoaded', () => {
  const btn = document.getElementById('sidebar-toggle');
  if (btn) {
    btn.addEventListener('click', () => {
      document.getElementById('sidebar').classList.toggle('collapsed');
    });
  }
});

// ════════════════════════════════════════
//  LOGIN
// ════════════════════════════════════════
async function handleLogin() {
  const correo = document.getElementById('username').value.trim();
  const pass   = document.getElementById('password').value.trim();
  const btn    = document.querySelector('.btn-login');

  if (!correo || !pass) { showError('Completa todos los campos'); return; }

  btn.disabled = true;
  btn.textContent = 'Ingresando...';

  try {
    const res  = await fetch(`${API_URL}/Usuario/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ correo, password: pass })
    });
    const data = await res.json();

    if (!res.ok || !data.isAuthenticated) {
      showError(data.mensaje || 'Correo o contraseña incorrectos');
      return;
    }

    token       = data.token;
    currentUser = { nombre: data.nombre, correo: data.correo, rol: data.rol };
    localStorage.setItem('token', token);
    localStorage.setItem('refreshToken', data.refreshToken || '');
    localStorage.setItem('currentUser', JSON.stringify(currentUser));

    document.getElementById('error-msg').style.display = 'none';
    showMainPage();
  } catch (e) {
    showError('No se pudo conectar con el servidor. ¿Está corriendo el backend?');
  } finally {
    btn.disabled = false;
    btn.textContent = 'Iniciar Sesión';
  }
}

function showError(msg) {
  const err = document.getElementById('error-msg');
  err.textContent = '⚠ ' + msg;
  err.style.display = 'block';
  err.style.animation = 'none';
  void err.offsetWidth;
  err.style.animation = 'shake 0.4s ease';
}

// ════════════════════════════════════════
//  MAIN PAGE
// ════════════════════════════════════════
function showMainPage() {
  const loginPage = document.getElementById('login-page');
  const mainPage  = document.getElementById('main-page');

  if (currentUser) {
    const initials = currentUser.nombre.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
    document.getElementById('welcome-name').textContent    = currentUser.nombre;
    document.getElementById('user-initials').textContent   = initials;
    document.getElementById('user-role-label').textContent = currentUser.rol || '—';

    // Mostrar pestaña Usuarios si es Admin
    if ((currentUser.rol || '').toLowerCase() === 'admin') {
      document.querySelector('.nav-item-admin').style.display = 'flex';
    }
  }

  loginPage.style.opacity = '0';
  loginPage.style.transition = 'opacity 0.4s ease';
  setTimeout(() => {
    loginPage.style.display = 'none';
    mainPage.style.display  = 'flex';
    mainPage.style.opacity  = '0';
    mainPage.style.transition = 'opacity 0.4s ease';
    setTimeout(() => { mainPage.style.opacity = '1'; }, 20);
  }, 400);

  cargarColores();
}

// ════════════════════════════════════════
//  LOGOUT
// ════════════════════════════════════════
function handleLogout() {
  token = null; currentUser = null;
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('currentUser');

  const mainPage  = document.getElementById('main-page');
  const loginPage = document.getElementById('login-page');

  mainPage.style.opacity = '0';
  mainPage.style.transition = 'opacity 0.3s ease';
  setTimeout(() => {
    mainPage.style.display  = 'none';
    loginPage.style.display = 'flex';
    loginPage.style.opacity = '0';
    loginPage.style.transition = 'opacity 0.3s ease';
    document.getElementById('username').value = '';
    document.getElementById('password').value = '';
    document.getElementById('error-msg').style.display = 'none';
    setTimeout(() => { loginPage.style.opacity = '1'; }, 20);
  }, 300);
}

// ════════════════════════════════════════
//  NAVEGACIÓN ENTRE TABS
// ════════════════════════════════════════
const TAB_TITLES = {
  inicio:     ['Inicio',      'Catálogo de colores'],
  empresas:   ['Empresas',    'Empresas registradas'],
  inventario: ['Inventario',  'Tintas base y stock'],
  usuarios:   ['Usuarios',    'Gestión de usuarios'],
  config:     ['Configuración','Perfil y seguridad']
};

function switchTab(name, btn) {
  document.querySelectorAll('.nav-item').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));

  if (btn) btn.classList.add('active');

  const panel = document.getElementById('panel-' + name);
  if (panel) panel.classList.add('active');

  const [title, crumb] = TAB_TITLES[name] || [name, ''];
  document.getElementById('page-title').textContent      = title;
  document.getElementById('page-breadcrumb').textContent = crumb;

  if (name === 'inicio')     cargarColores();
  if (name === 'empresas')   cargarEmpresas();
  if (name === 'inventario') cargarInventario();
  if (name === 'usuarios')   cargarUsuarios();
  if (name === 'config')     cargarConfig();
}

// ════════════════════════════════════════
//  INICIO — GRILLA DE COLORES
//
//  NUEVA FUNCIÓN — requiere endpoint:
//    GET /api/Formula   → array de { id, nombreColor, codigoHex, idEmpresa? }
// ════════════════════════════════════════
async function cargarColores() {
  const grid = document.getElementById('color-grid');
  grid.innerHTML = '<div class="loading-state">Cargando colores...</div>';
  try {
    // Cargar mapa Pantone y fórmulas en paralelo
    const [, res] = await Promise.all([
      cargarPantoneMap(),
      apiFetch('Formula')
    ]);
    const formulas = await res.json();
    if (formulas.length) console.log('[DEBUG] Primer formula:', formulas[0]);
    allColors = formulas;
    renderColorGrid(formulas);
  } catch (e) {
    grid.innerHTML = '<div class="error-cell">Error cargando colores: ' + e.message + '</div>';
  }
}

function renderColorGrid(formulas) {
  const grid = document.getElementById('color-grid');
  if (!formulas.length) {
    grid.innerHTML = '<div class="loading-state">No hay colores registrados.</div>';
    return;
  }
  grid.innerHTML = formulas.map(f => {
    // El campo puede venir como codigoHex, CodigoHex, codigo_hex, hexColor, etc.
    const hex = resolverHex(f);
    const hexDisplay = hex !== 'transparent' ? hex.toUpperCase() : 'Sin hex';
    // Texto oscuro o claro según luminancia del fondo
    const textColor = hex !== 'transparent' ? colorTextContrast(hex) : '#666';
    return `
      <div class="color-chip" onclick="abrirModalMezcla(${f.id}, '${escHtml(f.nombreColor)}', '${hex}')">
        <div class="chip-swatch" style="background:${hex}; position:relative;">
          ${hex === 'transparent' ? '<span style="position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:1.4rem;opacity:.3">🎨</span>' : ''}
        </div>
        <div class="chip-info">
          <div class="chip-name">${escHtml(f.nombreColor)}</div>
          <div class="chip-code">${hexDisplay}</div>
        </div>
      </div>
    `;
  }).join('');
}

// Resuelve el hex sin importar cómo lo nombre el backend
// Prioridad: 1) campo hex del backend, 2) mapa Pantone por nombre, 3) transparent
function resolverHex(obj) {
  // 1. Campo hex directo del backend (varios nombres posibles)
  const directVal = obj.codigoHex || obj.CodigoHex || obj.hexColor || obj.hex || obj.color || '';
  if (directVal && directVal !== 'string' && directVal.length >= 4) {
    const clean = directVal.trim();
    return clean.startsWith('#') ? clean : '#' + clean;
  }
  // 2. Buscar por nombre en el mapa Pantone
  const nombreCampo = obj.nombreColor || obj.NombreColor || obj.nombreTinta || obj.NombreTinta || '';
  if (nombreCampo) {
    const hexPantone = hexDesdePantone(nombreCampo);
    if (hexPantone) return hexPantone;
  }
  return 'transparent';
}

// Devuelve '#fff' o '#111' para garantizar legibilidad sobre el fondo
function colorTextContrast(hex) {
  try {
    const c = hex.replace('#', '');
    const r = parseInt(c.substring(0,2),16);
    const g = parseInt(c.substring(2,4),16);
    const b = parseInt(c.substring(4,6),16);
    const luminance = (0.299*r + 0.587*g + 0.114*b) / 255;
    return luminance > 0.55 ? '#111' : '#fff';
  } catch { return '#111'; }
}

// Filtrado local de colores
function filtrarColores() {
  const q = document.getElementById('color-search').value.trim().toLowerCase();
  if (!q) { renderColorGrid(allColors); return; }
  const filtrados = allColors.filter(f =>
    f.nombreColor.toLowerCase().includes(q) ||
    (f.codigoHex || '').toLowerCase().includes(q)
  );
  renderColorGrid(filtrados);
}

// ════════════════════════════════════════
//  POPUP / MODAL MEZCLA
// ════════════════════════════════════════
let _modalFormulaId = null;

function abrirModalMezcla(idFormula, nombreColor, hex) {
  _modalFormulaId = idFormula;
  document.getElementById('modal-color-preview').style.background = hex;
  document.getElementById('modal-color-nombre').textContent = nombreColor;
  document.getElementById('modal-formula-id').textContent   = 'Fórmula #' + idFormula;
  document.getElementById('peso-mezcla').value              = '';
  document.getElementById('resultado-mezcla').innerHTML     = '';
  document.getElementById('modal-mezcla').classList.add('open');
  document.getElementById('peso-mezcla').focus();
}

function cerrarModalMezcla(e) {
  if (e.target === document.getElementById('modal-mezcla')) {
    document.getElementById('modal-mezcla').classList.remove('open');
  }
}

async function calcularMezcla() {
  const idFormula       = _modalFormulaId;
  const pesoTotalGramos = parseFloat(document.getElementById('peso-mezcla').value);
  const resultado       = document.getElementById('resultado-mezcla');

  if (!idFormula || !pesoTotalGramos || pesoTotalGramos <= 0) {
    resultado.innerHTML = '<div class="error-cell">Ingresa un peso válido mayor a 0.</div>';
    return;
  }

  resultado.innerHTML = '<div class="loading-state" style="padding:20px">Calculando...</div>';

  try {
    const res  = await apiFetch('Mezcla/calcular', 'POST', { idFormula, pesoTotalGramos });
    const data = await res.json();

    if (!res.ok) {
      resultado.innerHTML = `<div class="error-cell">${escHtml(JSON.stringify(data))}</div>`;
      return;
    }

    const puedaConfirmar = data.porcentajesValidos && !data.tintas.some(t => !t.stockSuficiente);

    resultado.innerHTML = `
      <div class="mezcla-resultado">
        <div class="mezcla-header">
          <h3>${escHtml(data.nombreColor)}</h3>
          <span class="${data.porcentajesValidos ? 'badge-ok' : 'badge-err'}">
            ${data.porcentajesValidos ? '✅ Porcentajes válidos' : '⚠️ Porcentajes inválidos'}
          </span>
        </div>

        ${data.advertencias && data.advertencias.length ? `
          <div class="advertencias">
            ${data.advertencias.map(a => `<div class="advertencia">⚠ ${escHtml(a)}</div>`).join('')}
          </div>` : ''}

        <table class="tabla-mezcla">
          <thead>
            <tr>
              <th>Tinta</th>
              <th>%</th>
              <th>Gramos</th>
              <th>Stock</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            ${data.tintas.map(t => `
              <tr>
                <td>
                  <span class="color-dot" style="background:${t.codigoHex || '#ccc'}"></span>
                  ${escHtml(t.nombreTinta)}
                </td>
                <td>${t.porcentajeDisplay}%</td>
                <td><strong>${t.gramosNecesarios}g</strong></td>
                <td>${t.stockActual}g</td>
                <td>${t.stockSuficiente ? '✅' : '<span style="color:var(--red)">❌ Insuf.</span>'}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>

        <div class="mezcla-actions">
          <button class="btn-confirmar" onclick="confirmarMezcla(${data.idFormula}, ${pesoTotalGramos})"
            ${!puedaConfirmar ? 'disabled title="Revisa advertencias o stock insuficiente"' : ''}>
            ✅ Confirmar y descontar stock
          </button>
        </div>
      </div>
    `;
  } catch (e) {
    resultado.innerHTML = `<div class="error-cell">Error: ${escHtml(e.message)}</div>`;
  }
}

async function confirmarMezcla(idFormula, pesoTotalGramos) {
  if (!confirm('¿Confirmar la mezcla y descontar el stock de las tintas?')) return;
  const resultado = document.getElementById('resultado-mezcla');
  try {
    const res  = await apiFetch('Mezcla/confirmar', 'POST', { idFormula, pesoTotalGramos });
    const data = await res.json();
    if (!res.ok) { alert('Error: ' + data); return; }
    resultado.innerHTML = `
      <div class="mezcla-confirmada">
        <div class="confirm-icon">✅</div>
        <h3>Mezcla confirmada</h3>
        <p>Se descontó el stock de ${data.tintas.length} tintas para
           <strong>${pesoTotalGramos}g</strong> de <strong>${escHtml(data.nombreColor)}</strong>.</p>
        ${(data.advertencias || []).map(a => `<div class="advertencia">${escHtml(a)}</div>`).join('')}
        <button class="btn-calcular" style="margin-top:14px"
          onclick="document.getElementById('resultado-mezcla').innerHTML='';document.getElementById('peso-mezcla').value=''">
          Nueva consulta
        </button>
      </div>
    `;
  } catch (e) { alert('Error al confirmar: ' + e.message); }
}

// ════════════════════════════════════════
//  EMPRESAS
//
//  NUEVAS FUNCIONES — requieren endpoints:
//    GET /api/Empresa                   → array de { id, nombre, nit, ... }
//    GET /api/Empresa/{id}              → { id, nombre, nit, telefono, correo, ... }
//    GET /api/Logo?idEmpresa={id}       → array de { id, nombre, ... }
//    GET /api/Formula?idEmpresa={id}    → array de fórmulas propias
// ════════════════════════════════════════
async function cargarEmpresas() {
  const lista = document.getElementById('empresas-list');
  lista.innerHTML = '<div class="loading-state">Cargando empresas...</div>';
  try {
    const res      = await apiFetch('Empresa');
    const empresas = await res.json();
    if (!empresas.length) {
      lista.innerHTML = '<div class="no-data">No hay empresas registradas.</div>';
      return;
    }
    lista.innerHTML = empresas.map(e => `
      <div class="empresa-item" onclick="verEmpresa(${e.id}, this)">
        <div class="empresa-icon">${iniciales(e.nombre)}</div>
        <div>
          <div class="empresa-name">${escHtml(e.nombre)}</div>
          <div class="empresa-nit">NIT: ${e.nit || '—'}</div>
        </div>
      </div>
    `).join('');
  } catch (e) {
    lista.innerHTML = '<div class="error-cell">Error: ' + escHtml(e.message) + '</div>';
  }
}

async function verEmpresa(id, el) {
  document.querySelectorAll('.empresa-item').forEach(i => i.classList.remove('active'));
  el.classList.add('active');

  const detail = document.getElementById('empresa-detail');
  detail.innerHTML = '<div class="loading-state">Cargando información...</div>';

  try {
    // Empresa detalle
    const [resEmp, resLogos, resForm] = await Promise.all([
      apiFetch(`Empresa/${id}`),
      apiFetch(`Logo?idEmpresa=${id}`).catch(() => null),          // tolerante si no existe endpoint
      apiFetch(`Formula?idEmpresa=${id}`).catch(() => null)
    ]);

    const emp     = await resEmp.json();
    const logos   = resLogos   ? (await resLogos.json().catch(() => [])) : [];
    const formulas = resForm   ? (await resForm.json().catch(() => []))  : [];

    // Filtrar sólo fórmulas exclusivas (idEmpresa == id), por si el endpoint devuelve todas
    const exclusivas = formulas.filter ? formulas.filter(f => f.idEmpresa == id) : formulas;

    detail.innerHTML = `
      <div class="empresa-detail-name">${escHtml(emp.nombre)}</div>
      <div class="empresa-detail-nit">NIT: ${emp.nit || '—'} · ${emp.correo || ''} · ${emp.telefono || ''}</div>

      ${logos.length ? `
        <div class="detail-section">
          <div class="detail-section-title">Logotipos</div>
          <div class="logos-grid">
            ${logos.map(l => `<div class="logo-card">🖼 ${escHtml(l.nombre || 'Logo #' + l.id)}</div>`).join('')}
          </div>
        </div>
      ` : ''}

      ${exclusivas.length ? `
        <div class="detail-section">
          <div class="detail-section-title">Colores exclusivos (${exclusivas.length})</div>
          <div class="empresa-colores-grid">
            ${exclusivas.map(f => {
              const hex = resolverHex(f);
              return `
                <div class="color-chip" style="cursor:pointer"
                  onclick="abrirModalMezcla(${f.id},'${escHtml(f.nombreColor)}','${hex}')">
                  <div class="chip-swatch" style="background:${hex}"></div>
                  <div class="chip-info">
                    <div class="chip-name">${escHtml(f.nombreColor)}</div>
                    <div class="chip-code">${hex !== 'transparent' ? hex.toUpperCase() : '—'}</div>
                  </div>
                </div>
              `;
            }).join('')}
          </div>
        </div>
      ` : '<div class="no-data">Sin colores exclusivos registrados.</div>'}
    `;
  } catch (e) {
    detail.innerHTML = '<div class="error-cell">Error: ' + escHtml(e.message) + '</div>';
  }
}

// ════════════════════════════════════════
//  INVENTARIO
//
//  USA endpoint existente:
//    GET /api/TintaBase
// ════════════════════════════════════════
async function cargarInventario() {
  const tbody = document.getElementById('tabla-inventario');
  const stats = document.getElementById('inv-stats');
  tbody.innerHTML = '<tr><td colspan="6" class="loading-cell">Cargando...</td></tr>';

  try {
    const [, res] = await Promise.all([
      cargarPantoneMap(),
      apiFetch('TintaBase')
    ]);
    const tintas = await res.json();

    const total  = tintas.length;
    const bajo   = tintas.filter(t => t.stockActual <= t.stockMinimo_alerta).length;
    const critico = tintas.filter(t => t.stockActual === 0).length;

    document.getElementById('inv-count').textContent = total + ' tintas';

    stats.innerHTML = `
      <div class="inv-stat-card">
        <div class="inv-stat-val">${total}</div>
        <div class="inv-stat-label">Tintas base</div>
      </div>
      <div class="inv-stat-card ${bajo ? 'alert' : ''}">
        <div class="inv-stat-val">${bajo}</div>
        <div class="inv-stat-label">Stock bajo</div>
      </div>
      <div class="inv-stat-card ${critico ? 'alert' : ''}">
        <div class="inv-stat-val">${critico}</div>
        <div class="inv-stat-label">Sin stock</div>
      </div>
    `;

    tbody.innerHTML = tintas.map(t => {
      const hex     = resolverHex(t);
      const hexDisplay = hex !== 'transparent' ? hex.toUpperCase() : '—';
      const esBajo  = t.stockActual <= t.stockMinimo_alerta;
      const estadoClass = t.stockActual === 0 ? 'danger' : esBajo ? 'warn' : 'ok';
      const estadoLabel = t.stockActual === 0 ? 'Sin stock' : esBajo ? 'Bajo' : 'Normal';
      return `
        <tr>
          <td>
            <span class="color-swatch" style="background:${hex}; ${hex === 'transparent' ? 'border:2px dashed #ccc;' : ''}"></span>
          </td>
          <td>${escHtml(t.nombreTinta)}</td>
          <td style="font-family:monospace;font-size:.78rem">${hexDisplay}</td>
          <td class="${esBajo ? 'stock-low' : ''}">${t.stockActual}g</td>
          <td>${t.stockMinimo_alerta}g</td>
          <td><span class="stock-badge ${estadoClass}">${estadoLabel}</span></td>
        </tr>
      `;
    }).join('') || '<tr><td colspan="6" class="loading-cell">No hay tintas registradas.</td></tr>';
  } catch (e) {
    tbody.innerHTML = `<tr><td colspan="6" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`;
  }
}

// ════════════════════════════════════════
//  USUARIOS (solo Admin)
//
//  NUEVAS FUNCIONES — requieren endpoints:
//    GET    /api/Usuario          → array de usuarios
//    POST   /api/Usuario          → crear usuario { nombre, correo, password, rol }
//    DELETE /api/Usuario/{id}     → eliminar usuario
// ════════════════════════════════════════
async function cargarUsuarios() {
  const tbody = document.getElementById('tabla-usuarios');
  tbody.innerHTML = '<tr><td colspan="5" class="loading-cell">Cargando...</td></tr>';
  try {
    const res      = await apiFetch('Usuario');
    const usuarios = await res.json();
    tbody.innerHTML = usuarios.map(u => `
      <tr>
        <td><div class="user-row-avatar">${iniciales(u.nombre)}</div></td>
        <td>${escHtml(u.nombre)}</td>
        <td>${escHtml(u.correo)}</td>
        <td>
          <span class="role-badge ${(u.rol || '').toLowerCase() === 'admin' ? 'admin' : 'user'}">
            ${escHtml(u.rol || 'Usuario')}
          </span>
        </td>
        <td>
          ${u.correo !== currentUser.correo
            ? `<button class="btn-danger" onclick="eliminarUsuario(${u.id}, '${escHtml(u.nombre)}')">Eliminar</button>`
            : '<span style="font-size:.7rem;color:var(--text-muted)">(tú)</span>'
          }
        </td>
      </tr>
    `).join('') || '<tr><td colspan="5" class="loading-cell">No hay usuarios.</td></tr>';
  } catch (e) {
    tbody.innerHTML = `<tr><td colspan="5" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`;
  }
}

async function eliminarUsuario(id, nombre) {
  if (!confirm(`¿Eliminar al usuario "${nombre}"? Esta acción no se puede deshacer.`)) return;
  try {
    const res = await apiFetch(`Usuario/${id}`, 'DELETE');
    if (!res.ok) { alert('No se pudo eliminar el usuario.'); return; }
    cargarUsuarios();
  } catch (e) { alert('Error: ' + e.message); }
}

function abrirModalNuevoUsuario() {
  ['nu-nombre','nu-correo','nu-pass'].forEach(id => document.getElementById(id).value = '');
  document.getElementById('nu-rol').value = 'Usuario';
  document.getElementById('nu-msg').style.display = 'none';
  document.getElementById('modal-usuario').classList.add('open');
}

function cerrarModalUsuario(e) {
  if (e.target === document.getElementById('modal-usuario')) {
    document.getElementById('modal-usuario').classList.remove('open');
  }
}

async function crearUsuario() {
  const nombre   = document.getElementById('nu-nombre').value.trim();
  const correo   = document.getElementById('nu-correo').value.trim();
  const password = document.getElementById('nu-pass').value.trim();
  const rol      = document.getElementById('nu-rol').value;
  const msg      = document.getElementById('nu-msg');

  if (!nombre || !correo || !password) {
    showModalMsg(msg, 'Completa todos los campos.', 'err'); return;
  }

  try {
    const res  = await apiFetch('Usuario', 'POST', { nombre, correo, password, rol });
    const data = await res.json();
    if (!res.ok) {
      showModalMsg(msg, data.mensaje || 'No se pudo crear el usuario.', 'err'); return;
    }
    document.getElementById('modal-usuario').classList.remove('open');
    cargarUsuarios();
  } catch (e) {
    showModalMsg(msg, 'Error: ' + e.message, 'err');
  }
}

// ════════════════════════════════════════
//  CONFIGURACIÓN / PERFIL
//
//  NUEVA FUNCIÓN — requiere endpoint:
//    PUT /api/Usuario/{id}/cambiar-password
//        body: { passwordActual, passwordNueva }
// ════════════════════════════════════════
function cargarConfig() {
  if (!currentUser) return;
  const initials = currentUser.nombre.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  document.getElementById('config-avatar').textContent    = initials;
  document.getElementById('config-name').textContent      = currentUser.nombre;
  document.getElementById('config-correo').textContent    = currentUser.correo;
  document.getElementById('config-rol-badge').textContent = currentUser.rol || 'Usuario';
  ['pass-actual','pass-nueva','pass-confirmar'].forEach(id => document.getElementById(id).value = '');
  document.getElementById('config-msg').style.display = 'none';
}

async function cambiarContrasena() {
  const actual     = document.getElementById('pass-actual').value;
  const nueva      = document.getElementById('pass-nueva').value;
  const confirmar  = document.getElementById('pass-confirmar').value;
  const msg        = document.getElementById('config-msg');

  if (!actual || !nueva || !confirmar) {
    showModalMsg(msg, 'Completa todos los campos.', 'err'); return;
  }
  if (nueva !== confirmar) {
    showModalMsg(msg, 'La nueva contraseña no coincide.', 'err'); return;
  }
  if (nueva.length < 6) {
    showModalMsg(msg, 'La contraseña debe tener al menos 6 caracteres.', 'err'); return;
  }

  try {
    const res = await apiFetch('Usuario/cambiar-password', 'PUT', {
      passwordActual: actual,
      passwordNueva: nueva
    });
    if (!res.ok) {
      const data = await res.json().catch(() => ({}));
      showModalMsg(msg, data.mensaje || 'No se pudo cambiar la contraseña.', 'err');
      return;
    }
    showModalMsg(msg, 'Contraseña actualizada correctamente.', 'ok');
    ['pass-actual','pass-nueva','pass-confirmar'].forEach(id => document.getElementById(id).value = '');
  } catch (e) {
    showModalMsg(msg, 'Error: ' + e.message, 'err');
  }
}

// ════════════════════════════════════════
//  HELPERS
// ════════════════════════════════════════
function escHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g,'&amp;').replace(/</g,'&lt;')
    .replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function iniciales(nombre) {
  if (!nombre) return '??';
  return nombre.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
}

function showModalMsg(el, text, type) {
  el.textContent = text;
  el.className   = 'config-msg ' + type;
  el.style.display = 'block';
  setTimeout(() => { if (type === 'ok') el.style.display = 'none'; }, 4000);
}
