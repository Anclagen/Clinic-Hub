import Link from "next/link";
import type { Clinic } from "@/api/services/clinicsService";
import { useState } from "react";

type ClinicCardProps = {
  clinic: Clinic;
};

function resolveClinicImageUrl(imageUrl?: string | null): string | undefined {
  if (!imageUrl) return undefined;
  return imageUrl;
}

export function ClinicCard({ clinic }: ClinicCardProps) {
  const [imgFailed, setImgFailed] = useState(false);
  const imageUrl = resolveClinicImageUrl(clinic.imageUrl);
  const showImage = Boolean(imageUrl) && !imgFailed;

  return (
    <Link
      href={`/clinics/${clinic.id}`}
      className="group block overflow-hidden rounded-2xl border border-border bg-card shadow-sm transition hover:border-primary/40 hover:shadow"
    >
      <div className="relative h-44 w-full overflow-hidden border-b border-border bg-background text-primary">
        {showImage ? (
          <img
            src={imageUrl}
            alt={clinic.imageAlt ?? clinic.clinicName}
            className="h-full w-full object-cover transition duration-300 group-hover:scale-[1.02]"
            loading="lazy"
            onError={() => setImgFailed(true)}
          />
        ) : (
          <img
            src="/images/ui/clinic_placeholder.jpg"
            alt={clinic.imageAlt ?? clinic.clinicName}
            className="h-full w-full object-cover transition duration-300 group-hover:scale-[1.02]"
            loading="lazy"
          />
        )}
      </div>

      <div className="p-4">
        <h2 className="truncate text-lg font-semibold text-foreground group-hover:text-primary transition">
          {clinic.clinicName}
        </h2>
        <p className="mt-1 line-clamp-2 text-sm text-muted">
          {clinic.address ?? "Address not available"}
        </p>
        <span className="mt-3 inline-flex rounded-full bg-secondary-soft px-2.5 py-1 text-xs font-medium text-secondary">
          View Clinic
        </span>
      </div>
    </Link>
  );
}
