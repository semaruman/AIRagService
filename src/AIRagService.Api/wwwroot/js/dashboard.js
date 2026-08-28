/**
 * Dashboard page logic.
 */
(function () {
  const statsContainer = document.getElementById('stats-cards');
  const recentContainer = document.getElementById('recent-documents');

  const STAT_CONFIG = [
    { key: 'totalDocuments', label: 'Total Documents', icon: 'indigo', statClass: 'stat-indigo' },
    { key: 'indexedDocuments', label: 'Indexed', icon: 'emerald', statClass: 'stat-emerald' },
    { key: 'processingDocuments', label: 'Processing', icon: 'blue', statClass: 'stat-blue' },
    { key: 'pendingDocuments', label: 'Pending', icon: 'amber', statClass: 'stat-amber' },
    { key: 'failedDocuments', label: 'Failed', icon: 'rose', statClass: 'stat-rose' },
    { key: 'totalChunks', label: 'Total Chunks', icon: 'violet', statClass: 'stat-violet', subKey: 'indexedChunks', subLabel: 'indexed' },
  ];

  function renderStatCard(config, stats) {
    const value = stats[config.key] ?? 0;
    const sub = config.subKey ? `<p class="mt-1 text-xs text-slate-500">${stats[config.subKey] ?? 0} ${config.subLabel}</p>` : '';
    return `
      <div class="stat-card ${config.statClass} relative overflow-hidden rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <p class="text-sm font-medium text-slate-500">${escapeHtml(config.label)}</p>
        <p class="mt-2 text-3xl font-bold tracking-tight text-slate-900">${value.toLocaleString()}</p>
        ${sub}
      </div>
    `;
  }

  function renderDocumentsTable(documents) {
    if (!documents.length) {
      return `
        <div class="px-6 py-12 text-center text-sm text-slate-500">
          No documents yet. <a href="/documents.html" class="font-medium text-indigo-600 hover:text-indigo-500">Upload your first PDF</a>
        </div>
      `;
    }

    const rows = documents.map((doc) => `
      <tr class="border-t border-slate-100">
        <td class="px-6 py-4">
          <a href="/documents.html?id=${doc.id}" class="font-medium text-indigo-600 hover:text-indigo-500">${escapeHtml(doc.originalFileName || doc.fileName)}</a>
        </td>
        <td class="px-6 py-4 text-sm text-slate-500">${formatBytes(doc.fileSize)}</td>
        <td class="px-6 py-4">${statusBadge(doc.status)}</td>
        <td class="px-6 py-4 text-sm text-slate-500" title="${escapeHtml(formatDate(doc.uploadedAt))}">${escapeHtml(formatRelative(doc.uploadedAt))}</td>
        <td class="px-6 py-4 text-sm text-slate-500">${doc.chunkCount ?? 0}</td>
      </tr>
    `).join('');

    return `
      <div class="overflow-x-auto">
        <table class="data-table w-full text-left text-sm">
          <thead class="bg-slate-50 text-xs font-semibold uppercase tracking-wider text-slate-500">
            <tr>
              <th class="px-6 py-3">Name</th>
              <th class="px-6 py-3">Size</th>
              <th class="px-6 py-3">Status</th>
              <th class="px-6 py-3">Uploaded</th>
              <th class="px-6 py-3">Chunks</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 bg-white">${rows}</tbody>
        </table>
      </div>
    `;
  }

  async function loadDashboard() {
    showLoading(statsContainer, 'Loading stats…');
    showLoading(recentContainer, 'Loading documents…');

    try {
      const [stats, documents] = await Promise.all([
        api.get('/api/v1/stats'),
        api.get('/api/v1/documents?page=1&pageSize=5'),
      ]);

      statsContainer.innerHTML = STAT_CONFIG.map((c) => renderStatCard(c, stats)).join('');

      const items = documents.items || [];
      recentContainer.innerHTML = `
        <div class="flex items-center justify-between border-b border-slate-200 px-6 py-4">
          <h2 class="text-base font-semibold text-slate-900">Recent Documents</h2>
          <a href="/documents.html" class="text-sm font-medium text-indigo-600 hover:text-indigo-500">View all →</a>
        </div>
        ${renderDocumentsTable(items)}
      `;
    } catch (err) {
      showApiError(err);
      statsContainer.innerHTML = '<p class="col-span-full text-center text-sm text-rose-600">Failed to load stats.</p>';
      showEmpty(recentContainer, 'Could not load documents', 'Check that the API is running.');
    }
  }

  document.addEventListener('DOMContentLoaded', loadDashboard);
})();
