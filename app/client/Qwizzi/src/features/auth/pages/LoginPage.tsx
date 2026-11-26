import { useCallback, useState } from 'react';
import { Eye, EyeOff, ArrowLeft } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { ApiError } from '../../../core/api/apiError';
import { authService } from '../api/authService';
import { isTurnstileConfigured } from '../api/turnstileConfig';
import { TurnstileWidget } from '../components/TurnstileWidget';
import { useAuth } from '../context/useAuth';
import { loginSchema, type LoginFormValues } from '../validators/authSchemas';

interface LoginLocationState {
  from?: string;
  notice?: string;
}

export function LoginPage() {
  const [showPassword, setShowPassword] = useState(false);
  const [formMessage, setFormMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [showTurnstile, setShowTurnstile] = useState(false);
  const [turnstileToken, setTurnstileToken] = useState('');
  const [turnstileResetKey, setTurnstileResetKey] = useState(0);
  const navigate = useNavigate();
  const location = useLocation();
  const { signIn } = useAuth();
  const state = location.state as LoginLocationState | null;
  const handleTurnstileToken = useCallback((token: string) => {
    setTurnstileToken(token);
  }, []);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitted },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
      website: '',
    },
  });

  async function onSubmit(values: LoginFormValues) {
    setFormMessage('');
    setSuccessMessage('');
    if (values.website.trim()) {
      setSuccessMessage('Your sign-in was successful.');
      return;
    }

    try {
      const user = await authService.login({
        ...values,
        turnstileToken: showTurnstile ? turnstileToken : undefined,
      });
      signIn(user);
      const target = state?.from?.startsWith('/') ? state.from : '/';
      navigate(target, { replace: true });
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.code === 'turnstile_required' || error.code === 'turnstile_invalid') {
          setShowTurnstile(true);
          setTurnstileToken('');
          setTurnstileResetKey((key) => key + 1);
          setFormMessage(error.message);
          return;
        }

        if (error.status === 401) {
          setShowTurnstile(true);
        }
        if (showTurnstile) {
          setTurnstileToken('');
          setTurnstileResetKey((key) => key + 1);
        }
        setFormMessage(error.status === 423
          ? 'Your account is temporarily locked after too many unsuccessful sign-in attempts. Please try again later.'
          : error.status === 429
            ? 'Too many sign-in requests. Please wait a moment and try again.'
            : error.status === 503
              ? error.message
              : 'Invalid email or password.');
      } else {
        setFormMessage("We couldn't reach the service. Please try again.");
      }
    }
  }

  const invalidForm = isSubmitted && Object.keys(errors).length > 0;

  return (
    <div className="auth-page">
      <div className="auth-topbar">
        <Link className="auth-back-link" to="/" aria-label="Back to home">
          <ArrowLeft size={17} aria-hidden="true" />
          Back to home
        </Link>
        <Link className="brand auth-brand" to="/" aria-label="Qwizzi home">
          <img src="/favicons/logo-design.png" alt="Qwizzi" />
        </Link>
      </div>
      <main className="auth-card" aria-labelledby="login-title">
        <div className="auth-card-heading">
          <span className="eyebrow">WELCOME BACK</span>
          <h1 id="login-title">Hello again.</h1>
          <p>Sign in to pick up where you left off.</p>
        </div>

        {state?.notice && <div className="notice success-notice" role="status">{state.notice}</div>}
        {(formMessage || invalidForm) && (
          <div className="notice error-notice" role="alert" aria-live="assertive">
            {formMessage || 'Invalid email or password.'}
          </div>
        )}
        {successMessage && <div className="notice success-notice" role="status">{successMessage}</div>}

        <form className="auth-form" noValidate onSubmit={handleSubmit(onSubmit)}>
          <div className="form-field">
            <label htmlFor="login-email">Email address</label>
            <input
              id="login-email"
              type="email"
              autoComplete="email"
              placeholder="you@example.com"
              aria-invalid={Boolean(errors.email)}
              aria-describedby={errors.email ? 'login-error' : undefined}
              {...register('email')}
            />
          </div>

          <div className="form-field">
            <label htmlFor="login-password">Password</label>
            <div className="password-input-wrap">
              <input
                id="login-password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                placeholder="Enter your password"
                aria-invalid={Boolean(errors.password)}
                aria-describedby={errors.password ? 'login-error' : undefined}
                {...register('password')}
              />
              <button
                type="button"
                className="password-toggle"
                aria-label={showPassword ? 'Hide password' : 'Show password'}
                aria-pressed={showPassword}
                onClick={() => setShowPassword((show) => !show)}
              >
                {showPassword ? <EyeOff size={19} /> : <Eye size={19} />}
              </button>
            </div>
          </div>

          <div className="auth-options">
            <label className="checkbox-label">
              <input type="checkbox" {...register('rememberMe')} />
              <span>Remember me</span>
            </label>
            <Link to="/forgot-password" className="text-link">Forgot password?</Link>
          </div>

          <div className="honeypot" aria-hidden="true">
            <label htmlFor="login-website">Website</label>
            <input
              id="login-website"
              type="text"
              tabIndex={-1}
              autoComplete="off"
              {...register('website')}
            />
          </div>

          {showTurnstile && (
            <TurnstileWidget
              key={turnstileResetKey}
              action="login"
              resetKey={turnstileResetKey}
              onTokenChange={handleTurnstileToken}
            />
          )}

          <button
            type="submit"
            className="button button-primary auth-submit"
            disabled={isSubmitting || (showTurnstile && (!turnstileToken || !isTurnstileConfigured))}
          >
            {isSubmitting ? 'Signing in…' : 'Sign in'}
          </button>
          <p id="login-error" className="sr-only">
            {errors.email || errors.password ? 'Invalid email or password.' : ''}
          </p>
        </form>

        <p className="auth-switch">
          New to Qwizzi? <Link to="/register">Create an account</Link>
        </p>
      </main>
    </div>
  );
}