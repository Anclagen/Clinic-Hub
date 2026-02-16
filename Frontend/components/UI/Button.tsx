import { Spinner } from "./Spinner";

type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> & {
  loading?: boolean;
  variant?: "primary" | "secondary" | "outline";
};

export function Button({
  loading,
  variant = "primary",
  className = "",
  disabled,
  children,
  ...props
}: ButtonProps) {
  const base =
    "inline-flex items-center justify-center gap-2 rounded-[var(--radius-lg)] px-4 py-2 text-sm font-medium " +
    "transition disabled:opacity-60 disabled:cursor-not-allowed";

  const styles =
    variant === "primary"
      ? "bg-primary text-white hover:bg-primary-hover"
      : variant === "secondary"
        ? "bg-secondary text-white hover:bg-secondary-hover"
        : "bg-card text-foreground border border-border hover:bg-background";

  return (
    <button
      {...props}
      disabled={disabled || loading}
      className={[base, styles, className].join(" ")}
    >
      {loading ? <Spinner className="h-4 w-4" /> : null}
      {children}
    </button>
  );
}
