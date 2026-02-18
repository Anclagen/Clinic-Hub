import { HeartRateLoader } from "@/features/UI/HeartRateLoader";

export function ClinicCardSkeleton() {
  return (
    <article className="relative overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
      <div className="h-44 w-full border-b border-border bg-primary-soft/55" />

      <div className="space-y-3 p-4">
        <div className="h-5 w-3/4 rounded-md bg-primary-soft/70" />
        <div className="h-4 w-full rounded bg-primary-soft/50" />
        <div className="h-4 w-2/3 rounded bg-primary-soft/50" />
        <div className="h-6 w-24 rounded-full bg-secondary-soft/80" />
      </div>

      <div className="pointer-events-none absolute inset-0 z-10 grid place-items-center">
        <HeartRateLoader className="heart-rate-loader--card text-secondary" />
      </div>
    </article>
  );
}
