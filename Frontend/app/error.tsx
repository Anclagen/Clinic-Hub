"use client";

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-text-primary">Something broke</h1>
      <p className="mt-2 text-text-secondary">Could not load movie(s). Try again.</p>

      <pre className="mt-4 rounded-lg border border-border bg-surface p-4 text-xs text-text-muted overflow-auto">
        {error.message}
      </pre>

      <button
        type="button"
        onClick={() => reset()}
        className="mt-6 inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-hover focus:outline-none focus:ring-2 focus:ring-primary"
      >
        Retry
      </button>
    </div>
  );
}
