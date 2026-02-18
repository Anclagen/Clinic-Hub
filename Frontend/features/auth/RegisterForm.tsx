import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { registerSchema, type RegisterFormValues } from "./authSchemas";
import { InputField } from "@/features/UI/InputField";
import { Button } from "@/features/UI/Button";
import { ApiError } from "@/api/errors";

// You need to implement register() in your auth service to match your backend.
// If your backend doesn't support register yet, wire it when you add the endpoint.
import { register as registerUser } from "@/api/services/authService";

export function RegisterForm({ onRegistered }: { onRegistered?: () => void }) {
  const [apiError, setApiError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstname: "",
      lastname: "",
      dateOfBirth: "",
      email: "",
      password: "",
      confirmPassword: "",
    },
    mode: "onSubmit",
  });

  const onSubmit = async (values: RegisterFormValues) => {
    setApiError(null);
    setSuccess(false);

    try {
      const { confirmPassword, ...payload } = values;
      await registerUser(payload); // <- your API contract decides exact shape

      setSuccess(true);
      reset();
      onRegistered?.();
    } catch (e) {
      if (e instanceof ApiError) setApiError(e.message);
      else setApiError("Registration failed. Try again.");
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <InputField
          label="First name"
          error={errors.firstname?.message}
          {...register("firstname")}
          autoComplete="given-name"
        />
        <InputField
          label="Last name"
          error={errors.lastname?.message}
          {...register("lastname")}
          autoComplete="family-name"
        />
      </div>

      <InputField
        label="Date of birth"
        placeholder="YYYY-MM-DD"
        error={errors.dateOfBirth?.message}
        {...register("dateOfBirth")}
        autoComplete="bday"
      />

      <InputField
        label="Email"
        type="email"
        placeholder="you@domain.com"
        error={errors.email?.message}
        {...register("email")}
        autoComplete="email"
      />

      <InputField
        label="Password"
        type="password"
        placeholder="At least 8 characters"
        error={errors.password?.message}
        {...register("password")}
        autoComplete="new-password"
      />

      <InputField
        label="Confirm password"
        type="password"
        placeholder="Repeat password"
        error={errors.confirmPassword?.message}
        {...register("confirmPassword")}
        autoComplete="new-password"
      />

      {apiError ? (
        <div className="rounded-[var(--radius-lg)] border border-danger/30 bg-danger/5 p-3 text-sm text-danger">
          {apiError}
        </div>
      ) : null}

      {success ? (
        <div className="inline-flex items-center rounded-full bg-success/10 px-3 py-1 text-sm text-success">
          Registered successfully
        </div>
      ) : null}

      <Button type="submit" loading={isSubmitting} className="w-full" variant="secondary">
        {isSubmitting ? "Registering..." : "Register"}
      </Button>
    </form>
  );
}
