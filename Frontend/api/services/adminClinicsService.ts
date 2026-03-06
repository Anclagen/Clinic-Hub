import { adminApi } from "@/api";
import type { PagedResponse } from "@/api/apiTypes";

export type AdminClinic = {
  id: number;
  clinicName: string;
  address?: string | null;
  imageUrl?: string | null;
  imageAlt?: string | null;
};

export type ClinicPayload = {
  clinicName: string;
  address?: string;
  imageUrl?: string;
  imageAlt?: string;
};

export type ClinicUpdatePayload = Partial<ClinicPayload>;

export const AdminClinicsService = {
  all: (query?: { page?: number; pageSize?: number }) =>
    adminApi<PagedResponse<AdminClinic>>({
      path: "/clinics",
      query: { page: 1, pageSize: 100, ...(query ?? {}) },
    }),
  byId: (id: number) => adminApi<AdminClinic>({ path: `/clinics/${id}` }),
  create: (payload: ClinicPayload) =>
    adminApi<AdminClinic>({
      method: "POST",
      path: "/clinics",
      body: payload,
    }),
  update: (id: number, payload: ClinicUpdatePayload) =>
    adminApi<AdminClinic>({
      method: "PATCH",
      path: `/clinics/${id}`,
      body: payload,
    }),
  remove: (id: number) =>
    adminApi<void>({
      method: "DELETE",
      path: `/clinics/${id}`,
    }),
};
