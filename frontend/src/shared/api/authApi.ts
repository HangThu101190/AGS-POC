import { apiClient } from "@/shared/api/client";
import type { AppRole, AuthUser } from "@/shared/auth/types";

export interface AuthSessionResponse {
  accessToken: string;
  refreshToken: string;
  expiresInSeconds: number;
  user: AuthUserDto;
}

interface AuthUserDto {
  id: string;
  code: string;
  name: string;
  role: AppRole;
  departmentId: string;
  userId: string;
  loginName: string;
  departmentCode: string;
  departmentName: string;
  preferredLanguage: string;
  mustChangePassword: boolean;
  permissions?: string[];
}

function normalizeRole(role: string): AppRole {
  if (role === "hr" || role === "sup" || role === "staff" || role === "tbdh" || role === "shift_leader") {
    return role;
  }
  return "staff";
}

function mapUser(dto: AuthUserDto): AuthUser {
  return {
    id: dto.id,
    code: dto.code,
    name: dto.name,
    role: normalizeRole(dto.role),
    departmentId: dto.departmentId,
    userId: dto.userId,
    loginName: dto.loginName,
    departmentCode: dto.departmentCode,
    departmentName: dto.departmentName,
    preferredLanguage: dto.preferredLanguage === "en" ? "en" : "vi",
    mustChangePassword: dto.mustChangePassword,
    permissions: dto.permissions ?? [],
  };
}

export async function login(login: string, password: string): Promise<AuthSessionResponse & { user: AuthUser }> {
  const { data } = await apiClient.post<AuthSessionResponse>("/api/v1/auth/login", { login, password });
  return { ...data, user: mapUser(data.user) };
}

export async function refreshSession(refreshToken: string): Promise<AuthSessionResponse & { user: AuthUser }> {
  const { data } = await apiClient.post<AuthSessionResponse>("/api/v1/auth/refresh", { refreshToken });
  return { ...data, user: mapUser(data.user) };
}

export async function logout(refreshToken: string | null): Promise<void> {
  if (refreshToken) {
    await apiClient.post("/api/v1/auth/logout", { refreshToken });
  }
}

export async function fetchCurrentUser(): Promise<AuthUser> {
  const { data } = await apiClient.get<AuthUserDto>("/api/v1/auth/me");
  return mapUser(data);
}

export async function updateCurrentUser(body: { preferredLanguage?: string }): Promise<AuthUser> {
  const { data } = await apiClient.patch<AuthUserDto>("/api/v1/auth/me", body);
  return mapUser(data);
}

export async function changePassword(currentPassword: string, newPassword: string): Promise<void> {
  await apiClient.post("/api/v1/auth/me/change-password", { currentPassword, newPassword });
}
