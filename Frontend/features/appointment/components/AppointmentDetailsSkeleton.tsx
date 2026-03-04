export function AppointmentDetailsSkeleton() {
  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-8 animate-pulse">
      {/* Back link skeleton */}
      <div className="h-4 w-24 rounded bg-muted/60" />

      <section className="mt-4 rounded-2xl border border-border bg-card p-5 shadow-sm">
        <div className="flex items-start gap-4 p-2 py-8 md:p-6">
          <div className="w-full space-y-4">
            {/* Title Skeleton */}
            <div className="h-8 w-1/3 rounded bg-muted" />

            {/* Details list skeleton */}
            <div className="space-y-3">
              <div className="h-4 w-3/4 md:w-2/4 lg:w-1/4 rounded bg-muted/60" />
              <div className="h-4 w-1/5 rounded bg-muted/60" />
              <div className="h-4 w-3/4 md:w-2/4 lg:w-1/4 rounded bg-muted/60" />
              <div className="h-4 w-3/3 md:w-2/3 lg:w-1/3 rounded bg-muted/60" />
              <div className="h-4 w-3/6 md:w-2/6 lg:w-1/6  rounded bg-muted/60" />
              <div className="h-4 w-3/4 md:w-2/4 lg:w-1/4 rounded bg-muted/60" />
              <div className="h-4 w-1/5 rounded bg-muted/60" />
            </div>
          </div>
        </div>

        {/* Edit Form Skeleton Area */}
        <div className="border-t border-border pt-6 mt-6">
          <div className="h-8 w-1/3 rounded bg-muted mb-6" />
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <div className="h-4 w-20 rounded bg-muted/60" />
              <div className="h-10 w-full rounded-xl bg-muted" />
            </div>
            <div className="space-y-2">
              <div className="h-4 w-20 rounded bg-muted/60" />
              <div className="h-10 w-full rounded-xl bg-muted" />
            </div>

            {/* Calendar Placeholder */}
            <div className="md:col-span-2 mt-4">
              <div className="h-96 w-full rounded-2xl bg-muted/40" />
            </div>

            {/* Button Placeholder */}
            <div className="h-11 w-40 rounded-xl bg-muted mt-2" />
          </div>
        </div>
      </section>
    </div>
  );
}
