"use client";
import { type PatientAppointment } from "@/api/services/appointmentsService";
import Link from "next/link";
import { toZonedTime } from "date-fns-tz";
import { format } from "date-fns";
import { useAuthStore } from "@/stores/authStore";
import { AppointmentsService } from "@/api/services/appointmentsService";
import { useState } from "react";
import { Spinner } from "@/features/UI/Spinner";
import { useRouter } from "next/navigation";
import { isApiError } from "@/api/errors";

const APP_TIMEZONE = process.env.NEXT_PUBLIC_TIMEZONE || "UTC";

export default function AppointmentCard({
  appointment,
  onCancelled,
}: {
  appointment: PatientAppointment;
  onCancelled?: (appointmentId: string) => void;
}) {
  const [isCancelling, setIsCancelling] = useState(false);
  const [awaitingResponse, setAwaitingResponse] = useState(false);
  const [error, setError] = useState<null | string>(null);
  const [isDeleted, setIsDeleted] = useState(false);
  const utcDate = new Date(appointment.startAt);
  const zonedDate = toZonedTime(utcDate, APP_TIMEZONE);
  const displayDate = format(zonedDate, "EEE, MMM d, yyyy, HH:mm");
  const isFuture = utcDate > new Date();
  const id = useAuthStore((s) => s.id);
  const logout = useAuthStore((s) => s.logout);
  const router = useRouter();
  const canEdit = isFuture && id !== null;

  const toggleCancellation = () => {
    setIsCancelling(!isCancelling);
  };

  const confirmCancellation = async () => {
    setAwaitingResponse(true);
    try {
      await AppointmentsService.cancel(appointment.id);
      setAwaitingResponse(false);
      setIsDeleted(true);
      onCancelled?.(appointment.id);
    } catch (error) {
      if (isApiError(error) && error.status === 401) {
        logout();
        router.push("/auth/login?expired=true");
        return;
      }
      setAwaitingResponse(false);
      setError("An error occurred.");
    }
  };

  if (isDeleted)
    return (
      <div className="rounded-xl border border-border bg-background/70 p-4 shadow-sm">
        <p className="text-sm font-semibold text-foreground uppercase tracking-wider">
          Appointment Deleted
        </p>
      </div>
    );

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
        <>
          {error ? (
            <div className="mt-3 rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
              {error}
            </div>
          ) : null}
          <div className="mt-6 flex gap-3">
            {isCancelling ? (
              <>
                <button
                  onClick={confirmCancellation}
                  disabled={awaitingResponse}
                  className="flex-1 text-center border border-error-soft bg-error text-white rounded-[var(--radius-lg)] px-4 py-2 text-sm font-semibold transition hover:bg-error-soft hover:border-error hover:text-error"
                >
                  {awaitingResponse ? (
                    <span className="flex justify-center gap-3">
                      <Spinner /> Cancelling...
                    </span>
                  ) : (
                    "Confirm Cancellation"
                  )}
                </button>
                <button
                  onClick={toggleCancellation}
                  disabled={awaitingResponse}
                  className={`flex-1 text-center border border-error text-error rounded-[var(--radius-lg)] px-4 py-2 text-sm font-semibold transition hover:bg-error-soft ${awaitingResponse ? "disabled:opacity-50" : ""}`}
                >
                  Cancel Cancellation
                </button>
              </>
            ) : (
              <>
                <Link
                  href={`/profile/appointment/${appointment.id}`}
                  className="flex-1 text-center bg-primary rounded-[var(--radius-lg)] px-4 py-2 text-sm font-semibold text-white transition hover:bg-primary-hover"
                >
                  Edit
                </Link>
                <button
                  onClick={toggleCancellation}
                  className="flex-1 text-center border border-error text-error rounded-[var(--radius-lg)] px-4 py-2 text-sm font-semibold transition hover:bg-error-soft"
                >
                  Cancel
                </button>
              </>
            )}
          </div>
        </>
      )}
    </article>
  );
}
