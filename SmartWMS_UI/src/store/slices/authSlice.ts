import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  companyId: string;
  tenantId: string;
  warehouseId?: string;
  roles?: string[];
}

export interface SiteInfo {
  id: string;
  name: string;
  code?: string;
  address?: string;
  tenantId?: string;
  description?: string;
  isActive?: boolean;
}

export interface LicenseInfo {
  companyName?: string;
  licenseType?: string;
  expirationDate?: string;
  maxUsers?: number;
  advanced: boolean;
  boxStacking: boolean;
  serialNumbers: boolean;
  isValid?: boolean;
  expiryDate?: string;
  features?: string[];
}

export interface Permission {
  category: string;
  permissions: string[];
}

interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  tokenExpiresAt: string | null;
  loading: boolean;
  loginLoading: boolean;
  error: string | null;
  initialized: boolean;
  defaultSiteId: number;
  currentSelectedSiteId: string;
  licenseInfo: LicenseInfo | null;
  permissions: Permission[];
  sites: SiteInfo[];
  storageNameKey: string;
  userRoles: string[];
  permissionsLoaded: boolean;
}

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5064/api';

const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  token: null,
  refreshToken: null,
  tokenExpiresAt: null,
  loading: false,
  loginLoading: false,
  error: null,
  initialized: true,
  defaultSiteId: 0,
  currentSelectedSiteId: '',
  licenseInfo: null,
  permissions: [],
  sites: [],
  storageNameKey: '',
  userRoles: [],
  permissionsLoaded: false,
};

