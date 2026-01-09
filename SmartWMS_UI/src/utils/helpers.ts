import { SortDirection } from '@/models';

/**
 * Generate a unique identifier using crypto API
 */
export function generateId(): string {
  if (crypto?.randomUUID) {
    return crypto.randomUUID();
  }
  return `${Date.now()}-${Math.random().toString(36).substring(2, 11)}`;
}

/**
 * Build URL query string from parameters object
 * Excludes null, undefined, and empty values
 */
export function toQueryString(params: Record<string, unknown>): string {
  const entries = Object.entries(params).filter(
    ([, v]) => v !== null && v !== undefined && v !== ''
  );

  if (entries.length === 0) return '';

  const query = entries
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`)
    .join('&');

  return `?${query}`;
}

/**
 * Filter array items by text search across all string properties
 */
export function filterByText<T extends Record<string, unknown>>(
  items: T[],
  query: string,
  ignoreKeys: (keyof T)[] = []
): T[] {
  if (!items.length || !query.trim()) return items;

  const searchLower = query.toLowerCase().trim();

  return items.filter((item) => {
    const searchable = Object.entries(item)
      .filter(([key]) => !ignoreKeys.includes(key as keyof T))
      .map(([, val]) => (val != null ? String(val).toLowerCase() : ''))
      .join(' ');

    return searchable.includes(searchLower);
  });
}

/**
 * Sort array by object property
 */
export function sortBy<T>(
  items: T[],
  key: keyof T,
  direction: SortDirection = SortDirection.Ascending
): T[] {
  if (!items.length) return [];

  const sorted = [...items];
  const mult = direction === SortDirection.Ascending ? 1 : -1;

  sorted.sort((a, b) => {
    const va = a[key];
    const vb = b[key];

    if (va == null) return 1;
    if (vb == null) return -1;
    if (va === vb) return 0;

    return va < vb ? -1 * mult : mult;
  });

  return sorted;
}

/**
 * Remove duplicate items by key property
 */
export function uniqueBy<T>(items: T[], key: keyof T): T[] {
  const map = new Map<unknown, T>();
  items.forEach((item) => {
    const k = item[key];
    if (!map.has(k)) map.set(k, item);
  });
  return Array.from(map.values());
}

/**
 * Format large numbers with K/M/B suffixes
 */
export function formatCompact(num: number): string {
  if (num === 0) return '0';

  const abs = Math.abs(num);
  const sign = num < 0 ? '-' : '';

  if (abs >= 1e9) return `${sign}${(abs / 1e9).toFixed(1)}B`;
  if (abs >= 1e6) return `${sign}${(abs / 1e6).toFixed(1)}M`;
  if (abs >= 1e3) return `${sign}${(abs / 1e3).toFixed(1)}K`;

  return num.toString();
}

/**
 * Calculate percentage, returns 0 if denominator is 0
 */
export function percentage(numerator: number, denominator: number): number {
  if (denominator === 0) return 0;
  return Math.round((numerator / denominator) * 100);
}

/**
 * Remove null and undefined values from object
 */
export function compact<T extends Record<string, unknown>>(obj: T): Partial<T> {
  const result: Partial<T> = {};
  for (const [key, value] of Object.entries(obj)) {
    if (value != null) {
      (result as Record<string, unknown>)[key] = value;
    }
  }
  return result;
}

/**
 * Access nested property by path array
 */
export function getPath<T>(
  obj: Record<string, unknown>,
  path: string[]
): T | undefined {
  let current: unknown = obj;
  for (const key of path) {
    if (current == null || typeof current !== 'object') return undefined;
    current = (current as Record<string, unknown>)[key];
  }
  return current as T;
}

/**
 * Create portal container element if not exists
 */
export function ensurePortalRoot(id: string): HTMLElement {
  let el = document.getElementById(id);
  if (!el) {
    el = document.createElement('div');
    el.id = id;
    document.body.appendChild(el);
  }
  return el;
}

/**
 * Lowercase first character of string
 */
export function uncapitalize(str: string): string {
  return str ? str.charAt(0).toLowerCase() + str.slice(1) : '';
}

/**
 * Capitalize first character of string
 */
export function capitalize(str: string): string {
  return str ? str.charAt(0).toUpperCase() + str.slice(1) : '';
}
