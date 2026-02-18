import { useState } from "react";
import type { Doctor } from "@/api/services/doctorsService";

function resolveDoctorImageUrl(imageUrl?: string | null): string | null {
  if (!imageUrl) return null;
  if (
    imageUrl.startsWith("http://") ||
    imageUrl.startsWith("https://") ||
    imageUrl.startsWith("/")
  ) {
    return imageUrl;
  }
  return `/images/doctors/${imageUrl}`;
}

type DoctorCardProps = { doctor: Doctor };

export function DoctorCard({ doctor }: DoctorCardProps) {
  const [imgFailed, setImgFailed] = useState(false);

  const initials = `${doctor.firstname[0] ?? ""}${doctor.lastname[0] ?? ""}`.toUpperCase();
  const imageUrl = resolveDoctorImageUrl(doctor.imageUrl);
  const showImage = Boolean(imageUrl) && !imgFailed;

  return (
    <article className="rounded-2xl border border-border bg-card/80 p-4 shadow-clinic">
      <div className="flex items-start gap-3">
        <div className="relative h-26 w-26 shrink-0 overflow-hidden rounded-xl border border-border bg-white/75">
          {showImage ? (
            <img
              src={imageUrl!}
              alt={`Dr. ${doctor.firstname} ${doctor.lastname}`}
              className="h-full w-full object-contain"
              loading="lazy"
              onError={() => setImgFailed(true)}
            />
          ) : (
            <div className="grid h-full w-full place-items-center bg-primary-soft text-sm font-semibold text-primary">
              {initials || "DR"}
            </div>
          )}
        </div>

        <div className="min-w-0">
          <h2 className="truncate text-lg font-semibold text-foreground">
            Dr. {doctor.firstname} {doctor.lastname}
          </h2>

          <div className="mt-2 flex flex-wrap gap-2 text-xs">
            <span className="rounded-full bg-secondary-soft px-2.5 py-1 font-medium text-secondary">
              {doctor.clinicName}
            </span>
            <span className="rounded-full bg-primary-soft px-2.5 py-1 font-medium text-primary">
              {doctor.specialityName}
            </span>
          </div>
        </div>
      </div>
    </article>
  );
}
