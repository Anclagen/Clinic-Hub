"use client";

import { useEffect, useState } from "react";
import { DoctorsService, type Doctor, type Pagination } from "@/api/services/doctorsService";
import { DoctorCard } from "./DoctorCard";
import { DoctorsGridSkeleton } from "./DoctorsGridSkeleton";
import { PaginationControls } from "../../UI/PaginationControls";

export type DoctorsQuery = {
  q?: string;
  clinicId?: number;
  specialityId?: number;
};

type DoctorsGridProps = {
  query: DoctorsQuery;
  pageSize?: number;
  title?: string;
};

export function DoctorsGrid({ query, pageSize = 10, title }: DoctorsGridProps) {
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [pagination, setPagination] = useState<Pagination>({
    page: 1,
    pageSize,
    total: 0,
    totalPages: 0,
  });

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // reset page when query changes
  useEffect(() => {
    setPagination((p) => ({ ...p, page: 1 }));
  }, [query.q, query.clinicId, query.specialityId]);

  useEffect(() => {
    let active = true;

    const load = async () => {
      setLoading(true);
      setError(null);

      try {
        const res = await DoctorsService.all({
          ...query,
          page: pagination.page,
          pageSize,
        });

        if (!active) return;
        setDoctors(res.data);
        setPagination(res.pagination);
      } catch (e) {
        if (!active) return;
        setError(e instanceof Error ? e.message : "Failed to load doctors.");
      } finally {
        if (active) setLoading(false);
      }
    };

    void load();
    return () => {
      active = false;
    };
  }, [query.q, query.clinicId, query.specialityId, pagination.page, pageSize]);

  return (
    <section className="mt-6">
      {title ? <h2 className="mb-3 text-xl font-semibold text-foreground">{title}</h2> : null}

      <div className="mb-3 flex items-center justify-between">
        <p className="text-sm text-muted">
          {loading ? "Loading doctors..." : `${pagination.total} result(s)`}
        </p>
        <p className="text-xs text-muted">{pageSize} per page</p>
      </div>

      {error ? (
        <div className="rounded-xl border border-error bg-error-soft px-4 py-3 text-sm text-error">
          {error}
        </div>
      ) : null}

      {!error && !loading && doctors.length === 0 ? (
        <div className="rounded-2xl border border-border bg-card p-8 text-center text-muted shadow-sm">
          No doctors found for your current filters.
        </div>
      ) : null}

      {!error && loading ? <DoctorsGridSkeleton count={Math.min(pageSize, 10)} /> : null}

      {!error && !loading && doctors.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {doctors.map((doctor) => (
            <DoctorCard key={doctor.id} doctor={doctor} />
          ))}
        </div>
      ) : null}

      <PaginationControls
        pagination={pagination}
        disabled={loading}
        onPageChange={(page) => setPagination((p) => ({ ...p, page }))}
      />
    </section>
  );
}
