import Link from "next/link";

export type NavItem = {
  href: string;
  label: string;
  requiresAuth?: boolean;
  hideWhenAuth?: boolean;
  cta?: "primary" | "secondary";
};

type NavLinksProps = {
  items: NavItem[];
  isActive: (href: string) => boolean;
  onNavigate?: () => void;
  variant?: "desktop" | "mobile";
};

export default function NavLinks({
  items,
  isActive,
  onNavigate,
  variant = "desktop",
}: NavLinksProps) {
  const base =
    variant === "desktop"
      ? "rounded-full px-4 py-2 text-sm font-medium transition focus:outline-none focus:ring-2 focus:ring-primary"
      : "w-full rounded-xl px-4 py-3 text-left text-base font-medium transition";

  const getStateClasses = (item: NavItem, active: boolean) => {
    if (item.cta === "primary") {
      return "bg-primary text-white shadow-sm hover:bg-primary-hover";
    }

    if (item.cta === "secondary") {
      return "border border-secondary bg-secondary text-black  hover:bg-secondary-hover";
    }

    if (active) {
      return "bg-primary-soft text-primary";
    }

    return "text-foreground/80 hover:bg-primary-soft hover:text-primary";
  };

  return (
    <>
      {items.map((item) => {
        const active = isActive(item.href);

        return (
          <Link
            key={item.href}
            href={item.href}
            onClick={onNavigate}
            className={[base, getStateClasses(item, active)].join(" ")}
            aria-current={active ? "page" : undefined}
          >
            {item.label}
          </Link>
        );
      })}
    </>
  );
}
