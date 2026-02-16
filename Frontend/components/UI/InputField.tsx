import { forwardRef } from "react";
type Props = {
  label: string;
  name: string;
  type?: string;
  placeholder?: string;
  error?: string;
} & React.InputHTMLAttributes<HTMLInputElement>;

export const InputField = forwardRef<HTMLInputElement, Props>(
  ({ label, name, type = "text", placeholder, error, className = "", ...props }, ref) => {
    return (
      <div className="space-y-1">
        <label htmlFor={name} className="text-sm font-medium text-foreground">
          {label}
        </label>

        <input
          ref={ref}
          id={name}
          name={name}
          type={type}
          placeholder={placeholder}
          className={[
            "w-full rounded-[var(--radius-lg)] border bg-card px-3 py-2 text-sm text-foreground outline-none",
            "border-border focus:ring-2 focus:ring-primary-soft focus:border-primary",
            error ? "border-error focus:ring-error-soft focus:border-error" : "",
            className,
          ].join(" ")}
          {...props}
        />

        {error ? <p className="text-sm text-error">{error}</p> : null}
      </div>
    );
  },
);
InputField.displayName = "InputField";
