// ════════════════════════════════════════
//  CONFIGURACIÓN
// ════════════════════════════════════════
const API_URL = window.location.hostname === '127.0.0.1' || window.location.hostname === 'localhost'
  ? 'http://localhost:5115/api' 
  : '/api';

// ════════════════════════════════════════
//  HELPERS DE ROL
//  Roles del backend: "Administrador", "Operarios", "ventas"
// ════════════════════════════════════════
function esAdmin()    { return (currentUser?.rol || '').toLowerCase() === 'administrador'; }
function esVentas()   { return (currentUser?.rol || '').toLowerCase() === 'ventas'; }
function esOperario() { return (currentUser?.rol || '').toLowerCase() === 'operarios'; }

// ════════════════════════════════════════
//  ESTADO GLOBAL
// ════════════════════════════════════════
let token       = localStorage.getItem('token') || null;
let currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
let allColors   = [];
let allEmpresas = [];
let allOrdenes  = []; // cache para filtrado local // cache para filtrado local

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
    const [resLocal, resC, resU, resKorchy] = await Promise.all([
      fetch('./pantoneslocales.json'),
      fetch('https://raw.githubusercontent.com/brettapeters/pantones/master/pantone-coated.json'),
      fetch('https://raw.githubusercontent.com/brettapeters/pantones/master/pantone-uncoated.json'),
      fetch('https://raw.githubusercontent.com/Korchy/blender-color-matching/master/pantone.json')
    ]);
    const locales  = resLocal.ok  ? await resLocal.json()  : {};
    const coated   = resC.ok       ? await resC.json()       : {};
    const uncoated = resU.ok       ? await resU.json()       : {};
    const korchy   = resKorchy.ok  ? await resKorchy.json()  : [];

   // 1. Cargar archivo local (Array de objetos)
    if (Array.isArray(locales)) {
      locales.forEach(item => {
        if (item && item.pantone && item.hex) {
          const clave = normalizarPantone(item.pantone);
          const hex = String(item.hex).trim();
          pantoneMap[clave] = hex.startsWith('#') ? hex : '#' + hex;
        }
      });
    }

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

  if (res.status === 401 || res.status === 403) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      headers['Authorization'] = `Bearer ${token}`;
      res = await fetch(`${API_URL}/${endpoint}`, { method, headers, body: opts.body });
    } else { 
      handleLogout(); 
      throw new Error('Sesión expirada'); 
    }
  }

  if (!res.ok) {
    const textErr = await res.text().catch(() => '');
    try {
      const jsonErr = JSON.parse(textErr);
      throw new Error(jsonErr.mensaje || jsonErr.title || `Error ${res.status}`);
    } catch {
      throw new Error(textErr || `Error en la solicitud (${res.status})`);
    }
  }

  // Si la respuesta es 204 No Content, se retorna null de inmediato
  if (res.status === 204) return null;

  // Validación de cuerpo vacío antes de hacer JSON.parse
  const text = await res.text();
  return text ? JSON.parse(text) : null;
}

async function tryRefreshToken() {
  const rt = localStorage.getItem('refreshToken');
  if (!rt) return false;

  try {
    const res = await fetch(`${API_URL}/Usuario/refresh-token`, {
      method: 'POST', 
      headers: { 'Content-Type': 'application/json' },
      // Enviar como objeto JSON DTO en lugar de string suelto
      body: JSON.stringify({ refreshToken: rt })
    });

    if (!res.ok) return false;
    
    const data = await res.json();
    token = data.token ?? data.Token;
    const newRefreshToken = data.refreshToken ?? data.RefreshToken;

    localStorage.setItem('token', token);
    if (newRefreshToken) {
      localStorage.setItem('refreshToken', newRefreshToken);
    }
    return true;
  } catch { 
    return false; 
  }
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

    // Ocultar todo por defecto primero
    const navEmpresas   = document.querySelector('[data-tab="empresas"]');
    const navOrdenes    = document.querySelector('[data-tab="ordenes"]');
    const navInventario = document.querySelector('.nav-item-inventario');
    const navBasedatos  = document.querySelector('.nav-item-basedatos');
    const navUsuarios   = document.querySelector('.nav-item-admin');

    [navEmpresas, navOrdenes, navInventario, navBasedatos, navUsuarios].forEach(el => {
      if (el) el.style.display = 'none';
    });

    if (rol === 'administrador') {
      // Todo visible
      [navEmpresas, navOrdenes, navInventario, navBasedatos, navUsuarios].forEach(el => {
        if (el) el.style.display = 'flex';
      });

    } else if (rol === 'ventas') {
      // Pantones (ya visible por defecto) + Inventario + Base de Datos
      if (navInventario) navInventario.style.display = 'flex';
      if (navBasedatos)  navBasedatos.style.display  = 'flex';

    } else if (rol === 'operarios') {
      // Pantones (ya visible por defecto) + Empresas + Órdenes
      if (navEmpresas) navEmpresas.style.display = 'flex';
      if (navOrdenes)  navOrdenes.style.display  = 'flex';
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
    // const formulas = await res.json();
    const conHex = res.map(f => ({ ...f, hexResuelto: resolverHex(f) }));
    allColors = ordenarPorColorReal(conHex);
    renderColorGrid(allColors);
  } catch (e) {
    grid.innerHTML = `<div class="error-cell">Error: ${e.message}</div>`;
  }
}

