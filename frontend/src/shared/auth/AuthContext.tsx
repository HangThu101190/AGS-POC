import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import {
  login as loginApi,
  logout as logoutApi,
  refreshSession,
  fetchCurrentUser,
  updateCurrentUser,
} from "@/shared/api/authApi";
import { applyPreferredLanguage } from "@/shared/auth/applyPreferredLanguage";
import { clearTokens, getAccessToken, getRefreshToken, setTokens } from "@/shared/auth/tokenStorage";
import type { AuthUser, LoginCredentials } from "@/shared/auth/types";

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<AuthUser>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<AuthUser | null>;
  updatePreferredLanguage: (lang: "vi" | "en") => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const applySession = useCallback((accessToken: string, refreshToken: string, nextUser: AuthUser) => {
    setTokens(accessToken, refreshToken);
    applyPreferredLanguage(nextUser.preferredLanguage);
    setUser(nextUser);
  }, []);

  const login = useCallback(
    async (credentials: LoginCredentials) => {
      const session = await loginApi(credentials.login, credentials.password);
      applySession(session.accessToken, session.refreshToken, session.user);
      return session.user;
    },
    [applySession],
  );

  const logout = useCallback(async () => {
    const refresh = getRefreshToken();
    clearTokens();
    setUser(null);
    try {
      await logoutApi(refresh);
    } catch {
      /* ignore */
    }
  }, []);

  const refreshUser = useCallback(async () => {
    const access = getAccessToken();
    if (!access) return null;
    const me = await fetchCurrentUser();
    applyPreferredLanguage(me.preferredLanguage);
    setUser(me);
    return me;
  }, []);

  const updatePreferredLanguage = useCallback(
    async (lang: "vi" | "en") => {
      const me = await updateCurrentUser({ preferredLanguage: lang });
      applyPreferredLanguage(me.preferredLanguage);
      setUser(me);
    },
    [],
  );

  useEffect(() => {
    let cancelled = false;

    (async () => {
      const refresh = getRefreshToken();
      const access = getAccessToken();

      try {
        if (refresh) {
          const session = await refreshSession(refresh);
          if (!cancelled) {
            applySession(session.accessToken, session.refreshToken, session.user);
          }
        } else if (access) {
          const me = await fetchCurrentUser();
          if (!cancelled) {
            applyPreferredLanguage(me.preferredLanguage);
            setUser(me);
          }
        }
      } catch {
        if (!cancelled) {
          clearTokens();
          setUser(null);
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [applySession]);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: user !== null,
      isLoading,
      login,
      logout,
      refreshUser,
      updatePreferredLanguage,
    }),
    [user, isLoading, login, logout, refreshUser, updatePreferredLanguage],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
