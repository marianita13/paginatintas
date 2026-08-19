// ════════════════════════════════════════
//  CONFIGURACIÓN
// ════════════════════════════════════════
const API_URL = 'http://localhost:5115/api';

// ════════════════════════════════════════
//  HELPERS DE ROL
//  Roles del backend: "Administrador", "Operarios", "Admin2"
// ════════════════════════════════════════
function esAdmin()     { return (currentUser?.rol || '').toLowerCase() === 'administrador'; }
function esAdmin2()    { return (currentUser?.rol || '').toLowerCase() === 'admin2'; }
function esOperario()  { return (currentUser?.rol || '').toLowerCase() === 'operarios'; }

// ════════════════════════════════════════
//  ESTADO GLOBAL
// ════════════════════════════════════════
let token       = localStorage.getItem('token') || null;
let currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
let allColors   = [];

// Diccionario Pantone → HEX
let pantoneMap  = {};

// Estado selección de Pantones para nueva orden
let modoSeleccionOrden         = false;
let ordenPantonesSeleccionados = []; // [{ id, nombreColor, hex }]

// ════════════════════════════════════════
//  MAPA PANTONE (3 fuentes)
// ════════════════════════════════════════
async function cargarPantoneMap() {
  if (Object.keys(pantoneMap).length > 0) return;
  try {
    const [resC, resU, resKorchy] = await Promise.all([
      fetch('https://raw.githubusercontent.com/brettapeters/pantones/master/pantone-coated.json'),
      fetch('https://raw.githubusercontent.com/brettapeters/pantones/master/pantone-uncoated.json'),
      fetch('https://raw.githubusercontent.com/Korchy/blender-color-matching/master/pantone.json')
    ]);
    const coated   = resC.ok       ? await resC.json()       : {};
    const uncoated = resU.ok       ? await resU.json()       : {};
    const korchy   = resKorchy.ok  ? await resKorchy.json()  : [];

    const todos = { ...coated, ...uncoated };
    Object.keys(todos).forEach(nombre => {
      const c = todos[nombre];
      if (c && c.hex) {
        const hex = String(c.hex).trim();
        pantoneMap[normalizarPantone(nombre)] = hex.startsWith('#') ? hex : '#' + hex;
      }
    });

    if (Array.isArray(korchy)) {
      korchy.forEach(item => {
        if (Array.isArray(item) && item[1] && Array.isArray(item[1])) {
          const datos = item[1];
          const nombre = datos[0]; const hexOrig = datos[3];
          if (nombre && hexOrig) {
            const clave = normalizarPantone(nombre);
            if (!pantoneMap[clave]) {
              const hex = String(hexOrig).trim();
              pantoneMap[clave] = hex.startsWith('#') ? hex : '#' + hex;
            }
          }
        }
      });
    }
    console.log(`[Pantone] ${Object.keys(pantoneMap).length} colores cargados.`);
  } catch (e) { console.warn('[Pantone] Error al cargar mapa:', e.message); }
}

function normalizarPantone(nombre) {
  return nombre.replace(/pantone/i, '').replace(/\s+/g, '').toUpperCase().trim();
}

function hexDesdePantone(nombreColor) {
  if (!nombreColor) return null;
  const key = normalizarPantone(nombreColor);
  if (pantoneMap[key]) return pantoneMap[key];
  const sinSufijo = key.replace(/[CU]$/, '');
  if (pantoneMap[sinSufijo + 'C']) return pantoneMap[sinSufijo + 'C'];
  if (pantoneMap[sinSufijo + 'U']) return pantoneMap[sinSufijo + 'U'];
  const encontrado = Object.keys(pantoneMap).find(k => k.startsWith(sinSufijo));
  return encontrado ? pantoneMap[encontrado] : null;
}

function resolverHex(obj) {
  const directVal = obj.codigoHex || obj.CodigoHex || obj.hexColor || obj.hex || '';
  if (directVal && directVal.length >= 4) {
    const c = directVal.trim();
    return c.startsWith('#') ? c : '#' + c;
  }
  const nombre = obj.nombreColor || obj.NombreColor || obj.nombreTinta || obj.NombreTinta || '';
  if (nombre) { const hp = hexDesdePantone(nombre); if (hp) return hp; }
  return 'transparent';
}

function colorTextContrast(hex) {
  try {
    const c = hex.replace('#', '');
    const r = parseInt(c.substring(0,2),16);
    const g = parseInt(c.substring(2,4),16);
    const b = parseInt(c.substring(4,6),16);
    return (0.299*r + 0.587*g + 0.114*b) / 255 > 0.55 ? '#111' : '#fff';
  } catch { return '#111'; }
}

function hexToHsl(hex) {
  hex = hex.replace(/^#/, '');
  if (hex.length !== 6) return { h: 360, s: 0, l: 0 };
  const r = parseInt(hex.substring(0,2),16)/255;
  const g = parseInt(hex.substring(2,4),16)/255;
  const b = parseInt(hex.substring(4,6),16)/255;
  const max = Math.max(r,g,b), min = Math.min(r,g,b);
  let h, s, l = (max+min)/2;
  if (max === min) { h = s = 0; } else {
    const d = max - min;
    s = l > 0.5 ? d/(2-max-min) : d/(max+min);
    switch(max) {
      case r: h=(g-b)/d+(g<b?6:0); break;
      case g: h=(b-r)/d+2; break;
      case b: h=(r-g)/d+4; break;
    }
    h /= 6;
  }
  return { h: Math.round(h*360), s: Math.round(s*100), l: Math.round(l*100) };
}

function ordenarPorColorReal(lista) {
  return lista.sort((a, b) => {
    const ha = a.hexResuelto || '', hb = b.hexResuelto || '';
    if (ha === 'transparent') return 1;
    if (hb === 'transparent') return -1;
    const hslA = hexToHsl(ha), hslB = hexToHsl(hb);
    if (hslA.h !== hslB.h) return hslA.h - hslB.h;
    return hslB.l - hslA.l;
  });
}

// ════════════════════════════════════════
//  HTTP
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
      return fetch(`${API_URL}/${endpoint}`, { method, headers, body: opts.body });
    } else { handleLogout(); throw new Error('Sesión expirada'); }
  }
  return res;
}