function renderColorGrid(res) {
  const grid = document.getElementById('color-grid');
  if (!res.length) { grid.innerHTML = '<div class="loading-state">No hay Pantones registrados.</div>'; return; }
  grid.innerHTML = res.map(f => {
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
    .then(data => {
      document.getElementById('modal-orden-body').innerHTML = `
        <div style="display:flex;align-items:center;gap:12px;margin-bottom:16px">
          <div style="width:48px;height:48px;border-radius:12px;background:${hex};border:2px solid var(--border);flex-shrink:0"></div>
          <div>
            <div style="font-size:0.72rem;color:var(--text-muted);font-weight:700;letter-spacing:.1em;text-transform:uppercase">Proporciones para 100g</div>
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
                <td><span class="stock-badge ${t.stockSuficiente ? 'ok' : 'warn'}">${t.stockSuficiente ? 'Suficiente' : 'Insuficiente'}</span></td>
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
  ordenPantonesSeleccionados.forEach(p => cargarTintasPrueba(p));
}

function renderFormNuevaOrden() {
  return `
    <div class="form-group-flat" style="margin-bottom:16px">
      <label>N° Orden</label>
      <input type="text" id="ord-numero" placeholder="Ej: 001" min="1">
    </div>

    <div style="margin-bottom:4px">
      <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
        color:var(--text-muted);margin-bottom:10px;padding-bottom:6px;border-bottom:1px solid var(--border)">
        Pantones seleccionados — Proporciones para 100g
      </div>
      ${ordenPantonesSeleccionados.map(p => `
        <div style="margin-bottom:10px;padding:12px;background:var(--surface-2);border-radius:10px;border:1px solid var(--border)">
          <div style="display:flex;align-items:center;gap:8px;margin-bottom:8px">
            <div style="width:18px;height:18px;border-radius:5px;background:${p.hex};border:1px solid rgba(0,0,0,.1);flex-shrink:0"></div>
            <strong style="font-size:0.82rem">${escHtml(p.nombre)}</strong>
          </div>
          <div id="tintas-prueba-${p.id}" style="font-size:0.78rem;color:var(--text-muted)">Cargando tintas...</div>
        </div>`).join('')}
    </div>

    <div id="ord-msg" class="config-msg" style="display:none"></div>
    <button class="btn-primary" onclick="guardarOrden()" style="width:100%;margin-top:12px">
      Guardar Orden
    </button>`;
}

// Carga y muestra las tintas de prueba (100g) para un pantone
async function cargarTintasPrueba(pantone) {
  const cont = document.getElementById('tintas-prueba-' + pantone.id);
  if (!cont) return;
  try {
    const res  = await apiFetch('Mezcla/calcular', 'POST', { idFormula: pantone.id, pesoTotalGramos: 100 });
    // const data = await res.json();
    pantone.tintas = res.tintas; // guardar para la regla de tres en verOrden
    cont.innerHTML = `
      <table style="width:100%;border-collapse:collapse">
        ${res.tintas.map(t => `
          <tr>
            <td style="padding:3px 0;color:var(--text)">${escHtml(t.nombreTinta)}</td>
            <td style="text-align:right;font-weight:700;padding:3px 0">${t.gramosNecesarios}g</td>
          </tr>`).join('')}
      </table>`;
  } catch {
    if (cont) cont.textContent = 'Error al cargar tintas.';
  }
}

async function guardarOrden() {
  const numero = document.getElementById('ord-numero')?.value.trim();
  const msg    = document.getElementById('ord-msg');

  if (!numero) { showModalMsg(msg, 'Ingresa el N° de orden.', 'err'); return; }
  if (!ordenPantonesSeleccionados.length) { showModalMsg(msg, 'No hay Pantones seleccionados.', 'err'); return; }

  try {
    const res = await apiFetch('OrdenImpresion', 'POST', {
      numeroOrden: String(numero),
      numeroCajas: 0,
      pruebaColor: 0,
      idsFormulas: ordenPantonesSeleccionados.map(p => p.id)
    });
    // if (!res.ok) {
    //   const text = await res.text();
    //   showModalMsg(msg, text || 'Error al guardar la orden.', 'err'); return;
    // }
    document.getElementById('modal-orden').classList.remove('open');
    ordenPantonesSeleccionados = [];
    const btnOrdenes = document.querySelector('[data-tab="ordenes"]');
    switchTab('ordenes', btnOrdenes);
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
    const [ordenes, formulas, ordenFormulas] = await Promise.all([
      apiFetch('OrdenImpresion').catch(() => []),
      apiFetch('Formula').catch(() => []),
      apiFetch('OrdenFormula').catch(() => [])
    ]);
    if (!ordenes || !ordenes.length) {
      tbody.innerHTML = '<tr><td colspan="7" class="loading-cell">No hay órdenes registradas.</td></tr>';
      return;
    }
    const formulaMap = Object.fromEntries(formulas.map(f => [f.id ?? f.Id, f]));
    allOrdenes = ordenes.map(o => {
      const idOrden = o.id ?? o.Id;
      const relacionesDeEstaOrden = ordenFormulas.filter(of => {
        const fkOrden = of.idOrdenImpresion ?? of.IdOrdenImpresion ?? of.idOrden ?? of.IdOrden;
        return fkOrden == idOrden;
      });
      const pantonesDots = relacionesDeEstaOrden.map(of => {
        const idFormula = of.idFormula ?? of.IdFormula;
        const f = formulaMap[idFormula];
        if (!f) return '';
        const hex = resolverHex(f);
        const nombreColor = f.nombreColor ?? f.NombreColor ?? '';
        return `<span title="${escHtml(nombreColor)}" style="width:18px;height:18px;border-radius:4px;background:${hex};display:inline-block;border:1px solid rgba(0,0,0,.1)"></span>`;
      }).filter(Boolean).join(' ') || '—';
      return { ...o, _pantonesDots: pantonesDots };
    });
    renderOrdenes(allOrdenes);
  } catch (e) {
    console.error('Error en cargarOrdenes:', e);
    tbody.innerHTML = `<tr><td colspan="7" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`;
  }
}

function renderOrdenes(lista) {
  const tbody = document.getElementById('tabla-ordenes');
  if (!lista.length) { tbody.innerHTML = '<tr><td colspan="7" class="loading-cell">Sin resultados.</td></tr>'; return; }
  tbody.innerHTML = lista.map(o => {
    const idOrden     = o.id ?? o.Id;
    const estado      = o.estado ?? o.Estado;
    const estadoClass = estado ? 'ok' : 'danger';
    const estadoLabel = estado ? 'Completado' : 'Prueba de color';
    const numeroOrden = o.numeroOrden ?? o.NumeroOrden;
    const fechaOrden  = o.fechaOrden  ?? o.FechaOrden;
    const pruebaColor = o.pruebaColor ?? o.PruebaColor ?? 0;
    const numeroCajas = o.numeroCajas ?? o.NumeroCajas ?? 0;
    const costoTotal  = o.costoTotal  ?? o.CostoTotal  ?? 0;
    return `<tr style="cursor:pointer" onclick="verOrden(${idOrden})">
      <td style="font-weight:700">#${numeroOrden}</td>
      <td>${new Date(fechaOrden).toLocaleDateString('es-CO')}</td>
      <td><div style="display:flex;gap:4px;flex-wrap:wrap">${o._pantonesDots}</div></td>
      <td>${pruebaColor}</td>
      <td>${numeroCajas}</td>
      <td>$${costoTotal.toLocaleString('es-CO')}</td>
      <td><span class="stock-badge ${estadoClass}">${estadoLabel}</span></td>
    </tr>`;
  }).join('');
}

function filtrarOrdenes() {
  const q     = (document.getElementById('orden-search-num')?.value || '').trim().toLowerCase();
  const fecha = document.getElementById('orden-search-fecha')?.value || '';
  const filtradas = allOrdenes.filter(o => {
    const numero     = String(o.numeroOrden ?? o.NumeroOrden ?? '').toLowerCase();
    const fechaOrden = new Date(o.fechaOrden ?? o.FechaOrden).toISOString().split('T')[0];
    return (!q || numero.includes(q)) && (!fecha || fechaOrden === fecha);
  });
  renderOrdenes(filtradas);
}

function limpiarFiltrosOrdenes() {
  const num   = document.getElementById('orden-search-num');
  const fecha = document.getElementById('orden-search-fecha');
  if (num)   num.value   = '';
  if (fecha) fecha.value = '';
  renderOrdenes(allOrdenes);
}


// Ver detalle de una orden existente (popup completo)
async function verOrden(id) {
  document.getElementById('modal-orden-titulo').textContent = 'Orden #...';
  document.getElementById('modal-orden-body').innerHTML =
    '<div class="loading-state" style="padding:30px">Cargando orden...</div>';
  document.getElementById('modal-orden').classList.add('open');

  try {
    const res = await apiFetch(`OrdenImpresion/${id}`);
    document.getElementById('modal-orden-titulo').textContent = `Orden #${res.numeroOrden}`;

    const estadoLabel = res.estado ? 'Completado' : 'Prueba de color';
    const estadoClass = res.estado ? 'ok' : 'danger';

    document.getElementById('modal-orden-body').innerHTML = `
      <div style="display:flex;gap:16px;flex-wrap:wrap;align-items:center;margin-bottom:16px">
        <div style="flex:1">
          <div style="font-size:.65rem;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-muted)">Fecha</div>
          <div style="font-weight:600">${new Date(res.fechaOrden).toLocaleDateString('es-CO')}</div>
        </div>
        <span class="stock-badge ${estadoClass}">${estadoLabel}</span>
      </div>

      <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
        color:var(--text-muted);margin-bottom:10px;padding-bottom:6px;border-bottom:1px solid var(--border)">
        Pantones seleccionados — Proporciones para 100g
      </div>
      ${res.pantones.map(p => `
        <div style="margin-bottom:10px;padding:12px;background:var(--surface-2);border-radius:10px;border:1px solid var(--border)">
          <strong style="font-size:0.82rem">${escHtml(p.nombreColor)}</strong>
          <table style="width:100%;border-collapse:collapse;margin-top:6px">
            ${(p.mezclaPrueba?.tintas || []).map(t => `
              <tr>
                <td style="padding:3px 0;color:var(--text);font-size:0.78rem">${escHtml(t.nombreTinta)}</td>
                <td style="text-align:right;font-weight:700;font-size:0.78rem">${t.gramosNecesarios}g</td>
              </tr>`).join('')}
          </table>
        </div>`).join('')}

      <div style="background:var(--surface-2);border-radius:10px;padding:16px;border:1px solid var(--border);margin:16px 0">
        <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
          color:var(--text-muted);margin-bottom:12px">Cajas a producir</div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
          <div class="form-group-flat" style="margin:0">
            <label>N° cajas con prueba de color</label>
            <input type="number" id="ord-prueba-cajas" min="1" placeholder="Ej: 6"
              value="${res.pruebaColor || ''}" oninput="actualizarCalculoOrden()">
          </div>
          <div class="form-group-flat" style="margin:0">
            <label>N° cajas de la orden</label>
            <input type="number" id="ord-total-cajas" min="1" placeholder="Ej: 850"
              value="${res.numeroCajas || ''}" oninput="actualizarCalculoOrden()">
          </div>
        </div>
      </div>

      <div id="calculo-orden-section" style="display:none;margin-bottom:16px">
        <div style="font-size:0.7rem;font-weight:700;letter-spacing:.12em;text-transform:uppercase;
          color:var(--text-muted);margin-bottom:10px;padding-bottom:6px;border-bottom:1px solid var(--border)">
          Cantidad de tinta a preparar para la orden
        </div>
        <div id="calculo-orden-resultado"></div>
        <div style="margin-top:12px;padding:12px;border-radius:8px;background:var(--navy);color:#fff;
          display:flex;justify-content:space-between;align-items:center">
          <span style="font-size:0.8rem;font-weight:700;letter-spacing:.06em;text-transform:uppercase">Costo total estimado</span>
          <span id="calculo-costo-valor" style="font-size:1.2rem;font-weight:800">$0</span>
        </div>
      </div>

      <!-- UN SOLO BOTÓN: guarda cajas + marca completada -->
      <div style="display:flex;gap:10px;margin-top:4px">
        <button class="btn-primary" style="flex:1;background:linear-gradient(135deg,#16a34a,#15803d)"
          onclick="guardarYCompletar(${res.id})">
          ✅ Guardar y completar
        </button>
        ${res.estado
          ? `<button style="flex:1;padding:10px;border:1px solid var(--border);border-radius:8px;
              background:var(--surface-2);cursor:pointer;font-family:inherit;font-size:.78rem"
              onclick="cambiarEstadoOrden(${res.id}, false)">↩ Reabrir orden</button>`
          : ''}
      </div>
      <div id="ord-upd-msg" class="config-msg" style="display:none;margin-top:8px"></div>`;

    _ordenDetalleActual = res;
    if (res.pruebaColor > 0 && res.numeroCajas > 0) actualizarCalculoOrden();

  } catch (e) {
    document.getElementById('modal-orden-body').innerHTML =
      `<div class="error-cell">Error: ${escHtml(e.message)}</div>`;
  }
}


// Orden actualmente abierta en el popup (para cálculo reactivo)
let _ordenDetalleActual = null;

// Recalcula en tiempo real cuando cambian las cajas
function actualizarCalculoOrden() {
  const cajasPrueba = parseFloat(document.getElementById('ord-prueba-cajas')?.value) || 0;
  const cajasOrden  = parseFloat(document.getElementById('ord-total-cajas')?.value)  || 0;
  const seccion     = document.getElementById('calculo-orden-section');
  const resultado   = document.getElementById('calculo-orden-resultado');
  const costoEl     = document.getElementById('calculo-costo-valor');
  if (!seccion || !resultado || !_ordenDetalleActual) return;

  if (!cajasPrueba || !cajasOrden) { seccion.style.display = 'none'; return; }
  seccion.style.display = 'block';

  let costoTotal = 0;

  resultado.innerHTML = _ordenDetalleActual.pantones.map(p => {
    const tintas = p.mezclaPrueba?.tintas || [];
    if (!tintas.length) return '';
    return `
      <div style="margin-bottom:10px;padding:10px;background:var(--surface-2);
        border-radius:8px;border:1px solid var(--border)">
        <strong style="font-size:0.8rem">${escHtml(p.nombreColor)}</strong>
        <table style="width:100%;border-collapse:collapse;margin-top:6px">
          <thead>
            <tr>
              <th style="text-align:left;font-size:.65rem;font-weight:700;letter-spacing:.08em;
                text-transform:uppercase;color:var(--text-muted);padding:3px 0">Tinta</th>
              <th style="text-align:right;font-size:.65rem;font-weight:700;letter-spacing:.08em;
                text-transform:uppercase;color:var(--text-muted);padding:3px 0">Gramos</th>
              <th style="text-align:right;font-size:.65rem;font-weight:700;letter-spacing:.08em;
                text-transform:uppercase;color:var(--text-muted);padding:3px 0">Costo/g</th>
              <th style="text-align:right;font-size:.65rem;font-weight:700;letter-spacing:.08em;
                text-transform:uppercase;color:var(--text-muted);padding:3px 0">Subtotal</th>
            </tr>
          </thead>
          <tbody>
            ${tintas.map(t => {
              // Regla de tres: X = (cajasOrden × gramosBase) / cajasPrueba
              const gramosOrden   = Math.round((cajasOrden * t.gramosNecesarios) / cajasPrueba);
              const costoPorGramo = t.precioUnitario || 0;
              const subtotal      = gramosOrden * costoPorGramo;
              costoTotal += subtotal;
              return `
                <tr>
                  <td style="padding:4px 0;font-size:0.78rem;color:var(--text)">${escHtml(t.nombreTinta)}</td>
                  <td style="text-align:right;font-weight:700;font-size:0.78rem;color:var(--navy);
                    padding:4px 0">${gramosOrden}g</td>
                  <td style="text-align:right;font-size:0.72rem;color:var(--text-muted);padding:4px 0">
                    $${costoPorGramo.toLocaleString('es-CO')}</td>
                  <td style="text-align:right;font-weight:700;font-size:0.78rem;padding:4px 0">
                    $${subtotal.toLocaleString('es-CO')}</td>
                </tr>`;
            }).join('')}
          </tbody>
        </table>
      </div>`;
  }).join('');

  if (costoEl) {
    costoEl.textContent = '$' + costoTotal.toLocaleString('es-CO', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    });
  }
}

// Guarda cajas + costo + marca como completada en una sola acción
async function guardarYCompletar(id) {
  const cajasPrueba = parseInt(document.getElementById('ord-prueba-cajas')?.value) || 0;
  const cajasOrden  = parseInt(document.getElementById('ord-total-cajas')?.value)  || 0;
  const msg         = document.getElementById('ord-upd-msg');

  if (!cajasPrueba || !cajasOrden) {
    showModalMsg(msg, 'Ingresa ambos valores de cajas.', 'err'); return;
  }

  // Leer costo total del display calculado
  const costoText  = document.getElementById('calculo-costo-valor')?.textContent || '0';
  const costoLimpio = parseInt(costoText.replace(/\D/g, ''), 10);

  try {
    // 1. Guardar cajas y costo
    const r1 = await apiFetch(`OrdenImpresion/${id}/cajas`, 'PUT', {
      pruebaColor: cajasPrueba,
      numeroCajas: cajasOrden,
      costoTotal:  costoLimpio
    });
    if (!r1 || r1?.error) { showModalMsg(msg, 'No se pudo actualizar las cajas.', 'err'); return; }

    // 2. Marcar como completada
    cambiarEstadoOrden(id, true);

    document.getElementById('modal-orden').classList.remove('open');
    cargarOrdenes();
  } catch (e) { showModalMsg(msg, 'Error: ' + e.message, 'err'); }
}


async function cambiarEstadoOrden(id, nuevoEstado) {
  try {
    const res = await apiFetch(`OrdenImpresion/${id}/estado`, 'PUT', nuevoEstado);
    if (!res || res?.error) { alert('No se pudo cambiar el estado.'); return; }
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
    // allEmpresas = await res.json();
    renderEmpresas(res);
  } catch (e) { lista.innerHTML = `<div class="error-cell">Error: ${escHtml(e.message)}</div>`; }
}

function filtrarEmpresas() {
  const q = document.getElementById('empresa-search')?.value.trim().toLowerCase() || '';
  renderEmpresas(q ? allEmpresas.filter(e =>
    (e.nombreComercial || '').toLowerCase().includes(q) ||
    (e.telefono || '').toLowerCase().includes(q)
  ) : allEmpresas);
}

function renderEmpresas(lista) {
  const cont = document.getElementById('empresas-list');
 
  // Resetear siempre el panel de detalle al redibujar la lista
  document.getElementById('empresa-detail').innerHTML = `
    <div class="empty-detail">
      <div class="empty-icon-lg">🏢</div>
      <p>Selecciona una empresa para ver su información</p>
    </div>`;
 
  if (!lista.length) {
    cont.innerHTML = '<div class="no-data">Sin resultados.</div>';
    return;
  }
  cont.innerHTML = lista.map(e => `
    <div class="empresa-item" onclick="verEmpresa(${e.id}, this)">
      <div class="empresa-icon">${iniciales(e.nombreComercial)}</div>
      <div>
        <div class="empresa-name">${escHtml(e.nombreComercial)}</div>
      </div>
    </div>`).join('');
}

function abrirModalNuevaEmpresa() {
  ['ne-nombre', 'ne-informacion'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.value = '';
  });
  document.getElementById('ne-msg').style.display = 'none';
  document.getElementById('modal-nueva-empresa').classList.add('open');
}

function cerrarModalNuevaEmpresa(e) {
  if (e.target === document.getElementById('modal-nueva-empresa'))
    document.getElementById('modal-nueva-empresa').classList.remove('open');
}

async function guardarNuevaEmpresa() {
  const msg    = document.getElementById('ne-msg');
  const nombre = document.getElementById('ne-nombre').value.trim();

  if (!nombre) { showModalMsg(msg, 'El nombre comercial es obligatorio.', 'err'); return; }

  const dto = {
    nombreComercial: nombre,
    telefono:        '',
    información:     document.getElementById('ne-informacion').value.trim()
  };

  try {
    const res = await apiFetch('Empresa', 'POST', dto);
    if (!res || res?.error) {
      showModalMsg(msg, res || 'No se pudo guardar la empresa.', 'err'); return;
    }
    document.getElementById('modal-nueva-empresa').classList.remove('open');
    cargarEmpresas(); // refresca la lista
  } catch (e) {
    showModalMsg(msg, 'Error: ' + e.message, 'err');
  }
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
    const listaFormulas = Array.isArray(resForm) ? resForm : [];
    const exclusivas = listaFormulas.filter(f => (f.idEmpresa ?? f.IdEmpresa) == id);

    // Leer informacion sin tilde (así viene del backend C#)
    const infoActual = resEmp.informacion || resEmp.Informacion || resEmp.información || '';

    detail.innerHTML = `
      <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:12px;margin-bottom:4px">
        <div class="empresa-detail-name">${escHtml(resEmp.nombreComercial || resEmp.NombreComercial)}</div>
        <button onclick="eliminarEmpresa(${id})"
          title="Eliminar empresa"
          style="flex-shrink:0;padding:7px 10px;background:rgba(200,32,46,0.1);border:1px solid rgba(200,32,46,0.25);
            border-radius:8px;cursor:pointer;color:var(--red);transition:all .2s;display:flex;align-items:center;gap:6px;
            font-family:inherit;font-size:.75rem;font-weight:700"
          onmouseover="this.style.background='var(--red)';this.style.color='#fff'"
          onmouseout="this.style.background='rgba(200,32,46,0.1)';this.style.color='var(--red)'">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
            <path d="M10 11v6"/><path d="M14 11v6"/>
            <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/>
          </svg>
          Eliminar
        </button>
      </div>

      <div class="detail-section">
        <div class="detail-section-title">Información / Comentarios</div>
        <div style="display:flex;gap:8px;align-items:center">
          <input
            type="text"
            id="info-empresa-${id}"
            value="${escHtml(infoActual)}"
            placeholder="Añade un comentario sobre esta empresa..."
            style="flex:1;padding:9px 12px;border:1px solid var(--border);border-radius:8px;
                   background:var(--surface-2);font-family:inherit;font-size:0.82rem;
                   color:var(--text);outline:none;"
          />
          <button
            class="btn-primary"
            style="white-space:nowrap;padding:9px 16px"
            onclick="guardarInformacionEmpresa(${id})">
            Guardar
          </button>
        </div>
        <div id="info-empresa-msg-${id}" style="display:none;margin-top:6px;font-size:0.75rem"></div>
      </div>`;
  } catch (e) { detail.innerHTML = `<div class="error-cell">Error: ${escHtml(e.message)}</div>`; }
}

