/**
 * Query page: AI assistant UI.
 */
(function () {
  const form = document.getElementById('query-form');
  const questionInput = document.getElementById('question-input');
  const askBtn = document.getElementById('ask-btn');
  const clearBtn = document.getElementById('clear-btn');
  const copyBtn = document.getElementById('copy-btn');
  const answerSection = document.getElementById('answer-section');
  const answerText = document.getElementById('answer-text');
  const sourcesContainer = document.getElementById('sources-container');
  const emptyState = document.getElementById('query-empty');
  const charCount = document.getElementById('char-count');

  const MAX_CHARS = 2000;

  function setLoading(loading) {
    askBtn.disabled = loading;
    questionInput.disabled = loading;
    if (loading) {
      askBtn.innerHTML = '<span class="spinner mr-2"></span> Thinking…';
    } else {
      askBtn.innerHTML = `
        <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5"/></svg>
        Ask
      `;
    }
  }

  function updateCharCount() {
    const len = questionInput.value.length;
    charCount.textContent = `${len} / ${MAX_CHARS}`;
    charCount.classList.toggle('text-rose-500', len > MAX_CHARS);
  }

  function renderSourceCard(source, index) {
    return `
      <div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0 flex-1">
            <div class="flex flex-wrap items-center gap-2">
              <span class="inline-flex h-6 w-6 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-700">${index + 1}</span>
              <span class="truncate text-sm font-medium text-slate-900">${escapeHtml(source.fileName)}</span>
            </div>
            <div class="mt-2 flex flex-wrap gap-3 text-xs text-slate-500">
              ${source.pageNumber != null ? `<span>Page ${source.pageNumber}</span>` : ''}
              <span class="font-medium text-indigo-600">${formatSimilarity(source.similarity)} match</span>
            </div>
          </div>
        </div>
        <p class="source-excerpt mt-3 text-sm leading-relaxed text-slate-600"></p>
      </div>
    `;
  }

  function showAnswer(response) {
    emptyState.classList.add('hidden');
    answerSection.classList.remove('hidden');
    copyBtn.classList.remove('hidden');

    setTextContent(answerText, response.answer || 'No answer returned.');

    const sources = response.sources || [];
    if (!sources.length) {
      sourcesContainer.innerHTML = '<p class="text-sm text-slate-500">No source chunks were retrieved.</p>';
      return;
    }

    sourcesContainer.innerHTML = `
      <h3 class="mb-3 text-sm font-semibold text-slate-900">Sources (${sources.length})</h3>
      <div class="grid gap-3 sm:grid-cols-2">${sources.map((s, i) => renderSourceCard(s, i)).join('')}</div>
    `;

    sourcesContainer.querySelectorAll('.source-excerpt').forEach((el, i) => {
      setTextContent(el, truncate(sources[i].content, 300));
    });
  }

  function clearQuery() {
    questionInput.value = '';
    updateCharCount();
    answerSection.classList.add('hidden');
    emptyState.classList.remove('hidden');
    copyBtn.classList.add('hidden');
    setTextContent(answerText, '');
    sourcesContainer.innerHTML = '';
    questionInput.focus();
  }

  async function submitQuery(e) {
    e.preventDefault();

    const question = questionInput.value.trim();
    if (!question) {
      showToast('warning', 'Please enter a question.');
      return;
    }
    if (question.length > MAX_CHARS) {
      showToast('warning', `Question must be ${MAX_CHARS} characters or fewer.`);
      return;
    }

    setLoading(true);

    try {
      const response = await api.post('/api/v1/query', {
        question,
        topK: 5,
        documentIds: [],
      });
      showAnswer(response);
    } catch (err) {
      showApiError(err);
    } finally {
      setLoading(false);
    }
  }

  async function copyAnswer() {
    const text = answerText.textContent;
    if (!text) return;
    try {
      await navigator.clipboard.writeText(text);
      showToast('success', 'Answer copied to clipboard.');
    } catch {
      showToast('error', 'Failed to copy to clipboard.');
    }
  }

  document.addEventListener('DOMContentLoaded', () => {
    form.addEventListener('submit', submitQuery);
    clearBtn.addEventListener('click', clearQuery);
    copyBtn.addEventListener('click', copyAnswer);
    questionInput.addEventListener('input', updateCharCount);
    updateCharCount();

    questionInput.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
        e.preventDefault();
        form.requestSubmit();
      }
    });
  });
})();
