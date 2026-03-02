import axios from "axios";

const rawApiUrl = import.meta.env.VITE_API_URL || "http://localhost:9186";
const normalizedApiUrl = rawApiUrl.replace(/\/+$/, "").replace(/\/api$/i, "");

const api = axios.create({
  baseURL: normalizedApiUrl,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
