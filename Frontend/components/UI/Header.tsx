"use client";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useMemo, useState, useCallback, useEffect } from "react";
import { useAuthStore } from "@/stores/authStore";
import MobileMenu from "./MobileMenu";
import NavLinks, { type NavItem } from "./NavLinks";

type ThemeMode = "light" | "dark";
const THEME_KEY = "theme";

export default function Header() {
  const pathname = usePathname();
  const token = useAuthStore((s) => s.token);
  const logout = useAuthStore((s) => s.logout);
  const [isOpen, setIsOpen] = useState(false);
  const [theme, setTheme] = useState<ThemeMode>("light");

  const isAuthed = Boolean(token);

  const navItems: NavItem[] = useMemo(
    () => [
      { href: "/", label: "Home" },
      { href: "/booking", label: "Booking" },
      { href: "/doctors", label: "Doctors" },
      { href: "/clinics", label: "Clinics" },
      { href: "/profile", label: "Profile", requiresAuth: true },
      { href: "/login", label: "Login", hideWhenAuth: true, cta: "secondary" },
      { href: "/register", label: "Register", hideWhenAuth: true, cta: "primary" },
    ],
    [],
  );

  const visibleItems = useMemo(() => {
    return navItems.filter((item) => {
      if (item.requiresAuth && !isAuthed) return false;
      if (item.hideWhenAuth && isAuthed) return false;
      return true;
    });
  }, [navItems, isAuthed]);

  const isActive = (href: string) => {
    if (href === "/") return pathname === "/";
    return pathname?.startsWith(href);
  };

  const closeMobileMenu = useCallback(() => {
    setIsOpen(false);
  }, []);

  const applyTheme = useCallback((nextTheme: ThemeMode) => {
    const root = document.documentElement;
    root.classList.toggle("dark", nextTheme === "dark");
    root.style.colorScheme = nextTheme;
  }, []);

  useEffect(() => {
    const savedTheme = window.localStorage.getItem(THEME_KEY);

    if (savedTheme === "light" || savedTheme === "dark") {
      setTheme(savedTheme);
      applyTheme(savedTheme);
      return;
    }

    const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
    const initialTheme: ThemeMode = prefersDark ? "dark" : "light";

    setTheme(initialTheme);
    applyTheme(initialTheme);
  }, [applyTheme]);

  const toggleTheme = () => {
    const nextTheme: ThemeMode = theme === "dark" ? "light" : "dark";
    setTheme(nextTheme);
    applyTheme(nextTheme);
    window.localStorage.setItem(THEME_KEY, nextTheme);
  };

  return (
    <header className="sticky top-0 z-50 border-b border-border/80 bg-card/95 shadow-sm backdrop-blur supports-[backdrop-filter]:bg-card/85">
      <div className="mx-auto max-w-6xl px-4">
        <nav className="flex items-center justify-between py-3">
          <Link
            href="/"
            className="flex items-center gap-3 rounded-md px-1 py-1 focus:outline-none focus:ring-2 focus:ring-primary"
            aria-label="Go to home"
            onClick={closeMobileMenu}
          >
            <div className="grid h-9 w-9 place-items-center rounded-lg bg-primary text-white font-bold">
              CH
            </div>
            <span className="hidden text-lg font-semibold tracking-tight text-foreground sm:block">
              ClinicHub
            </span>
          </Link>

          <div className="hidden items-center gap-2 md:flex">
            <NavLinks
              items={visibleItems}
              isActive={isActive}
              variant="desktop"
              onNavigate={closeMobileMenu}
            />
            <button
              type="button"
              onClick={toggleTheme}
              className="rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground/80 transition hover:border-secondary hover:bg-secondary-soft hover:text-secondary dark:hover:bg-secondary-soft/40 focus:outline-none focus:ring-2 focus:ring-primary"
              aria-label={`Switch to ${theme === "dark" ? "light" : "dark"} mode`}
            >
              {theme === "dark" ? "Light" : "Dark"}
            </button>
            {isAuthed ? (
              <button
                type="button"
                onClick={() => logout?.()}
                className="ml-1 rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground/80 transition hover:border-error hover:bg-error-soft hover:text-error focus:outline-none focus:ring-2 focus:ring-primary"
              >
                Logout
              </button>
            ) : null}
          </div>

          <div className="md:hidden">
            <button
              type="button"
              onClick={() => setIsOpen(true)}
              className="inline-flex items-center justify-center rounded-md p-2 text-muted transition hover:bg-primary-soft hover:text-primary focus:outline-none focus:ring-2 focus:ring-primary"
              aria-label="Open menu"
              aria-expanded={isOpen}
            >
              <span className="sr-only">Open main menu</span>
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                <path
                  d="M4 6h16M4 12h16M4 18h16"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                />
              </svg>
            </button>
          </div>
        </nav>
      </div>

      <MobileMenu open={isOpen} onClose={closeMobileMenu}>
        <button
          type="button"
          onClick={toggleTheme}
          className="w-full rounded-xl border border-secondary px-4 py-3 text-left text-base font-medium text-secondary transition hover:bg-secondary-soft dark:hover:bg-secondary-soft/40"
        >
          Switch to {theme === "dark" ? "Light" : "Dark"} Mode
        </button>

        <NavLinks
          items={visibleItems}
          isActive={isActive}
          onNavigate={closeMobileMenu}
          variant="mobile"
        />

        {isAuthed ? (
          <button
            type="button"
            onClick={() => {
              logout?.();
              closeMobileMenu();
            }}
            className="w-full rounded-xl border border-border px-4 py-3 text-left text-base font-medium text-foreground/80 transition hover:border-error hover:bg-error-soft hover:text-error"
          >
            Logout
          </button>
        ) : null}
      </MobileMenu>
    </header>
  );
}