async function guardarInformacionEmpresa(id) {
  const input = document.getElementById(`info-empresa-${id}`);
  const msgEl = document.getElementById(`info-empresa-msg-${id}`);
  if (!input) return;

  try {
    // Primero traer la empresa completa para no pisar otros campos con el PUT
    const resGet = await apiFetch(`Empresa/${id}`);
    // const emp    = await resGet.json();

    // Actualizar solo el campo informacion
    const body = {
      ...resGet,
      informacion: input.value.trim()   // sin tilde — nombre exacto en la entidad C#
    };

    const res = await apiFetch(`Empresa/${id}`, 'PUT', body);

    if (res.ok) {
      if (msgEl) {
        msgEl.textContent  = '✅ Información guardada.';
        msgEl.style.color  = '#16a34a';
        msgEl.style.display = 'block';
        setTimeout(() => { msgEl.style.display = 'none'; }, 3000);
      }
    } else {
      // const data = await res.json().catch(() => ({}));
      if (msgEl) {
        msgEl.textContent  = '⚠ ' + (res.mensaje || 'No se pudo guardar.');
        msgEl.style.color  = 'var(--red)';
        msgEl.style.display = 'block';
      }
    }
  } catch (error) {
    if (msgEl) {
      msgEl.textContent  = '⚠ Error de conexión.';
      msgEl.style.color  = 'var(--red)';
      msgEl.style.display = 'block';
    }
    console.error('Error al actualizar empresa:', error);
  }
}

