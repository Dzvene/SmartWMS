import { useEffect, useCallback, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '@/store';
import { updateTokens, forceLogout } from '@/store/slices/authSlice';
import { API, STORAGE_KEYS } from '@/constants';

// Refresh token 5 minutes before expiry
const REFRESH_BUFFER_MS = 5 * 60 * 1000;

// Minimum time between refresh attempts
const MIN_REFRESH_INTERVAL_MS = 60 * 1000;

// Activity events to track
const ACTIVITY_EVENTS = ['mousedown', 'keydown', 'touchstart', 'scroll'];

/**
 * Hook that automatically refreshes the auth token when:
 * 1. Token is about to expire (5 min before)
 * 2. User is active (mouse/keyboard/touch activity)
 *
 * If user is inactive and token expires - they will be logged out
 * on next API call (handled by baseApi)
 */
export function useTokenRefresh() {
  const dispatch = useDispatch<AppDispatch>();
  const { isAuthenticated, tokenExpiresAt } = useSelector((state: RootState) => state.auth);

  const lastRefreshRef = useRef<number>(0);
  const isRefreshingRef = useRef<boolean>(false);
  const lastActivityRef = useRef<number>(Date.now());
  const refreshTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Track user activity
  const handleActivity = useCallback(() => {
    lastActivityRef.current = Date.now();
  }, []);

  // Refresh the token
  const refreshToken = useCallback(async () => {
    // Prevent concurrent refreshes
    if (isRefreshingRef.current) {
      return;
    }

    // Don't refresh too frequently
    const now = Date.now();
    if (now - lastRefreshRef.current < MIN_REFRESH_INTERVAL_MS) {
      return;
    }

    const storedRefreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
    if (!storedRefreshToken) {
      return;
    }

    isRefreshingRef.current = true;
    lastRefreshRef.current = now;

    try {
      const response = await fetch(`${API.BASE_URL}/v1/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken: storedRefreshToken }),
      });

      if (!response.ok) {
        // Refresh failed - logout
        dispatch(forceLogout());
        return;
      }

      const result = await response.json();

      if (result.success && result.data) {
        const { token, refreshToken: newRefresh, expiresAt } = result.data;
        dispatch(updateTokens({ token, refreshToken: newRefresh, expiresAt }));
      } else {
        dispatch(forceLogout());
      }
    } catch {
      // Network error - don't logout, will retry later
      console.warn('Token refresh failed, will retry');
    } finally {
      isRefreshingRef.current = false;
    }
  }, [dispatch]);

  // Schedule next refresh check
  const scheduleRefresh = useCallback(() => {
    // Clear existing timeout
    if (refreshTimeoutRef.current) {
      clearTimeout(refreshTimeoutRef.current);
      refreshTimeoutRef.current = null;
    }

    if (!isAuthenticated || !tokenExpiresAt) {
      return;
    }

    const expiresAtMs = new Date(tokenExpiresAt).getTime();
    const now = Date.now();
    const timeUntilExpiry = expiresAtMs - now;

    // If already expired, check on next activity
    if (timeUntilExpiry <= 0) {
      return;
    }

    // Calculate when to refresh (5 min before expiry)
    const refreshIn = Math.max(timeUntilExpiry - REFRESH_BUFFER_MS, 1000);

    refreshTimeoutRef.current = setTimeout(() => {
      // Only refresh if user was active recently (last 10 minutes)
      const inactiveTime = Date.now() - lastActivityRef.current;
      const isUserActive = inactiveTime < 10 * 60 * 1000;

      if (isUserActive) {
        refreshToken().then(() => {
          // Schedule next refresh after successful refresh
          scheduleRefresh();
        });
      }
      // If user inactive, don't refresh - they'll be logged out on next action
    }, refreshIn);
  }, [isAuthenticated, tokenExpiresAt, refreshToken]);

  // Set up activity listeners
  useEffect(() => {
    ACTIVITY_EVENTS.forEach((event) => {
      window.addEventListener(event, handleActivity, { passive: true });
    });

    return () => {
      ACTIVITY_EVENTS.forEach((event) => {
        window.removeEventListener(event, handleActivity);
      });
    };
  }, [handleActivity]);

  // Schedule refresh when auth state changes
  useEffect(() => {
    scheduleRefresh();

    return () => {
      if (refreshTimeoutRef.current) {
        clearTimeout(refreshTimeoutRef.current);
      }
    };
  }, [scheduleRefresh]);

  // Refresh on visibility change (user returns to tab)
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible' && isAuthenticated && tokenExpiresAt) {
        const expiresAtMs = new Date(tokenExpiresAt).getTime();
        const now = Date.now();
        const timeUntilExpiry = expiresAtMs - now;

        // If close to expiry when returning, refresh immediately
        if (timeUntilExpiry < REFRESH_BUFFER_MS && timeUntilExpiry > 0) {
          lastActivityRef.current = Date.now();
          refreshToken();
        }
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [isAuthenticated, tokenExpiresAt, refreshToken]);
}
