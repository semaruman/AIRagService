/**
 * Central API client for AIRagService.
 */
const API_KEY_STORAGE = 'airag-api-key';

function getApiKey() {
  return localStorage.getItem(API_KEY_STORAGE) || '';
}

function setApiKey(key) {
  if (key && key.trim()) {
    localStorage.setItem(API_KEY_STORAGE, key.trim());
  } else {
    localStorage.removeItem(API_KEY_STORAGE);
  }
}

/**
 * Parse ASP.NET ProblemDetails (application/problem+json) from a failed response.
 * @param {Response} response
 * @returns {Promise<{status: number, title: string, detail: string, traceId?: string}>}
 */
async function parseProblemDetails(response) {
  const contentType = response.headers.get('content-type') || '';
  if (contentType.includes('application/json') || contentType.includes('problem+json')) {
    try {
      const data = await response.json();
      return {
        status: response.status,
        title: data.title || response.statusText || 'Error',
        detail: data.detail || data.title || response.statusText || 'Request failed.',
        traceId: data.traceId || data.extensions?.traceId || null,
      };
    } catch {
      /* fall through */
    }
  }
  return {
    status: response.status,
    title: response.statusText || 'Error',
    detail: `Request failed with status ${response.status}.`,
    traceId: null,
  };
}

/**
 * @param {string} method
 * @param {string} path
 * @param {object|FormData|null} [body]
 * @param {boolean} [isFormData]
 * @returns {Promise<*>}
 */
async function apiRequest(method, path, body = null, isFormData = false) {
  const headers = {
    Accept: 'application/json',
  };

  const apiKey = getApiKey();
  if (apiKey) {
    headers['X-API-Key'] = apiKey;
  }

  if (!isFormData && body !== null && body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  /** @type {RequestInit} */
  const options = { method, headers };

  if (body !== null && body !== undefined) {
    options.body = isFormData ? body : JSON.stringify(body);
  }

  const response = await fetch(path, options);

  if (!response.ok) {
    const error = await parseProblemDetails(response);
    throw error;
  }

  if (response.status === 204) {
    return null;
  }

  const contentType = response.headers.get('content-type') || '';
  if (contentType.includes('application/json')) {
    return response.json();
  }

  const text = await response.text();
  return text || null;
}

/** Convenience helpers */
const api = {
  get: (path) => apiRequest('GET', path),
  post: (path, body) => apiRequest('POST', path, body),
  delete: (path) => apiRequest('DELETE', path),
  upload: (path, formData) => apiRequest('POST', path, formData, true),
};
