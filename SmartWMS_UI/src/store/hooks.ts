import { TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';

import type { RootState, AppDispatch } from './index';

/**
 * Typed dispatch hook for async thunks
 */
export const useAppDispatch = () => useDispatch<AppDispatch>();

/**
 * Typed selector hook with state inference
 */
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
