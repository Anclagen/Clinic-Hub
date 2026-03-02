import { api } from "../index";
import { PagedResponse } from "../apiTypes";

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
