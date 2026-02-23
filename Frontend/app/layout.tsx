import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import Header from "@/features/layout/Header";
import Footer from "@/features/layout/Footer";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "ClinicHub",
  description: "Clinic booking and doctor directory",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={[
          inter.className,
          "min-h-screen flex flex-col dark:bg-[url('/images/ui/pastel_stethoscope_background_dark.jpg')] bg-[url('/images/ui/vitals_card_background.jpg')] bg-cover bg-right lg:bg-center text-foreground",
        ].join(" ")}
      >
        <Header />
        <main className="flex-1">{children}</main>
        <Footer />
      </body>
    </html>
  );
}