export const loginAsync = createAsyncThunk(
  'auth/login',
  async (
    credentials: { email: string; password: string; tenantCode: string },
    { rejectWithValue }
  ) => {
    try {
      const response = await fetch(`${API_BASE_URL}/v1/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        return rejectWithValue(errorData?.message || 'Login failed');
      }

      const result = await response.json();

      if (result.success && result.data) {
        const { token, refreshToken, expiresAt, user } = result.data;

        if (token && user) {
          // tenantId comes from user object, not from root
          const userWithTenant = {
            ...user,
            companyId: user.tenantId,
            tenantId: user.tenantId,
          };

          return {
            token,
            refreshToken: refreshToken || null,
            expiresAt: expiresAt || null,
            user: userWithTenant,
          };
        }
      }

      return rejectWithValue('Invalid response from server');
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Network error';
      return rejectWithValue(message);
    }
  }
);

export const logoutAsync = createAsyncThunk('auth/logout', async () => {
  return true;
});

export const loadSitesAsync = createAsyncThunk(
  'auth/loadSites',
  async (_, { rejectWithValue, getState }) => {
    try {
      const state = getState() as { auth: AuthState };
      const token = state.auth.token;

      if (!token) {
        return rejectWithValue('No token available');
      }

      const user = state.auth.user;
      const tenantId = user?.tenantId || user?.companyId;

      if (!tenantId) {
        return rejectWithValue('No tenant ID available');
      }

      const response = await fetch(
        `${API_BASE_URL}/v1/tenant/${tenantId}/sites?page=1&limit=100&isActive=true`,
        {
          method: 'GET',
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        }
      );

      if (!response.ok) {
        return rejectWithValue('Failed to load sites');
      }

      const result = await response.json();
      return result.data?.items || result.items || result.data || [];
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Network error';
      return rejectWithValue(message);
    }
  }
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    setSelectedSiteId: (state, action: PayloadAction<string>) => {
      state.currentSelectedSiteId = action.payload;
    },
    setSites: (state, action: PayloadAction<SiteInfo[]>) => {
      state.sites = action.payload;
    },
    setDefaultSiteId: (state, action: PayloadAction<number>) => {
      state.defaultSiteId = action.payload;
    },
    setLicenseInfo: (state, action: PayloadAction<LicenseInfo | null>) => {
      state.licenseInfo = action.payload;
    },
    setPermissions: (state, action: PayloadAction<Permission[]>) => {
      state.permissions = action.payload;
    },
    setStorageNameKey: (state, action: PayloadAction<string>) => {
      state.storageNameKey = action.payload;
    },
    setUserRoles: (state, action: PayloadAction<string[]>) => {
      state.userRoles = action.payload;
    },
    setPermissionsLoaded: (state, action: PayloadAction<boolean>) => {
      state.permissionsLoaded = action.payload;
    },
    // Update tokens after refresh
    updateTokens: (
      state,
      action: PayloadAction<{ token: string; refreshToken: string; expiresAt: string }>
    ) => {
      state.token = action.payload.token;
      state.refreshToken = action.payload.refreshToken;
      state.tokenExpiresAt = action.payload.expiresAt;

      localStorage.setItem('smartwms_token', action.payload.token);
      localStorage.setItem('smartwms_refresh', action.payload.refreshToken);
      localStorage.setItem('smartwms_expires', action.payload.expiresAt);
    },
    // Force logout (called when refresh fails)
    forceLogout: (state) => {
      state.isAuthenticated = false;
      state.user = null;
      state.token = null;
      state.refreshToken = null;
      state.tokenExpiresAt = null;
      state.error = null;

      localStorage.removeItem('smartwms_token');
      localStorage.removeItem('smartwms_refresh');
      localStorage.removeItem('smartwms_expires');

      state.defaultSiteId = 0;
      state.currentSelectedSiteId = '';
      state.licenseInfo = null;
      state.permissions = [];
      state.sites = [];
      state.storageNameKey = '';
      state.userRoles = [];
      state.permissionsLoaded = false;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginAsync.pending, (state) => {
        state.loginLoading = true;
        state.error = null;
      })
      .addCase(loginAsync.fulfilled, (state, action) => {
        state.loginLoading = false;
        state.isAuthenticated = true;
        state.user = action.payload.user;
        state.token = action.payload.token;
        state.refreshToken = action.payload.refreshToken;
        state.tokenExpiresAt = action.payload.expiresAt;
        state.error = null;

        // Save tokens to localStorage
        if (action.payload.token) {
          localStorage.setItem('smartwms_token', action.payload.token);
        }
        if (action.payload.refreshToken) {
          localStorage.setItem('smartwms_refresh', action.payload.refreshToken);
        }
        if (action.payload.expiresAt) {
          localStorage.setItem('smartwms_expires', action.payload.expiresAt);
        }

        const user = action.payload.user;
        state.defaultSiteId = user.warehouseId
          ? parseInt(user.warehouseId, 10) || 0
          : 0;
        state.storageNameKey = `smartwms_${user.companyId}`;
        state.userRoles = user.roles || [];
        state.permissionsLoaded = true;
      })
      .addCase(loginAsync.rejected, (state, action) => {
        state.loginLoading = false;
        state.isAuthenticated = false;
        state.user = null;
        state.token = null;
        state.error = action.payload as string;
      });

    builder.addCase(logoutAsync.fulfilled, (state) => {
      state.isAuthenticated = false;
      state.user = null;
      state.token = null;
      state.refreshToken = null;
      state.tokenExpiresAt = null;
      state.error = null;

      localStorage.removeItem('smartwms_token');
      localStorage.removeItem('smartwms_refresh');
      localStorage.removeItem('smartwms_expires');

      state.defaultSiteId = 0;
      state.currentSelectedSiteId = '';
      state.licenseInfo = null;
      state.permissions = [];
      state.sites = [];
      state.storageNameKey = '';
      state.userRoles = [];
      state.permissionsLoaded = false;
    });

    builder
      .addCase(loadSitesAsync.pending, (state) => {
        state.loading = true;
      })
      .addCase(loadSitesAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.sites = action.payload;
        if (action.payload.length > 0 && !state.currentSelectedSiteId) {
          const defaultSite =
            action.payload.find(
              (site: SiteInfo) => site.id === state.defaultSiteId.toString()
            ) || action.payload[0];
          state.currentSelectedSiteId = defaultSite.id;
        }
      })
      .addCase(loadSitesAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const {
  clearError,
  setSelectedSiteId,
  setSites,
  setDefaultSiteId,
  setLicenseInfo,
  setPermissions,
  setStorageNameKey,
  setUserRoles,
  setPermissionsLoaded,
  updateTokens,
  forceLogout,
} = authSlice.actions;

export default authSlice.reducer;
