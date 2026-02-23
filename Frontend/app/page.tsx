import { HomeBookingSection } from "@/features/home/components/HomeBookingSection";
import { HomeCtaSection } from "@/features/home/components/HomeCtaSection";
import { HomeHero } from "@/features/home/components/HomeHero";
import { HomeServicesSection } from "@/features/home/components/HomeServicesSection";
import { HomeWhyChooseSection } from "@/features/home/components/HomeWhyChooseSection";

export default function Home() {
  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 md:px-4 py-6 md:py-8">
      <HomeHero />
      <HomeBookingSection />
      <HomeServicesSection />
      <HomeWhyChooseSection />
      <HomeCtaSection />
    </div>
  );
}
