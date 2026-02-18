import { api } from "../index";

export type Clinic = {
  id: number;
  clinicName: string;
  address?: string | null;
  imageUrl?: string | null;
  imageAlt?: string | null;
};

export const ClinicsService = {
  all: () => api<Clinic[]>({ path: "/clinics/", auth: false }),
  byId: (id: number) => api<Clinic>({ path: `/clinics/${id}`, auth: false }),
};
