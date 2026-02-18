import { ClinicDetails } from "@/features/clinics/details/ClinicDetails";

type ClinicDetailsPageProps = {
  params: Promise<{
    id: string;
  }>;
};

export default async function ClinicDetailsPage({ params }: ClinicDetailsPageProps) {
  const { id } = await params;
  const clinicId = Number(id);

  if (!Number.isInteger(clinicId) || clinicId <= 0) {
    return (
      <div className="mx-auto w-full max-w-6xl px-4 py-8">
        <p className="rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
          Invalid clinic id.
        </p>
      </div>
    );
  }

  return <ClinicDetails clinicId={clinicId} />;
}
