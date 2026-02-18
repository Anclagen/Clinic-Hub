import { api } from "../index";

export type Category = {
  id: number;
  categoryName: string;
  defaultDuration: number;
  description?: string | null;
};

export type CreateCategory = Omit<Category, "id">;

export const CategoriesService = {
  all: () => api<Category[]>({ path: "/categories/" }),
  byId: (id: number) => api<Category>({ path: `/categories/${id}` }),
  create: (payload: CreateCategory) =>
    api<Category>({ method: "POST", path: "/categories/", body: payload }),
  update: (id: number, payload: CreateCategory) =>
    api<void>({ method: "PUT", path: `/categories/${id}`, body: payload }),
};