async function tryRefreshToken() {
  const rt = localStorage.getItem('refreshToken');
  if (!rt) return false;
  try {
    const res = await fetch(`${API_URL}/Usuario/refresh-token`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(rt)
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
  if (localStorage.getItem('theme') === 'dark') applyTheme('dark');
  if (token && currentUser) showMainPage();
});

document.addEventListener('keydown', e => {
  const lp = document.getElementById('login-page');
  if (e.key === 'Enter' && lp && getComputedStyle(lp).display !== 'none') handleLogin();
});

document.addEventListener('DOMContentLoaded', () => {
  const btn = document.getElementById('sidebar-toggle');
  if (btn) btn.addEventListener('click', () =>
    document.getElementById('sidebar').classList.toggle('collapsed'));
});

// ════════════════════════════════════════
//  TEMA
// ════════════════════════════════════════
function toggleTheme() { applyTheme(document.body.classList.contains('dark') ? 'light' : 'dark'); }

function applyTheme(mode) {
  if (mode === 'dark') {
    document.body.classList.add('dark');
    document.getElementById('theme-label').textContent = 'Modo claro';
    document.querySelector('.theme-icon-light').style.display = 'none';
    document.querySelector('.theme-icon-dark').style.display  = 'flex';
  } else {
    document.body.classList.remove('dark');
    document.getElementById('theme-label').textContent = 'Modo oscuro';
    document.querySelector('.theme-icon-light').style.display = 'flex';
    document.querySelector('.theme-icon-dark').style.display  = 'none';
  }
  localStorage.setItem('theme', mode);
}

// ════════════════════════════════════════
//  LOGIN
// ════════════════════════════════════════
async function handleLogin() {
  const correo = document.getElementById('username').value.trim();
  const pass   = document.getElementById('password').value.trim();
  const btn    = document.querySelector('.btn-login');
  if (!correo || !pass) { showError('Completa todos los campos'); return; }
  btn.disabled = true; btn.textContent = 'Ingresando...';
  try {
    const res  = await fetch(`${API_URL}/Usuario/token`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ correo, password: pass })
    });
    const data = await res.json();
    if (!res.ok || !data.isAuthenticated) { showError(data.mensaje || 'Credenciales incorrectas'); return; }
    token       = data.token;
    currentUser = { nombre: data.nombre, correo: data.correo, rol: data.rol };
    localStorage.setItem('token', token);
    localStorage.setItem('refreshToken', data.refreshToken || '');
    localStorage.setItem('currentUser', JSON.stringify(currentUser));
    document.getElementById('error-msg').style.display = 'none';
    showMainPage();
  } catch { showError('No se pudo conectar con el servidor.'); }
  finally { btn.disabled = false; btn.textContent = 'Iniciar Sesión'; }
}

function showError(msg) {
  const err = document.getElementById('error-msg');
  err.textContent = '⚠ ' + msg; err.style.display = 'block';
  err.style.animation = 'none'; void err.offsetWidth; err.style.animation = 'shake 0.4s ease';
}

// ════════════════════════════════════════
//  SHOW MAIN + CONTROL DE ROLES
//  Admin       → todo
//  Admin2      → Pantones, Órdenes, Inventario, Base de Datos
//  Operarios   → Pantones, Empresas, Órdenes
// ════════════════════════════════════════
function showMainPage() {
  const loginPage = document.getElementById('login-page');
  const mainPage  = document.getElementById('main-page');

  if (currentUser) {
    const ini = currentUser.nombre.split(' ').map(n => n[0]).join('').toUpperCase().slice(0,2);
    document.getElementById('welcome-name').textContent    = currentUser.nombre;
    document.getElementById('user-initials').textContent   = ini;
    document.getElementById('user-role-label').textContent = currentUser.rol || '—';

    const rol = (currentUser.rol || '').toLowerCase();

    // Inventario y Base de Datos: Admin y Admin2
    if (rol === 'administrador' || rol === 'admin2') {
      document.querySelector('.nav-item-inventario').style.display = 'flex';
      document.querySelector('.nav-item-basedatos').style.display  = 'flex';
    }
    // Usuarios: solo Admin
    if (rol === 'administrador') {
      document.querySelector('.nav-item-admin').style.display = 'flex';
    }
    // Empresas: Admin y Operarios (no Admin2)
    if (rol === 'admin2') {
      const btnEmpresas = document.querySelector('[data-tab="empresas"]');
      if (btnEmpresas) btnEmpresas.style.display = 'none';
    }
  }

  loginPage.style.opacity = '0'; loginPage.style.transition = 'opacity 0.4s ease';
  setTimeout(() => {
    loginPage.style.display = 'none';
    mainPage.style.display  = 'flex'; mainPage.style.opacity = '0';
    mainPage.style.transition = 'opacity 0.4s ease';
    setTimeout(() => { mainPage.style.opacity = '1'; }, 20);
  }, 400);

  cargarColores();
}

function handleLogout() {
  token = null; currentUser = null;
  ['token','refreshToken','currentUser'].forEach(k => localStorage.removeItem(k));
  const mainPage = document.getElementById('main-page');
  const loginPage = document.getElementById('login-page');
  mainPage.style.opacity = '0'; mainPage.style.transition = 'opacity 0.3s ease';
  setTimeout(() => {
    mainPage.style.display = 'none'; loginPage.style.display = 'flex';
    loginPage.style.opacity = '0'; loginPage.style.transition = 'opacity 0.3s ease';
    document.getElementById('username').value = '';
    document.getElementById('password').value = '';
    document.getElementById('error-msg').style.display = 'none';
    setTimeout(() => { loginPage.style.opacity = '1'; }, 20);
  }, 300);
}

// ════════════════════════════════════════
//  NAVEGACIÓN
// ════════════════════════════════════════
const TAB_TITLES = {
  inicio:     ['Pantones',        'Selección de Pantones'],
  empresas:   ['Empresas',        'Empresas registradas'],
  ordenes:    ['Órdenes',         'Gestión de órdenes'],
  inventario: ['Inventario',      'Tintas base y stock'],
  basedatos:  ['Base de Datos',   'Inventario de tintas'],
  usuarios:   ['Usuarios',        'Gestión de usuarios'],
  config:     ['Configuración',   'Perfil y seguridad']
};

