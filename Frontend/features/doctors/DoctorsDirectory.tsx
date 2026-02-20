"use client";

import { useEffect, useState } from "react";
import { ClinicsService, type Clinic } from "@/api/services/clinicsService";
import { SpecialitiesService, type Speciality } from "@/api/services/specialitiesService";
import { DoctorsGrid, type DoctorsQuery } from "./components/DoctorsGrid";
import { DoctorsFiltersSkeleton } from "./components/DoctorsFiltersSkeleton";
import { InputField } from "../UI/forms/InputField";
import { SelectField } from "../UI/forms/SelectField";

export function DoctorsDirectory() {
  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  const [selectedClinicId, setSelectedClinicId] = useState<string>("");
  const [selectedSpecialityId, setSelectedSpecialityId] = useState<string>("");

  const [clinics, setClinics] = useState<Clinic[]>([]);
  const [specialities, setSpecialities] = useState<Speciality[]>([]);
  const [loadingFilters, setLoadingFilters] = useState(true);
  const [filtersError, setFiltersError] = useState<string | null>(null);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => setDebouncedSearch(searchInput.trim()), 350);
    return () => window.clearTimeout(timeoutId);
  }, [searchInput]);

  useEffect(() => {
    let active = true;

    const load = async () => {
      setLoadingFilters(true);
      setFiltersError(null);
      try {
        const [clinicList, specialityList] = await Promise.all([
          ClinicsService.all(),
          SpecialitiesService.all(),
        ]);
        if (!active) return;
        setClinics(clinicList);
        setSpecialities(specialityList);
      } catch (e) {
        if (!active) return;
        setFiltersError(e instanceof Error ? e.message : "Failed to load filters.");
      } finally {
        if (active) setLoadingFilters(false);
      }
    };

    void load();
    return () => {
      active = false;
    };
  }, []);

  const query: DoctorsQuery = {
    q: debouncedSearch || undefined,
    clinicId: selectedClinicId ? Number(selectedClinicId) : undefined,
    specialityId: selectedSpecialityId ? Number(selectedSpecialityId) : undefined,
  };

  const showFiltersSkeleton =
    loadingFilters && clinics.length === 0 && specialities.length === 0 && !filtersError;

  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-8">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-foreground">Find Doctors</h1>
        <p className="mt-1 text-sm text-muted">
          Search by name and filter by clinic or speciality.
        </p>
      </div>

      <section className="rounded-2xl border border-border bg-card/80 dark:bg-card/75 p-4 shadow-sm">
        {showFiltersSkeleton ? (
          <DoctorsFiltersSkeleton />
        ) : (
          <div className="grid gap-3 md:grid-cols-3">
            <InputField
              label="Search Doctor"
              name="search"
              type="search"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              placeholder="Search by first or last name"
              className=""
            />
            <SelectField
              label="Clinic"
              name="clinicId"
              value={selectedClinicId}
              onChange={(value) => setSelectedClinicId(value)}
              placeholder="Select a clinic"
              disabled={loadingFilters}
              options={clinics.map((c) => ({ value: String(c.id), label: c.clinicName }))}
            />
            <SelectField
              label="Speciality"
              name="specialityId"
              value={selectedSpecialityId}
              onChange={(value) => setSelectedSpecialityId(value)}
              placeholder="Select a speciality"
              disabled={loadingFilters}
              options={specialities.map((s) => ({ value: String(s.id), label: s.specialityName }))}
            />
            <div className="flex items-end">
              <button
                type="button"
                onClick={() => {
                  setSearchInput("");
                  setSelectedClinicId("");
                  setSelectedSpecialityId("");
                }}
                className="w-full rounded-xl border border-secondary px-4 py-2 text-sm font-medium text-secondary transition hover:bg-secondary-soft"
              >
                Clear filters
              </button>
            </div>
          </div>
        )}

        {filtersError ? <p className="mt-3 text-sm text-error">{filtersError}</p> : null}
      </section>

      <DoctorsGrid query={query} />
    </div>
  );
}
