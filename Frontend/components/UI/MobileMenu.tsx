import { useEffect } from "react";

type MobileMenuProps = {
  open: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
};

export default function MobileMenu({ open, onClose, title = "Menu", children }: MobileMenuProps) {
  useEffect(() => {
    if (!open) return;

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  return (
    <div
      className={[
        "fixed inset-0 z-50 md:hidden",
        open ? "pointer-events-auto" : "pointer-events-none",
      ].join(" ")}
      aria-hidden={!open}
    >
      <button
        type="button"
        onClick={onClose}
        className={[
          "absolute inset-0 h-full w-full",
          "bg-slate-900/35 transition-opacity",
          open ? "opacity-100" : "opacity-0",
        ].join(" ")}
        aria-label="Close menu"
      />

      <aside
        className={[
          "absolute right-0 top-0 h-full w-80 max-w-[85vw]",
          "bg-card",
          "border-l border-border shadow-xl",
          "transition-transform duration-200 ease-out",
          open ? "translate-x-0" : "translate-x-full",
        ].join(" ")}
      >
        <div className="flex items-center justify-between border-b border-border p-4">
          <span className="font-semibold text-foreground">{title}</span>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md p-2 text-muted transition hover:bg-primary-soft hover:text-primary focus:outline-none focus:ring-2 focus:ring-primary"
            aria-label="Close menu"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
              <path
                d="M6 6l12 12M18 6l-12 12"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
              />
            </svg>
          </button>
        </div>

        <div className="flex flex-col gap-2 p-4">{children}</div>
      </aside>
    </div>
  );
}
