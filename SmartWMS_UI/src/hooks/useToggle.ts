import { useState, useCallback } from 'react';

/**
 * Simple boolean toggle state
 * Useful for modals, dropdowns, expandable sections
 *
 * @param initialValue - Starting state (default false)
 */
export function useToggle(
  initialValue = false
): [boolean, () => void, (value: boolean) => void] {
  const [value, setValue] = useState(initialValue);

  const toggle = useCallback(() => {
    setValue((prev) => !prev);
  }, []);

  const set = useCallback((newValue: boolean) => {
    setValue(newValue);
  }, []);

  return [value, toggle, set];
}
