"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { z } from "zod";
import {
  BookingFormOptions,
  getBookingFormOptions,
} from "@/lib/bookingOptions";

const parseInputDate = (value: string) => new Date(`${value}T00:00:00`);
const datePattern = /^\d{4}-\d{2}-\d{2}$/;

const requiredIntId = (label: string) =>
  z
    .string()
    .trim()
    .min(1, `${label} is required`)
    .refine((value) => /^\d+$/.test(value), `${label} must be an integer id`)
    .transform((value) => Number(value))
    .refine((value) => Number.isInteger(value) && value > 0, `${label} must be a positive integer id`);

const validDate = (label: string) =>
  z
    .string()
    .trim()
    .min(1, `${label} is required`)
    .regex(datePattern, `${label} must use YYYY-MM-DD format`)
    .refine((value) => !Number.isNaN(parseInputDate(value).getTime()), `${label} is invalid`);

export const bookingFormSchema = z.object({
  firstName: z.string().trim().min(2, "First name is required").max(50, "First name is too long"),
  lastName: z.string().trim().min(2, "Last name is required").max(50, "Last name is too long"),
  dateOfBirth: validDate("Date of birth").refine(
    (value) => parseInputDate(value) <= new Date(),
    "Date of birth cannot be in the future",
  ),
  clinicId: requiredIntId("Clinic"),
  categoryId: requiredIntId("Category"),
  doctorId: z.string().trim().uuid("Doctor must be a valid UUID"),
  appointmentDate: validDate("Appointment date").refine(
    (value) => parseInputDate(value) >= new Date(new Date().toDateString()),
    "Appointment date cannot be in the past",
  ),
});

export type BookingFormValues = z.output<typeof bookingFormSchema>;

type BookingFormState = {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  clinicId: string;
  categoryId: string;
  doctorId: string;
  appointmentDate: string;
};

type BookingFormProps = {
  initialValues?: Partial<BookingFormState>;
  options?: Partial<BookingFormOptions>;
  submitLabel?: string;
  onSubmit?: (values: BookingFormValues) => Promise<void> | void;
};

const defaultState: BookingFormState = {
  firstName: "",
  lastName: "",
  dateOfBirth: "",
  clinicId: "",
  categoryId: "",
  doctorId: "",
  appointmentDate: "",
};

