import { isApiError } from "@/api/errors";
import { PatientProfile } from "@/api/services/patientsService";
import { ProfileFormState, ProfileFormErrors } from "./profileTypes";

export const genderOptions = [
  { value: "Male", label: "Man" },
  { value: "Female", label: "Woman" },
  { value: "Other", label: "Other" },
];

export function getFieldErrors(error: unknown): ProfileFormErrors {
  if (!isApiError(error)) return {};
  const payload = error.payload;

  if (typeof payload !== "object" || payload === null || !("errors" in payload)) {
    return {};
  }

  const apiErrors = (payload as any).errors as Record<string, string[] | string>;
  return Object.entries(apiErrors).reduce<ProfileFormErrors>((acc, [field, messages]) => {
    const message = Array.isArray(messages) ? messages[0] : messages;
    if (typeof message === "string") {
      acc[field as keyof ProfileFormState] = message;
    }
    return acc;
  }, {});
}

export function toFormState(profile: PatientProfile): ProfileFormState {
  return {
    firstname: profile.firstname ?? "",
    lastname: profile.lastname ?? "",
    email: profile.email ?? "",
    dateOfBirth: profile.dateOfBirth ? profile.dateOfBirth.slice(0, 10) : "",
    gender: profile.gender ?? "",
    address: profile.address ?? "",
    religion: profile.religion ?? "",
    driverLicenseNumber: profile.driverLicenseNumber ?? "",
    medicalInsuranceMemberNumber: profile.medicalInsuranceMemberNumber ?? "",
    taxNumber: profile.taxNumber ?? "",
    socialSecurityNumber: profile.socialSecurityNumber ?? "",
  };
}

export function toErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error) return error.message;
  if (typeof error === "string") return error;
  return fallback;
}
