import Link from "next/link";

export default function Footer() {
  const year = new Date().getFullYear();

  return (
    <footer className="mt-auto border-t border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-6">
        <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
          <div className="flex items-center gap-3">
            <span className="text-sm text-muted">� {year} ClinicHub. All rights reserved.</span>
            <span className="rounded-full bg-success-soft px-3 py-1 text-xs font-semibold text-success">
              Secure Platform
            </span>
          </div>

          <div className="flex items-center gap-4 text-sm">
            <Link href="/" className="text-foreground/80 transition hover:text-primary">
              Home
            </Link>
            <Link href="/booking" className="text-foreground/80 transition hover:text-primary">
              Booking
            </Link>
            <Link href="/doctors" className="text-foreground/80 transition hover:text-primary">
              Doctors
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
}
