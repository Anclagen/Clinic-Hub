"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { InputField } from "@/features/UI/forms/InputField";
import {
  PatientsService,
  type PatientProfile,
  resolveCurrentPatientId,
} from "@/api/services/patientsService";
import { AppointmentsService, type PatientAppointment } from "@/api/services/appointmentsService";
import { isApiError } from "@/api/errors";
import { useAuthStore } from "@/stores/authStore";
import AppointmentCard from "./components/AppointmentCard";

type ProfileFormState = {
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
};

const defaultForm: ProfileFormState = {
  firstname: "",
  lastname: "",
  email: "",
  dateOfBirth: "",
  gender: "",
  address: "",
  religion: "",
  driverLicenseNumber: "",
  medicalInsuranceMemberNumber: "",
  taxNumber: "",
  socialSecurityNumber: "",
};

function toDateOnly(value?: string | null): string {
  return value ? value.slice(0, 10) : "";
}

function toFormState(profile: PatientProfile): ProfileFormState {
  return {
    firstname: profile.firstname ?? "",
    lastname: profile.lastname ?? "",
    email: profile.email ?? "",
    dateOfBirth: toDateOnly(profile.dateOfBirth),
    gender: profile.gender ?? "",
    address: profile.address ?? "",
    religion: profile.religion ?? "",
    driverLicenseNumber: profile.driverLicenseNumber ?? "",
    medicalInsuranceMemberNumber: profile.medicalInsuranceMemberNumber ?? "",
    taxNumber: profile.taxNumber ?? "",
    socialSecurityNumber: profile.socialSecurityNumber ?? "",
  };
}

function toErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error) return error.message;
  if (typeof error === "string") return error;
  return fallback;
}

