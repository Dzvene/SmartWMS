import { useState } from 'react';
import type { ModalSectionProps } from './types';

/**
 * Modal Section Component
 *
 * Collapsible section for organizing form fields within FullscreenModal.
 */
export function ModalSection({
  title,
  description,
  children,
  collapsible = false,
  defaultExpanded = true,
}: ModalSectionProps) {
  const [expanded, setExpanded] = useState(defaultExpanded);

  const toggleExpanded = () => {
    if (collapsible) {
      setExpanded((prev) => !prev);
    }
  };

  return (
    <section className="modal-section">
      <header
        className={`modal-section__header ${collapsible ? 'modal-section__header--collapsible' : ''}`}
        onClick={toggleExpanded}
      >
        <div className="modal-section__header-content">
          <h3 className="modal-section__title">{title}</h3>
          {description && (
            <p className="modal-section__description">{description}</p>
          )}
        </div>
        {collapsible && (
          <button
            className="modal-section__toggle"
            aria-expanded={expanded}
            aria-label={expanded ? 'Collapse section' : 'Expand section'}
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 20 20"
              fill="none"
              style={{ transform: expanded ? 'rotate(180deg)' : 'rotate(0deg)' }}
            >
              <path
                d="M5 7.5l5 5 5-5"
                stroke="currentColor"
                strokeWidth="1.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </button>
        )}
      </header>
      {(!collapsible || expanded) && (
        <div className="modal-section__content">
          {children}
        </div>
      )}
    </section>
  );
}

export default ModalSection;
