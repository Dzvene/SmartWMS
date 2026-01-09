import { useEffect, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { useIntl } from 'react-intl';
import type { FullscreenModalProps } from './types';
import './FullscreenModal.scss';

/**
 * Fullscreen Modal Component
 *
 * Full-page modal for forms and editing.
 * Replaces multi-step wizards with single-view forms.
 * Responsive: full screen on mobile/tablet, centered on desktop.
 */
export function FullscreenModal({
  open,
  onClose,
  title,
  subtitle,
  children,
  footer,
  onSave,
  onCancel,
  saveLabel,
  cancelLabel,
  saveDisabled = false,
  loading = false,
  maxWidth = 'lg',
}: FullscreenModalProps) {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const handleEscape = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape' && open) {
        onClose();
      }
    },
    [open, onClose]
  );

  useEffect(() => {
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [handleEscape]);

  useEffect(() => {
    if (open) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [open]);

  if (!open) return null;

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  const modal = (
    <div className="fullscreen-modal__overlay" onClick={handleBackdropClick}>
      <div className={`fullscreen-modal fullscreen-modal--${maxWidth}`}>
        <header className="fullscreen-modal__header">
          <div className="fullscreen-modal__header-content">
            <h2 className="fullscreen-modal__title">{title}</h2>
            {subtitle && (
              <p className="fullscreen-modal__subtitle">{subtitle}</p>
            )}
          </div>
          <button
            className="fullscreen-modal__close"
            onClick={onClose}
            aria-label="Close"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
              <path
                d="M18 6L6 18M6 6l12 12"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </button>
        </header>

        <div className="fullscreen-modal__body">
          {children}
        </div>

        {(footer || onSave || onCancel) && (
          <footer className="fullscreen-modal__footer">
            {footer || (
              <>
                <button
                  className="btn btn-secondary"
                  onClick={onCancel || onClose}
                  disabled={loading}
                >
                  {cancelLabel || t('common.cancel')}
                </button>
                {onSave && (
                  <button
                    className="btn btn-primary"
                    onClick={onSave}
                    disabled={saveDisabled || loading}
                  >
                    {loading ? t('common.loading') : (saveLabel || t('common.save'))}
                  </button>
                )}
              </>
            )}
          </footer>
        )}
      </div>
    </div>
  );

  return createPortal(modal, document.body);
}

export default FullscreenModal;
