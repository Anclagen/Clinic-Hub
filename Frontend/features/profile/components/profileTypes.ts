export type ProfileFormState = {
  firstname: string;
  lastname: string;
  email: string;
  dateOfBirth: string;
  gender: string;
  address: string;
  religion: string;
  driverLicenseNumber: string;
  medicalInsuranceMemberNumber: string;
  taxNumber: string;
  socialSecurityNumber: string;
};

export type ProfileFormErrors = Partial<Record<keyof ProfileFormState, string>>;

export const defaultForm: ProfileFormState = {
  firstname: "",
  lastname: "",
  email: "",
  dateOfBirth: "",
  gender: "",
  address: "",
  religion: "",
  driverLicenseNumber: "",
  medicalInsuranceMemberNumber: "",
  taxNumber: "",
  socialSecurityNumber: "",
};
