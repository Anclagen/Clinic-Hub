const services = [
  {
    title: "General Checkups",
    description:
      "Routine appointments for everyday health concerns, annual reviews, and wellness planning.",
    tag: "Primary Care",
  },
  {
    title: "Specialist Referrals",
    description:
      "Connect with specialist doctors and book follow-up consultations through partnered clinics.",
    tag: "Referral Support",
  },
  {
    title: "Vaccination Visits",
    description:
      "Book vaccine appointments and preventive care sessions with clear time-slot availability.",
    tag: "Prevention",
  },
  {
    title: "Remote Follow-ups",
    description:
      "Arrange convenient follow-up appointments after in-clinic visits for ongoing treatment plans.",
    tag: "Continuity",
  },
];

export function HomeServicesSection() {
  return (
    <section className="rounded-3xl border border-border bg-card/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="mb-5 flex flex-col gap-2">
        <h2 className="text-2xl font-semibold text-foreground">Care options patients use most</h2>
        <p className="max-w-3xl text-sm leading-6 text-muted">
          Example service categories to help visitors understand what they can book through the
          platform.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {services.map((service) => (
          <article
            key={service.title}
            className="rounded-2xl border border-border bg-background/70 p-5 shadow-clean transition hover:border-primary/30"
          >
            <span className="inline-flex rounded-full bg-secondary-soft px-3 py-1 text-xs font-semibold text-secondary">
              {service.tag}
            </span>
            <h3 className="mt-3 text-lg font-semibold text-foreground">{service.title}</h3>
            <p className="mt-2 text-sm leading-6 text-muted">{service.description}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