function switchTab(name, btn) {
  // Si estábamos en modo selección de orden, cancelar
  if (modoSeleccionOrden && name !== 'inicio') cancelarSeleccionOrden();

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
  if (name === 'ordenes')    cargarOrdenes();
  if (name === 'inventario') cargarInventario();
  if (name === 'basedatos')  cargarBaseDatos();
  if (name === 'usuarios')   cargarUsuarios();
  if (name === 'config')     cargarConfig();
}

// ════════════════════════════════════════
//  PANTONES (antes Inicio)
// ════════════════════════════════════════
async function cargarColores() {
  const grid = document.getElementById('color-grid');
  grid.innerHTML = '<div class="loading-state">Cargando Pantones...</div>';
  try {
    const [, res] = await Promise.all([ cargarPantoneMap(), apiFetch('Formula') ]);
    const formulas = await res.json();
    const conHex = formulas.map(f => ({ ...f, hexResuelto: resolverHex(f) }));
    allColors = ordenarPorColorReal(conHex);
    renderColorGrid(allColors);
  } catch (e) {
    grid.innerHTML = `<div class="error-cell">Error: ${e.message}</div>`;
  }
}

function renderColorGrid(formulas) {
  const grid = document.getElementById('color-grid');
  if (!formulas.length) { grid.innerHTML = '<div class="loading-state">No hay Pantones registrados.</div>'; return; }
  grid.innerHTML = formulas.map(f => {
    const hex = f.hexResuelto || resolverHex(f);
    const hexDisplay = hex !== 'transparent' ? hex.toUpperCase() : 'Sin hex';
    const selClass = modoSeleccionOrden && ordenPantonesSeleccionados.find(p => p.id === f.id)
      ? ' seleccionando' : '';
    return `
      <div class="color-chip${selClass}" onclick="onChipClick(${f.id}, '${escHtml(f.nombreColor)}', '${hex}', this)">
        <div class="chip-swatch" style="background:${hex};position:relative">
          ${hex === 'transparent' ? '<span style="position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:1.4rem;opacity:.3">🎨</span>' : ''}
          ${selClass ? '<div class="chip-check">✓</div>' : ''}
        </div>
        <div class="chip-info">
          <div class="chip-name">${escHtml(f.nombreColor)}</div>
          <div class="chip-code">${hexDisplay}</div>
        </div>
      </div>`;
  }).join('');
}

function onChipClick(id, nombre, hex, el) {
  if (modoSeleccionOrden) {
    togglePantoneOrden(id, nombre, hex, el);
  } else {
    abrirModalMezcla(id, nombre, hex);
  }
}

function filtrarColores() {
  const q = document.getElementById('color-search').value.trim().toLowerCase();
  renderColorGrid(q ? allColors.filter(f =>
    f.nombreColor.toLowerCase().includes(q)) : allColors);
}

// ════════════════════════════════════════
//  MODAL MEZCLA (informativo 100g)
// ════════════════════════════════════════
let _modalFormulaId = null;

function abrirModalMezcla(idFormula, nombreColor, hex) {
  _modalFormulaId = idFormula;
  // Reutilizamos el modal-orden en modo visualización informativa
  document.getElementById('modal-orden-titulo').textContent = nombreColor;
  document.getElementById('modal-orden-body').innerHTML =
    '<div class="loading-state" style="padding:30px">Consultando fórmula...</div>';
  document.getElementById('modal-orden').classList.add('open');

  apiFetch('Mezcla/calcular', 'POST', { idFormula, pesoTotalGramos: 100 })
    .then(r => r.json())
    .then(data => {
      document.getElementById('modal-orden-body').innerHTML = `
        <div style="display:flex;align-items:center;gap:12px;margin-bottom:16px">
          <div style="width:48px;height:48px;border-radius:12px;background:${hex};border:2px solid var(--border);flex-shrink:0"></div>
          <div>
            <div style="font-size:0.72rem;color:var(--text-muted);font-weight:700;letter-spacing:.1em;text-transform:uppercase">Proporciones para 100g</div>
            <div style="font-size:0.78rem;color:var(--text-2);margin-top:2px">Solo visualización — el stock no se descuenta aquí</div>
          </div>
        </div>
        <table class="tabla-mezcla">
          <thead><tr><th>Tinta base</th><th>Gramos / 100g</th><th>%</th><th>Stock</th></tr></thead>
          <tbody>
            ${data.tintas.map(t => `
              <tr>
                <td>${escHtml(t.nombreTinta)}</td>
                <td><strong>${t.gramosNecesarios}g</strong></td>
                <td>${t.porcentajeDisplay}%</td>
                <td><span class="stock-badge ${t.stockSuficiente ? 'ok' : 'warn'}">${t.stockActual}g</span></td>
              </tr>`).join('')}
          </tbody>
        </table>`;
    })
    .catch(e => {
      document.getElementById('modal-orden-body').innerHTML =
        `<div class="error-cell">Error: ${escHtml(e.message)}</div>`;
    });
}

// ════════════════════════════════════════
//  ÓRDENES — SELECCIÓN DE PANTONES
// ════════════════════════════════════════
function abrirModalNuevaOrden() {
  // Activar modo selección sobre la grilla de Pantones
  ordenPantonesSeleccionados = [];
  modoSeleccionOrden = true;

  // Ir a pestaña Pantones
  const btnPantones = document.querySelector('[data-tab="inicio"]');
  switchTab('inicio', btnPantones);

  // Mostrar barra flotante
  const bar = document.getElementById('orden-float-bar');
  bar.style.display = 'flex';
  actualizarBarraOrden();
}

function togglePantoneOrden(id, nombre, hex, chipEl) {
  const idx = ordenPantonesSeleccionados.findIndex(p => p.id === id);
  if (idx === -1) {
    ordenPantonesSeleccionados.push({ id, nombre, hex });
    chipEl.classList.add('seleccionando');
    const swatch = chipEl.querySelector('.chip-swatch');
    if (!chipEl.querySelector('.chip-check')) {
      const chk = document.createElement('div');
      chk.className = 'chip-check'; chk.textContent = '✓';
      swatch.appendChild(chk);
    }
  } else {
    ordenPantonesSeleccionados.splice(idx, 1);
    chipEl.classList.remove('seleccionando');
    const chk = chipEl.querySelector('.chip-check');
    if (chk) chk.remove();
  }
  actualizarBarraOrden();
}

