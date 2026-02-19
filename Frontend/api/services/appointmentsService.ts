import { api } from "../index";

export type BookedTimeSlot = {
  startAt: string;
  endAt: string;
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
  bookedTimes: (doctorId: string, from: string, to: string) =>
    api<BookedTimeSlot[]>({
      path: "/appointments/booked-times",
      auth: false,
      query: { doctorId, from, to },
    }),

  create: (payload: CreateAppointmentPayload) =>
    api({
      method: "POST",
      path: "/appointments",
      auth: false,
      body: payload,
    }),
};
