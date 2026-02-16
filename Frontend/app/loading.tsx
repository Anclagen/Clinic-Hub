export default function Loading() {
  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-text-primary">Movies</h1>
      <p className="mt-1 text-sm text-text-muted">Loading…</p>

      <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <div key={i} className="overflow-hidden rounded-xl border border-border bg-surface">
            {/* Poster skeleton */}
            <div className="relative aspect-[2/3] w-full bg-border">
              <div className="absolute inset-0 animate-pulse bg-border" />
              <div className="absolute inset-x-0 bottom-0 p-4">
                <div className="h-5 w-3/4 animate-pulse rounded bg-border" />
              </div>
            </div>

            {/* Body skeleton */}
            <div className="p-4">
              <div className="h-4 w-full animate-pulse rounded bg-border" />
              <div className="mt-2 h-4 w-5/6 animate-pulse rounded bg-border" />
              <div className="mt-2 h-4 w-2/3 animate-pulse rounded bg-border" />

              <div className="mt-4 h-4 w-24 animate-pulse rounded bg-border" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
