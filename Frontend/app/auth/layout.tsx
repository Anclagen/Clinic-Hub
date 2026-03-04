"use client";

import { useSearchParams } from "next/navigation";
export default function AuthLayout({ children }: { children: React.ReactNode }) {
  const searchParams = useSearchParams();
  const expired = searchParams.get("expired") || null;

  return (
    <main className="min-h-screen bg-background text-foreground flex items-center justify-center p-6">
      <div className="w-full max-w-6xl justify-center items-center">
        <section className="justify-center text-center">
          <h1 className="text-3xl font-semibold">ClinicHub</h1>
          <p className="mt-2 mb-5">Clean booking, fewer phone calls, less chaos.</p>
          {expired === "true" ? (
            <p className="mt-2 mb-5 text-lg text-error">Your Session Has Expired</p>
          ) : null}
        </section>

        <section className="flex justify-center">{children}</section>
      </div>
    </main>
  );
}
