import { useCallback } from 'react';
import { useIntl } from 'react-intl';

/**
 * Custom hook for translations with memoized translate function.
 * Solves react-hooks/exhaustive-deps warnings when using formatMessage in useMemo/useCallback.
 *
 * @returns Memoized translate function
 *
 * @example
 * const t = useTranslate();
 * const columns = useMemo(() => [
 *   { header: t('column.name', 'Name') }
 * ], [t]); // t is stable, no re-renders
 */
export function useTranslate() {
  const { formatMessage } = useIntl();

  const t = useCallback(
    (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage }),
    [formatMessage]
  );

  return t;
}