export default function BookingForm({
  initialValues,
  options,
  submitLabel = "Confirm Booking",
  onSubmit,
}: BookingFormProps) {
  const [form, setForm] = useState<BookingFormState>({ ...defaultState, ...initialValues });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [lookupOptions, setLookupOptions] = useState<BookingFormOptions>({
    clinics: options?.clinics ?? [],
    categories: options?.categories ?? [],
    doctors: options?.doctors ?? [],
  });
  const [loadingOptions, setLoadingOptions] = useState(
    !options?.clinics || !options?.categories || !options?.doctors,
  );
  const [submitting, setSubmitting] = useState(false);
  const [submitMessage, setSubmitMessage] = useState<string | null>(null);

  const shouldLoadOptions = useMemo(
    () => !options?.clinics || !options?.categories || !options?.doctors,
    [options?.categories, options?.clinics, options?.doctors],
  );

  useEffect(() => {
    if (!shouldLoadOptions) {
      setLoadingOptions(false);
      return;
    }

    let active = true;

    const load = async () => {
      setLoadingOptions(true);
      try {
        const fetched = await getBookingFormOptions();
        if (!active) return;

        setLookupOptions({
          clinics: options?.clinics ?? fetched.clinics,
          categories: options?.categories ?? fetched.categories,
          doctors: options?.doctors ?? fetched.doctors,
        });
      } finally {
        if (active) setLoadingOptions(false);
      }
    };

    void load();

    return () => {
      active = false;
    };
  }, [options?.categories, options?.clinics, options?.doctors, shouldLoadOptions]);

  const setField = (field: keyof BookingFormState, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => {
      const next = { ...prev };
      delete next[field];
      return next;
    });
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitMessage(null);

    const result = bookingFormSchema.safeParse(form);
    if (!result.success) {
      const nextErrors: Record<string, string> = {};
      const flattened = result.error.flatten().fieldErrors;

      for (const [key, value] of Object.entries(flattened)) {
        if (value && value.length > 0) {
          nextErrors[key] = value[0];
        }
      }

      setErrors(nextErrors);
      return;
    }

    try {
      setSubmitting(true);
      await onSubmit?.(result.data);
      setSubmitMessage("Booking details are valid and ready to submit.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
      <h2 className="text-xl font-semibold text-foreground">Book an Appointment</h2>
      <p className="mt-1 text-sm text-muted">Fill in patient and booking details below.</p>

      {submitMessage ? (
        <div className="mt-4 inline-flex rounded-full bg-success-soft px-3 py-1 text-sm font-medium text-success">
          {submitMessage}
        </div>
      ) : null}

      <form className="mt-6 grid gap-4 md:grid-cols-2" onSubmit={handleSubmit} noValidate>
        <FormField
          label="First Name"
          name="firstName"
          value={form.firstName}
          onChange={(value) => setField("firstName", value)}
          error={errors.firstName}
          placeholder="e.g. Jane"
        />

        <FormField
          label="Last Name"
          name="lastName"
          value={form.lastName}
          onChange={(value) => setField("lastName", value)}
          error={errors.lastName}
          placeholder="e.g. Doe"
        />

        <FormField
          label="Date of Birth"
          name="dateOfBirth"
          type="date"
          value={form.dateOfBirth}
          onChange={(value) => setField("dateOfBirth", value)}
          error={errors.dateOfBirth}
        />

        <SelectField
          label="Clinic"
          name="clinicId"
          value={form.clinicId}
          onChange={(value) => setField("clinicId", value)}
          error={errors.clinicId}
          placeholder={loadingOptions ? "Loading clinics..." : "Select a clinic"}
          options={lookupOptions.clinics.map((clinic) => ({ value: String(clinic.id), label: clinic.name }))}
          disabled={loadingOptions}
        />

        <SelectField
          label="Category"
          name="categoryId"
          value={form.categoryId}
          onChange={(value) => setField("categoryId", value)}
          error={errors.categoryId}
          placeholder={loadingOptions ? "Loading categories..." : "Select a category"}
          options={lookupOptions.categories.map((category) => ({
            value: String(category.id),
            label: category.name,
          }))}
          disabled={loadingOptions}
        />

        <SelectField
          label="Doctor"
          name="doctorId"
          value={form.doctorId}
          onChange={(value) => setField("doctorId", value)}
          error={errors.doctorId}
          placeholder={loadingOptions ? "Loading doctors..." : "Select a doctor"}
          options={lookupOptions.doctors.map((doctor) => ({ value: doctor.id, label: doctor.name }))}
          disabled={loadingOptions}
        />

        <FormField
          label="Appointment Date"
          name="appointmentDate"
          type="date"
          value={form.appointmentDate}
          onChange={(value) => setField("appointmentDate", value)}
          error={errors.appointmentDate}
        />

        <div className="md:col-span-2 mt-1">
          <button
            type="submit"
            disabled={submitting || loadingOptions}
            className="inline-flex rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
          >
            {submitting ? "Submitting..." : submitLabel}
          </button>
        </div>
      </form>
    </section>
  );
}

type FieldProps = {
  label: string;
  name: string;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  type?: "text" | "date";
  placeholder?: string;
};

function FormField({ label, name, value, onChange, error, type = "text", placeholder }: FieldProps) {
  return (
    <label className="flex flex-col gap-1.5 text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <input
        name={name}
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        className="rounded-xl border border-border bg-background px-3 py-2 text-foreground outline-none transition focus:border-primary focus:ring-2 focus:ring-primary-soft"
      />
      {error ? <span className="text-xs text-error">{error}</span> : null}
    </label>
  );
}

type SelectFieldProps = {
  label: string;
  name: string;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  placeholder: string;
  options: Array<{ value: string; label: string }>;
  disabled?: boolean;
};

function SelectField({
  label,
  name,
  value,
  onChange,
  error,
  placeholder,
  options,
  disabled,
}: SelectFieldProps) {
  return (
    <label className="flex flex-col gap-1.5 text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <select
        name={name}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        disabled={disabled}
        className="rounded-xl border border-border bg-background px-3 py-2 text-foreground outline-none transition focus:border-primary focus:ring-2 focus:ring-primary-soft disabled:cursor-not-allowed disabled:opacity-60"
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error ? <span className="text-xs text-error">{error}</span> : null}
    </label>
  );
}
