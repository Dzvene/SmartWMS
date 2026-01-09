import { forwardRef, ReactNode } from 'react';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MuiSelect, { SelectProps as MuiSelectProps } from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import FormHelperText from '@mui/material/FormHelperText';

export interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

export interface SelectProps extends Omit<MuiSelectProps, 'size'> {
  options: SelectOption[];
  helperText?: ReactNode;
  size?: 'small' | 'medium';
}

/**
 * Dropdown select with options array
 */
export const Select = forwardRef<HTMLDivElement, SelectProps>(
  function Select(props, ref) {
    const {
      options,
      label,
      error,
      helperText,
      fullWidth = true,
      size = 'small',
      ...rest
    } = props;

    return (
      <FormControl
        ref={ref}
        fullWidth={fullWidth}
        error={error}
        size={size}
      >
        {label && <InputLabel>{label}</InputLabel>}
        <MuiSelect label={label} {...rest}>
          {options.map((opt) => (
            <MenuItem
              key={opt.value}
              value={opt.value}
              disabled={opt.disabled}
            >
              {opt.label}
            </MenuItem>
          ))}
        </MuiSelect>
        {helperText && <FormHelperText>{helperText}</FormHelperText>}
      </FormControl>
    );
  }
);
