import { useCallback, useState } from 'react';
import { KeyRound } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { accountService } from '../../auth/api/accountService';
import { isTurnstileConfigured } from '../../auth/api/turnstileConfig';
import { TurnstileWidget } from '../../auth/components/TurnstileWidget';
import { useAuth } from '../../auth/context/useAuth';

const schema = z.object({
  currentPassword: z.string().min(1),
  newPassword: z.string().min(8).max(64)
    .regex(/[a-z]/).regex(/[A-Z]/).regex(/\d/).regex(/[\W_]/),
  confirmPassword: z.string(),
}).refine((value) => value.newPassword === value.confirmPassword, {
  path: ['confirmPassword'],
});

type Values = z.infer<typeof schema>;

export function ChangePasswordPage() {
  const [message, setMessage] = useState('');
  const [turnstileToken, setTurnstileToken] = useState('');
  const [turnstileResetKey, setTurnstileResetKey] = useState(0);
  const { clearSession } = useAuth();
  const navigate = useNavigate();
  const handleTurnstileToken = useCallback((token: string) => {
    setTurnstileToken(token);
  }, []);
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  });

  async function submit(values: Values) {
    setMessage('');
    try {
      await accountService.changePassword(
        values.currentPassword,
        values.newPassword,
        turnstileToken,
      );
      clearSession();
      navigate('/login', { replace: true, state: { notice: 'Your password has been updated. Sign in with your new password.' } });
    } catch (error) {
      setTurnstileToken('');
      setTurnstileResetKey((key) => key + 1);
      setMessage(error instanceof Error ? error.message : 'Unable to update your password.');
    }
  }

  return (
    <section className="page-container">
      <div className="page-heading">
        <span className="eyebrow">ACCOUNT SECURITY</span>
        <h1>Choose a new password</h1>
        <p>For your protection, update your temporary or administrator-requested password to continue.</p>
      </div>
      <section className="settings-card password-required-card" aria-labelledby="password-required-title">
        <div className="settings-card-heading">
          <span className="settings-icon"><KeyRound size={19} aria-hidden="true" /></span>
          <div>
            <h2 id="password-required-title">Password update required</h2>
            <p>Use 8–64 characters with an uppercase letter, lowercase letter, number, and symbol.</p>
          </div>
        </div>
        {message && <div className="notice error-notice" role="alert">{message}</div>}
        <form className="settings-form" noValidate onSubmit={form.handleSubmit(submit)}>
          <div className="form-field">
            <label htmlFor="required-current-password">Current password</label>
            <input id="required-current-password" type="password" autoComplete="current-password" {...form.register('currentPassword')} />
            {form.formState.errors.currentPassword && <p className="field-error">Enter your current password.</p>}
          </div>
          <div className="form-field">
            <label htmlFor="required-new-password">New password</label>
            <input id="required-new-password" type="password" autoComplete="new-password" {...form.register('newPassword')} />
            {form.formState.errors.newPassword && <p className="field-error">The password must meet all the requirements above.</p>}
          </div>
          <div className="form-field">
            <label htmlFor="required-confirm-password">Confirm new password</label>
            <input id="required-confirm-password" type="password" autoComplete="new-password" {...form.register('confirmPassword')} />
            {form.formState.errors.confirmPassword && <p className="field-error">The passwords must match.</p>}
          </div>
          <TurnstileWidget
            key={turnstileResetKey}
            action="password-change"
            resetKey={turnstileResetKey}
            onTokenChange={handleTurnstileToken}
          />
          <button
            type="submit"
            className="button button-primary"
            disabled={form.formState.isSubmitting || !turnstileToken || !isTurnstileConfigured}
          >
            {form.formState.isSubmitting ? 'Updating…' : 'Update password'}
          </button>
        </form>
      </section>
    </section>
  );
}
