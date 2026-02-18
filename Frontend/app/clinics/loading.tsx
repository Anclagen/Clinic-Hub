import { ClinicsGridSkeleton } from "@/features/clinics/directory/ClinicsGridSkeleton";
import { ClinicsSearchSkeleton } from "@/features/clinics/directory/ClinicsSearchSkeleton";

export default function ClinicsLoading() {
  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-8" aria-hidden="true">
      <div className="mb-6">
        <div className="h-8 w-44 rounded-md bg-primary-soft/70" />
        <div className="mt-2 h-4 w-80 max-w-full rounded bg-primary-soft/50" />
      </div>

      <section className="rounded-2xl border border-border bg-card p-4 shadow-sm">
        <ClinicsSearchSkeleton />
      </section>

      <section className="mt-6">
        <div className="mb-3 h-4 w-24 rounded bg-primary-soft/70" />
        <ClinicsGridSkeleton count={6} />
      </section>
    </div>
  );
}
