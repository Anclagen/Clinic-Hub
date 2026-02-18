import { DoctorsGridSkeleton } from "@/features/doctors/components/DoctorsGridSkeleton";
import { HeartRateLoader } from "@/features/UI/HeartRateLoader";

export function ClinicDetailsSkeleton() {
  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-8" aria-hidden="true">
      <div className="h-4 w-28 rounded bg-primary-soft/70" />

      <section className="relative mt-4 overflow-hidden rounded-2xl border border-border bg-card p-5 shadow-sm">
        <div className="flex items-start gap-4">
          <div className="h-20 w-20 shrink-0 rounded-xl border border-border bg-primary-soft/65" />
          <div className="min-w-0 flex-1">
            <div className="h-7 w-56 rounded-md bg-primary-soft/70" />
            <div className="mt-2 h-4 w-72 max-w-full rounded bg-primary-soft/50" />
          </div>
        </div>

        <div className="pointer-events-none absolute inset-0 z-10 grid place-items-center">
          <HeartRateLoader className="heart-rate-loader--card text-secondary" />
        </div>
      </section>

      <section className="mt-6">
        <div className="mb-3 h-7 w-56 rounded-md bg-primary-soft/70" />
        <DoctorsGridSkeleton count={6} />
      </section>
    </div>
  );
}
