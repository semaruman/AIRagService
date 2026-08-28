/**
 * Documents page: upload, list, pagination, search, delete, detail modal, status polling.
 */
(function () {
  const PAGE_SIZE = 10;
  let currentPage = 1;
  let totalPages = 1;
  let allDocuments = [];
  let pollTimer = null;

  const dropZone = document.getElementById('drop-zone');
  const fileInput = document.getElementById('file-input');
  const uploadBtn = document.getElementById('upload-btn');
  const searchInput = document.getElementById('search-input');
  const tableBody = document.getElementById('documents-tbody');
  const paginationEl = document.getElementById('pagination');
  const pageInfo = document.getElementById('page-info');
  const modal = document.getElementById('document-modal');
  const modalContent = document.getElementById('modal-content');

  function isPendingOrProcessing(status) {
    const info = getStatusInfo(status);
    return info.label === 'Pending' || info.label === 'Processing';
  }

  function needsPolling(docs) {
    return docs.some((d) => isPendingOrProcessing(d.status));
  }

  function startPolling() {
    stopPolling();
    pollTimer = setInterval(() => loadDocuments(false), 3000);
  }

  function stopPolling() {
    if (pollTimer) {
      clearInterval(pollTimer);
      pollTimer = null;
    }
  }

  function getFilteredDocuments() {
    const query = (searchInput?.value || '').trim().toLowerCase();
    if (!query) return allDocuments;
    return allDocuments.filter((doc) => {
      const name = (doc.originalFileName || doc.fileName || '').toLowerCase();
      return name.includes(query);
    });
  }

  function renderTableRow(doc) {
    const name = doc.originalFileName || doc.fileName;
    return `
      <tr class="border-t border-slate-100" data-id="${doc.id}">
        <td class="px-6 py-4">
          <button type="button" class="doc-detail-btn text-left font-medium text-indigo-600 hover:text-indigo-500" data-id="${doc.id}">
            ${escapeHtml(name)}
          </button>
        </td>
        <td class="px-6 py-4 text-sm text-slate-500">${formatBytes(doc.fileSize)}</td>
        <td class="px-6 py-4 status-cell">${statusBadge(doc.status)}</td>
        <td class="px-6 py-4 text-sm text-slate-500">${doc.chunkCount ?? 0} <span class="text-slate-400">/ ${doc.indexedChunkCount ?? 0}</span></td>
        <td class="px-6 py-4 text-sm text-slate-500" title="${escapeHtml(formatDate(doc.uploadedAt))}">${escapeHtml(formatRelative(doc.uploadedAt))}</td>
        <td class="px-6 py-4 text-right">
          <button type="button" class="delete-btn rounded-lg p-2 text-slate-400 hover:bg-rose-50 hover:text-rose-600" data-id="${doc.id}" title="Delete">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0"/>
            </svg>
          </button>
        </td>
      </tr>
    `;
  }

  function renderTable() {
    const filtered = getFilteredDocuments();

    if (!filtered.length) {
      tableBody.innerHTML = `
        <tr>
          <td colspan="6" class="px-6 py-12 text-center text-sm text-slate-500">
            ${searchInput?.value ? 'No documents match your search.' : 'No documents yet. Upload a PDF to get started.'}
          </td>
        </tr>
      `;
      return;
    }

    tableBody.innerHTML = filtered.map(renderTableRow).join('');
    bindTableEvents();
  }

  function renderPagination() {
    if (pageInfo) {
      pageInfo.textContent = `Page ${currentPage} of ${totalPages || 1}`;
    }
    if (!paginationEl) return;

    paginationEl.innerHTML = `
      <button type="button" id="prev-page" class="rounded-lg border border-slate-200 px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-slate-50 disabled:opacity-40" ${currentPage <= 1 ? 'disabled' : ''}>Previous</button>
      <button type="button" id="next-page" class="rounded-lg border border-slate-200 px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-slate-50 disabled:opacity-40" ${currentPage >= totalPages ? 'disabled' : ''}>Next</button>
    `;

    document.getElementById('prev-page')?.addEventListener('click', () => {
      if (currentPage > 1) {
        currentPage--;
        loadDocuments();
      }
    });
    document.getElementById('next-page')?.addEventListener('click', () => {
      if (currentPage < totalPages) {
        currentPage++;
        loadDocuments();
      }
    });
  }

  async function loadDocuments(showLoader = true) {
    if (showLoader && tableBody) {
      tableBody.innerHTML = `<tr><td colspan="6" class="px-6 py-8 text-center"><div class="spinner mx-auto"></div></td></tr>`;
    }

    try {
      const data = await api.get(`/api/v1/documents?page=${currentPage}&pageSize=${PAGE_SIZE}`);
      allDocuments = data.items || [];
      totalPages = data.totalPages || 1;
      currentPage = data.page || currentPage;

      renderTable();
      renderPagination();

      if (needsPolling(allDocuments)) {
        startPolling();
      } else {
        stopPolling();
      }
    } catch (err) {
      showApiError(err);
      tableBody.innerHTML = `<tr><td colspan="6" class="px-6 py-8 text-center text-sm text-rose-600">Failed to load documents.</td></tr>`;
    }
  }

  async function uploadFile(file) {
    if (!file) return;
    if (!file.name.toLowerCase().endsWith('.pdf')) {
      showToast('warning', 'Only PDF files are supported.');
      return;
    }

    const formData = new FormData();
    formData.append('file', file);

    uploadBtn.disabled = true;
    dropZone.classList.add('opacity-60', 'pointer-events-none');

    try {
      const result = await api.upload('/api/v1/documents', formData);
      if (result.alreadyExists) {
        showToast('info', 'Document already exists in the system.');
      } else {
        showToast('success', 'Document uploaded — indexing started.');
      }
      currentPage = 1;
      await loadDocuments();
    } catch (err) {
      showApiError(err);
    } finally {
      uploadBtn.disabled = false;
      dropZone.classList.remove('opacity-60', 'pointer-events-none');
      fileInput.value = '';
    }
  }

  async function deleteDocument(id) {
    if (!confirm('Delete this document and all its chunks? This cannot be undone.')) return;

    try {
      await api.delete(`/api/v1/documents/${id}`);
      showToast('success', 'Document deleted.');
      await loadDocuments();
    } catch (err) {
      showApiError(err);
    }
  }

  async function showDocumentDetail(id) {
    modal.classList.remove('hidden');
    modalContent.innerHTML = '<div class="flex justify-center py-12"><div class="spinner"></div></div>';

    try {
      const doc = await api.get(`/api/v1/documents/${id}`);
      const chunks = doc.chunks || [];

      modalContent.innerHTML = `
        <div class="flex items-start justify-between gap-4 border-b border-slate-200 px-6 py-4">
          <div class="min-w-0">
            <h2 class="truncate text-lg font-semibold text-slate-900">${escapeHtml(doc.originalFileName || doc.fileName)}</h2>
            <p class="mt-1 text-sm text-slate-500">ID: ${escapeHtml(doc.id)}</p>
          </div>
          <button type="button" id="modal-close" class="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-600">
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>
        <div class="max-h-[60vh] overflow-y-auto px-6 py-4">
          <dl class="grid grid-cols-2 gap-4 text-sm sm:grid-cols-3">
            <div><dt class="text-slate-500">Status</dt><dd class="mt-1">${statusBadge(doc.status)}</dd></div>
            <div><dt class="text-slate-500">Size</dt><dd class="mt-1 font-medium text-slate-900">${formatBytes(doc.fileSize)}</dd></div>
            <div><dt class="text-slate-500">Uploaded</dt><dd class="mt-1 font-medium text-slate-900">${escapeHtml(formatDate(doc.uploadedAt))}</dd></div>
            <div><dt class="text-slate-500">Chunks</dt><dd class="mt-1 font-medium text-slate-900">${doc.chunkCount ?? 0} (${doc.indexedChunkCount ?? 0} indexed)</dd></div>
            <div class="col-span-2 sm:col-span-2"><dt class="text-slate-500">Content Hash</dt><dd class="mt-1 font-mono text-xs text-slate-700 break-all">${escapeHtml(doc.contentHash || '—')}</dd></div>
          </dl>
          ${doc.errorMessage ? `<div class="mt-4 rounded-lg bg-rose-50 p-3 text-sm text-rose-700">${escapeHtml(doc.errorMessage)}</div>` : ''}
          ${chunks.length ? `
            <h3 class="mt-6 text-sm font-semibold text-slate-900">Chunks (${chunks.length})</h3>
            <div class="mt-3 space-y-2">
              ${chunks.slice(0, 10).map((c) => `
                <div class="rounded-lg border border-slate-200 bg-slate-50 p-3 text-xs">
                  <span class="font-medium text-slate-600">#${c.chunkIndex ?? '—'}</span>
                  ${c.pageNumber != null ? `<span class="ml-2 text-slate-400">Page ${c.pageNumber}</span>` : ''}
                  <p class="mt-1 text-slate-600 line-clamp-2">${escapeHtml(truncate(c.content, 200))}</p>
                </div>
              `).join('')}
              ${chunks.length > 10 ? `<p class="text-xs text-slate-400">+ ${chunks.length - 10} more chunks</p>` : ''}
            </div>
          ` : ''}
        </div>
      `;

      document.getElementById('modal-close')?.addEventListener('click', closeModal);
    } catch (err) {
      showApiError(err);
      closeModal();
    }
  }

  function closeModal() {
    modal.classList.add('hidden');
    modalContent.innerHTML = '';
    const url = new URL(window.location);
    url.searchParams.delete('id');
    window.history.replaceState({}, '', url);
  }

  function bindTableEvents() {
    tableBody.querySelectorAll('.delete-btn').forEach((btn) => {
      btn.addEventListener('click', () => deleteDocument(btn.dataset.id));
    });
    tableBody.querySelectorAll('.doc-detail-btn').forEach((btn) => {
      btn.addEventListener('click', () => {
        const id = btn.dataset.id;
        const url = new URL(window.location);
        url.searchParams.set('id', id);
        window.history.replaceState({}, '', url);
        showDocumentDetail(id);
      });
    });
  }

  function setupUpload() {
    dropZone.addEventListener('click', () => fileInput.click());
    uploadBtn.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', () => {
      if (fileInput.files[0]) uploadFile(fileInput.files[0]);
    });

    ['dragenter', 'dragover'].forEach((evt) => {
      dropZone.addEventListener(evt, (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-over');
      });
    });

    ['dragleave', 'drop'].forEach((evt) => {
      dropZone.addEventListener(evt, (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
      });
    });

    dropZone.addEventListener('drop', (e) => {
      const file = e.dataTransfer?.files?.[0];
      if (file) uploadFile(file);
    });
  }

  function setupModal() {
    modal?.addEventListener('click', (e) => {
      if (e.target === modal || e.target.classList.contains('modal-backdrop')) closeModal();
    });
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && !modal.classList.contains('hidden')) closeModal();
    });
  }

  document.addEventListener('DOMContentLoaded', () => {
    setupUpload();
    setupModal();
    searchInput?.addEventListener('input', renderTable);
    loadDocuments();

    const params = new URLSearchParams(window.location.search);
    const id = params.get('id');
    if (id) showDocumentDetail(id);
  });

  window.addEventListener('beforeunload', stopPolling);
})();