function actualizarBarraOrden() {
  const n = ordenPantonesSeleccionados.length;
  document.getElementById('orden-float-count').textContent =
    n === 0 ? '0 Pantones seleccionados' : `${n} Pantone${n > 1 ? 's' : ''} seleccionado${n > 1 ? 's' : ''}`;
}

function cancelarSeleccionOrden() {
  modoSeleccionOrden = false;
  ordenPantonesSeleccionados = [];
  document.getElementById('orden-float-bar').style.display = 'none';
  document.querySelectorAll('.color-chip.seleccionando')
    .forEach(c => { c.classList.remove('seleccionando'); const chk = c.querySelector('.chip-check'); if (chk) chk.remove(); });
}

function aceptarSeleccionOrden() {
  if (!ordenPantonesSeleccionados.length) { alert('Selecciona al menos un Pantone.'); return; }
  modoSeleccionOrden = false;
  document.getElementById('orden-float-bar').style.display = 'none';
  document.querySelectorAll('.color-chip.seleccionando')
    .forEach(c => { c.classList.remove('seleccionando'); const chk = c.querySelector('.chip-check'); if (chk) chk.remove(); });

  // Abrir modal de nueva orden con los pantones seleccionados
  abrirPopupNuevaOrden();
}

function abrirPopupNuevaOrden() {
  document.getElementById('modal-orden-titulo').textContent = 'Nueva Orden';
  document.getElementById('modal-orden-body').innerHTML = renderFormNuevaOrden();
  document.getElementById('modal-orden').classList.add('open');
  // Mostrar tintas por cada pantone seleccionado
  ordenPantonesSeleccionados.forEach(p => cargarTintasPrueba(p));
}

function renderFormNuevaOrden() {
  return `
    <div class="form-group-flat" style="margin-bottom:16px">
      <label>N° Orden</label>
      <input type="number" id="ord-numero" placeholder="Ej: 001" min="1">
    </div>

    <div style="margin-bottom:16px">
      <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
        color:var(--text-muted);margin-bottom:10px;padding-bottom:6px;border-bottom:1px solid var(--border)">
        Pantones seleccionados — Prueba de color (100g base)
      </div>
      ${ordenPantonesSeleccionados.map(p => `
        <div style="margin-bottom:12px;padding:12px;background:var(--surface-2);border-radius:10px;border:1px solid var(--border)">
          <div style="display:flex;align-items:center;gap:8px;margin-bottom:8px">
            <div style="width:20px;height:20px;border-radius:6px;background:${p.hex};border:1px solid rgba(0,0,0,.1);flex-shrink:0"></div>
            <strong style="font-size:0.82rem">${escHtml(p.nombre)}</strong>
          </div>
          <div id="tintas-prueba-${p.id}" style="font-size:0.78rem;color:var(--text-muted)">Cargando tintas...</div>
        </div>`).join('')}
    </div>

    <div style="background:var(--surface-2);border-radius:10px;padding:16px;border:1px solid var(--border);margin-bottom:16px">
      <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
        color:var(--text-muted);margin-bottom:12px">Cajas a producir</div>
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
        <div class="form-group-flat" style="margin:0">
          <label>N° cajas con prueba de color</label>
          <input type="number" id="ord-prueba-cajas" placeholder="Ej: 6" min="1" oninput="actualizarCalculo()">
        </div>
        <div class="form-group-flat" style="margin:0">
          <label>N° cajas de la orden</label>
          <input type="number" id="ord-total-cajas" placeholder="Ej: 850" min="1" oninput="actualizarCalculo()">
        </div>
      </div>
    </div>

    <div id="calculo-orden-section" style="display:none;margin-bottom:16px">
      <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
        color:var(--text-muted);margin-bottom:10px;padding-bottom:6px;border-bottom:1px solid var(--border)">
        Cantidad de tinta a preparar para la orden
      </div>
      <div id="calculo-orden-resultado"></div>
    </div>

    <div id="ord-msg" class="config-msg" style="display:none"></div>
    <button class="btn-primary" onclick="guardarOrden()" style="width:100%;margin-top:8px">
      Guardar Orden
    </button>`;
}

// Carga y muestra las tintas de prueba (100g) para un pantone
async function cargarTintasPrueba(pantone) {
  const cont = document.getElementById('tintas-prueba-' + pantone.id);
  if (!cont) return;
  try {
    const res  = await apiFetch('Mezcla/calcular', 'POST', { idFormula: pantone.id, pesoTotalGramos: 100 });
    const data = await res.json();
    // Guardamos los datos de tintas en el pantone para usarlos en la regla de tres
    pantone.tintas = data.tintas;
    cont.innerHTML = `
      <table style="width:100%;border-collapse:collapse">
        ${data.tintas.map(t => `
          <tr>
            <td style="padding:3px 0;color:var(--text)">${escHtml(t.nombreTinta)}</td>
            <td style="text-align:right;font-weight:700;padding:3px 0" class="tintas-prueba-g" data-tinta="${t.idTinta}" data-formula="${pantone.id}">
              ${t.gramosNecesarios}g
            </td>
          </tr>`).join('')}
      </table>`;
    actualizarCalculo();
  } catch {
    if (cont) cont.textContent = 'Error al cargar tintas.';
  }
}