// ════════════════════════════════════════
//  ELIMINAR EMPRESA
// ════════════════════════════════════════
async function eliminarEmpresa(id) {
  if (!confirm('¿Eliminar esta empresa? Esta acción no se puede deshacer.')) return;
  try {
    const res = await apiFetch(`Empresa/${id}`, 'DELETE');
    if (res?.error) { alert('No se pudo eliminar la empresa.'); return; }
    // Limpiar el panel de detalle y recargar la lista
    document.getElementById('empresa-detail').innerHTML = `
      <div class="empty-detail">
        <div class="empty-icon-lg">🏢</div>
        <p>Selecciona una empresa para ver su información</p>
      </div>`;
    cargarEmpresas();
  } catch (e) { alert('Error: ' + e.message); }
}

// ════════════════════════════════════════
//  INVENTARIO — columnas: Nombre Color, Stock, StockMín, Costo/g, Estado
// ════════════════════════════════════════
async function cargarInventario() {
  const tbody = document.getElementById('tabla-inventario');
  const stats = document.getElementById('inv-stats');
  tbody.innerHTML = '<tr><td colspan="6" class="loading-cell">Cargando...</td></tr>';
  try {
    const res    = await apiFetch('TintaBase');
    // const tintas = await res.json();
    const total  = res.length;
    const bajo   = res.filter(t => t.stockActual <= t.stockMinimo_alerta).length;
    const cero   = res.filter(t => t.stockActual === 0).length;
    document.getElementById('inv-count').textContent = total + ' tintas';
    stats.innerHTML = `
      <div class="inv-stat-card"><div class="inv-stat-val">${total}</div><div class="inv-stat-label">Tintas base</div></div>
      <div class="inv-stat-card ${bajo ? 'alert' : ''}"><div class="inv-stat-val">${bajo}</div><div class="inv-stat-label">Stock bajo</div></div>
      <div class="inv-stat-card ${cero ? 'alert' : ''}"><div class="inv-stat-val">${cero}</div><div class="inv-stat-label">Sin stock</div></div>`;
    tbody.innerHTML = res.map(t => {
      const esBajo = t.stockActual <= t.stockMinimo_alerta;
      const ec = t.stockActual === 0 ? 'danger' : esBajo ? 'warn' : 'ok';
      const el = t.stockActual === 0 ? 'Sin stock' : esBajo ? 'Bajo' : 'Normal';
      return `<tr>
        <td style="font-weight:600">${escHtml(t.nombreTinta)}</td>
        <td class="${esBajo ? 'stock-low' : ''}">${t.stockActual/1000}kg</td>
        <td>${t.stockMinimo_alerta / 1000}kg</td>
        <td>$${(t.precioUnitario || 0).toLocaleString('es-CO')}/g</td>
        <td><span class="stock-badge ${ec}">${el}</span></td>
        <td><button class="btn-primary" style="padding:5px 12px;font-size:.72rem"
          onclick="abrirEditarTinta(${t.id}, '${escHtml(t.nombreTinta)}', ${t.stockActual}, ${t.stockMinimo_alerta}, ${t.precioUnitario || 0})">
          ✏ Editar
        </button></td>
      </tr>`;
    }).join('') || '<tr><td colspan="6" class="loading-cell">No hay tintas.</td></tr>';
  } catch (e) { tbody.innerHTML = `<tr><td colspan="5" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`; }
}

