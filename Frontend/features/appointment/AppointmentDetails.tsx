"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AppointmentsService, type PatientAppointment } from "@/api/services/appointmentsService";
import { EditBookingForm } from "../booking/EditBookingForm";
import { AppointmentDetailsSkeleton } from "./components/AppointmentDetailsSkeleton";

type AppointmentProps = {
  appointmentId: string;
};

const dateTimeFormatter = new Intl.DateTimeFormat(undefined, {
  weekday: "short",
  month: "short",
  day: "numeric",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

export function AppointmentDetails({ appointmentId }: AppointmentProps) {
  const [appointment, setAppointment] = useState<PatientAppointment>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    const loadAppointment = async () => {
      setLoading(true);
      setError(null);

      try {
        const data = await AppointmentsService.byId(appointmentId);
        if (!active) return;
        setAppointment(data);
      } catch (e) {
        if (!active) return;
        setError(e instanceof Error ? e.message : "Failed to load appointment.");
      } finally {
        if (active) setLoading(false);
      }
    };

    void loadAppointment();

    return () => {
      active = false;
    };
  }, [appointmentId]);

  if (loading) {
    return <AppointmentDetailsSkeleton />;
  }

  const start = new Date(appointment.startAt);

  if (error || !appointment) {
    return (
      <div className="mx-auto w-full max-w-6xl md:px-4 py-8">
        <Link href="/profile" className="text-sm font-medium text-primary hover:text-primary-hover">
          Back to profile
        </Link>
        <div className="mt-4 rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
          {error?.message ?? "Appointment not found."}
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-6xl md:px-4 py-8 ">
      <Link href="/profile" className="text-sm font-medium text-primary hover:text-primary-hover">
        Back to profile
      </Link>

      <section className="mt-4 rounded-2xl border border-border bg-card md:bg-[url('/images/ui/booking_background.jpg')] md:dark:bg-[url('/images/ui/booking_background_dark.jpg')] bg-cover md:p-5 md:shadow-sm">
        <div className="flex items-start gap-4 p-2 py-8 md:p-6">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">
              Appointment ({appointment.id})
            </h1>
            <p className="mt-2 text-sm text-muted">
              Name: {appointment.firstname + " " + appointment.lastname}
            </p>
            <p className="mt-2 text-sm text-muted">DOB: {appointment.dateOfBirth}</p>
            <p className="mt-2 text-sm text-muted">Clinic: {appointment.clinicName}</p>
            <p className="mt-2 text-sm text-muted">Doctor: Dr {appointment.doctorName}</p>
            <p className="mt-2 text-sm text-muted">Category: {appointment.categoryName}</p>
            <p className="mt-2 text-sm text-muted">StartAt: {dateTimeFormatter.format(start)}</p>
            <p className="mt-2 text-sm text-muted">Duration: {appointment.duration}</p>
          </div>
        </div>
        <div className="mx-auto w-full max-w-4xl md:px-4 py-8">
          <EditBookingForm appointment={appointment} setAppointment={setAppointment} />
        </div>
      </section>
    </div>
  );
}
