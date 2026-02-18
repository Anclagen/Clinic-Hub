import { api } from "../index";

export type Pagination = {
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
};

export type PagedResponse<T> = {
  data: T[];
  pagination: Pagination;
};

export type Doctor = {
  id: string;
  firstname: string;
  lastname: string;
  imageUrl?: string | null;
  clinicId: number;
  clinicName: string;
  specialityId: number;
  specialityName: string;
};

export type DoctorsQuery = {
  q?: string;
  clinicId?: number;
  specialityId?: number;
  page?: number;
  pageSize?: number;
};

export const DoctorsService = {
  all: (query: DoctorsQuery = {}) =>
    api<PagedResponse<Doctor>>({
      path: "/doctors/",
      query: { page: 1, pageSize: 10, ...query },
      auth: false,
    }),
  byId: (id: string | number) => api<Doctor>({ path: `/doctors/${id}`, auth: false }),
};
