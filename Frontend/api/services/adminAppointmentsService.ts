import { adminApi } from "@/api";
import type { PagedResponse } from "@/api/apiTypes";

export type AdminAppointment = {
  id: string;
  firstname: string;
  lastname: string;
  email?: string | null;
  dateOfBirth?: string | null;
  patientId: string;
  clinicId: number;
  clinicName: string;
  doctorId: string;
  doctorName: string;
  categoryId: number;
  categoryName: string;
  duration: number;
  startAt: string;
};

export type AdminAppointmentsQuery = {
  clinicId?: number;
  doctorId?: string;
  patientId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
  sortBy?: "startAt" | "patientname" | "doctorname" | "clinicname";
  sortDir?: "asc" | "desc";
};

export const AdminAppointmentsService = {
  all: (query: AdminAppointmentsQuery = {}) =>
    adminApi<PagedResponse<AdminAppointment>>({
      path: "/appointments",
      query: { page: 1, pageSize: 25, sortBy: "startAt", sortDir: "asc", ...query },
    }),
};
