"use client";

import { useSearchParams } from "next/navigation";
import { Suspense } from "react";

// 1. Move the search params logic into a small sub-component
function AuthHeader() {
  const searchParams = useSearchParams();
  const expired = searchParams.get("expired") === "true";

  return (
    <section className="justify-center text-center">
      <h1 className="text-3xl font-semibold">ClinicHub</h1>
      <p className="mt-2 mb-5">Clean booking, fewer phone calls, less chaos.</p>
      {expired && <p className="mt-2 mb-5 text-lg text-error">Your Session Has Expired</p>}
    </section>
  );
}

// 2. Wrap that component in Suspense in your main layout
export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <main className="min-h-screen bg-background text-foreground flex items-center justify-center p-6">
      <div className="w-full max-w-6xl justify-center items-center">
        <Suspense fallback={<p className="text-center">Loading...</p>}>
          <AuthHeader />
        </Suspense>

        <section className="flex justify-center">{children}</section>
      </div>
    </main>
  );
}
