import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import { refreshSession } from "@/shared/api/authApi";
import { clearTokens, getAccessToken, getRefreshToken, setTokens } from "@/shared/auth/tokenStorage";

const baseURL = import.meta.env.VITE_API_URL || "";

export const apiClient = axios.create({
  baseURL,
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use((config) => {
  const lang = localStorage.getItem("ags.lang") || "vi";
  config.headers["Accept-Language"] = lang === "en" ? "en-US" : "vi-VN";

  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

let refreshPromise: Promise<string> | null = null;

async function refreshAccessToken(): Promise<string> {
  const refresh = getRefreshToken();
  if (!refresh) {
    throw new Error("No refresh token");
  }

  const session = await refreshSession(refresh);
  setTokens(session.accessToken, session.refreshToken);
  return session.accessToken;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    if (error.response?.status !== 401 || !original || original._retry) {
      return Promise.reject(error);
    }

    const url = original.url ?? "";
    if (url.includes("/api/v1/auth/login") || url.includes("/api/v1/auth/refresh")) {
      return Promise.reject(error);
    }

    original._retry = true;

    try {
      refreshPromise ??= refreshAccessToken();
      const accessToken = await refreshPromise;
      refreshPromise = null;
      original.headers.Authorization = `Bearer ${accessToken}`;
      return apiClient(original);
    } catch (refreshError) {
      refreshPromise = null;
      clearTokens();
      return Promise.reject(refreshError);
    }
  },
);
