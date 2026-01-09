import type { DataTablePaginationProps } from './types';

const PAGE_SIZES = [10, 25, 50, 100];

/**
 * DataTable Pagination Component
 *
 * Displays pagination controls with page size selector.
 */
export function DataTablePagination({
  pageIndex,
  pageSize,
  pageCount,
  totalRows,
  onPageChange,
  onPageSizeChange,
}: DataTablePaginationProps) {
  const startRow = pageIndex * pageSize + 1;
  const endRow = Math.min((pageIndex + 1) * pageSize, totalRows);

  const canPreviousPage = pageIndex > 0;
  const canNextPage = pageIndex < pageCount - 1;

  return (
    <div className="data-table__pagination">
      <div className="data-table__pagination-info">
        <span>
          {totalRows > 0
            ? `${startRow}-${endRow} of ${totalRows}`
            : 'No results'}
        </span>
      </div>

      <div className="data-table__pagination-controls">
        <select
          className="data-table__page-size"
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
        >
          {PAGE_SIZES.map((size) => (
            <option key={size} value={size}>
              {size} / page
            </option>
          ))}
        </select>

        <div className="data-table__page-buttons">
          <button
            className="data-table__page-btn"
            onClick={() => onPageChange(0)}
            disabled={!canPreviousPage}
            aria-label="First page"
          >
            «
          </button>
          <button
            className="data-table__page-btn"
            onClick={() => onPageChange(pageIndex - 1)}
            disabled={!canPreviousPage}
            aria-label="Previous page"
          >
            ‹
          </button>
          <span className="data-table__page-indicator">
            {pageIndex + 1} / {pageCount || 1}
          </span>
          <button
            className="data-table__page-btn"
            onClick={() => onPageChange(pageIndex + 1)}
            disabled={!canNextPage}
            aria-label="Next page"
          >
            ›
          </button>
          <button
            className="data-table__page-btn"
            onClick={() => onPageChange(pageCount - 1)}
            disabled={!canNextPage}
            aria-label="Last page"
          >
            »
          </button>
        </div>
      </div>
    </div>
  );
}

export default DataTablePagination;
