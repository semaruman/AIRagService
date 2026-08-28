/**
 * Shared layout: sidebar navigation, mobile menu, active state.
 */
(function () {
  const NAV_ITEMS = [
    {
      id: 'dashboard',
      label: 'Dashboard',
      href: '/index.html',
      paths: ['/', '/index.html'],
      icon: '<path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6A2.25 2.25 0 016 3.75h2.25A2.25 2.25 0 0110.5 6v2.25a2.25 2.25 0 01-2.25 2.25H6a2.25 2.25 0 01-2.25-2.25V6zM3.75 15.75A2.25 2.25 0 016 13.5h2.25a2.25 2.25 0 012.25 2.25V18a2.25 2.25 0 01-2.25 2.25H6A2.25 2.25 0 013.75 18v-2.25zM13.5 6a2.25 2.25 0 012.25-2.25H18A2.25 2.25 0 0120.25 6v2.25A2.25 2.25 0 0118 10.5h-2.25a2.25 2.25 0 01-2.25-2.25V6zM13.5 15.75a2.25 2.25 0 012.25-2.25H18a2.25 2.25 0 012.25 2.25V18A2.25 2.25 0 0118 20.25h-2.25A2.25 2.25 0 0113.5 18v-2.25z"/>',
    },
    {
      id: 'documents',
      label: 'Documents',
      href: '/documents.html',
      paths: ['/documents.html'],
      icon: '<path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z"/>',
    },
    {
      id: 'query',
      label: 'Query',
      href: '/query.html',
      paths: ['/query.html'],
      icon: '<path stroke-linecap="round" stroke-linejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.456 2.456L21.75 6l-1.035.259a3.375 3.375 0 00-2.456 2.456zM16.894 20.567L16.5 21.75l-.394-1.183a2.25 2.25 0 00-1.423-1.423L13.5 18.75l1.183-.394a2.25 2.25 0 001.423-1.423l.394-1.183.394 1.183a2.25 2.25 0 001.423 1.423l1.183.394-1.183.394a2.25 2.25 0 00-1.423 1.423z"/>',
    },
    {
      id: 'system',
      label: 'System',
      href: '/system.html',
      paths: ['/system.html'],
      icon: '<path stroke-linecap="round" stroke-linejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.324.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.24-.438.613-.431.992a6.759 6.759 0 010 .255c-.007.378.138.75.43.99l1.005.828c.424.35.534.954.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.57 6.57 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.28c-.09.543-.56.941-1.11.941h-2.594c-.55 0-1.02-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.992a6.932 6.932 0 010-.255c.007-.378-.138-.75-.43-.99l-1.004-.828a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.128.332-.183.582-.495.644-.869l.214-1.281z"/><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>',
    },
  ];

  function getCurrentPath() {
    const path = window.location.pathname;
    return path.endsWith('/') && path.length > 1 ? path.slice(0, -1) : path;
  }

  function isActive(item) {
    const current = getCurrentPath();
    return item.paths.some((p) => p === current || (p === '/index.html' && current === '/'));
  }

  function renderNavItem(item) {
    const active = isActive(item);
    const base = 'group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors';
    const classes = active
      ? `${base} bg-indigo-600 text-white`
      : `${base} text-slate-300 hover:bg-slate-800 hover:text-white`;

    return `
      <a href="${item.href}" class="${classes}">
        <svg class="h-5 w-5 shrink-0 ${active ? 'text-white' : 'text-slate-400 group-hover:text-slate-300'}" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
          ${item.icon}
        </svg>
        ${item.label}
      </a>
    `;
  }

  function renderSidebar() {
    const layout = document.getElementById('app-layout');
    if (!layout) return;

    const pageTitle = document.body.dataset.pageTitle || 'AIRagService';
    const pageSubtitle = document.body.dataset.pageSubtitle || '';

    const sidebarHtml = `
      <div id="sidebar-overlay" class="fixed inset-0 z-40 bg-slate-900/50 opacity-0 pointer-events-none lg:hidden" aria-hidden="true"></div>

      <aside id="sidebar" class="fixed inset-y-0 left-0 z-50 flex w-64 flex-col bg-slate-900 lg:static lg:translate-x-0">
        <div class="flex h-16 items-center gap-3 border-b border-slate-800 px-5">
          <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-indigo-600">
            <svg class="h-5 w-5 text-white" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 6.042A8.967 8.967 0 006 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 016 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 016-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0018 18a8.967 8.967 0 00-6 2.292m0-14.25v14.25"/>
            </svg>
          </div>
          <div>
            <p class="text-sm font-semibold text-white">AIRagService</p>
            <p class="text-xs text-slate-400">RAG Platform</p>
          </div>
        </div>

        <nav class="flex-1 space-y-1 px-3 py-4">
          ${NAV_ITEMS.map(renderNavItem).join('')}
        </nav>

        <div class="border-t border-slate-800 p-4">
          <div class="rounded-lg bg-slate-800/50 p-3">
            <p class="text-xs font-medium text-slate-300">API Status</p>
            <p class="mt-1 flex items-center gap-1.5 text-xs text-slate-400">
              <span id="sidebar-health-dot" class="h-2 w-2 rounded-full bg-slate-500"></span>
              <span id="sidebar-health-text">Checking…</span>
            </p>
          </div>
        </div>
      </aside>
    `;

    const mainContent = layout.querySelector('#main-content');
    layout.insertAdjacentHTML('afterbegin', sidebarHtml);

    if (mainContent) {
      const headerHtml = `
        <header class="sticky top-0 z-30 flex h-16 items-center gap-4 border-b border-slate-200 bg-white/80 px-4 backdrop-blur-sm sm:px-6 lg:px-8">
          <button id="menu-toggle" type="button" class="rounded-lg p-2 text-slate-500 hover:bg-slate-100 lg:hidden" aria-label="Open menu">
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"/>
            </svg>
          </button>
          <div class="flex-1 min-w-0">
            <h1 class="truncate text-lg font-semibold text-slate-900">${escapeHtml(pageTitle)}</h1>
            ${pageSubtitle ? `<p class="truncate text-sm text-slate-500">${escapeHtml(pageSubtitle)}</p>` : ''}
          </div>
          <a href="/swagger" target="_blank" rel="noopener" class="hidden sm:inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-xs font-medium text-slate-600 hover:bg-slate-50">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M17.25 6.75L22.5 12l-5.25 5.25m-10.5 0L1.5 12l5.25-5.25m7.5-3l-4.5 16.5"/></svg>
            API Docs
          </a>
        </header>
      `;
      mainContent.insertAdjacentHTML('afterbegin', headerHtml);
    }

    setupMobileMenu();
    checkSidebarHealth();
  }

  function setupMobileMenu() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    const toggle = document.getElementById('menu-toggle');

    function open() {
      sidebar?.classList.add('open');
      overlay?.classList.add('open');
      document.body.classList.add('overflow-hidden');
    }

    function close() {
      sidebar?.classList.remove('open');
      overlay?.classList.remove('open');
      document.body.classList.remove('overflow-hidden');
    }

    toggle?.addEventListener('click', open);
    overlay?.addEventListener('click', close);

    sidebar?.querySelectorAll('a').forEach((link) => {
      link.addEventListener('click', () => {
        if (window.innerWidth < 1024) close();
      });
    });
  }

  async function checkSidebarHealth() {
    const dot = document.getElementById('sidebar-health-dot');
    const text = document.getElementById('sidebar-health-text');
    if (!dot || !text) return;

    try {
      const data = await api.get('/health');
      const statusText = typeof data === 'string' ? data : (data?.status || '');
      const healthy = statusText.toLowerCase() === 'healthy';
      dot.className = `h-2 w-2 rounded-full ${healthy ? 'bg-emerald-400' : 'bg-amber-400'}`;
      text.textContent = healthy ? 'All systems operational' : (statusText || 'Degraded');
    } catch {
      dot.className = 'h-2 w-2 rounded-full bg-rose-400';
      text.textContent = 'Unreachable';
    }
  }

  document.addEventListener('DOMContentLoaded', renderSidebar);
})();
