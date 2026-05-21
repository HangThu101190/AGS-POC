export type AppRole = "hr" | "sup" | "staff" | "tbdh" | "shift_leader";

export interface AuthUser {
  id: string;
  code: string;
  name: string;
  role: AppRole;
  departmentId: string;
  userId: string;
  loginName: string;
  departmentCode: string;
  departmentName: string;
  preferredLanguage: "vi" | "en";
  mustChangePassword: boolean;
  /** Permission codes from DB catalog for the user's role (synced on login / me). */
  permissions: string[];
}

export interface LoginCredentials {
  login: string;
  password: string;
}
