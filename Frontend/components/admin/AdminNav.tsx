"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

type NavItem = {
  href: string;
  label: string;
  exact?: boolean;
};

const items: NavItem[] = [
  { href: "/admin", label: "Admin", exact: true },
  { href: "/admin/movies", label: "Movies" },
  { href: "/admin/actors", label: "Actors" },
  { href: "/admin/studios", label: "Studios" },
  { href: "/admin/genres", label: "Genres" },
];

export default function AdminNav() {
  const pathname = usePathname();

  const isActive = (href: string, exact?: boolean) => {
    if (!pathname) return false;
    return exact ? pathname === href : pathname === href || pathname.startsWith(href + "/");
  };

  return (
    <nav className="flex gap-2 flex-wrap">
      {items.map(({ href, label, exact }) => {
        const active = isActive(href, exact);

        const className = active
          ? "px-3 py-2 rounded-md text-sm font-medium bg-primary text-primary-foreground focus:outline-none focus:ring-2 focus:ring-primary"
          : "px-3 py-2 rounded-md text-sm font-medium text-text-secondary hover:bg-highlight-soft hover:text-text-primary focus:outline-none focus:ring-2 focus:ring-primary";

        return (
          <Link key={href} href={href} className={className}>
            {label}
          </Link>
        );
      })}
    </nav>
  );
}
