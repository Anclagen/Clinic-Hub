import { adminApi } from "@/api";
import type { PagedResponse } from "@/api/apiTypes";

export type AdminPatient = {
  id: string;
  firstname: string;
  lastname: string;
  email?: string | null;
  isGuest?: boolean;
  dateOfBirth?: string | null;
  gender?: string | null;
  address?: string | null;
  religion?: string | null;
  driverLicenseNumber?: string | null;
  medicalInsuranceMemberNumber?: string | null;
  taxNumber?: string | null;
  socialSecurityNumber?: string | null;
};

export type AdminPatientsQuery = {
  q?: string;
  page?: number;
  pageSize?: number;
};

export type CreateAdminPatientPayload = {
  firstname: string;
  lastname: string;
  email?: string;
  dateOfBirth?: string;
};

export type UpdateAdminPatientPayload = Partial<{
  firstname: string;
  lastname: string;
  email: string;
  dateOfBirth: string;
  gender: string;
  address: string;
  religion: string;
  driverLicenseNumber: string;
  medicalInsuranceMemberNumber: string;
  taxNumber: string;
  socialSecurityNumber: string;
}>;

export const AdminPatientsService = {
  all: (query: AdminPatientsQuery = {}) =>
    adminApi<PagedResponse<AdminPatient>>({
      path: "/patients",
      query: { page: 1, pageSize: 20, ...query },
    }),
  byId: (id: string) => adminApi<AdminPatient>({ path: `/patients/${id}` }),
  create: (payload: CreateAdminPatientPayload) =>
    adminApi<AdminPatient>({
      method: "POST",
      path: "/patients",
      body: payload,
    }),
  update: (id: string, payload: UpdateAdminPatientPayload) =>
    adminApi<AdminPatient>({
      method: "PATCH",
      path: `/patients/${id}`,
      body: payload,
    }),
  anonymize: (id: string) =>
    adminApi<void>({
      method: "DELETE",
      path: `/patients/anonymize/${id}`,
    }),
  remove: (id: string) =>
    adminApi<void>({
      method: "DELETE",
      path: `/patients/${id}`,
    }),
};
