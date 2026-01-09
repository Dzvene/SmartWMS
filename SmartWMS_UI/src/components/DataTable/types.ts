import type { ColumnDef, SortingState, PaginationState } from '@tanstack/react-table';

export interface DataTableProps<TData> {
  data: TData[];
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  columns: ColumnDef<TData, any>[];
  loading?: boolean;
  pagination?: PaginationState;
  onPaginationChange?: (pagination: PaginationState) => void;
  sorting?: SortingState;
  onSortingChange?: (sorting: SortingState) => void;
  totalRows?: number;
  onRowClick?: (row: TData) => void;
  selectedRowId?: string | number | null;
  getRowId?: (row: TData) => string;
  emptyMessage?: string;
}

export interface DataTablePaginationProps {
  pageIndex: number;
  pageSize: number;
  pageCount: number;
  totalRows: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

export type { ColumnDef, SortingState, PaginationState };
