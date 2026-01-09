import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import IconButton from '@mui/material/IconButton';
import classNames from 'classnames';
import React, { useState, useCallback } from 'react';
import { UseFormRegisterReturn } from 'react-hook-form';

import { ErrorMessage } from '../ErrorMessage';
import './TextInput.scss';

interface TextInputProps {
  label?: string;
  name?: string;
  value?: string | number;
  defaultValue?: string | number;
  type?: string;
  placeholder?: string;
  errorText?: string;
  helpText?: string;
  step?: number;
  min?: number;
  max?: number;
  inputRef?: React.LegacyRef<HTMLDivElement>;
  disabled?: boolean;
  required?: boolean;
  hidden?: boolean;
  register?: UseFormRegisterReturn;
  className?: string;
  autoComplete?: string;
  onClick?: (event: React.MouseEvent<HTMLInputElement>) => void;
  onChange?: (value: string, name?: string) => void;
  onBlur?: (event: React.FocusEvent) => void;
  children?: React.ReactNode;
  readOnly?: boolean;
}

/**
 * Form input component with label, validation, and optional visibility toggle
 */
export const TextInput: React.FC<TextInputProps> = ({
  label,
  name,
  value,
  defaultValue,
  type = 'text',
  step,
  min,
  max,
  inputRef = null,
  placeholder,
  helpText,
  errorText,
  disabled = false,
  required = false,
  hidden = false,
  children,
  register,
  className,
  autoComplete = 'off',
  onClick,
  onChange,
  onBlur,
  readOnly = false,
}) => {
  const [isRevealed, setIsRevealed] = useState<boolean>(!hidden);

  const handleValueChange = useCallback(
    (evt: React.ChangeEvent<HTMLInputElement>) => {
      if (onChange && isRevealed) {
        onChange(evt.target.value, name);
      }
    },
    [onChange, isRevealed, name]
  );

  const toggleVisibility = useCallback(() => {
    setIsRevealed((prev) => !prev);
  }, []);

  const containerClasses = classNames('form-input', className, {
    'form-input--disabled': disabled,
  });

  const wrapperClasses = classNames('form-input__wrapper', {
    'form-input__wrapper--borderless': !isRevealed,
  });

  return (
    <div className={containerClasses} ref={inputRef}>
      {label && (
        <label className="form-input__label">
          {label}
          {required && <span className="form-input__required">*</span>}
        </label>
      )}

      <div className={wrapperClasses}>
        <input
          className="form-input__control"
          name={name}
          value={value}
          defaultValue={defaultValue}
          type={hidden && !isRevealed ? 'password' : type}
          step={step}
          min={min}
          max={max}
          disabled={disabled || !isRevealed}
          readOnly={readOnly}
          placeholder={placeholder}
          autoComplete={autoComplete}
          onChange={handleValueChange}
          onClick={onClick}
          onBlur={onBlur}
          {...register}
        />

        {children}

        {hidden && (
          <IconButton
            className="form-input__visibility-toggle"
            aria-label="toggle content visibility"
            onClick={toggleVisibility}
            edge="end"
          >
            {isRevealed ? <VisibilityOff /> : <Visibility />}
          </IconButton>
        )}
      </div>

      {helpText && <div className="form-input__help">{helpText}</div>}
      {errorText && <ErrorMessage error={errorText} label={label} />}
    </div>
  );
};

export default TextInput;
