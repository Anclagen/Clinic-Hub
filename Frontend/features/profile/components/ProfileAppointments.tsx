"use client";
import { useMemo, useState } from "react";
import { PatientAppointment } from "@/api/services/appointmentsService";
import AppointmentCard from "../../appointment/components/AppointmentCard";

export function ProfileAppointments({
  appointments,
  now,
  onCancelled,
}: {
  appointments: PatientAppointment[];
  now: number;
  onCancelled?: (appointmentId: string) => void;
}) {
  const [showOlderAppointments, setShowOlderAppointments] = useState(false);

  const { upcomingAppointments, pastAppointments } = useMemo(() => {
    const sorted = [...appointments].sort(
      (a, b) => new Date(a.startAt).getTime() - new Date(b.startAt).getTime(),
    );

    return {
      upcomingAppointments: sorted.filter((a) => new Date(a.startAt).getTime() >= now),
      pastAppointments: sorted.filter((a) => new Date(a.startAt).getTime() < now).reverse(),
    };
  }, [appointments, now]);

  return (
    <section className="mt-6 rounded-2xl border border-border bg-card p-6 shadow-sm">
      <div className="flex items-center justify-between gap-3 text-wrap">
        <h2 className="text-xl font-semibold text-foreground">Appointments</h2>
      </div>

      <div className="mt-4 space-y-3">
        {upcomingAppointments.length === 0 ? (
          <div className="rounded-xl border border-border bg-background/70 px-4 py-3 text-sm text-muted">
            No upcoming appointments.
          </div>
        ) : (
          upcomingAppointments.map((appointment) => (
            <AppointmentCard
              key={appointment.id}
              appointment={appointment}
              onCancelled={onCancelled}
            />
          ))
        )}
      </div>
      {pastAppointments.length > 0 ? (
        <div className="mt-5 text-center">
          <button
            type="button"
            onClick={() => setShowOlderAppointments((prev) => !prev)}
            className="rounded-xl border border-secondary px-4 py-2 text-sm font-medium text-secondary transition hover:bg-secondary-soft"
          >
            {showOlderAppointments
              ? "Hide older appointments"
              : `Show older appointments (${pastAppointments.length})`}
          </button>
        </div>
      ) : null}
      {showOlderAppointments ? (
        <div className="mt-6">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-muted">
            Older appointments
          </h3>
          <div className="mt-3 space-y-3">
            {pastAppointments.map((appointment) => (
              <AppointmentCard
                key={appointment.id}
                appointment={appointment}
                onCancelled={onCancelled}
              />
            ))}
          </div>
        </div>
      ) : null}
    </section>
  );
}
