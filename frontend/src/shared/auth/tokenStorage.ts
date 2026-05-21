/**
 * Session storage for JWT (SPA). XSS can read these values — production roadmap: HttpOnly
 * cookies + SameSite, with CSRF protection on mutating routes.
 */
const ACCESS_KEY = "ags.accessToken";
const REFRESH_KEY = "ags.refreshToken";

export function getAccessToken(): string | null {
  return sessionStorage.getItem(ACCESS_KEY);
}

export function getRefreshToken(): string | null {
  return sessionStorage.getItem(REFRESH_KEY);
}

export function setTokens(accessToken: string, refreshToken: string): void {
  sessionStorage.setItem(ACCESS_KEY, accessToken);
  sessionStorage.setItem(REFRESH_KEY, refreshToken);
}

export function clearTokens(): void {
  sessionStorage.removeItem(ACCESS_KEY);
  sessionStorage.removeItem(REFRESH_KEY);
}