export default function PatientProfilePage() {
  const router = useRouter();
  const token = useAuthStore((s) => s.token);
  const logout = useAuthStore((s) => s.logout);
  const setProfile = useAuthStore((s) => s.setProfile);

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [form, setForm] = useState<ProfileFormState>(defaultForm);
  const [appointments, setAppointments] = useState<PatientAppointment[]>([]);
  const [showOlderAppointments, setShowOlderAppointments] = useState(false);

  useEffect(() => {
    if (!token) {
      router.replace("/auth/login");
      return;
    }

    const patientId = resolveCurrentPatientId();
    if (!patientId) {
      logout();
      router.replace("/auth/login");
      return;
    }

    let active = true;
    const load = async () => {
      setLoading(true);
      setLoadError(null);

      try {
        const [profileResponse, appointmentResponse] = await Promise.all([
          PatientsService.byId(patientId),
          AppointmentsService.mine(),
        ]);

        if (!active) return;

        setForm(toFormState(profileResponse.data));
        setAppointments(appointmentResponse.data ?? []);
      } catch (err: unknown) {
        if (!active) return;
        if (isApiError(err) && err.status === 401) {
          logout();
          router.replace("/auth/login");
          return;
        }
        setLoadError(toErrorMessage(err, "Failed to load profile."));
      } finally {
        if (active) setLoading(false);
      }
    };

    void load();
    return () => {
      active = false;
    };
  }, [logout, router, token]);

  const sortedAppointments = useMemo(() => {
    return [...appointments].sort(
      (a, b) => new Date(a.startAt).getTime() - new Date(b.startAt).getTime(),
    );
  }, [appointments]);

  const { upcomingAppointments, pastAppointments } = useMemo(() => {
    const now = Date.now();
    const upcoming = sortedAppointments.filter((item) => new Date(item.startAt).getTime() >= now);
    const past = sortedAppointments
      .filter((item) => new Date(item.startAt).getTime() < now)
      .reverse();
    return { upcomingAppointments: upcoming, pastAppointments: past };
  }, [sortedAppointments]);

  const setField = (field: keyof ProfileFormState, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (saveMessage) setSaveMessage(null);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const patientId = resolveCurrentPatientId();
    if (!patientId) {
      logout();
      router.replace("/auth/login");
      return;
    }

    setSaving(true);
    setLoadError(null);
    setSaveMessage(null);

    try {
      await PatientsService.update(patientId, {
        firstname: form.firstname.trim(),
        lastname: form.lastname.trim(),
        email: form.email.trim(),
        dateOfBirth: form.dateOfBirth || undefined,
        gender: form.gender.trim(),
        address: form.address.trim(),
        religion: form.religion.trim(),
        driverLicenseNumber: form.driverLicenseNumber.trim(),
        medicalInsuranceMemberNumber: form.medicalInsuranceMemberNumber.trim(),
        taxNumber: form.taxNumber.trim(),
        socialSecurityNumber: form.socialSecurityNumber.trim(),
      });

      setProfile({
        id: patientId,
        firstname: form.firstname.trim(),
        lastname: form.lastname.trim(),
        email: form.email.trim(),
        dateOfBirth: form.dateOfBirth || null,
      });
      setSaveMessage("Profile updated.");
    } catch (err: unknown) {
      if (isApiError(err) && err.status === 401) {
        logout();
        router.replace("/auth/login");
        return;
      }
      setLoadError(toErrorMessage(err, "Failed to update profile."));
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="mx-auto w-full max-w-6xl px-4 py-8">
        <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="h-7 w-56 rounded bg-primary-soft/70" />
          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <div className="h-16 rounded-xl border border-border bg-background" />
            <div className="h-16 rounded-xl border border-border bg-background" />
            <div className="h-16 rounded-xl border border-border bg-background" />
            <div className="h-16 rounded-xl border border-border bg-background" />
          </div>
        </section>
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-8">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-foreground">Patient Profile</h1>
        <p className="mt-1 text-sm text-muted">
          Update your core details, add additional information, and review your appointments.
        </p>
      </div>

      {loadError ? (
        <div className="mb-4 rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
          {loadError}
        </div>
      ) : null}

      {saveMessage ? (
        <div className="mb-4 rounded-xl border border-success bg-success-soft px-4 py-3 text-sm text-success">
          {saveMessage}
        </div>
      ) : null}

      <form onSubmit={handleSubmit} className="space-y-6">
        <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <h2 className="text-xl font-semibold text-foreground">Personal Details</h2>
          <div className="mt-4 grid gap-4 md:grid-cols-2">
            <InputField
              label="First Name"
              name="firstname"
              value={form.firstname}
              onChange={(event) => setField("firstname", event.target.value)}
            />
            <InputField
              label="Last Name"
              name="lastname"
              value={form.lastname}
              onChange={(event) => setField("lastname", event.target.value)}
            />
            <InputField
              label="Email"
              name="email"
              type="email"
              value={form.email}
              onChange={(event) => setField("email", event.target.value)}
            />
            <InputField
              label="Date of Birth"
              name="dateOfBirth"
              type="date"
              value={form.dateOfBirth}
              onChange={(event) => setField("dateOfBirth", event.target.value)}
            />
          </div>
        </section>

        <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <h2 className="text-xl font-semibold text-foreground">Additional Information</h2>
          <div className="mt-4 grid gap-4 md:grid-cols-2">
            <InputField
              label="Gender"
              name="gender"
              value={form.gender}
              onChange={(event) => setField("gender", event.target.value)}
            />
            <InputField
              label="Religion"
              name="religion"
              value={form.religion}
              onChange={(event) => setField("religion", event.target.value)}
            />
            <label className="md:col-span-2 flex flex-col gap-1.5 text-sm">
              <span className="font-medium text-foreground">Address</span>
              <textarea
                name="address"
                value={form.address}
                onChange={(event) => setField("address", event.target.value)}
                rows={3}
                className="w-full rounded-[var(--radius-lg)] border border-border bg-card px-3 py-2 text-sm text-foreground outline-none focus:border-primary focus:ring-2 focus:ring-primary-soft"
              />
            </label>
            <InputField
              label="Driver License Number"
              name="driverLicenseNumber"
              value={form.driverLicenseNumber}
              onChange={(event) => setField("driverLicenseNumber", event.target.value)}
              autoComplete="off"
            />
            <InputField
              label="Medical Insurance Member Number"
              name="medicalInsuranceMemberNumber"
              value={form.medicalInsuranceMemberNumber}
              onChange={(event) => setField("medicalInsuranceMemberNumber", event.target.value)}
              autoComplete="off"
            />
            <InputField
              label="Tax Number"
              name="taxNumber"
              value={form.taxNumber}
              onChange={(event) => setField("taxNumber", event.target.value)}
              autoComplete="off"
            />
            <InputField
              label="Social Security Number"
              name="socialSecurityNumber"
              value={form.socialSecurityNumber}
              onChange={(event) => setField("socialSecurityNumber", event.target.value)}
              autoComplete="off"
            />
          </div>

          <div className="mt-6">
            <button
              type="submit"
              disabled={saving}
              className="inline-flex rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
            >
              {saving ? "Saving..." : "Save Profile"}
            </button>
          </div>
        </section>
      </form>

      <section className="mt-6 rounded-2xl border border-border bg-card p-6 shadow-sm">
        <div className="flex items-center justify-between gap-3">
          <h2 className="text-xl font-semibold text-foreground">Appointments</h2>
          {pastAppointments.length > 0 ? (
            <button
              type="button"
              onClick={() => setShowOlderAppointments((prev) => !prev)}
              className="rounded-xl border border-secondary px-4 py-2 text-sm font-medium text-secondary transition hover:bg-secondary-soft"
            >
              {showOlderAppointments
                ? "Hide older appointments"
                : `Show older appointments (${pastAppointments.length})`}
            </button>
          ) : null}
        </div>

        <div className="mt-4 space-y-3">
          {upcomingAppointments.length === 0 ? (
            <div className="rounded-xl border border-border bg-background/70 px-4 py-3 text-sm text-muted">
              No upcoming appointments.
            </div>
          ) : (
            upcomingAppointments.map((appointment) => (
              <AppointmentCard key={appointment.id} appointment={appointment} />
            ))
          )}
        </div>

        {showOlderAppointments ? (
          <div className="mt-6">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-muted">
              Older appointments
            </h3>
            <div className="mt-3 space-y-3">
              {pastAppointments.map((appointment) => (
                <AppointmentCard key={appointment.id} appointment={appointment} />
              ))}
            </div>
          </div>
        ) : null}
      </section>
    </div>
  );
}
