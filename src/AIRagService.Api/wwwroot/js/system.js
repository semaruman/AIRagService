/**
 * System page: health checks, config info, API key settings.
 */
(function () {
  const healthLive = document.getElementById('health-live');
  const healthReady = document.getElementById('health-ready');
  const statsGrid = document.getElementById('system-stats');
  const apiKeyInput = document.getElementById('api-key-input');
  const saveApiKeyBtn = document.getElementById('save-api-key');
  const clearApiKeyBtn = document.getElementById('clear-api-key');

  const CONFIG_ITEMS = [
    { label: 'Embedding Provider', value: 'Local (OpenAI-compatible)' },
    { label: 'Embedding Model', value: 'text-embedding-3-small' },
    { label: 'Embedding Dimensions', value: '1536' },
    { label: 'LLM Provider', value: 'OpenAI' },
    { label: 'LLM Model', value: 'gpt-4o-mini' },
    { label: 'RAG Top-K Default', value: '5' },
    { label: 'Chunk Size', value: '800 tokens' },
    { label: 'Chunk Overlap', value: '120 tokens' },
    { label: 'Max Upload Size', value: '20 MB' },
  ];

  function normalizeHealthData(data) {
    if (typeof data === 'string') {
      return { status: data.trim() };
    }
    return data || { status: 'Unknown' };
  }

  function renderHealthCard(el, label, rawData, error) {
    const data = error ? null : normalizeHealthData(rawData);
    if (error) {
      el.innerHTML = `
        <div class="flex items-center gap-3">
          <span class="flex h-10 w-10 items-center justify-center rounded-full bg-rose-100">
            <svg class="h-5 w-5 text-rose-600" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/></svg>
          </span>
          <div>
            <p class="text-sm font-medium text-slate-900">${escapeHtml(label)}</p>
            <p class="text-sm text-rose-600">Unreachable</p>
          </div>
        </div>
      `;
      return;
    }

    const status = (data?.status || 'Unknown').toLowerCase();
    const healthy = status === 'healthy';
    const dotBg = healthy ? 'bg-emerald-500' : 'bg-amber-500';
    const ringBg = healthy ? 'bg-emerald-100' : 'bg-amber-100';
    const statusText = healthy ? 'text-emerald-700' : 'text-amber-700';

    el.innerHTML = `
      <div class="flex items-center gap-3">
        <span class="flex h-10 w-10 items-center justify-center rounded-full ${ringBg}">
          <span class="h-3 w-3 rounded-full ${dotBg} ${healthy ? '' : 'animate-pulse'}"></span>
        </span>
        <div>
          <p class="text-sm font-medium text-slate-900">${escapeHtml(label)}</p>
          <p class="text-sm capitalize ${statusText}">${escapeHtml(data?.status || 'Unknown')}</p>
          ${data?.totalDuration ? `<p class="text-xs text-slate-400">${escapeHtml(data.totalDuration)}</p>` : ''}
        </div>
      </div>
      ${data?.entries ? renderHealthEntries(data.entries) : ''}
    `;
  }

  function renderHealthEntries(entries) {
    const items = Object.entries(entries).map(([name, entry]) => {
      const st = (entry.status || 'Unknown').toLowerCase();
      const ok = st === 'healthy';
      return `
        <div class="flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2 text-xs">
          <span class="font-medium text-slate-700">${escapeHtml(name)}</span>
          <span class="${ok ? 'text-emerald-600' : 'text-amber-600'}">${escapeHtml(entry.status)}</span>
        </div>
      `;
    }).join('');

    return items ? `<div class="mt-4 space-y-2">${items}</div>` : '';
  }

  function renderConfig() {
    const configHtml = CONFIG_ITEMS.map((item) => `
      <div class="flex items-center justify-between rounded-lg border border-slate-200 bg-white px-4 py-3">
        <span class="text-sm text-slate-500">${escapeHtml(item.label)}</span>
        <span class="text-sm font-medium text-slate-900">${escapeHtml(item.value)}</span>
      </div>
    `).join('');

    document.getElementById('config-grid').innerHTML = configHtml;
  }

  function renderSystemStats(stats) {
    if (!statsGrid || !stats) return;
    statsGrid.innerHTML = `
      <div class="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div class="rounded-lg bg-slate-50 p-4 text-center">
          <p class="text-2xl font-bold text-slate-900">${stats.totalDocuments ?? 0}</p>
          <p class="text-xs text-slate-500">Documents</p>
        </div>
        <div class="rounded-lg bg-slate-50 p-4 text-center">
          <p class="text-2xl font-bold text-emerald-600">${stats.indexedDocuments ?? 0}</p>
          <p class="text-xs text-slate-500">Indexed</p>
        </div>
        <div class="rounded-lg bg-slate-50 p-4 text-center">
          <p class="text-2xl font-bold text-slate-900">${stats.totalChunks ?? 0}</p>
          <p class="text-xs text-slate-500">Chunks</p>
        </div>
        <div class="rounded-lg bg-slate-50 p-4 text-center">
          <p class="text-2xl font-bold text-indigo-600">${stats.indexedChunks ?? 0}</p>
          <p class="text-xs text-slate-500">Indexed Chunks</p>
        </div>
      </div>
    `;
  }

  async function loadHealth() {
    const [liveResult, readyResult] = await Promise.allSettled([
      api.get('/health'),
      api.get('/health/ready'),
    ]);

    renderHealthCard(
      healthLive,
      'Liveness — /health',
      liveResult.status === 'fulfilled' ? liveResult.value : null,
      liveResult.status === 'rejected'
    );

    renderHealthCard(
      healthReady,
      'Readiness — /health/ready',
      readyResult.status === 'fulfilled' ? readyResult.value : null,
      readyResult.status === 'rejected'
    );
  }

  async function loadStats() {
    try {
      const stats = await api.get('/api/v1/stats');
      renderSystemStats(stats);
    } catch {
      if (statsGrid) statsGrid.innerHTML = '<p class="text-sm text-slate-500">Could not load live stats.</p>';
    }
  }

  function setupApiKey() {
    apiKeyInput.value = getApiKey();

    saveApiKeyBtn.addEventListener('click', () => {
      setApiKey(apiKeyInput.value);
      showToast('success', 'API key saved to local storage.');
    });

    clearApiKeyBtn.addEventListener('click', () => {
      setApiKey('');
      apiKeyInput.value = '';
      showToast('info', 'API key cleared.');
    });
  }

  document.addEventListener('DOMContentLoaded', () => {
    renderConfig();
    setupApiKey();
    loadHealth();
    loadStats();

    document.getElementById('refresh-health')?.addEventListener('click', () => {
      loadHealth();
      loadStats();
      showToast('info', 'Health status refreshed.');
    });
  });
})();
