import { useState, useMemo, useCallback } from 'react';

import { PAGINATION } from '@/constants';

export interface PaginationState {
  page: number;
  pageSize: number;
}

export interface PaginationActions {
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  nextPage: () => void;
  prevPage: () => void;
  reset: () => void;
}

export interface PaginationResult extends PaginationState, PaginationActions {
  offset: number;
  hasNext: boolean;
  hasPrev: boolean;
  totalPages: number;
}

/**
 * Manage pagination state for tables and lists
 *
 * @param totalCount - Total number of items
 * @param initialPage - Starting page (default 1)
 * @param initialPageSize - Items per page (default from constants)
 */
export function usePagination(
  totalCount: number,
  initialPage = 1,
  initialPageSize: number = PAGINATION.DEFAULT_PAGE_SIZE
): PaginationResult {
  const [page, setPageState] = useState(initialPage);
  const [pageSize, setPageSizeState] = useState<number>(initialPageSize);

  const totalPages = useMemo(
    () => Math.max(1, Math.ceil(totalCount / pageSize)),
    [totalCount, pageSize]
  );

  const setPage = useCallback(
    (newPage: number) => {
      const clamped = Math.min(Math.max(1, newPage), totalPages);
      setPageState(clamped);
    },
    [totalPages]
  );

  const setPageSize = useCallback((size: number) => {
    setPageSizeState(size);
    setPageState(1);
  }, []);

  const nextPage = useCallback(() => {
    setPage(page + 1);
  }, [page, setPage]);

  const prevPage = useCallback(() => {
    setPage(page - 1);
  }, [page, setPage]);

  const reset = useCallback(() => {
    setPageState(initialPage);
    setPageSizeState(initialPageSize);
  }, [initialPage, initialPageSize]);

  return {
    page,
    pageSize,
    offset: (page - 1) * pageSize,
    totalPages,
    hasNext: page < totalPages,
    hasPrev: page > 1,
    setPage,
    setPageSize,
    nextPage,
    prevPage,
    reset,
  };
}
