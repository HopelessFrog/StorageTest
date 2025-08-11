import axios from 'axios';
import { notifyError } from './toast';

const paramsSerializer = (params) => {
  const usp = new URLSearchParams();
  if (!params) return usp.toString();
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null) return;
    if (Array.isArray(value)) {
      value.forEach(v => usp.append(key, v));
    } else {
      usp.append(key, value);
    }
  });
  return usp.toString();
};

const api = axios.create({ baseURL: '/api', paramsSerializer: { serialize: paramsSerializer } });

function extractMessage(error) {
  const res = error?.response;
  if (!res) return error?.message || 'Сервер недоступен';
  const data = res.data;
  if (!data) return res.statusText || 'Ошибка запроса';
  if (typeof data === 'string') return data;
  if (data.errors && typeof data.errors === 'object') {
    const messages = [];
    for (const key of Object.keys(data.errors)) {
      const arr = data.errors[key];
      if (Array.isArray(arr)) messages.push(...arr);
    }
    if (messages.length > 0) return messages.join('\n');
  }
  const detail = data.detail;
  const title = data.title;
  if (detail && title && detail !== title) return `${title}: ${detail}`;
  return detail || title || data.message || error?.message || 'Ошибка запроса';
}

api.interceptors.response.use(
  (res) => res,
  (error) => {
    notifyError(extractMessage(error));
    return Promise.reject(error);
  }
);

const unwrap = (res) => res.data?.items ?? res.data ?? [];
const unwrapPaged = (res) => res.data;

export const receiptsApi = {
  get: async (params) => unwrapPaged(await api.get('/receipts', { params })),
  create: async (payload) => unwrap(await api.post('/receipts', payload)),
  update: async (payload) => unwrap(await api.put('/receipts', payload)),
  remove: async (id) => unwrap(await api.delete(`/receipts/${id}`)),
  incomeResources: async (id) => unwrap(await api.get(`/receipts/${id}/income-resources`)),
  numbers: async () => unwrap(await api.get('/receipts/numbers')),
  resources: async () => unwrap(await api.get('/receipts/resources')),
  units: async () => unwrap(await api.get('/receipts/units')),
};

export const resourcesApi = {
  list: async (params) => unwrapPaged(await api.get('/resources', { params })),
  create: async (payload) => unwrap(await api.post('/resources', payload)),
  update: async (payload) => unwrap(await api.patch('/resources', payload)),
  remove: async (id) => unwrap(await api.delete(`/resources/${id}`)),
  archive: async (id) => unwrap(await api.patch(`/resources/${id}/archive`)),
  unarchive: async (id) => unwrap(await api.patch(`/resources/${id}/unarchive`)),
};

export const unitsApi = {
  list: async (params) => unwrapPaged(await api.get('/units', { params })),
  create: async (payload) => unwrap(await api.post('/units', payload)),
  update: async (payload) => unwrap(await api.patch('/units', payload)),
  remove: async (id) => unwrap(await api.delete(`/units/${id}`)),
  archive: async (id) => unwrap(await api.patch(`/units/${id}/archive`)),
  unarchive: async (id) => unwrap(await api.patch(`/units/${id}/unarchive`)),
};