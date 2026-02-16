import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { loginSchema, type LoginFormValues } from "./authSchemas";
import { InputField } from "@/components/UI/InputField";
import { Button } from "@/components/UI/Button";
import { login } from "@/api/services/authService";
import { ApiError } from "@/api/errors";
import { useRouter } from "next/navigation";

export function LoginForm() {
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
    mode: "onSubmit",
  });

  const onSubmit = async (values: LoginFormValues) => {
    setApiError(null);
    try {
      await login(values);
      router.push("/profile");
    } catch (e) {
      if (e instanceof ApiError) setApiError(e.message);
      else setApiError("Login failed. Try again.");
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
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
        placeholder="••••••••"
        error={errors.password?.message}
        {...register("password")}
        autoComplete="current-password"
      />

      {apiError ? (
        <div className="rounded-[var(--radius-lg)] border border-danger/30 bg-danger/5 p-3 text-sm text-danger">
          {apiError}
        </div>
      ) : null}

      <Button type="submit" loading={isSubmitting} className="w-full">
        {isSubmitting ? "Logging in..." : "Login"}
      </Button>
    </form>
  );
}
