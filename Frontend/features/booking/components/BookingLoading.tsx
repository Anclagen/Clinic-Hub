import { HeartRateLoader } from "@/features/UI/HeartRateLoader";

function FieldSkeleton() {
  return (
    <div className="flex flex-col gap-1.5 text-sm">
      <div className="h-5 w-28 rounded bg-primary-soft/70" />
      <div className="h-10 w-full rounded-xl border border-border bg-background" />
    </div>
  );
}

export function BookingLoading() {
  return (
    <div className="mx-auto w-full max-w-4xl py-8" aria-hidden="true">
      <section className="rounded-2xl border border-border bg-card p-6 shadow-sm">
        <div className="h-7 w-60 rounded bg-primary-soft/70" />
        <div className="mt-6 grid gap-4 md:grid-cols-2">
          <FieldSkeleton />
          <FieldSkeleton />
          <FieldSkeleton />
          <FieldSkeleton />
          <FieldSkeleton />
          <FieldSkeleton />
          <FieldSkeleton />
          <div className="md:col-span-2 flex items-center gap-4">
            <div className="h-10 w-40 rounded-xl bg-primary/30" />
            <div className="flex-1" />
            <HeartRateLoader className="text-secondary" />
          </div>
        </div>
      </section>
    </div>
  );
}
