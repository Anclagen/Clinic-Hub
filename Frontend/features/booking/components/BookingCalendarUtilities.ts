import { addMinutes, areIntervalsOverlapping, format, set, isSameDay, isAfter } from "date-fns";

export function toLocalTimeLabel(d: Date) {
  return format(d, "HH:mm");
}

export function buildSlotsForDay(args: {
  day: Date;
  dayStartHour: number;
  dayEndHour: number;
  intervalMinutes: number;
  durationMinutes: number;
}) {
  const { day, dayStartHour, dayEndHour, intervalMinutes, durationMinutes } = args;

  const dayStart = set(day, {
    hours: dayStartHour,
    minutes: 0,
  });
  const dayEnd = set(day, {
    hours: dayEndHour,
    minutes: 0,
  });
  const now = new Date();
  const isSelectedDayToday = isSameDay(day, now);

  const slots: Array<{ start: Date; end: Date }> = [];
  for (
    let start = dayStart;
    addMinutes(start, durationMinutes) <= dayEnd;
    start = addMinutes(start, intervalMinutes)
  ) {
    const end = addMinutes(start, durationMinutes);
    if (isSelectedDayToday) {
      if (isAfter(start, now)) {
        slots.push({ start, end });
      }
    } else {
      slots.push({ start, end });
    }
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
