import Link from "next/link";

export default function NotFound() {
  return (
    <section className="mx-auto flex w-full max-w-6xl flex-1 px-4 py-6 md:py-8">
      <div className=" grid w-full overflow-hidden rounded-3xl border border-border bg-card shadow-air backdrop-blur-sm lg:grid-cols-[1.2fr_0.8fr]">
        <div className="relative flex flex-col justify-center px-6 py-10 md:px-10 md:py-14 lg:px-12">
          <h1 className="mt-5 max-w-2xl text-4xl font-semibold leading-tight text-foreground md:text-5xl">
            {"Error 404: This page couldn't be found"}
          </h1>

          <p className="mt-4 max-w-2xl text-base leading-7 text-muted md:text-lg">
            The address may be outdated, the page may have moved, or the link was typed incorrectly.
            Use one of the routes below to get back into the app.
          </p>

          <div className="mt-8 flex flex-wrap gap-3">
            <Link
              href="/"
              className="inline-flex items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover focus:outline-none focus:ring-2 focus:ring-primary"
            >
              Go Home
            </Link>
            <Link
              href="/book"
              className="inline-flex items-center justify-center rounded-full border border-border bg-secondary px-6 py-3 text-sm font-semibold text-white dark:text-black transition hover:bg-background focus:outline-none focus:ring-2 focus:ring-primary"
            >
              Booking
            </Link>
            <Link
              href="/search"
              className="inline-flex items-center justify-center rounded-full border border-border bg-card/80 px-6 py-3 text-sm font-semibold text-foreground transition hover:bg-background focus:outline-none focus:ring-2 focus:ring-primary"
            >
              Find a Doctor
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}
