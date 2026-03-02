"use client";
import { type PatientAppointment } from "@/api/services/appointmentsService";
import Link from "next/link";
import { toZonedTime } from "date-fns-tz";
import { format } from "date-fns";
import { useAuthStore } from "@/stores/authStore";

const APP_TIMEZONE = process.env.NEXT_PUBLIC_TIMEZONE || "UTC";

export default function AppointmentCard({ appointment }: { appointment: PatientAppointment }) {
  const utcDate = new Date(appointment.startAt);
  const zonedDate = toZonedTime(utcDate, APP_TIMEZONE);
  const displayDate = format(zonedDate, "EEE, MMM d, yyyy, HH:mm");
  const isFuture = utcDate > new Date();
  const id = useAuthStore((s) => s.id);
  const canEdit = isFuture && id !== null;

  return (
    <article className="rounded-xl border border-border bg-background/70 p-4 shadow-sm">
      <div className="flex justify-between items-start">
        <div>
          <h3 className="text-sm font-semibold text-foreground uppercase tracking-wider">
            {appointment.categoryName}
          </h3>
          <p className="mt-2 text-lg font-medium text-primary">{displayDate}</p>
          <p className="text-xs text-muted-foreground italic">Timezone: {APP_TIMEZONE}</p>
        </div>
      </div>

      <div className="mt-4 space-y-1">
        <p className="text-sm font-medium">Dr. {appointment.doctorName}</p>
        <p className="text-sm text-muted">{appointment.clinicName}</p>
        <p className="text-sm text-muted">Duration: {appointment.duration} min</p>
      </div>

      {canEdit && (
        <div className="mt-6 flex gap-3">
          <Link
            href={`/profile/appointment/${appointment.id}`}
            className="flex-1 text-center bg-primary rounded-[var(--radius-lg)] px-4 py-2 text-sm font-semibold text-white transition hover:bg-primary-hover"
          >
            Edit
          </Link>
          <button className="flex-1 text-center border border-error text-error rounded-[var(--radius-lg)] px-4 py-2 text-sm font-semibold transition hover:bg-error-soft">
            Cancel
          </button>
        </div>
      )}
    </article>
  );
}