// ════════════════════════════════════════
//  BASE DE DATOS
// ════════════════════════════════════════
// ════════════════════════════════════════
//  EDITAR TINTA BASE (modal)
// ════════════════════════════════════════
let _editarTintaId = null;

function abrirEditarTinta(id, nombre, stockActual, stockMinimo, precio) {
  _editarTintaId = id;
  document.getElementById('editar-tinta-titulo').textContent = `Editar — ${nombre}`;
  document.getElementById('et-stock-actual').value  = stockActual/1000;
  document.getElementById('et-stock-minimo').value  = stockMinimo/1000;
  document.getElementById('et-precio').value        = precio;
  document.getElementById('et-msg').style.display   = 'none';
  document.getElementById('modal-editar-tinta').classList.add('open');
}

function cerrarModalEditarTinta(e) {
  if (e.target === document.getElementById('modal-editar-tinta'))
    document.getElementById('modal-editar-tinta').classList.remove('open');
}

async function guardarEdicionTinta() {
  const msg         = document.getElementById('et-msg');
  const stockActual = parseFloat(document.getElementById('et-stock-actual').value);
  const stockMinimo = parseFloat(document.getElementById('et-stock-minimo').value);
  const precio      = parseFloat(document.getElementById('et-precio').value);
  const nombreTinta = document.getElementById('editar-tinta-titulo').textContent.replace('Editar — ', '').trim();

  if (isNaN(stockActual) || isNaN(stockMinimo) || isNaN(precio)) {
    showModalMsg(msg, 'Completa todos los campos con valores válidos.', 'err'); return;
  }

  try {
    const res = await apiFetch(`TintaBase/${_editarTintaId}`, 'PUT', {
      id: _editarTintaId,
      nombreTinta: nombreTinta,
      stockActual:       stockActual*1000,
      stockMinimo_alerta: stockMinimo*1000,
      precioUnitario:    precio
    });
    if (!res || res.error) { showModalMsg(msg, 'No se pudo guardar.', 'err'); return; }
    document.getElementById('modal-editar-tinta').classList.remove('open');
    cargarInventario();
  } catch (e) { showModalMsg(msg, 'Error: ' + e.message, 'err'); }
}

