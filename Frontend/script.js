// Hide loader
  window.addEventListener('load', () => {
    setTimeout(() => {
      document.getElementById('loading').classList.add('hidden');
    }, 600);
  });

  // Allow Enter key on login form
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && document.getElementById('login-page').style.display !== 'none'
        && !document.getElementById('login-page').style.display) {
      handleLogin();
    }
  });

  function handleLogin() {
    const user = document.getElementById('username').value.trim();
    const pass = document.getElementById('password').value.trim();
    const err = document.getElementById('error-msg');

    if (user === 'admin' && pass === 'carbolsas') {
      err.style.display = 'none';
      const loginPage = document.getElementById('login-page');
      const mainPage = document.getElementById('main-page');

      loginPage.style.opacity = '0';
      loginPage.style.transition = 'opacity 0.4s ease';

      setTimeout(() => {
        loginPage.style.display = 'none';
        mainPage.style.display = 'flex';
        mainPage.style.opacity = '0';
        mainPage.style.transition = 'opacity 0.4s ease';
        setTimeout(() => { mainPage.style.opacity = '1'; }, 20);
      }, 400);

    } else {
      err.style.display = 'block';
      err.style.animation = 'none';
      void err.offsetWidth;
      err.style.animation = 'shake 0.4s ease';
    }
  }

  function handleLogout() {
    const mainPage = document.getElementById('main-page');
    const loginPage = document.getElementById('login-page');

    mainPage.style.opacity = '0';
    mainPage.style.transition = 'opacity 0.3s ease';

    setTimeout(() => {
      mainPage.style.display = 'none';
      loginPage.style.display = 'flex';
      loginPage.style.opacity = '0';
      loginPage.style.transition = 'opacity 0.3s ease';
      document.getElementById('username').value = '';
      document.getElementById('password').value = '';
      setTimeout(() => { loginPage.style.opacity = '1'; }, 20);
    }, 300);
  }

  function switchTab(name, btn) {
    // Deactivate all tabs
    document.querySelectorAll('.nav-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));

    btn.classList.add('active');
    document.getElementById('panel-' + name).classList.add('active');
  }