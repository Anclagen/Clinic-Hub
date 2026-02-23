import BookingForm from "@/features/booking/BookingForm";

export function HomeBookingSection() {
  return (
    <div className="my-4 relative w-full">
      <div className="md:border md:border-border md:rounded-3xl md:shadow-sm md:bg-[url('/images/ui/booking_background.jpg')] md:dark:bg-[url('/images/ui/booking_background_dark.jpg')]">
        <div
          aria-hidden
          className="pointer-events-none absolute inset-x-0 rounded-3xl
      bg-[radial-gradient(ellipse_at_center,rgba(37,99,235,0.18),rgba(37,99,235,0.06),transparent)]
      blur-0 h-full"
        />
        <div className="mx-auto w-full max-w-4xl md:px-4 py-8">
          <BookingForm />
        </div>
      </div>
    </div>
  );
}
