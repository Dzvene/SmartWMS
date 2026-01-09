import { createColumnHelper } from '@tanstack/react-table';
import type { ColumnDef } from '@tanstack/react-table';

/**
 * Column Helper Factory
 *
 * Creates typed column definitions for DataTable.
 * Provides better DX than raw ColumnDef objects.
 */
export function createColumns<TData>() {
  return createColumnHelper<TData>();
}

/**
 * Common column presets for reuse across modules
 */
export const columnPresets = {
  /**
   * SKU column with monospace font
   */
  sku: <TData extends { sku: string }>(): ColumnDef<TData, string> => ({
    accessorKey: 'sku',
    header: 'SKU',
    size: 120,
    cell: ({ getValue }) => (
      <span className="sku">{getValue()}</span>
    ),
  }),

  /**
   * Status badge column
   */
  status: <TData extends { status: string }>(
    statusLabels?: Record<string, string>
  ): ColumnDef<TData, string> => ({
    accessorKey: 'status',
    header: 'Status',
    size: 100,
    cell: ({ getValue }) => {
      const status = getValue();
      const label = statusLabels?.[status] ?? status;
      return <span className={`status-badge status-badge--${status.toLowerCase()}`}>{label}</span>;
    },
  }),

  /**
   * Date column with formatting
   */
  date: <TData,>(
    accessorKey: keyof TData & string,
    header: string
  ): ColumnDef<TData, string> => ({
    accessorKey,
    header,
    size: 120,
    cell: ({ getValue }) => {
      const value = getValue();
      if (!value) return '—';
      try {
        return new Date(value).toLocaleDateString();
      } catch {
        return value;
      }
    },
  }),

  /**
   * Numeric column with right alignment
   */
  numeric: <TData,>(
    accessorKey: keyof TData & string,
    header: string,
    options?: { decimals?: number }
  ): ColumnDef<TData, number> => ({
    accessorKey,
    header,
    size: 80,
    meta: { align: 'right' },
    cell: ({ getValue }) => {
      const value = getValue();
      if (value === null || value === undefined) return '—';
      return options?.decimals !== undefined
        ? value.toFixed(options.decimals)
        : value.toLocaleString();
    },
  }),

  /**
   * Actions column (non-sortable)
   */
  actions: <TData,>(
    cell: ColumnDef<TData, unknown>['cell']
  ): ColumnDef<TData, unknown> => ({
    id: 'actions',
    header: '',
    size: 60,
    enableSorting: false,
    cell,
  }),
};

export type { ColumnDef };
