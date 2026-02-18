import { DoctorCardSkeleton } from "./DoctorCardSkeleton";

type DoctorsGridSkeletonProps = {
  count?: number;
};

export function DoctorsGridSkeleton({ count = 10 }: DoctorsGridSkeletonProps) {
  return (
    <div className="grid gap-4 sm:grid-cols-2" aria-hidden="true">
      {Array.from({ length: count }).map((_, index) => (
        <DoctorCardSkeleton key={`doctor-skeleton-${index}`} />
      ))}
    </div>
  );
}
