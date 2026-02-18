export function DoctorsFiltersSkeleton() {
  return (
    <div className="grid gap-3 md:grid-cols-3" aria-hidden="true">
      <div className="md:col-span-3">
        <div className="h-4 w-32 rounded bg-primary-soft/70" />
        <div className="mt-1.5 h-10 w-full rounded-xl border border-border bg-card" />
      </div>

      <div>
        <div className="h-4 w-20 rounded bg-primary-soft/70" />
        <div className="mt-1.5 h-10 w-full rounded-xl border border-border bg-card" />
      </div>

      <div>
        <div className="h-4 w-24 rounded bg-primary-soft/70" />
        <div className="mt-1.5 h-10 w-full rounded-xl border border-border bg-card" />
      </div>

      <div className="flex items-end">
        <div className="h-10 w-full rounded-xl border border-secondary/40 bg-secondary-soft/60" />
      </div>
    </div>
  );
}
