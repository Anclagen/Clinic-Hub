import { HeartRateLoader } from "@/features/UI/HeartRateLoader";

export function BookingCalendarSkeleton() {
  return (
    <div className="grid gap-4 md:grid-cols-2 opacity-60 animate-pulse">
      <div className="rounded-2xl border border-border bg-card p-4 h-[320px] flex flex-col gap-4">
        <div className="h-5 w-24 rounded bg-primary-soft/70" />
        <div className="flex-1 rounded-xl bg-background/50" />
      </div>

      <div className="rounded-2xl border border-border bg-card p-4 h-[320px] flex flex-col">
        <div className="mb-2 h-5 w-24 rounded bg-primary-soft/70" />
        <div className="flex flex-col items-center justify-center flex-1 gap-4">
          <HeartRateLoader className="w-20 text-muted/30" />
          <p className="text-xs text-muted">Awaiting selection...</p>
        </div>
      </div>
    </div>
  );
}
