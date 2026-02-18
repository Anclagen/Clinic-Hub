import { HeartRateLoader } from "../../UI/HeartRateLoader";

export function DoctorCardSkeleton() {
  return (
    <article className="relative overflow-hidden rounded-2xl border border-border bg-card/80 p-4 shadow-clinic">
      <div className="flex items-start gap-3">
        <div className="h-26 w-26 shrink-0 rounded-xl border border-border bg-primary-soft/65" />
        <div className="min-w-0 flex-1">
          <div className="h-5 w-3/4 rounded-md bg-primary-soft/70" />
          <div className="mt-2 flex flex-wrap gap-2">
            <div className="h-6 w-24 rounded-full bg-secondary-soft/80" />
            <div className="h-6 w-28 rounded-full bg-primary-soft/70" />
          </div>
        </div>
      </div>
      <div className="pointer-events-none absolute inset-0 z-10 grid place-items-center">
        <HeartRateLoader className="heart-rate-loader--card text-secondary" />
      </div>
    </article>
  );
}
