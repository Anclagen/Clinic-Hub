"use client";

import { useEffect, useMemo, useState } from "react";
import { ClinicsService, type Clinic } from "@/api/services/clinicsService";
import { ClinicCard } from "../components/ClinicCard";
import { ClinicsGridSkeleton } from "./ClinicsGridSkeleton";
import { ClinicsSearchSkeleton } from "./ClinicsSearchSkeleton";
import { InputField } from "@/features/UI/forms/InputField";

export function ClinicsDirectory() {
  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [clinics, setClinics] = useState<Clinic[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const timeoutId = window.setTimeout(
      () => setDebouncedSearch(searchInput.trim().toLowerCase()),
      300,
    );
    return () => window.clearTimeout(timeoutId);
  }, [searchInput]);

  useEffect(() => {
    let active = true;

    const loadClinics = async () => {
      setLoading(true);
      setError(null);

      try {
        const list = await ClinicsService.all();
        if (!active) return;
        setClinics(list.data);
      } catch (e) {
        if (!active) return;
        setError(e instanceof Error ? e.message : "Failed to load clinics.");
      } finally {
        if (active) setLoading(false);
      }
    };

    void loadClinics();

    return () => {
      active = false;
    };
  }, []);

  const visibleClinics = useMemo(() => {
    if (!debouncedSearch) return clinics;
    return clinics.filter((clinic) => clinic.clinicName.toLowerCase().includes(debouncedSearch));
  }, [clinics, debouncedSearch]);

  const showSearchSkeleton = loading && clinics.length === 0 && !error;

  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-8">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-foreground">Find Clinics</h1>
        <p className="mt-1 text-sm text-muted">
          Search clinics by name and open each clinic for details.
        </p>
      </div>

      <section className="rounded-2xl border border-border bg-card p-4 shadow-sm">
        {showSearchSkeleton ? (
          <ClinicsSearchSkeleton />
        ) : (
          <InputField
            label="Search Clinic"
            name="search"
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Search by clinic name"
            className="rounded-xl border border-border bg-background px-3 py-2 text-foreground outline-none transition focus:border-primary focus:ring-2 focus:ring-primary-soft"
          />
        )}
      </section>

      <section className="mt-6">
        <div className="mb-3 flex items-center justify-between">
          <p className="text-sm text-muted">
            {loading ? "Loading clinics..." : `${visibleClinics.length} result(s)`}
          </p>
        </div>

        {error ? (
          <div className="rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
            {error}
          </div>
        ) : null}

        {!error && !loading && visibleClinics.length === 0 ? (
          <div className="rounded-2xl border border-border bg-card p-8 text-center text-muted shadow-sm">
            No clinics found for your search.
          </div>
        ) : null}

        {!error && loading ? <ClinicsGridSkeleton count={6} /> : null}

        {!error && !loading && visibleClinics.length > 0 ? (
          <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
            {visibleClinics.map((clinic) => (
              <ClinicCard key={clinic.id} clinic={clinic} />
            ))}
          </div>
        ) : null}
      </section>
    </div>
  );
}
