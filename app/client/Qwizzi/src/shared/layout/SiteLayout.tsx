import { useCallback, useEffect, useRef, useState } from 'react';
import {
  ArrowUp,
  ChevronDown,
  LogOut,
  Menu,
  X,
} from 'lucide-react';
import { Link, Navigate, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../features/auth/context/useAuth';

const primaryLinks = [
  { label: 'Home', to: '/' },
  { label: 'Browse', to: '/browse' },
  { label: 'Quiz Manager', to: '/quiz-manager' },
];
const currentYear = new Date().getFullYear();

function NavigationLinks({ onNavigate }: { onNavigate?: () => void }) {
  return (
    <>
      {primaryLinks.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.to === '/'}
          onClick={onNavigate}
          className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
        >
          {item.label}
        </NavLink>
      ))}
    </>
  );
}

function AccountMenu({ onClose }: { onClose: () => void }) {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();
  const [logoutError, setLogoutError] = useState('');
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    menuRef.current?.querySelector<HTMLElement>('[role="menuitem"]')?.focus();
  }, []);

  function handleMenuKeyDown(event: React.KeyboardEvent<HTMLDivElement>) {
    if (!['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) return;
    const items = Array.from(
      menuRef.current?.querySelectorAll<HTMLElement>('[role="menuitem"]') ?? [],
    );
    if (!items.length) return;

    event.preventDefault();
    const currentIndex = items.indexOf(document.activeElement as HTMLElement);
    const lastIndex = items.length - 1;
    const nextIndex =
      event.key === 'Home'
        ? 0
        : event.key === 'End'
          ? lastIndex
          : event.key === 'ArrowDown'
            ? currentIndex < 0 || currentIndex === lastIndex
              ? 0
              : currentIndex + 1
            : currentIndex <= 0
              ? lastIndex
              : currentIndex - 1;
    items[nextIndex]?.focus();
  }

  async function handleSignOut() {
    try {
      await signOut();
      onClose();
      navigate('/');
    } catch (error) {
      setLogoutError(
        error instanceof Error ? error.message : 'Unable to sign out.',
      );
    }
  }

  return (
    <div
      className="account-menu"
      ref={menuRef}
      role="menu"
      aria-label="Account menu"
      onKeyDown={handleMenuKeyDown}
    >
      {user ? (
        <>
          <div className="account-menu-user">
            <strong>{user.username}</strong>
            <span>{user.email}</span>
          </div>
          <NavLink to="/account" role="menuitem" onClick={onClose}>Account</NavLink>
          <NavLink to="/account/statistics" role="menuitem" onClick={onClose}>Statistics</NavLink>
          {user.role === 'Administrator' && (
            <>
              <span className="menu-divider" />
              <NavLink to="/admin/users" role="menuitem" onClick={onClose}>Manage users</NavLink>
              <NavLink to="/admin/quizzes" role="menuitem" onClick={onClose}>Quizzes</NavLink>
              <NavLink to="/admin/security" role="menuitem" onClick={onClose}>Security &amp; audit</NavLink>
              <NavLink to="/admin/statistics" role="menuitem" onClick={onClose}>Admin statistics</NavLink>
            </>
          )}
          {logoutError && <p className="menu-error" role="alert">{logoutError}</p>}
          <button type="button" className="account-menu-signout" role="menuitem" onClick={handleSignOut}>
            <LogOut size={17} aria-hidden="true" />
            Sign out
          </button>
        </>
      ) : (
        <>
          <NavLink to="/login" role="menuitem" onClick={onClose}>Sign in</NavLink>
          <NavLink to="/register" role="menuitem" onClick={onClose}>Create an account</NavLink>
        </>
      )}
    </div>
  );
}

export function SiteLayout() {
  const { user, signOut } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [mobileMenu, setMobileMenu] = useState({ open: false, pathname: location.pathname });
  const [accountMenu, setAccountMenu] = useState({ open: false, pathname: location.pathname });
  const mobileOpen = mobileMenu.open && mobileMenu.pathname === location.pathname;
  const accountOpen = accountMenu.open && accountMenu.pathname === location.pathname;
  const [showBackToTop, setShowBackToTop] = useState(() => window.scrollY > 320);
  const [mobileError, setMobileError] = useState('');
  const accountRef = useRef<HTMLDivElement>(null);

  const closeMobileMenu = useCallback(() => {
    setMobileMenu({ open: false, pathname: location.pathname });
  }, [location.pathname]);

  const closeAccountMenu = useCallback(() => {
    setAccountMenu({ open: false, pathname: location.pathname });
  }, [location.pathname]);

  function toggleMobileMenu() {
    setMobileMenu({
      open: !mobileOpen,
      pathname: location.pathname,
    });
  }

  function toggleAccountMenu() {
    setAccountMenu({
      open: !accountOpen,
      pathname: location.pathname,
    });
  }

  useEffect(() => {
    if (!mobileOpen) return;

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        closeMobileMenu();
        document.getElementById('mobile-menu-trigger')?.focus();
      }
    };
    document.addEventListener('keydown', closeOnEscape);

    return () => {
      document.body.style.overflow = previousOverflow;
      document.removeEventListener('keydown', closeOnEscape);
    };
  }, [mobileOpen, closeMobileMenu]);

  useEffect(() => {
    if (!accountOpen) return;

    const closeOnOutsideClick = (event: MouseEvent) => {
      if (
        event.target instanceof Node &&
        !accountRef.current?.contains(event.target)
      ) {
        closeAccountMenu();
      }
    };
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        closeAccountMenu();
        accountRef.current?.querySelector<HTMLButtonElement>('.account-trigger')?.focus();
      }
    };
    document.addEventListener('mousedown', closeOnOutsideClick);
    document.addEventListener('keydown', closeOnEscape);

    return () => {
      document.removeEventListener('mousedown', closeOnOutsideClick);
      document.removeEventListener('keydown', closeOnEscape);
    };
  }, [accountOpen, closeAccountMenu]);

  useEffect(() => {
    const updateScrollState = () => setShowBackToTop(window.scrollY > 320);
    window.addEventListener('scroll', updateScrollState, { passive: true });
    return () => window.removeEventListener('scroll', updateScrollState);
  }, []);

  async function handleMobileSignOut() {
    try {
      await signOut();
      closeMobileMenu();
      navigate('/');
    } catch (error) {
      setMobileError(
        error instanceof Error ? error.message : 'Unable to sign out.',
      );
    }
  }

  return (
    <div className="site-shell">
      <a className="skip-link" href="#main-content">Skip to content</a>
      <header className="site-header">
        <div className="header-inner">
          <Link className="brand" to="/" aria-label="Qwizzi home">
            <img src="/favicons/logo-design.png" alt="Qwizzi" />
          </Link>

          <nav className="desktop-nav" aria-label="Main navigation">
            <NavigationLinks />
          </nav>

          <div className="header-actions">
            <div className="account-control" ref={accountRef}>
              <button
                type="button"
                className={`account-trigger${accountOpen ? ' is-open' : ''}`}
                aria-label={user ? `Open account menu for ${user.username}` : 'Open account menu'}
                aria-expanded={accountOpen}
                aria-haspopup="menu"
                onClick={toggleAccountMenu}
              >
                <img src="/favicons/user.png" alt="" />
                <ChevronDown size={15} aria-hidden="true" />
              </button>
              {accountOpen && <AccountMenu onClose={closeAccountMenu} />}
            </div>
            <button
              type="button"
              id="mobile-menu-trigger"
              className="mobile-menu-trigger"
              aria-label={mobileOpen ? 'Close navigation menu' : 'Open navigation menu'}
              aria-expanded={mobileOpen}
              aria-controls="mobile-navigation"
              onClick={toggleMobileMenu}
            >
              {mobileOpen ? <X size={23} /> : <Menu size={23} />}
            </button>
          </div>
        </div>
      </header>

      {mobileOpen && (
        <div className="mobile-menu-backdrop" onMouseDown={closeMobileMenu}>
          <nav
            id="mobile-navigation"
            className="mobile-menu"
            aria-label="Mobile navigation"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <div className="mobile-menu-heading">
              <Link to="/" className="brand" onClick={closeMobileMenu}>
                <img src="/favicons/logo-design.png" alt="Qwizzi" />
              </Link>
              <button
                type="button"
                className="icon-button"
                aria-label="Close navigation menu"
                onClick={closeMobileMenu}
              >
                <X size={23} />
              </button>
            </div>
            <div className="mobile-menu-links">
              <NavigationLinks onNavigate={closeMobileMenu} />
              <span className="menu-divider" />
              {user ? (
                <>
                  <NavLink className="nav-link" to="/account" onClick={closeMobileMenu}>Account</NavLink>
                  <NavLink className="nav-link" to="/account/statistics" onClick={closeMobileMenu}>Statistics</NavLink>
                  {user.role === 'Administrator' && (
                    <>
                      <NavLink className="nav-link" to="/admin/users" onClick={closeMobileMenu}>Manage users</NavLink>
                      <NavLink className="nav-link" to="/admin/quizzes" onClick={closeMobileMenu}>Quizzes</NavLink>
                      <NavLink className="nav-link" to="/admin/security" onClick={closeMobileMenu}>Security &amp; audit</NavLink>
                      <NavLink className="nav-link" to="/admin/statistics" onClick={closeMobileMenu}>Admin statistics</NavLink>
                    </>
                  )}
                  {mobileError && <p className="menu-error" role="alert">{mobileError}</p>}
                  <button type="button" className="mobile-sign-in" onClick={handleMobileSignOut}>Sign out</button>
                </>
              ) : (
                <>
                  <NavLink className="nav-link" to="/login" onClick={closeMobileMenu}>Sign in</NavLink>
                  <NavLink className="mobile-sign-in" to="/register" onClick={closeMobileMenu}>Create an account</NavLink>
                </>
              )}
            </div>
          </nav>
        </div>
      )}

      <main id="main-content" className="site-main">
        {user?.mustChangePassword && location.pathname !== '/account/change-password'
          ? <Navigate to="/account/change-password" replace />
          : <Outlet />}
      </main>

      <footer className="site-footer">
        <div className="footer-inner">
          <div className="footer-brand">
            <Link className="brand" to="/" aria-label="Qwizzi home">
              <img src="/favicons/logo-design.png" alt="Qwizzi" />
            </Link>
            <p>Make every moment a chance to learn.</p>
          </div>
          <nav className="footer-nav" aria-label="Footer navigation">
            <NavigationLinks />
            <Link className="nav-link" to="/license">License &amp; agreement</Link>
          </nav>
          <div className="footer-bottom">
            <span>© {currentYear} Qwizzi. All rights reserved.</span>
            <span>Learn something new today.</span>
          </div>
        </div>
      </footer>

      {showBackToTop && (
        <button
          type="button"
          className="back-to-top"
          aria-label="Back to top"
          onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
        >
          <ArrowUp size={19} aria-hidden="true" />
        </button>
      )}
    </div>
  );
}