async function cargarBaseDatos() {
  const tbody = document.getElementById('tabla-basedatos');
  tbody.innerHTML = '<tr><td colspan="8" class="loading-cell">Cargando...</td></tr>';
  try {
    const res      = await apiFetch('InventarioTinta');
    // const registros = await res.json();
    console.log(res);
    
    if (!res.length) {
      tbody.innerHTML = '<tr><td colspan="8" class="loading-cell">No hay registros. Usa el botón "+" para agregar.</td></tr>';
      return;
    }
    tbody.innerHTML = res.map(r => `
      <tr>
        <td style="font-family:monospace;font-size:.78rem">${escHtml(r.idInterno || '—')}</td>
        <td>${escHtml(r.lote || '—')}</td>
        <td style="font-weight:600">${escHtml(r.nombre || '—')}</td>
        <td>${escHtml(r.proveedor || '—')}</td>
        <td>${escHtml(r.fabricante || '—')}</td>
        <td>${escHtml(r.presentacion || '—')}kg</td>
        <td>$${(r.costo || 0).toLocaleString('es-CO')}</td>
        <td><button class="btn-danger" onclick="eliminarRegistroTinta(${r.id})">Eliminar</button></td>
      </tr>`).join('');
  } catch (e) { tbody.innerHTML = `<tr><td colspan="8" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`; }
}

