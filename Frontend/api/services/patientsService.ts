import { api } from "../index";
import { useAuthStore } from "@/stores/authStore";

export type PatientProfile = {
  id: string;
  firstname: string;
  lastname: string;
  email?: string | null;
  isGuest?: boolean | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  address?: string | null;
  religion?: string | null;
  driverLicenseNumber?: string | null;
  medicalInsuranceMemberNumber?: string | null;
  taxNumber?: string | null;
  socialSecurityNumber?: string | null;
};

export type PatientProfileUpdatePayload = Partial<{
  firstname: string;
  lastname: string;
  email: string;
  dateOfBirth: string;
  gender: string;
  address: string;
  religion: string;
  driverLicenseNumber: string;
  medicalInsuranceMemberNumber: string;
  taxNumber: string;
  socialSecurityNumber: string;
}>;

function parseJwtSub(token: string | null): string | null {
  if (!token) return null;
  const parts = token.split(".");
  if (parts.length < 2) return null;

  try {
    const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
    const json = atob(padded);
    const payload = JSON.parse(json) as { sub?: unknown };
    return typeof payload.sub === "string" ? payload.sub : null;
  } catch {
    return null;
  }
}

export function resolveCurrentPatientId(): string | null {
  const auth = useAuthStore.getState();
  if (auth.id) return auth.id;
  return parseJwtSub(auth.token);
}

export const PatientsService = {
  byId: (id: string) =>
    api<PatientProfile>({
      path: `/patients/${id}`,
      auth: true,
    }),

  update: (id: string, payload: PatientProfileUpdatePayload) =>
    api<void>({
      method: "PATCH",
      path: `/patients/${id}`,
      auth: true,
      body: payload,
    }),

  anonymize: (id: string) =>
    api<void>({
      method: "DELETE",
      path: `/patients/anonymize/${id}`,
      auth: true,
    }),

  remove: (id: string) =>
    api<void>({
      method: "DELETE",
      path: `/patients/${id}`,
      auth: true,
    }),
};
