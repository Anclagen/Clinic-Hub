import Link from "next/link";

export default function Footer() {
  const year = new Date().getFullYear();

  return (
    <footer className="mt-auto px-4 pb-4 pt-3">
      <div className="mx-auto max-w-6xl rounded-2xl border border-border bg-card shadow-sm">
        <div className="flex flex-col items-center justify-between gap-4 px-4 py-4 sm:flex-row md:px-6">
          <div className="flex items-center gap-3">
            <span className="text-sm text-muted">� {year} ClinicHub. All rights reserved.</span>
            <span className="rounded-full bg-success-soft px-3 py-1 text-xs font-semibold text-success">
              Secure Platform
            </span>
          </div>

          <div className="flex items-center gap-4 text-sm">
            <Link
              href="/"
              className="rounded-full px-3 py-1.5 text-foreground/80 transition hover:bg-primary-soft hover:text-primary"
            >
              Home
            </Link>
            <Link
              href="/book"
              className="rounded-full px-3 py-1.5 text-foreground/80 transition hover:bg-primary-soft hover:text-primary"
            >
              Booking
            </Link>
            <Link
              href="/doctors"
              className="rounded-full px-3 py-1.5 text-foreground/80 transition hover:bg-primary-soft hover:text-primary"
            >
              Doctors
            </Link>
            <Link
              href="/clinics"
              className="rounded-full px-3 py-1.5 text-foreground/80 transition hover:bg-primary-soft hover:text-primary"
            >
              Clinics
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
}
