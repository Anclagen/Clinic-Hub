import { adminApi } from "@/api";
import type { PagedResponse } from "@/api/apiTypes";

export type AdminSpeciality = {
  id: number;
  specialityName: string;
  description?: string | null;
};

export type SpecialityPayload = {
  specialityName: string;
  description?: string;
};

export type SpecialityUpdatePayload = Partial<SpecialityPayload>;

export const AdminSpecialitiesService = {
  all: (query?: { page?: number; pageSize?: number }) =>
    adminApi<PagedResponse<AdminSpeciality>>({
      path: "/specialities",
      query: { page: 1, pageSize: 100, ...(query ?? {}) },
    }),
  byId: (id: number) => adminApi<AdminSpeciality>({ path: `/specialities/${id}` }),
  create: (payload: SpecialityPayload) =>
    adminApi<AdminSpeciality>({
      method: "POST",
      path: "/specialities",
      body: payload,
    }),
  update: (id: number, payload: SpecialityUpdatePayload) =>
    adminApi<AdminSpeciality>({
      method: "PATCH",
      path: `/specialities/${id}`,
      body: payload,
    }),
  remove: (id: number) =>
    adminApi<void>({
      method: "DELETE",
      path: `/specialities/${id}`,
    }),
};
