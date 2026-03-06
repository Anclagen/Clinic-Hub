import { adminApi } from "@/api";
import type { PagedResponse } from "@/api/apiTypes";

export type AdminDoctor = {
  id: string;
  firstname: string;
  lastname: string;
  imageUrl?: string | null;
  clinicId: number;
  clinicName: string;
  specialityId: number;
  specialityName: string;
};

export type AdminDoctorsQuery = {
  clinicId?: number;
  specialityId?: number;
  page?: number;
  pageSize?: number;
};

export type DoctorPayload = {
  firstname: string;
  lastname: string;
  imageUrl?: string;
  clinicId: number;
  specialityId: number;
};

export type DoctorUpdatePayload = Partial<DoctorPayload>;

export const AdminDoctorsService = {
  all: (query: AdminDoctorsQuery = {}) =>
    adminApi<PagedResponse<AdminDoctor>>({
      path: "/doctors",
      query: { page: 1, pageSize: 100, ...query },
    }),
  byId: (id: string) => adminApi<AdminDoctor>({ path: `/doctors/${id}` }),
  create: (payload: DoctorPayload) =>
    adminApi<AdminDoctor>({
      method: "POST",
      path: "/doctors",
      body: payload,
    }),
  update: (id: string, payload: DoctorUpdatePayload) =>
    adminApi<AdminDoctor>({
      method: "PATCH",
      path: `/doctors/${id}`,
      body: payload,
    }),
  remove: (id: string) =>
    adminApi<void>({
      method: "DELETE",
      path: `/doctors/${id}`,
    }),
};
