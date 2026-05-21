/** Hand-written Sprint 1 types — replace with openapi-typescript generated when OpenAPI is complete. */

export interface ResourceDto {
  id: string;
}

export interface AuditableResourceDto extends ResourceDto {
  createdAt: string;
  updatedAt?: string | null;
}

export interface DepartmentDto extends AuditableResourceDto {
  siteId: string;
  code: string;
  name: string;
  isActive: boolean;
  allowedRoles: string[];
}

export interface EmployeeDto extends AuditableResourceDto {
  departmentId: string;
  code: string;
  name: string;
  role: string;
  managerId?: string | null;
  isActive: boolean;
  hasUser: boolean;
  userIsActive?: boolean | null;
}

export interface HealthResponse {
  status: string;
  service?: string;
  utc?: string;
}
