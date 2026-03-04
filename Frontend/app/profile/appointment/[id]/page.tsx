import { AppointmentDetails } from "@/features/appointment/AppointmentDetails";

type AppointmentPageProps = {
  params: Promise<{
    id: string;
  }>;
};

export default async function ClinicDetailsPage({ params }: AppointmentPageProps) {
  const { id } = await params;

  return (
    <>
      <AppointmentDetails appointmentId={id} />
    </>
  );
}
