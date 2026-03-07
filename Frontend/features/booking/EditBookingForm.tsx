"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { parseISO, isBefore, addHours } from "date-fns";

import { SelectField } from "../UI/forms/SelectField";
import { BookingLoading } from "./components/BookingLoading";
import { BookingCalendar, type AppointmentRange } from "./components/BookingCalendar";
import { BookingCalendarSkeleton } from "./components/BookingCalendarSkeleton";
import { appointmentEnd, appointmentInterval, appointmentStart } from "./components/variables";

import { ClinicsService, type ClinicDoctorOption } from "@/api/services/clinicsService";
import { CategoriesService, type Category } from "@/api/services/categoriesService";
import { AppointmentsService, type PatientAppointment } from "@/api/services/appointmentsService";

import { useAuthStore } from "@/stores/authStore";
import { isApiError, getUnknownMessage } from "@/api/errors";

type FormState = {
  categoryId: string;
  doctorId: string;
  appointmentStartAt: string;
};

type EditBookingProp = {
  appointment: PatientAppointment;
  setAppointment: (app: PatientAppointment) => void;
};

export function EditBookingForm({ appointment, setAppointment }: EditBookingProp) {
  const router = useRouter();
  const logout = useAuthStore((s) => s.logout);
  const userId = useAuthStore((s) => s.id);

  const [categories, setCategories] = useState<Category[]>([]);
  const [doctors, setDoctors] = useState<ClinicDoctorOption[]>([]);
  const [bookings, setBookings] = useState<AppointmentRange[]>([]);

  const [loadingOptions, setLoadingOptions] = useState(true);
  const [loadingDoctors, setLoadingDoctors] = useState(false);
  const [loadingBookings, setLoadingBookings] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [bookingFetchFailed, setBookingFetchFailed] = useState(false);

  const [form, setForm] = useState<FormState>(() => {
    const startDate = parseISO(appointment.startAt);

    return {
      categoryId: String(appointment.categoryId),
      doctorId: appointment.doctorId,
      appointmentStartAt: startDate.toISOString(),
    };
  });

  const [selectedDate, setSelectedDate] = useState<Date | null>(() =>
    parseISO(appointment.startAt),
  );

  const isLocked = useMemo(() => {
    const start = parseISO(appointment.startAt);
    const lockTime = addHours(new Date(), 24);
    return isBefore(start, lockTime);
  }, [appointment.startAt]);

  const selectedCategory = useMemo(
    () => categories.find((c) => c.id === Number(form.categoryId)),
    [categories, form.categoryId],
  );

  const appointmentDurationMinutes = selectedCategory?.defaultDuration ?? 15;

  const setField = useCallback((key: keyof FormState, value: string) => {
    setForm((prev) => ({ ...prev, [key]: value }));
  }, []);

  useEffect(() => {
    async function loadCategories() {
      try {
        const res = await CategoriesService.all();
        setCategories(res.data);
      } catch (e) {
        setError("Failed to load categories.");
      } finally {
        setLoadingOptions(false);
      }
    }
    loadCategories();
  }, []);

  useEffect(() => {
    async function loadDoctors() {
      setLoadingDoctors(true);
      try {
        const clinic = await ClinicsService.byId(Number(appointment.clinicId));
        setDoctors(clinic.doctors ?? []);
      } catch (e) {
        setDoctors([]);
        setError("Failed to load doctors for this clinic.");
      } finally {
        setLoadingDoctors(false);
      }
    }
    loadDoctors();
  }, [appointment.clinicId]);

  useEffect(() => {
    if (!form.doctorId || !selectedDate) return;

    const date = selectedDate;
    const doctorId = form.doctorId;

    let active = true;
    async function loadBookings() {
      setLoadingBookings(true);
      setBookingFetchFailed(false);

      try {
        const from = date.toISOString();
        const toDate = new Date(date);
        toDate.setHours(23, 59, 59, 999);
        const to = toDate.toISOString();

        const result = await AppointmentsService.bookedTimes(doctorId, from, to);

        if (active) {
          const filtered = (result ?? []).filter((b) => b.startAt !== appointment.startAt);
          setBookings(filtered);
        }
      } catch {
        if (active) setBookingFetchFailed(true);
      } finally {
        if (active) setLoadingBookings(false);
      }
    }

    loadBookings();
    return () => {
      active = false;
    };
  }, [form.doctorId, selectedDate, appointment.startAt]);

  const handleCalendarChange = (iso: string) => {
    setField("appointmentStartAt", iso);
    setErrors({});
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isLocked || isSubmitting) return;

    setIsSubmitting(true);
    setError(null);

    const payload = {
      doctorId: form.doctorId,
      categoryId: Number(form.categoryId),
      startAt: form.appointmentStartAt,
      durationMinutes: appointmentDurationMinutes,
    };

    try {
      const response: PatientAppointment = await AppointmentsService.update(
        appointment.id,
        payload,
      );
      setAppointment(response);
    } catch (err) {
      if (isApiError(err)) {
        if (err.status === 401) {
          logout();
          router.push("/auth/login?expired=true");
          return;
        }
        if (err.status === 409) {
          setErrors({ appointmentStartAt: "This time slot is no longer available." });
          return;
        }
        setError(err.message);
      } else {
        setError(getUnknownMessage(err));
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loadingOptions) return <BookingLoading />;

  return (
    <section className="rounded-2xl border border-border bg-card p-2 md:p-6 shadow-sm">
      <div className="flex flex-col gap-2">
        <h2 className="text-2xl font-semibold text-foreground">Modify Appointment</h2>
        {isLocked && (
          <p className="text-sm font-medium text-error">
            Changes are restricted within 24 hours of the appointment time.
          </p>
        )}
      </div>

      <form className="mt-6 grid gap-6 md:grid-cols-2" onSubmit={handleSubmit} noValidate>
        <SelectField
          label="Service Category"
          placeholder="Select A Category"
          name="categoryId"
          disabled={isLocked}
          value={form.categoryId}
          onChange={(val) => {
            setField("categoryId", val);
            setField("appointmentStartAt", "");
          }}
          options={categories.map((c) => ({
            value: String(c.id),
            label: `${c.categoryName} (${c.defaultDuration} min)`,
          }))}
        />

        <SelectField
          label="Doctor"
          name="doctorId"
          disabled={isLocked || loadingDoctors}
          value={form.doctorId}
          onChange={(val) => {
            setField("doctorId", val);
            setField("appointmentStartAt", "");
          }}
          placeholder={loadingDoctors ? "Loading..." : "Select Doctor"}
          options={doctors.map((d) => ({
            value: d.id,
            label: `Dr. ${d.firstname} ${d.lastname}`,
          }))}
        />

        <div className="md:col-span-2">
          {loadingDoctors ? (
            <BookingCalendarSkeleton />
          ) : (
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
          )}
          {errors.appointmentStartAt && (
            <p className="mt-2 text-sm text-error font-medium">{errors.appointmentStartAt}</p>
          )}
        </div>

        {error && (
          <div className="md:col-span-2 p-3 rounded-lg bg-error-soft border border-error text-error text-sm">
            {error}
          </div>
        )}

        <div className="md:col-span-2 flex items-center gap-4">
          <button
            type="submit"
            disabled={isLocked || isSubmitting || !form.appointmentStartAt}
            className="rounded-xl bg-primary px-8 py-3 text-sm font-bold text-white shadow-sm transition hover:bg-primary-hover disabled:opacity-50"
          >
            {isSubmitting ? "Saving..." : "Save Changes"}
          </button>

          <button
            type="button"
            onClick={() => router.back()}
            className="text-sm font-semibold text-muted hover:text-foreground"
          >
            Go Back
          </button>
        </div>
      </form>
    </section>
  );
}
