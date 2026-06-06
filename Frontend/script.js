// ════════════════════════════════════════
//  CONFIGURACIÓN
// ════════════════════════════════════════
const API_URL = 'http://localhost:5115/api';

// ════════════════════════════════════════
//  ESTADO GLOBAL
// ════════════════════════════════════════
let token = localStorage.getItem('token') || null;
let currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');

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
    // Token expirado - intentar refresh
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
  } catch {
    return false;
  }
}

// ════════════════════════════════════════
//  INICIO DE APP
// ════════════════════════════════════════
window.addEventListener('load', () => {
  setTimeout(() => {
    document.getElementById('loading').classList.add('hidden');
  }, 600);

  // Si ya hay sesión activa, ir directo al main
  if (token && currentUser) {
    showMainPage();
  }
});

document.addEventListener('keydown', (e) => {
  const loginPage = document.getElementById('login-page');
  if (e.key === 'Enter' && loginPage && getComputedStyle(loginPage).display !== 'none') {
    handleLogin();
  }
});

// ════════════════════════════════════════
//  LOGIN
// ════════════════════════════════════════
async function handleLogin() {
  const correo = document.getElementById('username').value.trim();
  const pass   = document.getElementById('password').value.trim();
  const err    = document.getElementById('error-msg');
  const btn    = document.querySelector('.btn-login');

  if (!correo || !pass) {
    showError('Completa todos los campos');
    return;
  }

  btn.disabled = true;
  btn.textContent = 'Ingresando...';

  try {
    const res = await fetch(`${API_URL}/Usuario/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ correo, password: pass })
    });

    const data = await res.json();

    if (!res.ok || !data.isAuthenticated) {
      showError(data.mensaje || 'Correo o contraseña incorrectos');
      return;
    }

    // Guardar sesión
    token = data.token;
    currentUser = { nombre: data.nombre, correo: data.correo, rol: data.rol };
    localStorage.setItem('token', token);
    localStorage.setItem('refreshToken', data.refreshToken || '');
    localStorage.setItem('currentUser', JSON.stringify(currentUser));

    err.style.display = 'none';
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

  // Actualizar datos del usuario en el header
  if (currentUser) {
    document.getElementById('welcome-name').textContent = currentUser.nombre;
    const initials = currentUser.nombre.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
    document.getElementById('user-initials').textContent = initials;
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

  // Cargar datos iniciales
  cargarDashboard();
}

// ════════════════════════════════════════
//  LOGOUT
// ════════════════════════════════════════
function handleLogout() {
  token       = null;
  currentUser = null;
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
//  TABS
// ════════════════════════════════════════
function switchTab(name, btn) {
  document.querySelectorAll('.nav-tab').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
  btn.classList.add('active');
  document.getElementById('panel-' + name).classList.add('active');

  if (name === 'inicio')       cargarDashboard();
  if (name === 'tintas')       cargarTintas();
  if (name === 'formulas')     cargarFormulas();
  if (name === 'mezcla')       renderMezcla();
}

// ════════════════════════════════════════
//  DASHBOARD — INICIO
// ════════════════════════════════════════
async function cargarDashboard() {
  try {
    const [resTintas, resFormulas] = await Promise.all([
      apiFetch('TintaBase'),
      apiFetch('Formula')
    ]);

    const tintas   = await resTintas.json();
    const formulas = await resFormulas.json();

    const bajo = tintas.filter(t => t.stockActual <= t.stockMinimoAlerta);

    document.getElementById('stat-tintas').textContent   = tintas.length;
    document.getElementById('stat-formulas').textContent = formulas.length;
    document.getElementById('stat-alertas').textContent  = bajo.length;

    // Alertas de stock bajo
    const listaAlertas = document.getElementById('lista-alertas');
    if (bajo.length === 0) {
      listaAlertas.innerHTML = '<p class="no-data">✅ Todas las tintas tienen stock suficiente</p>';
    } else {
      listaAlertas.innerHTML = bajo.map(t => `
        <div class="alerta-item">
          <span class="alerta-color" style="background:${t.codigoHex || '#ccc'}"></span>
          <span class="alerta-nombre">${t.nombreTinta}</span>
          <span class="alerta-stock">Stock: ${t.stockActual}g / Mín: ${t.stockMinimo_alerta}g</span>
        </div>
      `).join('');
    }
  } catch (e) {
    console.error('Error cargando dashboard:', e);
  }
}

// ════════════════════════════════════════
//  TINTAS BASE
// ════════════════════════════════════════
async function cargarTintas() {
  const tbody = document.getElementById('tabla-tintas');
  tbody.innerHTML = '<tr><td colspan="5" class="loading-cell">Cargando...</td></tr>';
  try {
    const res    = await apiFetch('TintaBase');
    const tintas = await res.json();
    tbody.innerHTML = tintas.map(t => `
      <tr>
        <td><span class="color-dot" style="background:${t.codigoHex || '#ccc'}"></span> ${t.nombreTinta}</td>
        <td>${t.codigoHex || '—'}</td>
        <td class="${t.stockActual <= t.stockMinimo_alerta ? 'stock-bajo' : ''}">${t.stockActual}g</td>
        <td>${t.stockMinimo_alerta}g</td>
        <td>
          <button class="btn-sm btn-edit" onclick="editarTinta(${t.id})">✏️</button>
        </td>
      </tr>
    `).join('');
  } catch(e) {
    tbody.innerHTML = '<tr><td colspan="5" class="error-cell">Error cargando tintas</td></tr>';
  }
}

// ════════════════════════════════════════
//  FÓRMULAS
// ════════════════════════════════════════
async function cargarFormulas() {
  const lista = document.getElementById('lista-formulas');
  lista.innerHTML = '<p class="loading-cell">Cargando...</p>';
  try {
    const res      = await apiFetch('Formula');
    const formulas = await res.json();
    lista.innerHTML = formulas.map(f => `
      <div class="formula-card" onclick="verFormula(${f.id})">
        <div class="formula-color" style="background:${f.codigoHex || '#ccc'}"></div>
        <div class="formula-info">
          <strong>${f.nombreColor}</strong>
          <span>ID: ${f.id}</span>
        </div>
        <button class="btn-sm btn-mezcla" onclick="event.stopPropagation(); prepararMezcla(${f.id}, '${f.nombreColor}')">
          🧪 Mezclar
        </button>
      </div>
    `).join('') || '<p class="no-data">No hay fórmulas registradas</p>';
  } catch(e) {
    lista.innerHTML = '<p class="error-cell">Error cargando fórmulas</p>';
  }
}

// ════════════════════════════════════════
//  MEZCLA
// ════════════════════════════════════════
function renderMezcla() {
  // El panel de mezcla ya está en el HTML, solo limpiar resultado
  document.getElementById('resultado-mezcla').innerHTML = '';
}

function prepararMezcla(idFormula, nombreColor) {
  switchTab('mezcla', document.querySelector('[onclick*="mezcla"]'));
  document.getElementById('select-formula').value = idFormula;
  document.getElementById('label-formula-sel').textContent = nombreColor;
}

async function calcularMezcla() {
  const idFormula      = parseInt(document.getElementById('select-formula').value);
  const pesoTotalGramos = parseFloat(document.getElementById('peso-mezcla').value);
  const resultado       = document.getElementById('resultado-mezcla');

  if (!idFormula || !pesoTotalGramos || pesoTotalGramos <= 0) {
    resultado.innerHTML = '<div class="error-cell">Selecciona una fórmula e ingresa un peso válido.</div>';
    return;
  }

  resultado.innerHTML = '<p class="loading-cell">Calculando...</p>';

  try {
    const res  = await apiFetch('Mezcla/calcular', 'POST', { idFormula, pesoTotalGramos });
    const data = await res.json();

    if (!res.ok) {
      resultado.innerHTML = `<div class="error-cell">${data}</div>`;
      return;
    }

    resultado.innerHTML = `
      <div class="mezcla-resultado">
        <div class="mezcla-header">
          <h3>${data.nombreColor}</h3>
          <span class="${data.porcentajesValidos ? 'badge-ok' : 'badge-err'}">
            ${data.porcentajesValidos ? '✅ Porcentajes válidos' : '⚠️ Porcentajes inválidos'}
          </span>
        </div>

        ${data.advertencias.length ? `
          <div class="advertencias">
            ${data.advertencias.map(a => `<div class="advertencia">${a}</div>`).join('')}
          </div>` : ''}

        <table class="tabla-mezcla">
          <thead>
            <tr>
              <th>Tinta</th>
              <th>%</th>
              <th>Gramos necesarios</th>
              <th>Stock actual</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            ${data.tintas.map(t => `
              <tr>
                <td>
                  <span class="color-dot" style="background:${t.codigoHex || '#ccc'}"></span>
                  ${t.nombreTinta}
                </td>
                <td>${t.porcentajeDisplay}%</td>
                <td><strong>${t.gramosNecesarios}g</strong></td>
                <td>${t.stockActual}g</td>
                <td>${t.stockSuficiente ? '✅' : '❌ Insuficiente'}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>

        <div class="mezcla-actions">
          <button class="btn-confirmar" onclick="confirmarMezcla(${data.idFormula}, ${pesoTotalGramos})"
            ${!data.porcentajesValidos || data.tintas.some(t => !t.stockSuficiente) ? 'disabled title="No se puede confirmar: revisa las advertencias"' : ''}>
            ✅ Confirmar y descontar stock
          </button>
        </div>
      </div>
    `;
  } catch(e) {
    resultado.innerHTML = `<div class="error-cell">Error: ${e.message}</div>`;
  }
}

async function confirmarMezcla(idFormula, pesoTotalGramos) {
  if (!confirm(`¿Confirmar la mezcla y descontar el stock de las tintas?`)) return;

  const resultado = document.getElementById('resultado-mezcla');
  try {
    const res  = await apiFetch('Mezcla/confirmar', 'POST', { idFormula, pesoTotalGramos });
    const data = await res.json();

    if (!res.ok) {
      alert('Error: ' + data);
      return;
    }

    resultado.innerHTML = `
      <div class="mezcla-confirmada">
        <div class="confirm-icon">✅</div>
        <h3>Mezcla confirmada exitosamente</h3>
        <p>Se descontó el stock de ${data.tintas.length} tintas para <strong>${pesoTotalGramos}g</strong> de <strong>${data.nombreColor}</strong>.</p>
        ${data.advertencias.map(a => `<div class="advertencia">${a}</div>`).join('')}
        <button class="btn-sm" onclick="renderMezcla()">Nueva mezcla</button>
      </div>
    `;
  } catch(e) {
    alert('Error al confirmar: ' + e.message);
  }
}