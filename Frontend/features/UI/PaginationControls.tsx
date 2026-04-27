import type { Pagination } from "@/api/apiTypes";

type PaginationControlsProps = {
  pagination: Pagination;
  disabled?: boolean;
  onPageChange: (page: number) => void;
};

export function PaginationControls({
  pagination,
  disabled = false,
  onPageChange,
}: PaginationControlsProps) {
  if (pagination.totalPages <= 1) return null;

  const canGoPrev = pagination.page > 1 && !disabled;
  const canGoNext = pagination.page < pagination.totalPages && !disabled;

  const totalPages = Math.max(pagination.totalPages, 1);
  const start = Math.max(1, pagination.page - 2);
  const end = Math.min(totalPages, start + 4);

  const pageNumbers: number[] = [];
  for (let p = start; p <= end; p += 1) pageNumbers.push(p);

  return (
    <div className="mt-6 flex flex-wrap items-center justify-center gap-2">
      <button
        type="button"
        onClick={() => canGoPrev && onPageChange(pagination.page - 1)}
        disabled={!canGoPrev}
        className="rounded-lg border border-border px-3 py-1.5 text-sm text-foreground transition hover:bg-primary-soft disabled:opacity-50"
      >
        Previous
      </button>

      {pageNumbers.map((page) => (
        <button
          key={page}
          type="button"
          onClick={() => onPageChange(page)}
          disabled={disabled}
          className={[
            "rounded-lg px-3 py-1.5 text-sm transition",
            page === pagination.page
              ? "bg-primary text-white"
              : "border border-border text-foreground hover:bg-primary-soft",
            disabled ? "cursor-not-allowed opacity-50 hover:bg-transparent" : "",
          ].join(" ")}
        >
          {page}
        </button>
      ))}

      <button
        type="button"
        onClick={() => canGoNext && onPageChange(pagination.page + 1)}
        disabled={!canGoNext}
        className="rounded-lg border border-border px-3 py-1.5 text-sm text-foreground transition hover:bg-primary-soft disabled:opacity-50"
      >
        Next
      </button>
    </div>
  );
}
