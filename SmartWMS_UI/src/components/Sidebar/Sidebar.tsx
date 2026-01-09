import { useState, useCallback, useRef, useEffect } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import { useIntl } from 'react-intl';
import { useAppDispatch, useAppSelector } from '@/store';
import { logoutAsync, setSelectedSiteId, loadSitesAsync } from '@/store/slices/authSlice';
import { setLocale } from '@/store/slices/settingsSlice';
import { locales, type SupportedLocale } from '@/localization';
import { navigationGroups } from './navigation';
import type { SidebarProps, NavGroup } from './types';
import Logo from '@/assets/icons/svg/logo.svg';
import './Sidebar.scss';

// Icons as SVG components for cleaner code
const icons = {
  collapse: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M13 4l-6 6 6 6" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  expand: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M7 4l6 6-6 6" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  chevronDown: (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
      <path d="M4 6l4 4 4-4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  logout: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M7.5 17.5H4.167A1.667 1.667 0 012.5 15.833V4.167A1.667 1.667 0 014.167 2.5H7.5M13.333 14.167L17.5 10l-4.167-4.167M17.5 10H7.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  language: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <circle cx="10" cy="10" r="7.5" stroke="currentColor" strokeWidth="1.5"/>
      <path d="M2.5 10h15M10 2.5c-2 2.5-2 12.5 0 15M10 2.5c2 2.5 2 12.5 0 15" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round"/>
    </svg>
  ),
  site: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M17.5 8.333c0 5.834-7.5 10-7.5 10s-7.5-4.166-7.5-10a7.5 7.5 0 1115 0z" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
      <circle cx="10" cy="8.333" r="2.5" stroke="currentColor" strokeWidth="1.5"/>
    </svg>
  ),
  dashboard: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M8.333 2.5H2.5v5.833h5.833V2.5zM17.5 2.5h-5.833v5.833H17.5V2.5zM17.5 11.667h-5.833V17.5H17.5v-5.833zM8.333 11.667H2.5V17.5h5.833v-5.833z" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  inbound: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M17.5 10H7.5M12.5 5l5 5-5 5M2.5 2.5v15" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  outbound: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M2.5 10h10M7.5 5l-5 5 5 5M17.5 2.5v15" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  inventory: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M2.5 5.833L10 2.5l7.5 3.333M2.5 5.833v8.334L10 17.5M2.5 5.833L10 9.167M10 17.5l7.5-3.333V5.833M10 17.5V9.167M17.5 5.833L10 9.167" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  warehouse: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M2.5 17.5V7.5L10 2.5l7.5 5v10M7.5 17.5v-5h5v5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  config: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M10 12.5a2.5 2.5 0 100-5 2.5 2.5 0 000 5z" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
      <path d="M16.167 12.5a1.375 1.375 0 00.275 1.517l.05.05a1.667 1.667 0 11-2.359 2.358l-.05-.05a1.375 1.375 0 00-1.516-.275 1.375 1.375 0 00-.834 1.258v.142a1.667 1.667 0 11-3.333 0v-.075a1.375 1.375 0 00-.9-1.258 1.375 1.375 0 00-1.517.275l-.05.05a1.667 1.667 0 11-2.358-2.359l.05-.05a1.375 1.375 0 00.275-1.516 1.375 1.375 0 00-1.258-.834h-.142a1.667 1.667 0 110-3.333h.075a1.375 1.375 0 001.258-.9 1.375 1.375 0 00-.275-1.517l-.05-.05a1.667 1.667 0 112.359-2.358l.05.05a1.375 1.375 0 001.516.275h.067a1.375 1.375 0 00.833-1.258v-.142a1.667 1.667 0 113.334 0v.075a1.375 1.375 0 00.833 1.258 1.375 1.375 0 001.517-.275l.05-.05a1.667 1.667 0 112.358 2.359l-.05.05a1.375 1.375 0 00-.275 1.516v.067a1.375 1.375 0 001.259.833h.141a1.667 1.667 0 010 3.334h-.075a1.375 1.375 0 00-1.258.833z" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  monitoring: (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
      <path d="M18.333 10h-3.333l-2.5 7.5-5-15-2.5 7.5H1.667" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
};

