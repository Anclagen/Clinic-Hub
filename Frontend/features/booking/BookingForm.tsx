"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { parseISO } from "date-fns";

import { InputField } from "../UI/forms/InputField";
import { SelectField } from "../UI/forms/SelectField";
import { BookingLoading } from "./components/BookingLoading";

import { ClinicsService, type ClinicDoctorOption } from "@/api/services/clinicsService";
import { CategoriesService, type Category } from "@/api/services/categoriesService";
import { AppointmentsService } from "@/api/services/appointmentsService";

import { useAuthStore } from "@/stores/authStore";
import { isApiError, getUnknownMessage } from "@/api/errors";

import { BookingCalendar, type AppointmentRange } from "./components/BookingCalendar";
import { BookingCalendarSkeleton } from "./components/BookingCalendarSkeleton";
import AppointmentCard from "../appointment/components/AppointmentCard";
import { PatientAppointment } from "@/api/services/appointmentsService";
import Link from "next/link";
import { Button } from "../UI/Button";
import { appointmentEnd, appointmentInterval, appointmentStart } from "./components/variables";

type ClinicOption = { id: number; clinicName: string };

type FormState = {
  firstname: string;
  lastname: string;
  dateOfBirth: string;
  email: string;
  clinicId: string;
  categoryId: string;
  doctorId: string;
  appointmentStartAt: string;
};

const defaultForm: FormState = {
  firstname: "",
  lastname: "",
  dateOfBirth: "",
  email: "",
  clinicId: "",
  categoryId: "",
  doctorId: "",
  appointmentStartAt: "",
};

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
  const [appointment, setAppointment] = useState<PatientAppointment | null>(null);

  const [form, setForm] = useState<FormState>(defaultForm);
  const [bookings, setBookings] = useState<AppointmentRange[]>([]);
  const selectedStart = useMemo(
    () => (form.appointmentStartAt ? parseISO(form.appointmentStartAt) : null),
    [form.appointmentStartAt],
  );
  const [selectedDate, setSelectedDate] = useState<Date | null>(selectedStart);

  // auth prefill
  const id = useAuthStore((s) => s.id);
  const firstname = useAuthStore((s) => s.firstname);
  const lastname = useAuthStore((s) => s.lastname);
  const dateOfBirth = useAuthStore((s) => s.dateOfBirth);
  const email = useAuthStore((s) => s.email);
  const logout = useAuthStore((s) => s.logout);

  const didPrefill = useRef(false);

  useEffect(() => {
    if (!firstname || !lastname || !dateOfBirth || !email) return setForm(defaultForm);
    if (didPrefill.current) return;

    setForm((prev) => ({ ...prev, firstname, lastname, dateOfBirth, email }));
    didPrefill.current = true;
  }, [firstname, lastname, dateOfBirth, email]);

  const setField = useCallback((key: keyof FormState, value: string) => {
    setForm((prev) => ({ ...prev, [key]: value }));
  }, []);

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

        setClinics(clinicList.data.map((c) => ({ id: c.id, clinicName: c.clinicName })));
        setCategories(categoryList.data);
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
          setBookingFetchFailed(true);
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
  }, [form.doctorId, selectedDate, form.categoryId]);

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
      patientId: id ? id : null,
      firstname: form.firstname,
      lastname: form.lastname,
      dateOfBirth: form.dateOfBirth,
      email: form.email,
      clinicId: Number(form.clinicId),
      categoryId: Number(form.categoryId),
      doctorId: form.doctorId,
      startAt: form.appointmentStartAt,
      durationMinutes: appointmentDurationMinutes,
    };

    try {
      const response = await AppointmentsService.create(payload);
      setAppointment(response);
    } catch (err: unknown) {
      if (isApiError(err)) {
        if (err.status === 401) {
          logout();
          router.push("/auth/login?expired=true");
          return;
        }
        if (err.status === 409) {
          setErrors({ appointmentStartAt: err.message });
          return;
        }
        if (err.status === 400) setError(err.message);
        return;
      }
      setError(getUnknownMessage(err));
    }
  };

  const resetFormHandler = () => {
    setAppointment(null);
    setForm(defaultForm);
    if (!firstname || !lastname || !dateOfBirth || !email) return;
    setForm((prev) => ({ ...prev, firstname, lastname, dateOfBirth, email }));
  };

  if (loadingOptions) return <BookingLoading />;
  if (appointment)
    return (
      <>
        <h2 className="text-2xl font-semibold text-foreground mb-4">Appointment Created</h2>
        <AppointmentCard appointment={appointment} />
        <Button variant="secondary" className="mt-4" onClick={resetFormHandler}>
          Book Another Appointment
        </Button>
      </>
    );
  return (
    <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
      <h2 className="text-2xl font-semibold text-foreground">Book an Appointment</h2>
      {!firstname || !lastname || !dateOfBirth || !email ? (
        <p className="md:hidden">
          Have an account?{" "}
          <Link href={"/auth/login"} className="text-secondary">
            Please login before booking.
          </Link>
        </p>
      ) : null}

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
          disabled={firstname ? true : false}
        />

        <InputField
          label="Last Name"
          name="lastName"
          value={form.lastname}
          onChange={(event) => setField("lastname", event.target.value)}
          error={errors.lastName}
          placeholder="e.g. Doe"
          disabled={lastname ? true : false}
        />

        <InputField
          label="Date of Birth"
          name="dateOfBirth"
          type="date"
          value={form.dateOfBirth}
          onChange={(event) => setField("dateOfBirth", event.target.value)}
          error={errors.dateOfBirth}
          disabled={dateOfBirth ? true : false}
        />

        <InputField
          label="Email"
          name="email"
          type="email"
          placeholder="jane_doe@example.com"
          value={form.email}
          onChange={(event) => setField("email", event.target.value)}
          error={errors.email}
          disabled={email ? true : false}
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
          {!form.doctorId || !form.categoryId || loadingDoctors ? (
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
        <div className="md:col-span-2">
          {!firstname || !lastname || !dateOfBirth || !email ? (
            <p>
              Have an account?{" "}
              <Link href={"/auth/login"} className="text-secondary">
                Please login before booking.
              </Link>
            </p>
          ) : null}
        </div>

        {error ? (
          <div className="md:col-span-2 md:hidden">
            <div className="mt-3 rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
              {error}
            </div>
          </div>
        ) : null}

        <div className="md:col-span-2">
          <button
            type="submit"
            className="inline-flex rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover"
            disabled={!form.appointmentStartAt}
          >
            Confirm booking
          </button>
        </div>
      </form>
    </section>
  );
}
