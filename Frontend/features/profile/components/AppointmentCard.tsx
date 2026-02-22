import { type PatientAppointment } from "@/api/services/appointmentsService";

const dateTimeFormater = new Intl.DateTimeFormat(undefined, {
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
      <p className="mt-1 text-sm">{dateTimeFormater.format(start)}</p>
      <p className="mt-1 text-sm text-muted">Duration: {appointment.duration} Minutes</p>
      <p className="mt-1 text-sm text-muted">Dr. {appointment.doctorName}</p>
      <p className="text-sm text-muted">{appointment.clinicName}</p>
    </article>
  );
}
