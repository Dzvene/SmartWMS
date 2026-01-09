import { useIntl } from 'react-intl';
import './Header.scss';

export interface HeaderProps {
  onMenuToggle?: () => void;
}

/**
 * Application Header Component
 *
 * Displays top bar with user actions and global controls.
 */
export function Header({ onMenuToggle }: HeaderProps) {
  const { formatMessage } = useIntl();

  return (
    <header className="header">
      <div className="header__left">
        <button
          className="header__menu-btn"
          onClick={onMenuToggle}
          aria-label="Toggle menu"
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M3 5h14M3 10h14M3 15h14"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
            />
          </svg>
        </button>
      </div>

      <div className="header__center">
        <div className="header__search">
          <svg
            className="header__search-icon"
            width="18"
            height="18"
            viewBox="0 0 18 18"
            fill="none"
          >
            <path
              d="M8 14A6 6 0 108 2a6 6 0 000 12zM16 16l-3.5-3.5"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
          <input
            type="search"
            className="header__search-input"
            placeholder={formatMessage({ id: 'common.search' })}
          />
        </div>
      </div>

      <div className="header__right">
        <button className="header__action" aria-label="Notifications">
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
          >
            <path
              d="M15 6.667a5 5 0 00-10 0c0 5.833-2.5 7.5-2.5 7.5h15S15 12.5 15 6.667M11.441 17.5a1.667 1.667 0 01-2.883 0"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </button>

        <div className="header__user">
          <div className="header__avatar">
            <span>U</span>
          </div>
        </div>
      </div>
    </header>
  );
}

export default Header;
