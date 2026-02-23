import Image from "next/image";
import Link from "next/link";

export function HomeHero() {
  return (
    <section className="relative overflow-hidden rounded-3xl border border-border shadow-air">
      <div className="absolute inset-0 bg-[url('/images/ui/banner_background.jpg')] bg-cover bg-center" />
      <div className="absolute inset-0 bg-gradient-to-r from-white/92 via-white/88 to-white/25 dark:from-slate-950/88 dark:via-slate-900/80 dark:to-slate-900/35" />
      <div className="absolute -left-10 top-10 h-28 w-28 rounded-full bg-primary/10 blur-2xl" />
      <div className="absolute bottom-4 right-20 h-36 w-36 rounded-full bg-secondary/10 blur-2xl" />
      <div className="relative grid min-h-[360px] md:gap-8 p-6 md:grid-cols-[1.5fr_0.85fr] md:p-10 lg:min-h-[430px] lg:px-12">
        <div className="max-w-2xl self-center">
          <h1 className="text-3xl font-semibold leading-tight text-foreground md:text-4xl lg:text-5xl">
            Your Health, <span className="text-primary">Our Priority</span>
          </h1>

          <p className="mt-4 max-w-xl text-base leading-7 text-muted md:text-lg">
            Book appointments with trusted doctors in minutes. Use the quick booking form below or
            browse the full booking page when you need more time.
          </p>

          <div className="mt-6 flex flex-wrap gap-3">
            <Link
              href="/doctors"
              className="inline-flex items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-hover focus:outline-none focus:ring-2 focus:ring-primary"
            >
              Find a Doctor
            </Link>
            <Link
              href="/booking"
              className="inline-flex items-center justify-center rounded-full border border-border bg-card/70 px-6 py-3 text-sm font-semibold text-foreground transition hover:bg-background focus:outline-none focus:ring-2 focus:ring-primary"
            >
              Open Booking Page
            </Link>
          </div>
        </div>

        <div className="relative mx-auto flex h-full w-full max-w-[260px] items-end md:max-w-[380px] lg:max-w-[430px]">
          <div className="absolute inset-x-6 bottom rounded-full bg-primary/10 blur-xl" />
          <Image
            src="/images/ui/banner_doctor.png"
            alt="Doctor smiling with stethoscope"
            width={900}
            height={900}
            priority
            className="-bottom-[2.75rem] relative h-auto w-full self-end object-contain object-bottom drop-shadow-[0_18px_36px_rgba(3,4,94,0.12)]"
          />
        </div>
      </div>
    </section>
  );
}
