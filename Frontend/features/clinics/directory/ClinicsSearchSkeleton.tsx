import { HeartRateLoader } from "@/features/UI/HeartRateLoader";

export function ClinicsSearchSkeleton() {
  return (
    <div className="space-y-3" aria-hidden="true">
      <div className="h-4 w-28 rounded bg-primary-soft/70" />
      <div className="h-10 w-full rounded-xl border border-border bg-card" />
      <div className="flex justify-end">
        <HeartRateLoader className="text-primary" />
      </div>
    </div>
  );
}
