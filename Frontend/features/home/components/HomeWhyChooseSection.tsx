const highlights = [
  {
    title: "Secure booking details",
    description: "Appointment requests are submitted through the same secured flow used site-wide.",
  },
  {
    title: "Clear doctor availability",
    description: "Choose a date and time slot from live availability after selecting a doctor.",
  },
  {
    title: "Clinic-first experience",
    description: "Find care by clinic, category, and doctor so patients can book with confidence.",
  },
];

export function HomeWhyChooseSection() {
  return (
    <section>
      <div className="rounded-3xl border border-border bg-card/90 p-6 shadow-sm backdrop-blur-sm">
        <h2 className="mt-2 text-2xl font-semibold text-foreground">
          Booking designed to feel simple and reliable
        </h2>
        <p className="mt-3 text-sm leading-6 text-muted">
          Built for patients who want a straightforward route from selecting a clinic to securing an
          appointment slot.
        </p>

        <div className="mt-5 space-y-3">
          {highlights.map((item, index) => (
            <div
              key={item.title}
              className="flex items-start gap-3 rounded-2xl border border-border bg-background/60 p-4"
            >
              <div className="grid h-8 w-8 shrink-0 place-items-center rounded-full bg-primary-soft text-sm font-semibold text-primary">
                {index + 1}
              </div>
              <div>
                <h3 className="text-sm font-semibold text-foreground">{item.title}</h3>
                <p className="mt-1 text-sm leading-6 text-muted">{item.description}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
