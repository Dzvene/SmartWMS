import classNames from 'classnames';
import React, { useCallback } from 'react';

import './ActionButton.scss';

type ButtonVariant =
  | 'primary'
  | 'secondary'
  | 'success'
  | 'warning'
  | 'error'
  | 'ghost'
  | 'outline'
  | 'cancel'
  | 'subtle';
type ButtonSize = 'sm' | 'md' | 'lg';

export interface ActionButtonProps {
  label?: string;
  variant?: ButtonVariant;
  size?: ButtonSize;
  type?: 'submit' | 'button';
  icon?: React.ReactNode;
  form?: string;
  title?: string;
  className?: string;
  helpText?: string;
  disabled?: boolean;
  fullwidth?: boolean;
  iconButton?: boolean;
  loading?: boolean;
  onClick?: (event: React.MouseEvent<HTMLButtonElement>) => void;
}

/**
 * Action button component with multiple visual variants
 */
export const ActionButton: React.FC<ActionButtonProps> = ({
  label,
  variant = 'primary',
  size = 'md',
  type = 'button',
  icon,
  form,
  title,
  className,
  helpText,
  disabled = false,
  fullwidth = false,
  iconButton = false,
  loading = false,
  onClick,
}) => {
  const handleClick = useCallback(
    (e: React.MouseEvent<HTMLButtonElement>) => {
      if (!disabled && !loading) {
        onClick?.(e);
      }
    },
    [disabled, loading, onClick]
  );

  const containerClasses = classNames('action-btn', className, {
    'action-btn--has-help': !!helpText,
    'action-btn--has-title': !!title,
    'action-btn--full-width': fullwidth,
  });

  const buttonClasses = classNames(
    'action-btn__control',
    `action-btn__control--${variant}`,
    `action-btn__control--${size}`,
    {
      'action-btn__control--full-width': fullwidth,
      'action-btn__control--icon-only': iconButton,
      'action-btn__control--loading': loading,
      'action-btn__control--disabled': disabled,
    }
  );

  return (
    <div className={containerClasses}>
      {title && <div className="action-btn__title">{title}</div>}

      <button
        form={form}
        className={buttonClasses}
        type={type}
        onClick={handleClick}
        disabled={disabled || loading}
        aria-busy={loading}
        aria-disabled={disabled}
      >
        {loading && (
          <span className="action-btn__spinner" aria-hidden="true">
            <span className="action-btn__spinner-ring" />
          </span>
        )}

        {icon && !loading && (
          <span className="action-btn__icon" aria-hidden="true">
            {icon}
          </span>
        )}

        {label && <span className="action-btn__label">{label}</span>}
      </button>

      {helpText && <div className="action-btn__help">{helpText}</div>}
    </div>
  );
};

export default ActionButton;
