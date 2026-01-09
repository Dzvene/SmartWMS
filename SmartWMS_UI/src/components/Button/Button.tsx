import { forwardRef } from 'react';
import MuiButton, { ButtonProps as MuiButtonProps } from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';

export interface ButtonProps extends Omit<MuiButtonProps, 'size'> {
  loading?: boolean;
  size?: 'small' | 'medium' | 'large';
}

/**
 * Primary button component with loading state support
 */
export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  function Button(props, ref) {
    const {
      children,
      loading = false,
      disabled,
      startIcon,
      size = 'medium',
      ...rest
    } = props;

    return (
      <MuiButton
        ref={ref}
        disabled={disabled || loading}
        size={size}
        startIcon={
          loading ? <CircularProgress size={16} color="inherit" /> : startIcon
        }
        {...rest}
      >
        {children}
      </MuiButton>
    );
  }
);
