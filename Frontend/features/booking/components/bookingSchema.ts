import { z } from "zod";

const datePattern = /^\d{4}-\d{2}-\d{2}$/;
const parseInputDate = (value: string) => new Date(`${value}T00:00:00`);

export const validDate = (label: string) =>
  z
    .string()
    .trim()
    .min(1, `${label} is required`)
    .regex(datePattern, `${label} must use YYYY-MM-DD format`)
    .refine((value) => !Number.isNaN(parseInputDate(value).getTime()), `${label} is invalid`);

export const bookingFormSchema = z.object({
  firstname: z.string().trim().min(2, "First name is required").max(50, "First name is too long"),
  lastname: z.string().trim().min(2, "Last name is required").max(50, "Last name is too long"),
  dateOfBirth: validDate("Date of birth").refine(
    (value) => parseInputDate(value) <= new Date(),
    "Date of birth cannot be in the future",
  ),
  email: z.string().trim().email("Email is invalid"),
});

export type BookingFormValues = z.output<typeof bookingFormSchema>;