// Regla de tres: actualiza la sección de tinta para la orden completa
function actualizarCalculo() {
  const cajasPrueba = parseFloat(document.getElementById('ord-prueba-cajas')?.value) || 0;
  const cajasOrden  = parseFloat(document.getElementById('ord-total-cajas')?.value)  || 0;
  const seccion     = document.getElementById('calculo-orden-section');
  const resultado   = document.getElementById('calculo-orden-resultado');
  if (!seccion || !resultado) return;

  if (!cajasPrueba || !cajasOrden) { seccion.style.display = 'none'; return; }
  seccion.style.display = 'block';

  resultado.innerHTML = ordenPantonesSeleccionados.map(p => {
    if (!p.tintas || !p.tintas.length) return '';
    return `
      <div style="margin-bottom:10px;padding:10px;background:var(--surface-2);border-radius:8px;border:1px solid var(--border)">
        <div style="display:flex;align-items:center;gap:8px;margin-bottom:6px">
          <div style="width:14px;height:14px;border-radius:4px;background:${p.hex}"></div>
          <strong style="font-size:0.8rem">${escHtml(p.nombre)}</strong>
        </div>
        <table style="width:100%;border-collapse:collapse">
          ${p.tintas.map(t => {
            // X = (cajasOrden × gramosPrueba) / cajasPrueba  → redondeado a entero
            const gramosOrden = Math.round((cajasOrden * t.gramosNecesarios) / cajasPrueba);
            return `
              <tr>
                <td style="padding:3px 0;color:var(--text);font-size:0.78rem">${escHtml(t.nombreTinta)}</td>
                <td style="text-align:right;font-weight:700;font-size:0.8rem;padding:3px 0">${gramosOrden}g</td>
              </tr>`;
          }).join('')}
        </table>
      </div>`;
  }).join('');
}

async function guardarOrden() {
  const numero      = parseInt(document.getElementById('ord-numero')?.value) || 0;
  const cajasPrueba = parseInt(document.getElementById('ord-prueba-cajas')?.value) || 0;
  const cajasOrden  = parseInt(document.getElementById('ord-total-cajas')?.value)  || 0;
  const msg         = document.getElementById('ord-msg');

  if (!numero)      { showModalMsg(msg, 'Ingresa el N° de orden.', 'err'); return; }
  if (!cajasPrueba) { showModalMsg(msg, 'Ingresa las cajas de prueba.', 'err'); return; }
  if (!cajasOrden)  { showModalMsg(msg, 'Ingresa las cajas de la orden.', 'err'); return; }
  if (!ordenPantonesSeleccionados.length) { showModalMsg(msg, 'No hay Pantones seleccionados.', 'err'); return; }

  try {
    const res = await apiFetch('OrdenImpresion', 'POST', {
      numeroOrden:  numero,
      numeroCajas:  cajasOrden,
      pruebaColor:  cajasPrueba,
      idsFormulas:  ordenPantonesSeleccionados.map(p => p.id)
    });
    if (!res.ok) {
      const data = await res.json().catch(() => ({}));
      showModalMsg(msg, data || 'Error al guardar la orden.', 'err'); return;
    }
    document.getElementById('modal-orden').classList.remove('open');
    ordenPantonesSeleccionados = [];
    // Si está en panel órdenes, recargar
    if (document.getElementById('panel-ordenes')?.classList.contains('active')) cargarOrdenes();
  } catch (e) {
    showModalMsg(msg, 'Error: ' + e.message, 'err');
  }
}

// ════════════════════════════════════════
//  ÓRDENES — TABLA
// ════════════════════════════════════════
async function cargarOrdenes() {
  const tbody = document.getElementById('tabla-ordenes');
  tbody.innerHTML = '<tr><td colspan="7" class="loading-cell">Cargando...</td></tr>';
  try {
    const [resOrd, resForm] = await Promise.all([
      apiFetch('OrdenImpresion'),
      apiFetch('Formula')
    ]);
    const ordenes  = await resOrd.json();
    const formulas = await resForm.json();
    const formulaMap = Object.fromEntries(formulas.map(f => [f.id, f]));

    if (!ordenes.length) {
      tbody.innerHTML = '<tr><td colspan="7" class="loading-cell">No hay órdenes registradas.</td></tr>';
      return;
    }

    // Para cada orden obtener sus pantones vía OrdenFormula
    const rows = await Promise.all(ordenes.map(async o => {
      let pantonesDots = '—';
      try {
        const resOf = await apiFetch(`OrdenFormula?idOrden=${o.id}`).catch(() => null);
        if (resOf && resOf.ok) {
          const ofs = await resOf.json();
          pantonesDots = ofs.map(of => {
            const f = formulaMap[of.idFormula];
            if (!f) return '';
            const hex = resolverHex(f);
            return `<span title="${escHtml(f.nombreColor)}" style="width:18px;height:18px;border-radius:4px;
              background:${hex};display:inline-block;border:1px solid rgba(0,0,0,.1)"></span>`;
          }).join(' ') || '—';
        }
      } catch {}

      const estadoClass = o.estado ? 'ok' : 'danger';
      const estadoLabel = o.estado ? 'Completado' : 'Prueba de color';

      return `<tr style="cursor:pointer" onclick="verOrden(${o.id})">
        <td style="font-weight:700">#${o.numeroOrden}</td>
        <td>${new Date(o.fechaOrden).toLocaleDateString('es-CO')}</td>
        <td><div style="display:flex;gap:4px;flex-wrap:wrap">${pantonesDots}</div></td>
        <td>${o.pruebaColor}</td>
        <td>${o.numeroCajas}</td>
        <td>$${(o.costoTotal || 0).toLocaleString('es-CO')}</td>
        <td><span class="stock-badge ${estadoClass}">${estadoLabel}</span></td>
      </tr>`;
    }));

    tbody.innerHTML = rows.join('');
  } catch (e) {
    tbody.innerHTML = `<tr><td colspan="7" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`;
  }
}