// Map group IDs to icons
const groupIcons: Record<string, React.ReactNode> = {
  main: icons.dashboard,
  inbound: icons.inbound,
  outbound: icons.outbound,
  inventory: icons.inventory,
  warehouse: icons.warehouse,
  config: icons.config,
  monitoring: icons.monitoring,
};

/**
 * Main Sidebar Navigation Component
 *
 * Full navigation with:
 * - Logo at top
 * - Collapse toggle under logo
 * - Navigation groups with icons
 * - Profile & Logout at bottom
 */
export function Sidebar({ collapsed = false, onToggle }: SidebarProps) {
  const { formatMessage } = useIntl();
  const location = useLocation();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const user = useAppSelector((state) => state.auth.user);
  const currentLocale = useAppSelector((state) => state.settings.locale);
  const sites = useAppSelector((state) => state.auth.sites);
  const currentSiteId = useAppSelector((state) => state.auth.currentSelectedSiteId);

  const [expandedGroups, setExpandedGroups] = useState<Set<string>>(() => {
    // Initially expand 'main' and the group containing current path
    const initial = new Set(['main']);
    const currentPath = window.location.pathname;
    for (const group of navigationGroups) {
      if (group.items.some(item => currentPath.startsWith(item.path))) {
        initial.add(group.id);
      }
    }
    return initial;
  });
  const [popupGroup, setPopupGroup] = useState<string | null>(null);
  const [popupTop, setPopupTop] = useState<number>(0);
  const [languageMenuOpen, setLanguageMenuOpen] = useState(false);
  const [siteMenuOpen, setSiteMenuOpen] = useState(false);
  const sidebarRef = useRef<HTMLElement>(null);
  const languageBtnRef = useRef<HTMLButtonElement>(null);
  const siteBtnRef = useRef<HTMLButtonElement>(null);

  const t = useCallback(
    (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage }),
    [formatMessage]
  );

  // Load sites on mount
  useEffect(() => {
    if (sites.length === 0) {
      dispatch(loadSitesAsync());
    }
  }, [dispatch, sites.length]);

  // Close popups on outside click
  useEffect(() => {
    if (!popupGroup && !languageMenuOpen && !siteMenuOpen) return;

    const handleClickOutside = (event: MouseEvent | TouchEvent) => {
      const target = event.target as Node;

      // Check nav popup
      if (popupGroup) {
        const popup = document.querySelector('.sidebar__popup');
        const isClickOnPopup = popup?.contains(target);
        const isClickOnGroupHeader = (target as Element).closest?.('.sidebar__group-header');

        if (!isClickOnPopup && !isClickOnGroupHeader) {
          setPopupGroup(null);
        }
      }

      // Check language menu
      if (languageMenuOpen) {
        const langMenu = document.querySelector('.sidebar__lang-menu');
        const isClickOnLangMenu = langMenu?.contains(target);
        const isClickOnLangBtn = languageBtnRef.current?.contains(target);

        if (!isClickOnLangMenu && !isClickOnLangBtn) {
          setLanguageMenuOpen(false);
        }
      }

      // Check site menu
      if (siteMenuOpen) {
        const siteMenu = document.querySelector('.sidebar__site-menu');
        const isClickOnSiteMenu = siteMenu?.contains(target);
        const isClickOnSiteBtn = siteBtnRef.current?.contains(target);

        if (!isClickOnSiteMenu && !isClickOnSiteBtn) {
          setSiteMenuOpen(false);
        }
      }
    };

    // Delay adding listener to avoid catching the opening click
    const timeoutId = setTimeout(() => {
      document.addEventListener('mousedown', handleClickOutside);
      document.addEventListener('touchstart', handleClickOutside);
    }, 10);

    return () => {
      clearTimeout(timeoutId);
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('touchstart', handleClickOutside);
    };
  }, [popupGroup, languageMenuOpen, siteMenuOpen]);

  const handleGroupClick = useCallback((groupId: string, event: React.MouseEvent) => {
    if (collapsed) {
      if (popupGroup === groupId) {
        setPopupGroup(null);
      } else {
        // Calculate popup position based on clicked element
        const target = event.currentTarget as HTMLElement;
        const rect = target.getBoundingClientRect();
        setPopupTop(rect.top);
        setPopupGroup(groupId);
      }
    } else {
      setExpandedGroups((prev) => {
        const next = new Set(prev);
        if (next.has(groupId)) {
          next.delete(groupId);
        } else {
          next.add(groupId);
        }
        return next;
      });
    }
  }, [collapsed, popupGroup]);

  const isGroupActive = useCallback(
    (group: NavGroup) => {
      return group.items.some((item) =>
        location.pathname.startsWith(item.path)
      );
    },
    [location.pathname]
  );

  const handleLogout = () => {
    dispatch(logoutAsync());
    navigate('/login');
  };

  const handleLanguageMenuToggle = () => {
    setLanguageMenuOpen((prev) => !prev);
  };

  const handleLanguageSelect = (locale: SupportedLocale) => {
    dispatch(setLocale(locale));
    setLanguageMenuOpen(false);
  };

  const handleSiteMenuToggle = () => {
    setSiteMenuOpen((prev) => !prev);
  };

  const handleSiteSelect = (siteId: string) => {
    dispatch(setSelectedSiteId(siteId));
    setSiteMenuOpen(false);
  };

  const currentSite = sites.find(s => s.id === currentSiteId);
  const showSiteSwitcher = sites.length > 1;

  const userInitials = user
    ? `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`.toUpperCase() || 'U'
    : 'U';

  return (
    <aside
      ref={sidebarRef}
      className={`sidebar ${collapsed ? 'sidebar--collapsed' : ''}`}
      aria-label="Main navigation"
    >
      {/* Logo */}
      <div className="sidebar__logo-section">
        <div className="sidebar__logo">
          <span className="sidebar__logo-icon"><Logo /></span>
          {!collapsed && <span className="sidebar__logo-text">SmartWMS</span>}
        </div>
      </div>

      {/* Collapse Toggle */}
      <button
        className="sidebar__collapse-btn"
        onClick={onToggle}
        aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
      >
        {collapsed ? icons.expand : icons.collapse}
      </button>

      {/* Site Switcher - only show if more than one site */}
      {showSiteSwitcher && (
        <div className="sidebar__site-wrapper">
          <button
            ref={siteBtnRef}
            className={`sidebar__site-btn ${siteMenuOpen ? 'sidebar__site-btn--active' : ''}`}
            onClick={handleSiteMenuToggle}
            title={collapsed ? currentSite?.name || t('nav.selectSite', 'Select Site') : undefined}
          >
            <span className="sidebar__site-icon">{icons.site}</span>
            {!collapsed && (
              <>
                <span className="sidebar__site-info">
                  <span className="sidebar__site-label">{t('nav.site', 'Site')}</span>
                  <span className="sidebar__site-name">{currentSite?.name || t('nav.selectSite', 'Select Site')}</span>
                </span>
                <span className="sidebar__site-chevron">{icons.chevronDown}</span>
              </>
            )}
          </button>

          {siteMenuOpen && (
            <div className="sidebar__site-menu">
              {sites.map((site) => (
                <button
                  key={site.id}
                  className={`sidebar__site-option ${currentSiteId === site.id ? 'sidebar__site-option--active' : ''}`}
                  onClick={() => handleSiteSelect(site.id)}
                >
                  <span className="sidebar__site-option-name">{site.name}</span>
                  {site.code && <span className="sidebar__site-option-code">{site.code}</span>}
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Navigation */}
      <nav className="sidebar__nav">
        {navigationGroups.map((group) => {
          const isExpanded = expandedGroups.has(group.id);
          const isPopupOpen = popupGroup === group.id;
          const groupIcon = groupIcons[group.id];

          // Dashboard - single item, no group header
          if (group.id === 'main') {
            return (
              <div key={group.id} className="sidebar__group sidebar__group--main">
                {group.items.map((item) => (
                  <NavLink
                    key={item.id}
                    to={item.path}
                    className={({ isActive }) =>
                      `sidebar__link sidebar__link--main ${isActive ? 'sidebar__link--active' : ''}`
                    }
                    title={collapsed ? t(item.label, 'Dashboard') : undefined}
                  >
                    <span className="sidebar__link-icon">{icons.dashboard}</span>
                    {!collapsed && (
                      <span className="sidebar__link-text">{t(item.label, 'Dashboard')}</span>
                    )}
                  </NavLink>
                ))}
              </div>
            );
          }

          return (
            <div
              key={group.id}
              className={`sidebar__group ${isExpanded ? 'sidebar__group--expanded' : ''}`}
            >
              {/* Group Header */}
              <button
                className={`sidebar__group-header ${isGroupActive(group) ? 'sidebar__group-header--active' : ''}`}
                onClick={(e) => handleGroupClick(group.id, e)}
                aria-expanded={isExpanded}
                title={collapsed ? t(group.label) : undefined}
              >
                <span className="sidebar__group-icon">{groupIcon}</span>
                {!collapsed && (
                  <>
                    <span className="sidebar__group-label">{t(group.label)}</span>
                    <span className="sidebar__group-chevron">{icons.chevronDown}</span>
                  </>
                )}
              </button>

              {/* Items - expanded view */}
              {!collapsed && (
                <ul
                  className="sidebar__items"
                  role="list"
                  aria-hidden={!isExpanded}
                >
                  {group.items.map((item) => (
                    <li key={item.id} className="sidebar__item">
                      <NavLink
                        to={item.path}
                        className={({ isActive }) =>
                          `sidebar__link ${isActive ? 'sidebar__link--active' : ''}`
                        }
                      >
                        <span className="sidebar__link-text">{t(item.label)}</span>
                      </NavLink>
                    </li>
                  ))}
                </ul>
              )}

              {/* Popup - collapsed view */}
              {collapsed && isPopupOpen && (
                <div className="sidebar__popup" style={{ top: popupTop }}>
                  <div className="sidebar__popup-header">{t(group.label)}</div>
                  <ul className="sidebar__popup-items">
                    {group.items.map((item) => (
                      <li key={item.id}>
                        <NavLink
                          to={item.path}
                          className={({ isActive }) =>
                            `sidebar__popup-link ${isActive ? 'sidebar__popup-link--active' : ''}`
                          }
                          onClick={() => setPopupGroup(null)}
                        >
                          {t(item.label)}
                        </NavLink>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="sidebar__footer">
        {/* User Profile */}
        <div className="sidebar__user">
          <div
            className="sidebar__user-btn"
            title={collapsed ? user?.email || 'Profile' : undefined}
          >
            <span className="sidebar__avatar">{userInitials}</span>
            {!collapsed && (
              <span className="sidebar__user-info">
                <span className="sidebar__user-name">
                  {user?.firstName} {user?.lastName}
                </span>
                <span className="sidebar__user-email">{user?.email}</span>
              </span>
            )}
          </div>
        </div>

        {/* Language Selector */}
        <div className="sidebar__lang-wrapper">
          <button
            ref={languageBtnRef}
            className={`sidebar__footer-btn ${languageMenuOpen ? 'sidebar__footer-btn--active' : ''}`}
            onClick={handleLanguageMenuToggle}
            aria-label={t('nav.language', 'Language')}
            title={collapsed ? locales[currentLocale] : undefined}
          >
            <span className="sidebar__footer-icon">{icons.language}</span>
            {!collapsed && <span className="sidebar__footer-text">{locales[currentLocale]}</span>}
          </button>

          {languageMenuOpen && (
            <div className="sidebar__lang-menu">
              {(Object.entries(locales) as [SupportedLocale, string][]).map(([code, name]) => (
                <button
                  key={code}
                  className={`sidebar__lang-option ${currentLocale === code ? 'sidebar__lang-option--active' : ''}`}
                  onClick={() => handleLanguageSelect(code)}
                >
                  {name}
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Logout */}
        <button
          className="sidebar__footer-btn sidebar__footer-btn--logout"
          onClick={handleLogout}
          aria-label={t('nav.logout', 'Logout')}
          title={collapsed ? t('nav.logout', 'Logout') : undefined}
        >
          <span className="sidebar__footer-icon">{icons.logout}</span>
          {!collapsed && <span className="sidebar__footer-text">{t('nav.logout', 'Logout')}</span>}
        </button>
      </div>
    </aside>
  );
}

export default Sidebar;
