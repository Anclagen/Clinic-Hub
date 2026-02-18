import { ClinicCardSkeleton } from "../components/ClinicCardSkeleton";

type ClinicsGridSkeletonProps = {
  count?: number;
};

export function ClinicsGridSkeleton({ count = 6 }: ClinicsGridSkeletonProps) {
  return (
    <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3" aria-hidden="true">
      {Array.from({ length: count }).map((_, index) => (
        <ClinicCardSkeleton key={`clinic-skeleton-${index}`} />
      ))}
    </div>
  );
}
