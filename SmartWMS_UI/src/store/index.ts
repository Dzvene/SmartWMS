import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { persistStore, persistReducer } from 'redux-persist';
import storage from 'redux-persist/lib/storage';

import { baseApi } from '@/api';
import authReducer from './slices/authSlice';
import settingsReducer from './slices/settingsSlice';

// Persist config for auth slice
const authPersistConfig = {
  key: 'auth',
  storage,
  whitelist: ['isAuthenticated', 'user', 'token', 'userRoles', 'sites', 'currentSelectedSiteId'],
};

const persistedAuthReducer = persistReducer(authPersistConfig, authReducer);

/**
 * Redux Store Configuration
 *
 * Uses RTK Query for data fetching with:
 * - Automatic caching and deduplication
 * - Background refetching
 * - Optimistic updates support
 * - Redux Persist for auth state persistence
 */
export const store = configureStore({
  reducer: {
    auth: persistedAuthReducer,
    settings: settingsReducer,
    [baseApi.reducerPath]: baseApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST', 'persist/REHYDRATE'],
      },
    }).concat(baseApi.middleware),
});

export const persistor = persistStore(store);

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export { useAppDispatch, useAppSelector } from './hooks';
