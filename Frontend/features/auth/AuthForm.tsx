"use client";
import Link from "next/link";
import { LoginForm } from "./LoginForm";
import { RegisterForm } from "./RegisterForm";

type Mode = "login" | "register";

export function AuthForm({ mode }: { mode: Mode }) {
  const isLogin = mode === "login";

  return (
    <div className="w-full max-w-md rounded-2xl bg-card shadow-[var(--shadow-card)] border border-border p-6">
      <div>
        <h1 className="text-xl font-semibold text-foreground">
          {isLogin ? "Welcome back" : "Create your account"}
        </h1>

        <p className="mt-1 text-sm text-muted">
          {isLogin
            ? "Log in to manage your appointments."
            : "Register to book and manage appointments."}
        </p>
      </div>

      <div className="my-6">{isLogin ? <LoginForm /> : <RegisterForm />}</div>

      <div className="text-sm text-muted text-center">
        {isLogin ? (
          <>
            Don’t have an account?{" "}
            <Link
              href="/auth/register"
              className="text-primary hover:text-primary-hover font-medium transition"
            >
              Register
            </Link>
          </>
        ) : (
          <>
            Already have an account?{" "}
            <Link
              href="/auth/login"
              className="text-primary hover:text-primary-hover font-medium transition"
            >
              Log in
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
