import { useCallback, useState } from 'react';
import { ArrowLeft, Eye, EyeOff } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { ApiError } from '../../../core/api/apiError';
import { authService } from '../api/authService';
import { isTurnstileConfigured } from '../api/turnstileConfig';
import { TurnstileWidget } from '../components/TurnstileWidget';
import {
  registerSchema,
  type RegisterFormValues,
} from '../validators/authSchemas';

export function RegisterPage() {
  const [showPassword, setShowPassword] = useState(false);
  const [formMessage, setFormMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [turnstileToken, setTurnstileToken] = useState('');
  const [turnstileResetKey, setTurnstileResetKey] = useState(0);
  const navigate = useNavigate();
  const handleTurnstileToken = useCallback((token: string) => {
    setTurnstileToken(token);
  }, []);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
      website: '',
    },
  });

  async function onSubmit(values: RegisterFormValues) {
    setFormMessage('');
    setSuccessMessage('');
    if (values.website.trim()) {
      setSuccessMessage('Your account has been created. You can now sign in.');
      return;
    }

    try {
      await authService.register({
        username: values.username,
        email: values.email,
        password: values.password,
        website: values.website,
        turnstileToken,
      });
      navigate('/login', {
        replace: true,
        state: { notice: 'Your account has been created. You can now sign in.' },
      });
    } catch (error) {
      setTurnstileToken('');
      setTurnstileResetKey((key) => key + 1);
      setFormMessage(
        error instanceof ApiError
          ? error.message
          : "We couldn't reach the service. Please try again.",
      );
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-topbar">
        <Link className="auth-back-link" to="/login">
          <ArrowLeft size={17} aria-hidden="true" />
          Back to sign in
        </Link>
        <Link className="brand auth-brand" to="/" aria-label="Qwizzi home">
          <img src="/favicons/logo-design.png" alt="Qwizzi" />
        </Link>
      </div>
      <main className="auth-card" aria-labelledby="register-title">
        <div className="auth-card-heading">
          <span className="eyebrow">START LEARNING</span>
          <h1 id="register-title">Create your account.</h1>
          <p>Join Qwizzi and make learning a little more fun.</p>
        </div>

        {formMessage && <div className="notice error-notice" role="alert">{formMessage}</div>}
        {successMessage && <div className="notice success-notice" role="status">{successMessage}</div>}

        <form className="auth-form" noValidate onSubmit={handleSubmit(onSubmit)}>
          <div className="form-field">
            <label htmlFor="register-username">Username</label>
            <input
              id="register-username"
              type="text"
              autoComplete="username"
              placeholder="Choose a username"
              aria-invalid={Boolean(errors.username)}
              aria-describedby={errors.username ? 'register-username-error' : undefined}
              {...register('username')}
            />
            {errors.username && <p className="field-error" id="register-username-error">{errors.username.message}</p>}
          </div>
          <div className="form-field">
            <label htmlFor="register-email">Email address</label>
            <input
              id="register-email"
              type="email"
              autoComplete="email"
              placeholder="you@example.com"
              aria-invalid={Boolean(errors.email)}
              aria-describedby={errors.email ? 'register-email-error' : undefined}
              {...register('email')}
            />
            {errors.email && <p className="field-error" id="register-email-error">{errors.email.message}</p>}
          </div>
          <div className="form-field">
            <label htmlFor="register-password">Password</label>
            <div className="password-input-wrap">
              <input
                id="register-password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                placeholder="Create a strong password"
                aria-invalid={Boolean(errors.password)}
                aria-describedby={errors.password ? 'register-password-error password-hint' : 'password-hint'}
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
            <p className="field-hint" id="password-hint">8–64 characters with uppercase, lowercase, a number, and a special character.</p>
            {errors.password && <p className="field-error" id="register-password-error">{errors.password.message}</p>}
          </div>
          <div className="form-field">
            <label htmlFor="register-confirm-password">Confirm password</label>
            <input
              id="register-confirm-password"
              type={showPassword ? 'text' : 'password'}
              autoComplete="new-password"
              placeholder="Enter your password again"
              aria-invalid={Boolean(errors.confirmPassword)}
              aria-describedby={errors.confirmPassword ? 'register-confirm-error' : undefined}
              {...register('confirmPassword')}
            />
            {errors.confirmPassword && <p className="field-error" id="register-confirm-error">{errors.confirmPassword.message}</p>}
          </div>
          <div className="honeypot" aria-hidden="true">
            <label htmlFor="register-website">Website</label>
            <input
              id="register-website"
              type="text"
              tabIndex={-1}
              autoComplete="off"
              {...register('website')}
            />
          </div>
          <TurnstileWidget
            key={turnstileResetKey}
            action="register"
            resetKey={turnstileResetKey}
            onTokenChange={handleTurnstileToken}
          />
          <button
            type="submit"
            className="button button-primary auth-submit"
            disabled={isSubmitting || !turnstileToken || !isTurnstileConfigured}
          >
            {isSubmitting ? 'Creating account…' : 'Create account'}
          </button>
        </form>

        <p className="auth-switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </main>
    </div>
  );
}