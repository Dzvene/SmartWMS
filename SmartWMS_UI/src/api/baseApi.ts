import {
  createApi,
  fetchBaseQuery,
  BaseQueryFn,
  FetchArgs,
  FetchBaseQueryError,
} from '@reduxjs/toolkit/query/react';

import { API, STORAGE_KEYS } from '@/constants';
import type { RootState } from '@/store';
import { forceLogout, updateTokens } from '@/store/slices/authSlice';

/**
 * Custom base query with authentication
 */
const baseQuery = fetchBaseQuery({
  baseUrl: API.BASE_URL,
  timeout: API.TIMEOUT,
  prepareHeaders: (headers) => {
    const token = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
    headers.set('Content-Type', 'application/json');
    return headers;
  },
});

/**
 * Base query with automatic tenant ID injection
 *
 * - Auth endpoints (/auth/*) - no tenant prefix
 * - All other endpoints - automatically prefixed with /v1/tenant/{tenantId}
 *
 * This means endpoints only need to specify the path after tenant:
 * - `/products` becomes `/v1/tenant/{tenantId}/products`
 * - `/users` becomes `/v1/tenant/{tenantId}/users`
 */
const baseQueryWithTenant: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  const state = api.getState() as RootState;
  const tenantId = state.auth.user?.tenantId;

  // Get URL from args
  const url = typeof args === 'string' ? args : args.url;

  // Auth endpoints don't need tenant prefix
  const isAuthEndpoint = url.startsWith('/auth');

  let adjustedArgs = args;

  if (!isAuthEndpoint && tenantId) {
    if (typeof args === 'string') {
      adjustedArgs = `/v1/tenant/${tenantId}${args}`;
    } else {
      adjustedArgs = {
        ...args,
        url: `/v1/tenant/${tenantId}${args.url}`,
      };
    }
  }

  // Execute the query
  let result = await baseQuery(adjustedArgs, api, extraOptions);

  // Handle 401 - try to refresh token
  if (result.error?.status === 401) {
    const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

    if (refreshToken) {
      const refreshResult = await baseQuery(
        {
          url: '/v1/auth/refresh',
          method: 'POST',
          body: { refreshToken },
        },
        api,
        extraOptions
      );

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const refreshData = refreshResult.data as any;

      if (refreshData?.success && refreshData?.data) {
        const { token, refreshToken: newRefresh, expiresAt } = refreshData.data;

        // Update tokens in Redux store
        api.dispatch(
          updateTokens({
            token,
            refreshToken: newRefresh,
            expiresAt,
          })
        );

        // Retry the original request
        result = await baseQuery(adjustedArgs, api, extraOptions);
      } else {
        // Refresh failed - force logout
        api.dispatch(forceLogout());
      }
    } else {
      // No refresh token - force logout
      api.dispatch(forceLogout());
    }
  }

  return result;
};

/**
 * Base API slice - extend this for domain-specific endpoints
 *
 * All endpoints automatically get tenant prefix injected.
 * Just specify the path: `/products`, `/users`, etc.
 */
export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: baseQueryWithTenant,
  tagTypes: [
    // Core
    'Product',
    'ProductCategory',
    'Location',
    'Inventory',
    'User',
    'Site',
    'Warehouse',
    'Zone',

    // Orders
    'SalesOrders',
    'PurchaseOrders',
    'Customers',
    'Suppliers',

    // Fulfillment
    'PickTasks',
    'FulfillmentBatches',
    'Shipments',

    // Receiving
    'GoodsReceipts',

    // Putaway
    'PutawayTasks',

    // Packing
    'PackingTasks',
    'PackingStations',

    // Returns
    'Returns',

    // Equipment & Carriers
    'Equipment',
    'Carriers',

    // Config
    'Integration',
    'Roles',
    'Notifications',
    'NotificationCount',
    'NotificationPreferences',
    'NotificationTemplates',

    // Dashboard & Reports
    'Dashboard',
    'Reports',

    // Audit & Activity
    'AuditLogs',
    'ActivityLogs',
    'SystemEvents',
    'AuditStatistics',

    // Operations & Sessions
    'Sessions',
    'LoginAttempts',
    'TrustedDevices',
    'Tasks',

    // Adjustments & Inventory Operations
    'Adjustments',
    'CycleCounts',
    'Transfers',

    // Configuration
    'BarcodePrefixes',
    'ReasonCodes',
    'NumberSequences',
    'SystemSettings',

    // Integrations
    'Integrations',
    'SyncJobs',
    'SyncLogs',
    'Webhooks',

    // Automation
    'AutomationRules',
    'AutomationExecutions',
    'AutomationStats',
  ],
  endpoints: () => ({}),
});
