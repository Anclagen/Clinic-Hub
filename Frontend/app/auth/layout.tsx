export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <main className="min-h-screen bg-background text-foreground flex items-center justify-center p-6">
      <div className="w-full max-w-6xl justify-center items-center">
        <section className="justify-center text-center">
          <h2 className="text-3xl font-semibold">ClinicHub</h2>
          <p className="mt-2 mb-5">Clean booking, fewer phone calls, less chaos.</p>
        </section>

        <section className="flex justify-center">{children}</section>
      </div>
    </main>
  );
}
