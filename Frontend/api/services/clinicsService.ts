import { api } from "../index";
import { PagedResponse } from "../apiTypes";

export type Clinic = {
  id: number;
  clinicName: string;
  address?: string | null;
  imageUrl?: string | null;
  imageAlt?: string | null;
  doctors?: ClinicDoctorOption[];
};

export type ClinicDoctorOption = {
  id: string;
  firstname: string;
  lastname: string;
  imageUrl?: string | null;
  specialityId: number;
  specialityName: string;
};

export const ClinicsService = {
  all: () => api<PagedResponse<Clinic>>({ path: "/clinics/", auth: false }),
  byId: (id: number) => api<Clinic>({ path: `/clinics/${id}`, auth: false }),
};
