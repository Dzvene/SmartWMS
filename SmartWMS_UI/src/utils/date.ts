import { DateTime } from 'luxon';

/**
 * Date and time formatting utilities
 * Using Luxon for consistent timezone handling
 */

/**
 * Format date for display in tables and lists
 */
export function formatDate(
  date: string | Date | null | undefined,
  format = 'dd MMM yyyy'
): string {
  if (!date) return '-';

  const dt = typeof date === 'string' ? DateTime.fromISO(date) : DateTime.fromJSDate(date);

  return dt.isValid ? dt.toFormat(format) : '-';
}

/**
 * Format datetime with time component
 */
export function formatDateTime(
  date: string | Date | null | undefined,
  format = 'dd MMM yyyy HH:mm'
): string {
  return formatDate(date, format);
}

/**
 * Format time only
 */
export function formatTime(
  date: string | Date | null | undefined,
  format = 'HH:mm'
): string {
  return formatDate(date, format);
}

/**
 * Get relative time description (e.g., "5 minutes ago")
 */
export function formatRelative(date: string | Date | null | undefined): string {
  if (!date) return '-';

  const dt = typeof date === 'string' ? DateTime.fromISO(date) : DateTime.fromJSDate(date);

  if (!dt.isValid) return '-';

  const now = DateTime.now();
  const diff = now.diff(dt, ['days', 'hours', 'minutes', 'seconds']);

  if (diff.days >= 7) {
    return dt.toFormat('dd MMM');
  }

  if (diff.days >= 1) {
    return diff.days === 1 ? 'Yesterday' : `${Math.floor(diff.days)} days ago`;
  }

  if (diff.hours >= 1) {
    return `${Math.floor(diff.hours)} hours ago`;
  }

  if (diff.minutes >= 1) {
    return `${Math.floor(diff.minutes)} min ago`;
  }

  return 'Just now';
}

/**
 * Check if date is today
 */
export function isToday(date: string | Date): boolean {
  const dt = typeof date === 'string' ? DateTime.fromISO(date) : DateTime.fromJSDate(date);
  return dt.hasSame(DateTime.now(), 'day');
}

/**
 * Convert ISO string to local Date object
 */
export function parseIsoDate(isoString: string): Date | null {
  const dt = DateTime.fromISO(isoString);
  return dt.isValid ? dt.toJSDate() : null;
}

/**
 * Convert Date to ISO string for API requests
 */
export function toIsoString(date: Date): string {
  return DateTime.fromJSDate(date).toISO() ?? '';
}

/**
 * Generate hour options for time picker (0-23)
 */
export function getHourOptions(): Array<{ label: string; value: number }> {
  return Array.from({ length: 24 }, (_, i) => ({
    label: i.toString().padStart(2, '0'),
    value: i,
  }));
}

/**
 * Generate minute options for time picker with step
 */
export function getMinuteOptions(
  step = 5
): Array<{ label: string; value: number }> {
  const count = Math.floor(60 / step);
  return Array.from({ length: count }, (_, i) => {
    const min = i * step;
    return {
      label: min.toString().padStart(2, '0'),
      value: min,
    };
  });
}
