import Link from "next/link";

export function HomeCtaSection() {
  return (
    <section className="rounded-3xl border border-border bg-background/80 p-6 shadow-sm backdrop-blur-sm my-4">
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h2 className="mt-1 text-2xl font-semibold text-foreground">
            Start with the form above or explore doctors first
          </h2>
          <p className="mt-2 text-sm leading-6 text-muted">
            Browse doctor profiles and clinic locations, then return to book when you are ready.
          </p>
        </div>

        <div className="flex flex-wrap gap-3">
          <Link
            href="/doctors"
            className="inline-flex items-center justify-center rounded-xl border border-border bg-background px-4 py-2.5 text-sm font-medium text-foreground transition hover:bg-primary-soft hover:text-primary focus:outline-none focus:ring-2 focus:ring-primary"
          >
            Browse Doctors
          </Link>
          <Link
            href="/clinics"
            className="inline-flex items-center justify-center rounded-xl bg-secondary px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-secondary-hover focus:outline-none focus:ring-2 focus:ring-secondary"
          >
            View Clinics
          </Link>
        </div>
      </div>
    </section>
  );
}
