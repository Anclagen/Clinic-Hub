"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";

import { InputField } from "../UI/forms/InputField";
import { SelectField } from "../UI/forms/SelectField";
import { BookingLoading } from "./components/BookingLoading";

import { ClinicsService, type ClinicDoctorOption } from "@/api/services/clinicsService";
import { CategoriesService, type Category } from "@/api/services/categoriesService";
import { AppointmentsService } from "@/api/services/appointmentsService";

import { useAuthStore } from "@/stores/authStore";
import { isApiError, getUnknownMessage } from "@/api/errors";
import { parseISO } from "date-fns";

import { BookingCalendar, type AppointmentRange } from "./components/BookingCalendar";
import { BookingCalendarSkeleton } from "./components/BookingCalendarSkeleton";

type ClinicOption = { id: number; clinicName: string };

type FormState = {
  firstname: string;
  lastname: string;
  dateOfBirth: string;
  clinicId: string;
  categoryId: string;
  doctorId: string;
  appointmentStartAt: string; // ISO string (UTC) coming from BookingCalendar
};

const defaultForm: FormState = {
  firstname: "",
  lastname: "",
  dateOfBirth: "",
  clinicId: "",
  categoryId: "",
  doctorId: "",
  appointmentStartAt: "",
};

const appointmentInterval = process.env.NEXT_PUBLIC_APPOINTMENT_INTERVAL
  ? Number(process.env.NEXT_PUBLIC_APPOINTMENT_INTERVAL)
  : 15;
const appointmentStart = process.env.NEXT_PUBLIC_APPOINTMENT_START
  ? Number(process.env.NEXT_PUBLIC_APPOINTMENT_START)
  : 8;
const appointmentEnd = process.env.NEXT_PUBLIC_APPOINTMENT_END
  ? Number(process.env.NEXT_PUBLIC_APPOINTMENT_END)
  : 16;

