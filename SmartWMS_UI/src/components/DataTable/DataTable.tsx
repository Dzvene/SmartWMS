import { useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getPaginationRowModel,
  flexRender,
} from '@tanstack/react-table';
import type { DataTableProps } from './types';
import { DataTablePagination } from './DataTablePagination';
import './DataTable.scss';

/**
 * DataTable Component
 *
 * Generic table built on TanStack Table v8.
 * Supports sorting, pagination, row selection, and custom rendering.
 */
export function DataTable<TData>({
  data,
  columns,
  loading = false,
  pagination,
  onPaginationChange,
  sorting,
  onSortingChange,
  totalRows,
  onRowClick,
  selectedRowId,
  getRowId,
  emptyMessage = 'No data available',
}: DataTableProps<TData>) {
  const tableData = useMemo(() => data, [data]);

  const table = useReactTable({
    data: tableData,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    manualPagination: !!onPaginationChange,
    manualSorting: !!onSortingChange,
    pageCount: totalRows && pagination
      ? Math.ceil(totalRows / pagination.pageSize)
      : undefined,
    state: {
      sorting: sorting ?? [],
      pagination: pagination ?? { pageIndex: 0, pageSize: 25 },
    },
    onSortingChange: onSortingChange
      ? (updater) => {
          const newSorting = typeof updater === 'function'
            ? updater(sorting ?? [])
            : updater;
          onSortingChange(newSorting);
        }
      : undefined,
    onPaginationChange: onPaginationChange
      ? (updater) => {
          const newPagination = typeof updater === 'function'
            ? updater(pagination ?? { pageIndex: 0, pageSize: 25 })
            : updater;
          onPaginationChange(newPagination);
        }
      : undefined,
    getRowId: getRowId as (row: TData) => string,
  });

  const isEmpty = !loading && data.length === 0;

  return (
    <div className="data-table">
      <div className="data-table__container">
        <table className="data-table__table">
          <thead className="data-table__thead">
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id} className="data-table__header-row">
                {headerGroup.headers.map((header) => {
                  const canSort = header.column.getCanSort();
                  const sorted = header.column.getIsSorted();

                  return (
                    <th
                      key={header.id}
                      className={`data-table__th ${canSort ? 'data-table__th--sortable' : ''} ${sorted ? `data-table__th--sorted-${sorted}` : ''}`}
                      style={{ width: header.getSize() !== 150 ? header.getSize() : undefined }}
                      onClick={canSort ? header.column.getToggleSortingHandler() : undefined}
                    >
                      <div className="data-table__th-content">
                        {header.isPlaceholder
                          ? null
                          : flexRender(header.column.columnDef.header, header.getContext())}
                        {canSort && (
                          <span className="data-table__sort-icon">
                            {sorted === 'asc' && '↑'}
                            {sorted === 'desc' && '↓'}
                            {!sorted && '↕'}
                          </span>
                        )}
                      </div>
                    </th>
                  );
                })}
              </tr>
            ))}
          </thead>
          <tbody className="data-table__tbody">
            {loading ? (
              <tr className="data-table__loading-row">
                <td colSpan={columns.length}>
                  <div className="data-table__loading">Loading...</div>
                </td>
              </tr>
            ) : isEmpty ? (
              <tr className="data-table__empty-row">
                <td colSpan={columns.length}>
                  <div className="data-table__empty">{emptyMessage}</div>
                </td>
              </tr>
            ) : (
              table.getRowModel().rows.map((row) => {
                const isSelected = selectedRowId !== undefined && selectedRowId !== null
                  && row.id === String(selectedRowId);

                return (
                  <tr
                    key={row.id}
                    className={`data-table__row ${isSelected ? 'data-table__row--selected' : ''} ${onRowClick ? 'data-table__row--clickable' : ''}`}
                    onClick={() => onRowClick?.(row.original)}
                  >
                    {row.getVisibleCells().map((cell) => (
                      <td key={cell.id} className="data-table__td">
                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                      </td>
                    ))}
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {pagination && onPaginationChange && (
        <DataTablePagination
          pageIndex={pagination.pageIndex}
          pageSize={pagination.pageSize}
          pageCount={table.getPageCount()}
          totalRows={totalRows ?? data.length}
          onPageChange={(page) => onPaginationChange({ ...pagination, pageIndex: page })}
          onPageSizeChange={(size) => onPaginationChange({ pageIndex: 0, pageSize: size })}
        />
      )}
    </div>
  );
}

export default DataTable;
