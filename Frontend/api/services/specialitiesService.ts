import { api } from "../index";

export type Speciality = {
  id: number;
  specialityName: string;
  description?: string | null;
};

export const SpecialitiesService = {
  all: () => api<Speciality[]>({ path: "/specialities/", auth: false }),
  byId: (id: number) => api<Speciality>({ path: `/specialities/${id}`, auth: false }),
};
