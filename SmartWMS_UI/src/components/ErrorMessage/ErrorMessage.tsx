import React from 'react';
import './ErrorMessage.scss';

interface ErrorMessageProps {
  label?: string;
  error?: string;
}

/**
 * Error message display component for form validation
 */
export const ErrorMessage: React.FC<ErrorMessageProps> = ({ error }) => {
  if (!error) {
    return null;
  }

  return (
    <div className="error-text" role="alert">
      {error}
    </div>
  );
};

export default ErrorMessage;
