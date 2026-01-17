/**
 * SmartWMS Custom Hooks
 * Reusable stateful logic for components
 */

export { useDebounce } from './useDebounce';
export { useLocalStorage } from './useLocalStorage';
export { useToggle } from './useToggle';
export { usePagination } from './usePagination';
export type { PaginationState, PaginationActions, PaginationResult } from './usePagination';
export { useClickOutside } from './useClickOutside';
export { useTranslate } from './useTranslate';

// Re-export store hooks
export { useAppDispatch, useAppSelector } from '@/store';