async function cargarTintasBase() {
    const select = document.getElementById('nt-idtinta');
    if (!select) return;

    select.innerHTML = '<option value="">Cargando tintas...</option>';

    try {
        const res = await apiFetch('TintaBase');
        if (!res || res.error) throw new Error('No se pudieron cargar las tintas base.');

        select.innerHTML = '<option value="">-- Selecciona una tinta --</option>';

        if (!res || !res.length) {
            select.innerHTML = '<option value="">No hay tintas disponibles</option>';
            return;
        }

        // Ordenar alfabéticamente por nombre
        res.sort((a, b) => (a.nombreTinta || '').localeCompare(b.nombreTinta || ''));

        res.forEach(tinta => {
            const option = document.createElement('option');
            option.value = tinta.id;                    // ID para el backend
            option.textContent = tinta.nombreTinta;     // Nombre visible al usuario
            option.dataset.nombre = tinta.nombreTinta;  // Guardamos nombre para autocompletar
            select.appendChild(option);
        });

        // Al seleccionar una tinta, autocompletar el campo "Nombre de la Tinta"
        select.onchange = () => {
            const seleccionada = select.options[select.selectedIndex];
            const campoNombre  = document.getElementById('nt-nombre');
            if (campoNombre && seleccionada.value) {
                campoNombre.value = seleccionada.dataset.nombre || '';
            } else if (campoNombre) {
                campoNombre.value = '';
            }
        };

    } catch (e) {
        console.error('Error cargando tintas base:', e);
        select.innerHTML = '<option value="">Error al cargar las tintas</option>';
    }
}

