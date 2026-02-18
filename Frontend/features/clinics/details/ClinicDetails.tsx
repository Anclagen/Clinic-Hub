"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { ClinicsService, type Clinic } from "@/api/services/clinicsService";
import { DoctorsGrid } from "@/features/doctors/components/DoctorsGrid";
import { ClinicDetailsSkeleton } from "./ClinicDetailsSkeleton";

type ClinicDetailsProps = {
  clinicId: number;
};

function resolveClinicImageUrl(imageUrl?: string | null): string | null {
  if (!imageUrl) return null;
  if (
    imageUrl.startsWith("http://") ||
    imageUrl.startsWith("https://") ||
    imageUrl.startsWith("/")
  ) {
    return imageUrl;
  }
  return imageUrl;
}

export function ClinicDetails({ clinicId }: ClinicDetailsProps) {
  const [clinic, setClinic] = useState<Clinic | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    const loadClinic = async () => {
      setLoading(true);
      setError(null);

      try {
        const data = await ClinicsService.byId(clinicId);
        if (!active) return;
        setClinic(data);
      } catch (e) {
        if (!active) return;
        setError(e instanceof Error ? e.message : "Failed to load clinic.");
      } finally {
        if (active) setLoading(false);
      }
    };

    void loadClinic();

    return () => {
      active = false;
    };
  }, [clinicId]);

  if (loading) {
    return <ClinicDetailsSkeleton />;
  }

  if (error || !clinic) {
    return (
      <div className="mx-auto w-full max-w-6xl px-4 py-8">
        <Link href="/clinics" className="text-sm font-medium text-primary hover:text-primary-hover">
          Back to clinics
        </Link>
        <div className="mt-4 rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
          {error ?? "Clinic not found."}
        </div>
      </div>
    );
  }

  const imageUrl = resolveClinicImageUrl(clinic.imageUrl);

  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-8">
      <Link href="/clinics" className="text-sm font-medium text-primary hover:text-primary-hover">
        Back to clinics
      </Link>

      <section className="mt-4 rounded-2xl border border-border bg-card p-5 shadow-sm">
        <div className="flex items-start gap-4">
          <div className="relative grid h-20 w-20 shrink-0 place-items-center overflow-hidden rounded-xl border border-border bg-background text-primary">
            {imageUrl ? (
              <img
                src={imageUrl}
                alt={clinic.imageAlt ?? clinic.clinicName}
                className="h-full w-full object-contain p-1"
              />
            ) : (
              <span className="text-lg font-semibold">
                {clinic.clinicName.slice(0, 2).toUpperCase()}
              </span>
            )}
          </div>

          <div>
            <h1 className="text-2xl font-semibold text-foreground">{clinic.clinicName}</h1>
            <p className="mt-2 text-sm text-muted">{clinic.address ?? "Address not available"}</p>
          </div>
        </div>
      </section>

      <DoctorsGrid query={{ clinicId }} title="Doctors at this clinic" />
    </div>
  );
}
