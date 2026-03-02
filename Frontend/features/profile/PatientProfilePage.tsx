"use client";

import { SubmitEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { InputField } from "@/features/UI/forms/InputField";
import { SelectField } from "../UI/forms/SelectField";
import { PatientsService, resolveCurrentPatientId } from "@/api/services/patientsService";
import { AppointmentsService, type PatientAppointment } from "@/api/services/appointmentsService";
import { isApiError } from "@/api/errors";
import { useAuthStore } from "@/stores/authStore";
import {
  defaultForm,
  type ProfileFormState,
  type ProfileFormErrors,
} from "./components/profileTypes";
import {
  genderOptions,
  getFieldErrors,
  toFormState,
  toErrorMessage,
} from "./components/profileUtilities";
import { ProfileAppointments } from "./components/ProfileAppointments";

export default function PatientProfilePage() {
  const router = useRouter();
  const token = useAuthStore((s) => s.token);
  const hasHydrated = useAuthStore((s) => s.hydrated);
  const logout = useAuthStore((s) => s.logout);
  const setProfile = useAuthStore((s) => s.setProfile);

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [form, setForm] = useState<ProfileFormState>(defaultForm);
  const [fieldErrors, setFieldErrors] = useState<ProfileFormErrors>({});
  const [appointments, setAppointments] = useState<PatientAppointment[]>([]);

  const [editing, setEditing] = useState(false);

  useEffect(() => {
    console.log(hasHydrated);
    if (!hasHydrated) return;
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
        console.log(profileResponse, appointmentResponse);

        setForm(toFormState(profileResponse));
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
  }, [logout, router, token, hasHydrated]);

  const setField = (field: keyof ProfileFormState, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (saveMessage) setSaveMessage(null);
    setFieldErrors((prev) => {
      if (!prev[field]) return prev;
      const next = { ...prev };
      delete next[field];
      return next;
    });
  };

  const handleSubmit = async (event: SubmitEvent<HTMLFormElement>) => {
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
    setFieldErrors({});

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
      setEditing(false);
    } catch (err: unknown) {
      if (isApiError(err) && err.status === 401) {
        logout();
        router.replace("/auth/login");
        return;
      }

      const errors = getFieldErrors(err);
      setFieldErrors(errors);

      const hasFieldErrors = Object.keys(errors).length > 0;
      setLoadError(hasFieldErrors ? null : toErrorMessage(err, "Failed to update profile."));
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
          <fieldset disabled={!editing || saving}>
            <div className="mt-4 grid gap-4 md:grid-cols-2">
              <InputField
                label="First Name"
                name="firstname"
                value={form.firstname}
                onChange={(event) => setField("firstname", event.target.value)}
                error={fieldErrors.firstname}
                className="disabled:opacity-80"
              />
              <InputField
                label="Last Name"
                name="lastname"
                value={form.lastname}
                onChange={(event) => setField("lastname", event.target.value)}
                error={fieldErrors.lastname}
                className="disabled:opacity-80"
              />
              <InputField
                label="Email"
                name="email"
                type="email"
                value={form.email}
                onChange={(event) => setField("email", event.target.value)}
                error={fieldErrors.email}
                className="disabled:opacity-80"
              />
              <InputField
                label="Date of Birth"
                name="dateOfBirth"
                type="date"
                value={form.dateOfBirth}
                onChange={(event) => setField("dateOfBirth", event.target.value)}
                error={fieldErrors.dateOfBirth}
                className="disabled:opacity-80"
              />
            </div>
          </fieldset>
        </section>

        <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <h2 className="text-xl font-semibold text-foreground">Additional Information</h2>
          <fieldset disabled={!editing || saving}>
            <div className="mt-4 grid gap-4 md:grid-cols-2">
              <SelectField
                placeholder="Select a gender"
                value={form.gender}
                label="Gender"
                name="gender"
                options={genderOptions}
                onChange={(value) => setField("gender", value)}
              />
              <InputField
                label="Religion"
                name="religion"
                value={form.religion}
                onChange={(event) => setField("religion", event.target.value)}
                error={fieldErrors.religion}
                className="disabled:opacity-80"
              />
              <label className="md:col-span-2 flex flex-col gap-1.5 text-sm">
                <span className="font-medium text-foreground">Address</span>
                <textarea
                  name="address"
                  value={form.address}
                  onChange={(event) => setField("address", event.target.value)}
                  rows={3}
                  aria-invalid={fieldErrors.address ? true : undefined}
                  aria-describedby={fieldErrors.address ? "address-error" : undefined}
                  className={`w-full rounded-[var(--radius-lg)] border bg-card px-3 py-2 text-sm text-foreground outline-none focus:ring-2 ${
                    fieldErrors.address
                      ? "border-error focus:border-error focus:ring-error-soft"
                      : "border-border focus:border-primary focus:ring-primary-soft"
                  }`}
                />
                {fieldErrors.address ? (
                  <p id="address-error" className="text-sm text-error">
                    {fieldErrors.address}
                  </p>
                ) : null}
              </label>
              <InputField
                label="Driver License Number"
                name="driverLicenseNumber"
                value={form.driverLicenseNumber}
                onChange={(event) => setField("driverLicenseNumber", event.target.value)}
                error={fieldErrors.driverLicenseNumber}
                autoComplete="off"
                className="disabled:opacity-80"
              />
              <InputField
                label="Medical Insurance Member Number"
                name="medicalInsuranceMemberNumber"
                value={form.medicalInsuranceMemberNumber}
                onChange={(event) => setField("medicalInsuranceMemberNumber", event.target.value)}
                error={fieldErrors.medicalInsuranceMemberNumber}
                autoComplete="off"
                className="disabled:opacity-80"
              />
              <InputField
                label="Tax Number"
                name="taxNumber"
                value={form.taxNumber}
                onChange={(event) => setField("taxNumber", event.target.value)}
                error={fieldErrors.taxNumber}
                autoComplete="off"
                className="disabled:opacity-80"
              />
              <InputField
                label="Birth Number"
                name="socialSecurityNumber"
                value={form.socialSecurityNumber}
                onChange={(event) => setField("socialSecurityNumber", event.target.value)}
                error={fieldErrors.socialSecurityNumber}
                autoComplete="off"
                className="disabled:opacity-80"
              />
            </div>
            {editing ? (
              <div className="mt-6 flex justify-between">
                <button
                  type="submit"
                  disabled={saving}
                  className="inline-flex rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {saving ? "Saving..." : "Save Profile"}
                </button>

                <button
                  type="button"
                  disabled={saving}
                  className="inline-flex rounded-xl bg-secondary px-5 py-2.5 ms-5 text-sm font-semibold text-white shadow-sm transition hover:bg-secondary-hover disabled:cursor-not-allowed disabled:opacity-60"
                  onClick={() => setEditing(false)}
                >
                  Cancel
                </button>
              </div>
            ) : null}
          </fieldset>
          {!editing ? (
            <div className="mt-6">
              <button
                type="button"
                className="inline-flex rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
                onClick={() => setEditing(true)}
              >
                Update Profile
              </button>
            </div>
          ) : null}
        </section>
      </form>

      <ProfileAppointments appointments={appointments} now={Date.now()} />
    </div>
  );
}
