export function Alert({
  variant = "error",
  children,
}: {
  variant?: "error" | "success" | "warning";
  children: React.ReactNode;
}) {
  const styles =
    variant === "success"
      ? "border-success bg-success-soft text-success"
      : variant === "warning"
        ? "border-warning bg-warning-soft text-warning"
        : "border-error bg-error-soft text-error";

  return (
    <div className={`rounded-[var(--radius-lg)] border p-3 text-sm ${styles}`}>{children}</div>
  );
}
