import { api } from "../index";

export type BookedTimeSlot = {
  startAt: string;
  endAt: string;
};

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

export type PatientAppointment = {
  id: string;
  firstname: string;
  lastname: string;
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

export type CreateAppointmentPayload = {
  firstname: string;
  lastname: string;
  dateOfBirth: string;
  clinicId: number;
  doctorId: string;
  categoryId: number;
  durationMinutes: number;
  startAt: string;
};

export const AppointmentsService = {
  mine: (query?: { page?: number; pageSize?: number }) =>
    api<PagedResponse<PatientAppointment>>({
      path: "/appointments/me",
      auth: true,
      query: { page: 1, pageSize: 100, ...(query ?? {}) },
    }),

  bookedTimes: (doctorId: string, from: string, to: string) =>
    api<BookedTimeSlot[]>({
      path: "/appointments/booked-times",
      auth: false,
      query: { doctorId, from, to },
    }),

  createAnonymous: (payload: CreateAppointmentPayload) =>
    api({
      method: "POST",
      path: "/appointments",
      auth: false,
      body: payload,
    }),

  create: (payload: CreateAppointmentPayload) =>
    api({
      method: "POST",
      path: "/appointments",
      auth: true,
      body: payload,
    }),

  cancel: (id: string) =>
    api({
      method: "DELETE",
      path: `/appointments/${id}`,
      auth: true,
    }),
};