// Ver detalle de una orden existente (popup)
async function verOrden(id) {
  document.getElementById('modal-orden-titulo').textContent = 'Detalle de Orden';
  document.getElementById('modal-orden-body').innerHTML =
    '<div class="loading-state" style="padding:30px">Cargando orden...</div>';
  document.getElementById('modal-orden').classList.add('open');

  try {
    const res    = await apiFetch(`OrdenImpresion/${id}`);
    const orden  = await res.json();

    const estadoLabel = orden.estado ? 'Completado' : 'Prueba de color';
    const estadoClass = orden.estado ? 'ok' : 'danger';

    document.getElementById('modal-orden-body').innerHTML = `
      <div style="display:flex;gap:20px;flex-wrap:wrap;margin-bottom:16px">
        <div><div style="font-size:.65rem;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-muted)">N° Orden</div>
          <div style="font-size:1.1rem;font-weight:800">#${orden.numeroOrden}</div></div>
        <div><div style="font-size:.65rem;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-muted)">Fecha</div>
          <div>${new Date(orden.fechaOrden).toLocaleDateString('es-CO')}</div></div>
        <div><div style="font-size:.65rem;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-muted)">Estado</div>
          <span class="stock-badge ${estadoClass}">${estadoLabel}</span></div>
        <div><div style="font-size:.65rem;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-muted)">Costo total</div>
          <div style="font-weight:700">$${(orden.costoTotal||0).toLocaleString('es-CO')}</div></div>
      </div>

      <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-bottom:16px">
        <div style="background:var(--surface-2);border-radius:8px;padding:12px;border:1px solid var(--border);text-align:center">
          <div style="font-size:.65rem;font-weight:700;text-transform:uppercase;color:var(--text-muted)">Cajas prueba</div>
          <div style="font-size:1.4rem;font-weight:800">${orden.pruebaColor}</div>
        </div>
        <div style="background:var(--surface-2);border-radius:8px;padding:12px;border:1px solid var(--border);text-align:center">
          <div style="font-size:.65rem;font-weight:700;text-transform:uppercase;color:var(--text-muted)">Cajas orden</div>
          <div style="font-size:1.4rem;font-weight:800">${orden.numeroCajas}</div>
        </div>
      </div>

      <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
        color:var(--text-muted);margin-bottom:10px;padding-bottom:6px;border-bottom:1px solid var(--border)">
        Pantones — Prueba (100g base)
      </div>
      ${orden.pantones.map(p => `
        <div style="margin-bottom:10px;padding:12px;background:var(--surface-2);border-radius:10px;border:1px solid var(--border)">
          <strong style="font-size:0.82rem">${escHtml(p.nombreColor)}</strong>
          <table style="width:100%;border-collapse:collapse;margin-top:6px">
            ${p.tintasPrueba.map(t => `
              <tr>
                <td style="padding:3px 0;color:var(--text);font-size:0.78rem">${escHtml(t.nombreTinta)}</td>
                <td style="text-align:right;font-weight:700;font-size:0.78rem">${t.gramosCalculados}g</td>
              </tr>`).join('')}
          </table>
        </div>`).join('')}

      <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
        color:var(--text-muted);margin:16px 0 10px;padding-bottom:6px;border-bottom:1px solid var(--border)">
        Cantidad de tinta para la orden completa (${orden.numeroCajas} cajas)
      </div>
      ${orden.pantones.map(p => `
        <div style="margin-bottom:10px;padding:12px;background:var(--surface-2);border-radius:10px;border:1px solid var(--border)">
          <strong style="font-size:0.82rem">${escHtml(p.nombreColor)}</strong>
          <table style="width:100%;border-collapse:collapse;margin-top:6px">
            ${p.tintasOrden.map(t => `
              <tr>
                <td style="padding:3px 0;color:var(--text);font-size:0.78rem">${escHtml(t.nombreTinta)}</td>
                <td style="text-align:right;font-weight:800;color:var(--navy);font-size:0.82rem">${t.gramosCalculados}g</td>
              </tr>`).join('')}
          </table>
        </div>`).join('')}

      <div style="display:flex;gap:10px;margin-top:16px">
        ${!orden.estado ? `
          <button class="btn-primary" style="flex:1" onclick="cambiarEstadoOrden(${orden.id}, true)">
            ✅ Marcar como Completada
          </button>` : `
          <button style="flex:1;padding:10px;border:1px solid var(--border);border-radius:8px;
            background:var(--surface-2);cursor:pointer;font-family:inherit;font-size:.78rem"
            onclick="cambiarEstadoOrden(${orden.id}, false)">
            Reabrir orden
          </button>`}
      </div>`;
  } catch (e) {
    document.getElementById('modal-orden-body').innerHTML =
      `<div class="error-cell">Error: ${escHtml(e.message)}</div>`;
  }
}

async function cambiarEstadoOrden(id, nuevoEstado) {
  try {
    const res = await apiFetch(`OrdenImpresion/${id}/estado`, 'PUT', nuevoEstado);
    if (!res.ok) { alert('No se pudo cambiar el estado.'); return; }
    document.getElementById('modal-orden').classList.remove('open');
    cargarOrdenes();
  } catch (e) { alert('Error: ' + e.message); }
}

function cerrarModalOrden(e) {
  if (e.target === document.getElementById('modal-orden'))
    document.getElementById('modal-orden').classList.remove('open');
}

// ════════════════════════════════════════
//  EMPRESAS
// ════════════════════════════════════════
async function cargarEmpresas() {
  const lista = document.getElementById('empresas-list');
  lista.innerHTML = '<div class="loading-state">Cargando empresas...</div>';
  try {
    const res = await apiFetch('Empresa');
    const empresas = await res.json();
    if (!empresas.length) { lista.innerHTML = '<div class="no-data">No hay empresas registradas.</div>'; return; }
    lista.innerHTML = empresas.map(e => `
      <div class="empresa-item" onclick="verEmpresa(${e.id}, this)">
        <div class="empresa-icon">${iniciales(e.nombreComercial)}</div>
        <div>
          <div class="empresa-name">${escHtml(e.nombreComercial)}</div>
          <div class="empresa-nit">${e.telefono || ''}</div>
        </div>
      </div>`).join('');
  } catch (e) { lista.innerHTML = `<div class="error-cell">Error: ${escHtml(e.message)}</div>`; }
}