async function abrirModalNuevaTinta() {
    // Limpiar todos los campos
    ['nt-idinterno','nt-lote','nt-nombre','nt-fabricante', 'nt-proveedor','nt-presentacion','nt-costo']
        .forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });

    // Resetear select al estado inicial
    const select = document.getElementById('nt-idtinta');
    if (select) select.innerHTML = '<option value="">-- Selecciona una tinta --</option>';

    document.getElementById('nt-msg').style.display = 'none';
    document.getElementById('modal-tinta').classList.add('open');

    // Cargar las tintas en el selector
    await cargarTintasBase();
}

function cerrarModalTinta(e) {
  if (e.target === document.getElementById('modal-tinta'))
    document.getElementById('modal-tinta').classList.remove('open');
}

async function guardarEntradaTinta() {

    const msg = document.getElementById('nt-msg');
    const idTintaBase = parseInt(
        document.getElementById('nt-idtinta').value
    );

    // Validar que haya seleccionado una tinta
    if (!idTintaBase) {
        showModalMsg(
            msg,
            'Debes seleccionar una tinta base.',
            'err'
        );
        return;
    }
    const select      = document.getElementById('nt-idtinta');
    const seleccionada = select?.options[select.selectedIndex];
    const dto = {
        idTintaBase:  idTintaBase,
        idInterno:    document.getElementById('nt-idinterno').value.trim(),
        lote:         document.getElementById('nt-lote').value.trim(),
        nombre:       seleccionada?.dataset.nombre || seleccionada?.textContent || '',
        fabricante:   document.getElementById('nt-fabricante').value.trim(),
        proveedor:    document.getElementById('nt-proveedor').value.trim(),
        presentacion: document.getElementById('nt-presentacion').value.trim(),
        costo:        parseFloat(document.getElementById('nt-costo').value) || 0
    };
    if (!dto.nombre) {
        showModalMsg(msg, 'No se pudo determinar el nombre de la tinta.', 'err'); return;
    }
    try {
    const res = await apiFetch('InventarioTinta', 'POST', dto);
    
    if (!res || res.error) {
      // const d = await res.json().catch(() => ({}));
      // Si ASP.NET devuelve errores de validación de ModelState
      if (res.errors) {
        const primerosErrores = Object.values(res.errors).flat().join(' ');
        showModalMsg(msg, primerosErrores || 'Error de validación.', 'err');
        return;
      }
      showModalMsg(msg, res.mensaje || res.title || 'Error al guardar.', 'err');
      return;
    }

    document.getElementById('modal-tinta').classList.remove('open');
    cargarBaseDatos();
  } catch (e) {
    showModalMsg(msg, 'Error: ' + e.message, 'err');
  }
}

async function eliminarRegistroTinta(id) {
  if (!confirm('¿Eliminar este registro? Esta acción no se puede deshacer.')) return;
  try {
    const res = await apiFetch(`InventarioTinta/${id}`, 'DELETE');
    if (res?.error) { alert('No se pudo eliminar.'); return; }
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
    const rolesNombre = {
      1: 'Administrador',
      2: 'Ventas',
      3: 'Operario'
    };

    const res = await apiFetch('Usuario');

    tbody.innerHTML = res.map(u => {
      const rolId = parseInt(u.rol) || 0;
      const nombreRol = rolesNombre[rolId] || 'Sin Rol';
      const claseRol = `role-${rolId}`; // Genera clases como role-1, role-2, role-3

      return `
        <tr>
          <td><div class="user-row-avatar">${iniciales(u.nombre)}</div></td>
          <td>${escHtml(u.nombre)}</td>
          <td>${escHtml(u.correo)}</td>
          <td><span class="role-badge ${claseRol}">${nombreRol}</span></td>
          <td>${u.correo !== currentUser.correo
            ? `<button class="btn-danger" onclick="eliminarUsuario(${u.id},'${escHtml(u.nombre)}')">Eliminar</button>`
            : '<span style="font-size:.7rem;color:var(--text-muted)">(tú)</span>'}</td>
        </tr>`;
    }).join('') || '<tr><td colspan="5" class="loading-cell">No hay usuarios.</td></tr>';

  } catch (e) { 
    tbody.innerHTML = `<tr><td colspan="5" class="error-cell">Error: ${escHtml(e.message)}</td></tr>`; 
  }
}

async function eliminarUsuario(id, nombre) {
  if (!confirm(`¿Eliminar a "${nombre}"?`)) return;
  try {
    const res = await apiFetch(`Usuario/${id}`, 'DELETE');
    if (res?.error) { alert('No se pudo eliminar.'); return; }
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
    // const data = await res.json();
    if (!res || res.error) { showModalMsg(msg, res.mensaje || 'Error al crear usuario.', 'err'); return; }
    document.getElementById('modal-usuario').classList.remove('open');
    cargarUsuarios();
  } catch (e) { showModalMsg(msg, 'Error: '  + e.message, 'err'); }
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
    if (!res.ok) { showModalMsg(msg, res.mensaje||'Error.', 'err'); return; }
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