export default function BookingPage() {
  const router = useRouter();

  const [clinics, setClinics] = useState<ClinicOption[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [doctors, setDoctors] = useState<ClinicDoctorOption[]>([]);

  const [loadingOptions, setLoadingOptions] = useState(true);
  const [loadingDoctors, setLoadingDoctors] = useState(false);
  const [loadingBookings, setLoadingBookings] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [bookingFetchFailed, setBookingFetchFailed] = useState(false);

  const [form, setForm] = useState<FormState>(defaultForm);
  const [bookings, setBookings] = useState<AppointmentRange[]>([]);
  const selectedStart = useMemo(
    () => (form.appointmentStartAt ? parseISO(form.appointmentStartAt) : null),
    [form.appointmentStartAt],
  );
  const [selectedDate, setSelectedDate] = useState<Date | null>(selectedStart);

  // auth prefill
  const firstname = useAuthStore((s) => s.firstname);
  const lastname = useAuthStore((s) => s.lastname);
  const dateOfBirth = useAuthStore((s) => s.dateOfBirth);
  const logout = useAuthStore((s) => s.logout);

  const didPrefill = useRef(false);
  useEffect(() => {
    if (didPrefill.current) return;
    if (!firstname || !lastname || !dateOfBirth) return;

    setForm((prev) => ({ ...prev, firstname, lastname, dateOfBirth }));
    didPrefill.current = true;
  }, [firstname, lastname, dateOfBirth]);

  // single field setter (stable)
  const setField = useCallback((key: keyof FormState, value: string) => {
    setForm((prev) => ({ ...prev, [key]: value }));
  }, []);

  // load clinics + categories
  useEffect(() => {
    let active = true;

    async function loadOptions() {
      setLoadingOptions(true);
      setError(null);

      try {
        const [clinicList, categoryList] = await Promise.all([
          ClinicsService.all(),
          CategoriesService.all(),
        ]);

        if (!active) return;

        setClinics(clinicList.map((c) => ({ id: c.id, clinicName: c.clinicName })));
        setCategories(categoryList);
      } catch (e) {
        if (!active) return;
        setError(e instanceof Error ? e.message : "Failed to load options.");
      } finally {
        if (active) setLoadingOptions(false);
      }
    }

    void loadOptions();
    return () => {
      active = false;
    };
  }, []);

  // load doctors when clinic changes
  useEffect(() => {
    if (!form.clinicId) {
      setDoctors([]);
      setForm((prev) => ({ ...prev, doctorId: "", appointmentStartAt: "" }));
      return;
    }

    let active = true;

    async function loadDoctors() {
      setLoadingDoctors(true);
      setError(null);

      try {
        const clinic = await ClinicsService.byId(Number(form.clinicId));
        if (!active) return;

        setDoctors(clinic.doctors ?? []);
      } catch (e) {
        if (!active) return;
        setDoctors([]);
        setError(e instanceof Error ? e.message : "Failed to load doctors.");
      } finally {
        if (active) setLoadingDoctors(false);
      }
    }

    void loadDoctors();
    return () => {
      active = false;
    };
  }, [form.clinicId]);

  const doctorDisabled = !form.clinicId || loadingDoctors || doctors.length === 0;

  const selectedCategory = useMemo(
    () => categories.find((c) => c.id === Number(form.categoryId)),
    [categories, form.categoryId],
  );

  const appointmentDurationMinutes = selectedCategory?.defaultDuration ?? 15;

  useEffect(() => {
    setForm((prev) => ({ ...prev, appointmentStartAt: "" }));
  }, [appointmentDurationMinutes]);

  useEffect(() => {
    if (!form.doctorId || !selectedDate) {
      setBookings([]);
      setBookingFetchFailed(false);
      return;
    }
    const safeSelectedDate = selectedDate;
    let active = true;

    async function loadBookings() {
      setLoadingBookings(true);
      setBookingFetchFailed(false);
      setError(null);

      try {
        const from = safeSelectedDate.toISOString();
        const toDate = new Date(safeSelectedDate);
        toDate.setHours(23, 59, 59, 999);
        const to = toDate.toISOString();

        const result = await AppointmentsService.bookedTimes(form.doctorId, from, to);
        if (active) setBookings(result ?? []);
      } catch (e) {
        if (active) {
          setBookings([]);
          setBookingFetchFailed(true); // <--- Triggers the UI lock
          setError(e instanceof Error ? e.message : "Failed to load availability.");
        }
      } finally {
        if (active) setLoadingBookings(false);
      }
    }

    void loadBookings();
    return () => {
      active = false;
    };
  }, [form.doctorId, selectedDate]);

  const handleCalendarChange = useCallback(
    (iso: string) => {
      setField("appointmentStartAt", iso);
      setErrors((prev) => {
        if (!prev.appointmentStartAt) return prev;
        const { appointmentStartAt, ...rest } = prev;
        return rest;
      });
    },
    [setField],
  );

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setErrors({});

    if (!form.clinicId || !form.categoryId || !form.doctorId || !form.appointmentStartAt) {
      setError("Fill in clinic, category, doctor, and appointment time.");
      return;
    }

    const payload = {
      firstname: form.firstname,
      lastname: form.lastname,
      dateOfBirth: form.dateOfBirth,
      clinicId: Number(form.clinicId),
      categoryId: Number(form.categoryId),
      doctorId: form.doctorId,
      startAt: form.appointmentStartAt,
      durationMinutes: appointmentDurationMinutes,
    };

    try {
      await AppointmentsService.create(payload);
      router.push("/booking/success"); // adjust to your route
    } catch (err: unknown) {
      if (isApiError(err)) {
        if (err.status === 401) {
          logout();
          router.push("/auth/login");
          return;
        }
        if (err.status === 409) {
          setErrors({ appointmentStartAt: err.message });
          return;
        }
        setError(err.message);
        return;
      }
      setError(getUnknownMessage(err));
    }
  };

  if (loadingOptions) return <BookingLoading />;

  return (
    <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
      <h2 className="text-xl font-semibold text-foreground">Book an Appointment</h2>

      {error ? (
        <div className="mt-3 rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
          {error}
        </div>
      ) : null}

      <form className="mt-6 grid gap-4 md:grid-cols-2" onSubmit={handleSubmit} noValidate>
        <InputField
          label="First Name"
          name="firstName"
          value={form.firstname}
          onChange={(event) => setField("firstname", event.target.value)}
          error={errors.firstName}
          placeholder="e.g. Jane"
        />

        <InputField
          label="Last Name"
          name="lastName"
          value={form.lastname}
          onChange={(event) => setField("lastname", event.target.value)}
          error={errors.lastName}
          placeholder="e.g. Doe"
        />

        <InputField
          label="Date of Birth"
          name="dateOfBirth"
          type="date"
          value={form.dateOfBirth}
          onChange={(event) => setField("dateOfBirth", event.target.value)}
          error={errors.dateOfBirth}
        />

        <SelectField
          label="Clinic"
          name="clinicId"
          value={form.clinicId}
          onChange={(value) => {
            setField("clinicId", value);
            setField("doctorId", "");
            setField("appointmentStartAt", "");
            setBookings([]);
          }}
          placeholder="Select a clinic"
          options={clinics.map((c) => ({ value: String(c.id), label: c.clinicName }))}
        />

        <SelectField
          label="Category"
          name="categoryId"
          value={form.categoryId}
          onChange={(value) => {
            setField("categoryId", value);
            setField("appointmentStartAt", "");
          }}
          placeholder="Select a category"
          options={categories.map((c) => ({
            value: String(c.id),
            label: `${c.categoryName} (${c.defaultDuration} min)`,
          }))}
        />

        <SelectField
          label="Doctor"
          name="doctorId"
          value={form.doctorId}
          onChange={(value) => {
            setField("doctorId", value);
            setField("appointmentStartAt", "");
          }}
          placeholder={
            !form.clinicId
              ? "Select a clinic first"
              : loadingDoctors
                ? "Loading doctors..."
                : doctors.length === 0
                  ? "No doctors found"
                  : "Select a doctor"
          }
          options={doctors.map((d) => ({
            value: d.id,
            label: `Dr. ${d.firstname} ${d.lastname} (${d.specialityName})`,
          }))}
          disabled={doctorDisabled}
        />

        <div className="md:col-span-2">
          {!form.doctorId || loadingDoctors ? (
            <BookingCalendarSkeleton />
          ) : (
            <>
              <div className="mb-2 text-sm text-muted">&nbsp;</div>

              <BookingCalendar
                loadingBookings={loadingBookings}
                hasError={bookingFetchFailed}
                value={form.appointmentStartAt}
                durationMinutes={appointmentDurationMinutes}
                booked={bookings}
                onChange={handleCalendarChange}
                onDateChange={setSelectedDate}
                selectedDate={selectedDate}
                dayStartHour={appointmentStart}
                dayEndHour={appointmentEnd}
                intervalMinutes={appointmentInterval}
              />
            </>
          )}

          {errors.appointmentStartAt ? (
            <div className="mt-2 text-sm text-error">{errors.appointmentStartAt}</div>
          ) : null}
        </div>
      </form>
    </section>
  );
}
