/**
 * Shared UI utilities: toasts, badges, formatting, XSS-safe escaping.
 */

const DOCUMENT_STATUS = {
  0: { label: 'Pending', color: 'amber' },
  1: { label: 'Processing', color: 'blue' },
  2: { label: 'Indexed', color: 'emerald' },
  3: { label: 'Failed', color: 'rose' },
  Pending: { label: 'Pending', color: 'amber' },
  Processing: { label: 'Processing', color: 'blue' },
  Indexed: { label: 'Indexed', color: 'emerald' },
  Failed: { label: 'Failed', color: 'rose' },
};

const STATUS_BADGE_CLASSES = {
  amber: 'bg-amber-50 text-amber-700 ring-amber-600/20',
  blue: 'bg-blue-50 text-blue-700 ring-blue-600/20',
  emerald: 'bg-emerald-50 text-emerald-700 ring-emerald-600/20',
  rose: 'bg-rose-50 text-rose-700 ring-rose-600/20',
  slate: 'bg-slate-100 text-slate-600 ring-slate-500/20',
};

/**
 * Escape HTML to prevent XSS — use when inserting user/API text into innerHTML.
 * @param {string} str
 * @returns {string}
 */
function escapeHtml(str) {
  if (str == null) return '';
  const div = document.createElement('div');
  div.textContent = String(str);
  return div.innerHTML;
}

/**
 * Set element text safely (preferred over innerHTML for API content).
 * @param {HTMLElement} el
 * @param {string} text
 */
function setTextContent(el, text) {
  el.textContent = text ?? '';
}

/**
 * @param {number|string} status
 * @returns {{label: string, color: string}}
 */
function getStatusInfo(status) {
  return DOCUMENT_STATUS[status] || { label: String(status), color: 'slate' };
}

/**
 * @param {number|string} status
 * @returns {string} HTML string for badge
 */
function statusBadge(status) {
  const info = getStatusInfo(status);
  const classes = STATUS_BADGE_CLASSES[info.color] || STATUS_BADGE_CLASSES.slate;
  const isProcessing = info.label === 'Processing';
  const dot = isProcessing
    ? '<span class="inline-block h-1.5 w-1.5 rounded-full bg-blue-500 animate-pulse mr-1.5"></span>'
    : '';
  return `<span class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${classes}">${dot}${escapeHtml(info.label)}</span>`;
}

/**
 * @param {number} bytes
 * @returns {string}
 */
function formatBytes(bytes) {
  if (bytes == null || bytes === 0) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  const value = bytes / Math.pow(1024, i);
  return `${value.toFixed(i > 0 ? 1 : 0)} ${units[i]}`;
}

/**
 * @param {string|Date} date
 * @returns {string}
 */
function formatDate(date) {
  if (!date) return '—';
  const d = new Date(date);
  if (isNaN(d.getTime())) return '—';
  return d.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/**
 * @param {string} date
 * @returns {string}
 */
function formatRelative(date) {
  if (!date) return '';
  const d = new Date(date);
  const now = Date.now();
  const diff = now - d.getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'just now';
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return formatDate(date);
}

/**
 * @param {'success'|'error'|'info'|'warning'} type
 * @param {string} message
 * @param {number} [duration=4000]
 */
function showToast(type, message, duration = 4000) {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const colors = {
    success: 'bg-emerald-600',
    error: 'bg-rose-600',
    info: 'bg-indigo-600',
    warning: 'bg-amber-600',
  };

  const icons = {
    success: '<path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5"/>',
    error: '<path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>',
    info: '<path stroke-linecap="round" stroke-linejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"/>',
    warning: '<path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"/>',
  };

  const toast = document.createElement('div');
  toast.className = `toast-enter pointer-events-auto flex items-center gap-3 rounded-lg px-4 py-3 text-sm text-white shadow-lg ${colors[type] || colors.info}`;
  toast.innerHTML = `
    <svg class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">${icons[type] || icons.info}</svg>
    <span class="flex-1">${escapeHtml(message)}</span>
    <button type="button" class="shrink-0 rounded p-0.5 hover:bg-white/20" aria-label="Dismiss">
      <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/></svg>
    </button>
  `;

  const dismiss = () => {
    toast.classList.remove('toast-enter');
    toast.classList.add('toast-exit');
    setTimeout(() => toast.remove(), 250);
  };

  toast.querySelector('button').addEventListener('click', dismiss);
  container.appendChild(toast);

  if (duration > 0) {
    setTimeout(dismiss, duration);
  }
}

/**
 * Show error from apiRequest ProblemDetails throw.
 * @param {{title?: string, detail?: string, status?: number}} error
 */
function showApiError(error) {
  const msg = error.detail || error.title || 'An unexpected error occurred.';
  showToast('error', msg);
}

/**
 * Render a loading skeleton placeholder.
 * @param {HTMLElement} el
 * @param {string} [message]
 */
function showLoading(el, message = 'Loading…') {
  el.innerHTML = `
    <div class="flex flex-col items-center justify-center py-16 text-slate-400">
      <div class="spinner mb-3"></div>
      <p class="text-sm">${escapeHtml(message)}</p>
    </div>
  `;
}

/**
 * Render empty state.
 * @param {HTMLElement} el
 * @param {string} title
 * @param {string} [subtitle]
 */
function showEmpty(el, title, subtitle = '') {
  el.innerHTML = `
    <div class="flex flex-col items-center justify-center py-16 text-center">
      <div class="mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-slate-100">
        <svg class="h-7 w-7 text-slate-400" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z"/>
        </svg>
      </div>
      <p class="text-sm font-medium text-slate-700">${escapeHtml(title)}</p>
      ${subtitle ? `<p class="mt-1 text-sm text-slate-500">${escapeHtml(subtitle)}</p>` : ''}
    </div>
  `;
}

/**
 * Format similarity score as percentage.
 * @param {number} similarity
 * @returns {string}
 */
function formatSimilarity(similarity) {
  if (similarity == null) return '—';
  return `${(similarity * 100).toFixed(1)}%`;
}

/**
 * Truncate text with ellipsis.
 * @param {string} text
 * @param {number} max
 * @returns {string}
 */
function truncate(text, max = 120) {
  if (!text) return '';
  if (text.length <= max) return text;
  return text.slice(0, max).trimEnd() + '…';
}