async function verEmpresa(id, el) {
  document.querySelectorAll('.empresa-item').forEach(i => i.classList.remove('active'));
  el.classList.add('active');
  const detail = document.getElementById('empresa-detail');
  detail.innerHTML = '<div class="loading-state">Cargando...</div>';
  try {
    const [resEmp, resForm] = await Promise.all([
      apiFetch(`Empresa/${id}`),
      apiFetch(`Formula?idEmpresa=${id}`).catch(() => null)
    ]);
    const emp = await resEmp.json();
    const formulas = resForm ? (await resForm.json().catch(() => [])) : [];
    const exclusivas = formulas.filter ? formulas.filter(f => f.idEmpresa == id) : formulas;

    detail.innerHTML = `
      <div class="empresa-detail-name">${escHtml(emp.nombreComercial || emp.NombreComercial)}</div>
      <div class="empresa-detail-nit">Tel: ${emp.telefono || '—'} · ${emp.informacion || ''}</div>
      ${exclusivas.length ? `
        <div class="detail-section">
          <div class="detail-section-title">Colores exclusivos (${exclusivas.length})</div>
          <div class="empresa-colores-grid">
            ${exclusivas.map(f => {
              const hex = resolverHex(f);
              return `<div class="color-chip" onclick="abrirModalMezcla(${f.id},'${escHtml(f.nombreColor)}','${hex}')">
                <div class="chip-swatch" style="background:${hex}"></div>
                <div class="chip-info"><div class="chip-name">${escHtml(f.nombreColor)}</div></div>
              </div>`;
            }).join('')}
          </div>
        </div>` : '<div class="no-data" style="margin-top:20px">Sin colores exclusivos registrados.</div>'}`;
  } catch (e) { detail.innerHTML = `<div class="error-cell">Error: ${escHtml(e.message)}</div>`; }
}

// ════════════════════════════════════════
//  INVENTARIO — columnas: Nombre Color, Stock, StockMín, Costo/g, Estado
// ════════════════════════════════════════
async function cargarInventario() {
  const tbody = document.getElementById('tabla-inventario');
  const stats = document.getElementById('inv-stats');
  tbody.innerHTML = '<tr><td colspan="5" class="loading-cell">Cargando...</td></tr>';
  try {
    const res    = await apiFetch('TintaBase');
    const tintas = await res.json();
    const total  = tintas.length;
    const bajo   = tintas.filter(t => t.stockActual <= t.stockMinimo_alerta).length;
    const cero   = tintas.filter(t => t.stockActual === 0).length;
    document.getElementById('inv-count').textContent = total + ' tintas';
    stats.innerHTML = `
      <div class="inv-stat-card"><div class="inv-stat-val">${total}</div><div class="inv-stat-label">Tintas base</div></div>
      <div class="inv-stat-card ${bajo ? 'alert' : ''}"><div class="inv-stat-val">${bajo}</div><div class="inv-stat-label">Stock bajo</div></div>
      <div class="inv-stat-card ${cero ? 'alert' : ''}"><div class="inv-stat-val">${cero}</div><div class="inv-stat-label">Sin stock</div></div>`;
    tbody.innerHTML = tintas.map(t => {
      const esBajo = t.stockActual <= t.stockMinimo_alerta;
      const ec = t.stockActual === 0 ? 'danger' : esBajo ? 'warn' : 'ok';
      const el = t.stockActual === 0 ? 'Sin stock' : esBajo ? 'Bajo' : 'Normal';
      return `<tr>
        <td style="font-weight:600">${escHtml(t.nombreTinta)}</td>
        <td class="${esBajo ? 'stock-low' : ''}">${Math.round(t.stockActual)}g</td>
        <td>${Math.round(t.stockMinimo_alerta)}g</td>
        <td>$${(t.precioUnitario || 0).toLocaleString('es-CO')}/g</td>
        <td><span class="stock-badge ${ec}">${el}</span></td>
      </tr>`;
    }).join('') || '<tr><td colspan="5" class="loading-cell">No hay tintas.</td></tr>';
  } catch (e) { tbody.innerHTML = `<tr><td colspan="5" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`; }
}

// ════════════════════════════════════════
//  BASE DE DATOS
// ════════════════════════════════════════
async function cargarBaseDatos() {
  const tbody = document.getElementById('tabla-basedatos');
  tbody.innerHTML = '<tr><td colspan="7" class="loading-cell">Cargando...</td></tr>';
  try {
    const res      = await apiFetch('InventarioTinta');
    const registros = await res.json();
    if (!registros.length) {
      tbody.innerHTML = '<tr><td colspan="7" class="loading-cell">No hay registros. Usa el botón "+" para agregar.</td></tr>';
      return;
    }
    tbody.innerHTML = registros.map(r => `
      <tr>
        <td style="font-family:monospace;font-size:.78rem">${escHtml(r.idInterno || '—')}</td>
        <td>${escHtml(r.lote || '—')}</td>
        <td style="font-weight:600">${escHtml(r.nombre || '—')}</td>
        <td>${escHtml(r.fabricante || '—')}</td>
        <td>${escHtml(r.presentacion || '—')}</td>
        <td>$${(r.costo || 0).toLocaleString('es-CO')}</td>
        <td><button class="btn-danger" onclick="eliminarRegistroTinta(${r.id})">Eliminar</button></td>
      </tr>`).join('');
  } catch (e) { tbody.innerHTML = `<tr><td colspan="7" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`; }
}

function abrirModalNuevaTinta() {
  ['nt-idtinta','nt-idinterno','nt-lote','nt-nombre','nt-fabricante','nt-presentacion','nt-costo']
    .forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
  document.getElementById('nt-msg').style.display = 'none';
  document.getElementById('modal-tinta').classList.add('open');
}

function cerrarModalTinta(e) {
  if (e.target === document.getElementById('modal-tinta'))
    document.getElementById('modal-tinta').classList.remove('open');
}

async function guardarEntradaTinta() {
  const msg = document.getElementById('nt-msg');
  const dto = {
    idTintaBase:  parseInt(document.getElementById('nt-idtinta').value) || 0,
    idInterno:    document.getElementById('nt-idinterno').value.trim(),
    lote:         document.getElementById('nt-lote').value.trim(),
    nombre:       document.getElementById('nt-nombre').value.trim(),
    fabricante:   document.getElementById('nt-fabricante').value.trim(),
    presentacion: document.getElementById('nt-presentacion').value.trim(),
    costo:        parseFloat(document.getElementById('nt-costo').value) || 0
  };
  if (!dto.nombre) { showModalMsg(msg, 'El nombre es obligatorio.', 'err'); return; }
  try {
    const res = await apiFetch('InventarioTinta', 'POST', dto);
    if (!res.ok) { const d = await res.json().catch(()=>({})); showModalMsg(msg, d.mensaje || 'Error al guardar.', 'err'); return; }
    document.getElementById('modal-tinta').classList.remove('open');
    cargarBaseDatos();
  } catch (e) { showModalMsg(msg, 'Error: ' + e.message, 'err'); }
}

