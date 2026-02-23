"use client";

import { useMemo } from "react";
import { DayPicker } from "react-day-picker";
import "react-day-picker/style.css";
import { isSameDay, parseISO } from "date-fns";
import { buildSlotsForDay, overlapsAny, toLocalTimeLabel } from "./BookingCalendarUtilities";

export type AppointmentRange = { startAt: string; endAt: string };

type Props = {
  value: string;
  onChange: (nextIso: string) => void;
  onDateChange: (date: Date | null) => void;
  selectedDate: Date | null;
  dayStartHour?: number;
  dayEndHour?: number;
  intervalMinutes?: number;
  durationMinutes: number;
  booked: AppointmentRange[];
  loadingBookings: boolean;
};

export function BookingCalendar({
  value,
  onChange,
  selectedDate,
  onDateChange,
  booked,
  durationMinutes,
  dayStartHour = 8,
  dayEndHour = 16,
  intervalMinutes = 15,
  loadingBookings,
}: Props) {
  const selectedStart = useMemo(() => (value ? parseISO(value) : null), [value]);

  const bookedForDay = useMemo(() => {
    if (!selectedDate) return [];
    const map = booked
      .map((b) => ({ start: parseISO(b.startAt), end: parseISO(b.endAt) }))
      .filter((b) => isSameDay(b.start, selectedDate));
    return map;
  }, [booked, selectedDate]);

  const slots = useMemo(() => {
    if (!selectedDate) return [];
    const slots = buildSlotsForDay({
      day: selectedDate,
      dayStartHour,
      dayEndHour,
      intervalMinutes,
      durationMinutes,
    }).map((slot) => ({
      ...slot,
      disabled: overlapsAny(slot, bookedForDay),
      label: `${toLocalTimeLabel(slot.start)}–${toLocalTimeLabel(slot.end)}`,
    }));
    return slots;
  }, [selectedDate, dayStartHour, dayEndHour, intervalMinutes, durationMinutes, bookedForDay]);

  return (
    <div className="grid gap-4 md:grid-cols-2">
      <div className="rounded-2xl border border-border bg-card p-4">
        <div className="mb-2 text-sm font-medium text-foreground">Select date</div>
        <DayPicker
          mode="single"
          selected={selectedDate ?? undefined}
          onSelect={(d) => {
            onDateChange(d ?? null);
            onChange("");
          }}
          weekStartsOn={1}
          disabled={{ before: new Date() }}
        />
      </div>

      <div className="rounded-2xl border border-border bg-card p-4">
        <div className="mb-2 text-sm font-medium text-foreground">Select time</div>
        {loadingBookings ? (
          <div className="text-sm text-muted">Loading bookings…</div>
        ) : !selectedDate ? (
          <div className="text-sm text-muted">Pick a date first.</div>
        ) : (
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {slots.map((s) => {
              const isSelected = selectedStart?.getTime() === s.start.getTime();
              return (
                <button
                  key={s.start.toISOString()}
                  type="button"
                  disabled={s.disabled}
                  onClick={() => onChange(s.start.toISOString())}
                  className={[
                    "rounded-xl border px-3 py-2 text-sm transition",
                    s.disabled
                      ? "cursor-not-allowed border-border bg-background/50 opacity-60"
                      : "border-border bg-background hover:border-primary",
                    isSelected ? "border-primary bg-primary-soft" : "",
                  ].join(" ")}
                >
                  {s.label}
                </button>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
