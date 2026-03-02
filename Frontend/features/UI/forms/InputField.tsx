import * as React from "react";

type Props = {
  label: string;
  name: string;
  error?: string;
  onValueChange?: (value: string) => void;
} & Omit<React.InputHTMLAttributes<HTMLInputElement>, "name" | "onChange"> & {
    onChange?: React.ChangeEventHandler<HTMLInputElement>;
  };

export const InputField = React.forwardRef<HTMLInputElement, Props>(
  (
    {
      label,
      name,
      type = "text",
      placeholder,
      error,
      onChange,
      onValueChange,
      className = "",
      id,
      ...props
    },
    ref,
  ) => {
    const inputId = id ?? name;

    const handleChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
      onChange?.(event);
      onValueChange?.(event.target.value);
    };

    return (
      <label htmlFor={inputId} className="flex flex-col gap-1.5 text-sm">
        <span className="font-medium text-foreground">{label}</span>
        <input
          ref={ref}
          id={inputId}
          name={name}
          type={type}
          placeholder={placeholder}
          onChange={handleChange}
          className={`w-full rounded-[var(--radius-lg)] disabled:bg-primary-soft disabled:opacity-90 border bg-card px-3 py-2 text-sm text-foreground outline-none border-border focus:ring-2 focus:ring-primary-soft focus:border-primary ${error ? "border-error focus:ring-error-soft focus:border-error" : ""} ${className}`}
          aria-invalid={error ? true : undefined}
          aria-describedby={error ? `${inputId}-error` : undefined}
          {...props}
        />

        {error ? (
          <p id={`${inputId}-error`} className="text-sm text-error">
            {error}
          </p>
        ) : null}
      </label>
    );
  },
);

InputField.displayName = "InputField";