async function eliminarRegistroTinta(id) {
  if (!confirm('¿Eliminar este registro? Esta acción no se puede deshacer.')) return;
  try {
    const res = await apiFetch(`InventarioTinta/${id}`, 'DELETE');
    if (!res.ok) { alert('No se pudo eliminar.'); return; }
    cargarBaseDatos();
  } catch (e) { alert('Error: ' + e.message); }
}

// ════════════════════════════════════════
//  USUARIOS
// ════════════════════════════════════════
async function cargarUsuarios() {
  const tbody = document.getElementById('tabla-usuarios');
  tbody.innerHTML = '<tr><td colspan="5" class="loading-cell">Cargando...</td></tr>';
  try {
    const res = await apiFetch('Usuario');
    const usuarios = await res.json();
    tbody.innerHTML = usuarios.map(u => `
      <tr>
        <td><div class="user-row-avatar">${iniciales(u.nombre)}</div></td>
        <td>${escHtml(u.nombre)}</td>
        <td>${escHtml(u.correo)}</td>
        <td><span class="role-badge ${(u.rol||'').toLowerCase()==='administrador'?'admin':'user'}">${escHtml(u.rol||'—')}</span></td>
        <td>${u.correo !== currentUser.correo
          ? `<button class="btn-danger" onclick="eliminarUsuario(${u.id},'${escHtml(u.nombre)}')">Eliminar</button>`
          : '<span style="font-size:.7rem;color:var(--text-muted)">(tú)</span>'}</td>
      </tr>`).join('') || '<tr><td colspan="5" class="loading-cell">No hay usuarios.</td></tr>';
  } catch (e) { tbody.innerHTML = `<tr><td colspan="5" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`; }
}

async function eliminarUsuario(id, nombre) {
  if (!confirm(`¿Eliminar a "${nombre}"?`)) return;
  try {
    const res = await apiFetch(`Usuario/${id}`, 'DELETE');
    if (!res.ok) { alert('No se pudo eliminar.'); return; }
    cargarUsuarios();
  } catch (e) { alert('Error: ' + e.message); }
}

function abrirModalNuevoUsuario() {
  ['nu-nombre','nu-correo','nu-pass','nu-rol-id'].forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
  document.getElementById('nu-msg').style.display = 'none';
  document.getElementById('modal-usuario').classList.add('open');
}

function cerrarModalUsuario(e) {
  if (e.target === document.getElementById('modal-usuario'))
    document.getElementById('modal-usuario').classList.remove('open');
}

async function crearUsuario() {
  const nombre   = document.getElementById('nu-nombre').value.trim();
  const correo   = document.getElementById('nu-correo').value.trim();
  const password = document.getElementById('nu-pass').value.trim();
  const idRol    = parseInt(document.getElementById('nu-rol-id').value) || 0;
  const msg      = document.getElementById('nu-msg');
  if (!nombre || !correo || !password || !idRol) { showModalMsg(msg, 'Completa todos los campos.', 'err'); return; }
  try {
    const res  = await apiFetch('Usuario/register', 'POST', { nombre, correo, password, idRol });
    const data = await res.json();
    if (!res.ok) { showModalMsg(msg, data.mensaje || 'Error al crear usuario.', 'err'); return; }
    document.getElementById('modal-usuario').classList.remove('open');
    cargarUsuarios();
  } catch (e) { showModalMsg(msg, 'Error: ' + e.message, 'err'); }
}

// ════════════════════════════════════════
//  CONFIG
// ════════════════════════════════════════
function cargarConfig() {
  if (!currentUser) return;
  const ini = currentUser.nombre.split(' ').map(n => n[0]).join('').toUpperCase().slice(0,2);
  document.getElementById('config-avatar').textContent    = ini;
  document.getElementById('config-name').textContent      = currentUser.nombre;
  document.getElementById('config-correo').textContent    = currentUser.correo;
  document.getElementById('config-rol-badge').textContent = currentUser.rol || '—';
  ['pass-actual','pass-nueva','pass-confirmar'].forEach(id => document.getElementById(id).value = '');
  document.getElementById('config-msg').style.display = 'none';
}

async function cambiarContrasena() {
  const actual    = document.getElementById('pass-actual').value;
  const nueva     = document.getElementById('pass-nueva').value;
  const confirmar = document.getElementById('pass-confirmar').value;
  const msg       = document.getElementById('config-msg');
  if (!actual || !nueva || !confirmar) { showModalMsg(msg, 'Completa todos los campos.', 'err'); return; }
  if (nueva !== confirmar)             { showModalMsg(msg, 'Las contraseñas no coinciden.', 'err'); return; }
  if (nueva.length < 6)               { showModalMsg(msg, 'Mínimo 6 caracteres.', 'err'); return; }
  try {
    const res = await apiFetch('Usuario/cambiar-password', 'PUT', { passwordActual: actual, passwordNueva: nueva });
    if (!res.ok) { const d = await res.json().catch(()=>({})); showModalMsg(msg, d.mensaje||'Error.', 'err'); return; }
    showModalMsg(msg, 'Contraseña actualizada.', 'ok');
    ['pass-actual','pass-nueva','pass-confirmar'].forEach(id => document.getElementById(id).value = '');
  } catch (e) { showModalMsg(msg, 'Error: ' + e.message, 'err'); }
}

// ════════════════════════════════════════
//  HELPERS GLOBALES
// ════════════════════════════════════════
function escHtml(str) {
  if (!str) return '';
  return String(str).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function iniciales(nombre) {
  if (!nombre) return '??';
  return nombre.split(' ').map(n => n[0]).join('').toUpperCase().slice(0,2);
}

function showModalMsg(el, text, type) {
  el.textContent = text; el.className = 'config-msg ' + type; el.style.display = 'block';
  if (type === 'ok') setTimeout(() => { el.style.display = 'none'; }, 4000);
}
