export type ClinicOption = {
  id: number;
  name: string;
};

export type CategoryOption = {
  id: number;
  name: string;
};

export type DoctorOption = {
  id: string;
  name: string;
};

export type BookingFormOptions = {
  clinics: ClinicOption[];
  categories: CategoryOption[];
  doctors: DoctorOption[];
};

// TODO: Replace these placeholder functions with real API calls.
export async function getBookingFormOptions(): Promise<BookingFormOptions> {
  return Promise.resolve({
    clinics: [
      { id: 1, name: "Downtown Clinic" },
      { id: 2, name: "Northside Clinic" },
      { id: 3, name: "West End Clinic" },
    ],
    categories: [
      { id: 1, name: "General Practice" },
      { id: 2, name: "Dermatology" },
      { id: 3, name: "Pediatrics" },
    ],
    doctors: [
      { id: "65a87d83-e181-4f4b-a3e6-2fbc4d3849c7", name: "Dr. Rachel Kim" },
      { id: "5b2e7f71-bc31-4c8b-b8f6-b5770dc7d08f", name: "Dr. Michael Green" },
      { id: "9d1f7e20-58f4-4f96-9d7f-1e6ce9fa485c", name: "Dr. Sarah Patel" },
    ],
  });
}
