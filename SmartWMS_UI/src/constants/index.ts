/**
 * SmartWMS Constants
 * Application-wide configuration values
 */

export * from './routes';

/**
 * API configuration
 */
export const API = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5064/api',
  TIMEOUT: 30000,
} as const;

/**
 * Pagination defaults
 */
export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 25,
  PAGE_SIZE_OPTIONS: [10, 25, 50, 100],
} as const;

/**
 * Date/time formats
 */
export const DATE_FORMATS = {
  DATE: 'dd MMM yyyy',
  DATETIME: 'dd MMM yyyy HH:mm',
  TIME: 'HH:mm',
  API: "yyyy-MM-dd'T'HH:mm:ss",
} as const;

/**
 * Local storage keys
 */
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'smartwms_token',
  REFRESH_TOKEN: 'smartwms_refresh',
  USER_PREFERENCES: 'smartwms_prefs',
  LOCALE: 'smartwms_locale',
} as const;

/**
 * Supported locales
 */
export const LOCALES = {
  EN: 'en',
  SV: 'sv',
} as const;

/**
 * HTTP status codes
 */
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  INTERNAL_ERROR: 500,
} as const;
