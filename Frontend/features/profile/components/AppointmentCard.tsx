import { type PatientAppointment } from "@/api/services/appointmentsService";
import Link from "next/link";

const dateTimeFormatter = new Intl.DateTimeFormat(undefined, {
  weekday: "short",
  month: "short",
  day: "numeric",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

export default function AppointmentCard({ appointment }: { appointment: PatientAppointment }) {
  const start = new Date(appointment.startAt);

  return (
    <article className="rounded-xl border border-border bg-background/70 p-4">
      <h3 className="text-sm font-semibold text-foreground">{appointment.categoryName}</h3>
      <p className="mt-1 text-sm">{dateTimeFormatter.format(start)}</p>
      <p className="mt-1 text-sm text-muted">Duration: {appointment.duration} Minutes</p>
      <p className="mt-1 text-sm text-muted">Dr. {appointment.doctorName}</p>
      <p className="text-sm text-muted">{appointment.clinicName}</p>
      {start > new Date() ? (
        <>
          <Link
            href={"/profile/appointment/" + appointment.id}
            className="inline-flex rounded-xl bg-primary mt-4 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover"
          >
            Edit Appointment
          </Link>
          <button className="inline-flex rounded-xl bg-danger mt-4 ms-5 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-error-soft hover:text-primary">
            Cancel Appointment
          </button>
        </>
      ) : null}
    </article>
  );
}
