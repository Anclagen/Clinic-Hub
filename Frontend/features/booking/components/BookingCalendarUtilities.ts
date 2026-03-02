import { addMinutes, areIntervalsOverlapping, format, isSameDay, isAfter } from "date-fns";
import { toZonedTime, fromZonedTime } from "date-fns-tz";

const APP_TIMEZONE = process.env.NEXT_PUBLIC_TIMEZONE || "UTC";

export function toLocalTimeLabel(d: Date) {
  const zoned = toZonedTime(d, APP_TIMEZONE);
  return format(zoned, "HH:mm");
}

export function buildSlotsForDay(args: {
  day: Date;
  dayStartHour: number;
  dayEndHour: number;
  intervalMinutes: number;
  durationMinutes: number;
}) {
  const { day, dayStartHour, dayEndHour, intervalMinutes, durationMinutes } = args;

  const dayStart = fromZonedTime(
    new Date(day.getFullYear(), day.getMonth(), day.getDate(), dayStartHour),
    APP_TIMEZONE,
  );

  const dayEnd = fromZonedTime(
    new Date(day.getFullYear(), day.getMonth(), day.getDate(), dayEndHour),
    APP_TIMEZONE,
  );

  const now = new Date();
  const slots: Array<{ start: Date; end: Date }> = [];

  let current = dayStart;
  while (addMinutes(current, durationMinutes) <= dayEnd) {
    const slotEnd = addMinutes(current, durationMinutes);

    if (isAfter(current, now)) {
      slots.push({ start: current, end: slotEnd });
    }

    current = addMinutes(current, intervalMinutes);
  }
  return slots;
}

export function overlapsAny(
  slot: { start: Date; end: Date },
  booked: Array<{ start: Date; end: Date }>,
) {
  return booked.some((b) =>
    areIntervalsOverlapping(
      { start: slot.start, end: slot.end },
      { start: b.start, end: b.end },
      { inclusive: false },
    ),
  );
}
