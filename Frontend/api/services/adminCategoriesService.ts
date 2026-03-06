import { adminApi } from "@/api";
import type { PagedResponse } from "@/api/apiTypes";

export type AdminCategory = {
  id: number;
  categoryName: string;
  defaultDuration: number;
  description?: string | null;
};

export type CategoryPayload = {
  categoryName: string;
  defaultDuration: number;
  description?: string;
};

export type CategoryUpdatePayload = Partial<CategoryPayload>;

export const AdminCategoriesService = {
  all: (query?: { page?: number; pageSize?: number }) =>
    adminApi<PagedResponse<AdminCategory>>({
      path: "/categories",
      query: { page: 1, pageSize: 100, ...(query ?? {}) },
    }),
  byId: (id: number) => adminApi<AdminCategory>({ path: `/categories/${id}` }),
  create: (payload: CategoryPayload) =>
    adminApi<AdminCategory>({
      method: "POST",
      path: "/categories",
      body: payload,
    }),
  update: (id: number, payload: CategoryUpdatePayload) =>
    adminApi<AdminCategory>({
      method: "PATCH",
      path: `/categories/${id}`,
      body: payload,
    }),
  remove: (id: number) =>
    adminApi<void>({
      method: "DELETE",
      path: `/categories/${id}`,
    }),
};
