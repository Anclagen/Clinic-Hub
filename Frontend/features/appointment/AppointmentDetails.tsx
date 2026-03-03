"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AppointmentsService, type PatientAppointment } from "@/api/services/appointmentsService";
import EditBookingForm from "../booking/EditBookingForm";

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
  const [appointment, setAppointment] = useState<PatientAppointment | null>(null);
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
    return <h1>Loading</h1>;
    // return <AppointmentDetailsSkeleton />;
  }

  const start = new Date(appointment.startAt);
  console.log(appointment?.startAt);

  if (error || !appointment) {
    return (
      <div className="mx-auto w-full max-w-6xl px-4 py-8">
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
    <div className="mx-auto w-full max-w-6xl px-4 py-8">
      <Link href="/profile" className="text-sm font-medium text-primary hover:text-primary-hover">
        Back to profile
      </Link>

      <section className="mt-4 rounded-2xl border border-border bg-card p-5 shadow-sm">
        <div className="flex items-start gap-4">
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
        <EditBookingForm />
      </section>
    </div>
  );
}
