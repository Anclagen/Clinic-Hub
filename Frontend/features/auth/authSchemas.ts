import { z } from "zod";

const email = z.string().trim().email("Enter a valid email");
const password = z
  .string()
  .min(8, "Password must be at least 8 characters")
  .max(128, "Password is too long");

export const loginSchema = z.object({
  email,
  password,
});

export type LoginFormValues = z.infer<typeof loginSchema>;

export const registerSchema = z
  .object({
    firstname: z.string().trim().min(2, "First name is too short").max(50),
    lastname: z.string().trim().min(2, "Last name is too short").max(50),
    dateOfBirth: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, "Use YYYY-MM-DD"),
    email,
    password,
    confirmPassword: z.string(),
  })
  .refine((v) => v.password === v.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

export type RegisterFormValues = z.infer<typeof registerSchema>;
