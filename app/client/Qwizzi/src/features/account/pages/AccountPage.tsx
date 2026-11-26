import { useCallback, useState } from 'react';
import { AlertTriangle, Check, LockKeyhole, Save, UserRound } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { accountService } from '../../auth/api/accountService';
import { isTurnstileConfigured } from '../../auth/api/turnstileConfig';
import { TurnstileWidget } from '../../auth/components/TurnstileWidget';
import { useAuth } from '../../auth/context/useAuth';
import {
  accountPasswordSchema,
  accountProfileSchema,
  type AccountPasswordValues,
  type AccountProfileValues,
} from '../../auth/validators/authSchemas';

export function AccountPage() {
  const { user, updateUser, clearSession } = useAuth();
  const navigate = useNavigate();
  const [profileMessage, setProfileMessage] = useState('');
  const [profileError, setProfileError] = useState('');
  const [passwordMessage, setPasswordMessage] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [deleteError, setDeleteError] = useState('');
  const [deleting, setDeleting] = useState(false);
  const [turnstileToken, setTurnstileToken] = useState('');
  const [turnstileResetKey, setTurnstileResetKey] = useState(0);
  const handleTurnstileToken = useCallback((token: string) => {
    setTurnstileToken(token);
  }, []);

  const profileForm = useForm<AccountProfileValues>({
    resolver: zodResolver(accountProfileSchema),
    defaultValues: {
      username: user?.username ?? '',
      email: user?.email ?? '',
    },
  });
  const passwordForm = useForm<AccountPasswordValues>({
    resolver: zodResolver(accountPasswordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  });

  async function saveProfile(values: AccountProfileValues) {
    if (!user) return;
    setProfileMessage('');
    setProfileError('');
    let updatedUser = user;

    try {
      if (values.username !== user.username) {
        updatedUser = await accountService.updateUsername(values.username);
        updateUser(updatedUser);
        profileForm.reset({
          username: updatedUser.username,
          email: updatedUser.email,
        });
      }
      if (values.email !== user.email) {
        updatedUser = await accountService.updateEmail(values.email);
        updateUser(updatedUser);
      }
      updateUser(updatedUser);
      profileForm.reset({
        username: updatedUser.username,
        email: updatedUser.email,
      });
      setProfileMessage('Your account details have been saved.');
    } catch (error) {
      setProfileError(
        error instanceof Error ? error.message : 'Unable to save your account details.',
      );
    }
  }

  async function changePassword(values: AccountPasswordValues) {
    setPasswordMessage('');
    setPasswordError('');
    try {
      await accountService.changePassword(
        values.currentPassword,
        values.newPassword,
        turnstileToken,
      );
      setPasswordMessage('Your password has been changed. Please sign in again.');
      passwordForm.reset();
      clearSession();
      navigate('/login', { replace: true });
    } catch (error) {
      setTurnstileToken('');
      setTurnstileResetKey((key) => key + 1);
      setPasswordError(
        error instanceof Error ? error.message : 'Unable to change your password.',
      );
    }
  }

  async function deleteAccount() {
    if (!window.confirm('Delete your account? This action cannot be undone.')) return;
    setDeleting(true);
    setDeleteError('');
    try {
      await accountService.deleteAccount();
      clearSession();
      navigate('/', { replace: true });
    } catch (error) {
      setDeleteError(
        error instanceof Error ? error.message : 'Unable to delete your account.',
      );
      setDeleting(false);
    }
  }

  return (
    <section className="page-container">
      <div className="page-heading">
        <span className="eyebrow">YOUR PROFILE</span>
        <h1>Account settings</h1>
        <p>Keep your details up to date and your account secure.</p>
      </div>

      <div className="account-summary">
        <div className="account-summary-avatar"><UserRound size={25} aria-hidden="true" /></div>
        <div>
          <strong>{user?.username}</strong>
          <span>{user?.email}</span>
        </div>
        <span className="role-pill">{user?.role === 'Administrator' ? 'Administrator' : 'Member'}</span>
      </div>

      <div className="settings-grid">
        <section className="settings-card">
          <div className="settings-card-heading">
            <div className="settings-icon"><UserRound size={19} aria-hidden="true" /></div>
            <div><h2>Personal details</h2><p>Change your username or email address.</p></div>
          </div>
          {profileError && <div className="notice error-notice" role="alert">{profileError}</div>}
          {profileMessage && <div className="notice success-notice" role="status"><Check size={17} />{profileMessage}</div>}
          <form className="settings-form" noValidate onSubmit={profileForm.handleSubmit(saveProfile)}>
            <div className="form-field">
              <label htmlFor="account-username">Username</label>
              <input id="account-username" autoComplete="username" aria-invalid={Boolean(profileForm.formState.errors.username)} aria-describedby={profileForm.formState.errors.username ? 'account-username-error' : undefined} {...profileForm.register('username')} />
              {profileForm.formState.errors.username && <p className="field-error" id="account-username-error">{profileForm.formState.errors.username.message}</p>}
            </div>
            <div className="form-field">
              <label htmlFor="account-email">Email address</label>
              <input id="account-email" type="email" autoComplete="email" aria-invalid={Boolean(profileForm.formState.errors.email)} aria-describedby={profileForm.formState.errors.email ? 'account-email-error' : undefined} {...profileForm.register('email')} />
              {profileForm.formState.errors.email && <p className="field-error" id="account-email-error">{profileForm.formState.errors.email.message}</p>}
            </div>
            <button type="submit" className="button button-primary" disabled={profileForm.formState.isSubmitting}>
              <Save size={16} aria-hidden="true" /> {profileForm.formState.isSubmitting ? 'Saving…' : 'Save changes'}
            </button>
          </form>
        </section>

        <section className="settings-card">
          <div className="settings-card-heading">
            <div className="settings-icon"><LockKeyhole size={19} aria-hidden="true" /></div>
            <div><h2>Change password</h2><p>Choose a strong password you haven’t used before.</p></div>
          </div>
          {passwordError && <div className="notice error-notice" role="alert">{passwordError}</div>}
          {passwordMessage && <div className="notice success-notice" role="status">{passwordMessage}</div>}
          <form className="settings-form" noValidate onSubmit={passwordForm.handleSubmit(changePassword)}>
            <div className="form-field">
              <label htmlFor="current-password">Current password</label>
              <input id="current-password" type="password" autoComplete="current-password" aria-invalid={Boolean(passwordForm.formState.errors.currentPassword)} aria-describedby={passwordForm.formState.errors.currentPassword ? 'current-password-error' : undefined} {...passwordForm.register('currentPassword')} />
              {passwordForm.formState.errors.currentPassword && <p className="field-error" id="current-password-error">{passwordForm.formState.errors.currentPassword.message}</p>}
            </div>
            <div className="form-field">
              <label htmlFor="new-password">New password</label>
              <input id="new-password" type="password" autoComplete="new-password" aria-invalid={Boolean(passwordForm.formState.errors.newPassword)} aria-describedby={passwordForm.formState.errors.newPassword ? 'new-password-error' : 'new-password-hint'} {...passwordForm.register('newPassword')} />
              <p className="field-hint" id="new-password-hint">8–64 characters with uppercase, lowercase, a number, and a special character.</p>
              {passwordForm.formState.errors.newPassword && <p className="field-error" id="new-password-error">{passwordForm.formState.errors.newPassword.message}</p>}
            </div>
            <div className="form-field">
              <label htmlFor="confirm-new-password">Confirm new password</label>
              <input id="confirm-new-password" type="password" autoComplete="new-password" aria-invalid={Boolean(passwordForm.formState.errors.confirmPassword)} aria-describedby={passwordForm.formState.errors.confirmPassword ? 'confirm-new-password-error' : undefined} {...passwordForm.register('confirmPassword')} />
              {passwordForm.formState.errors.confirmPassword && <p className="field-error" id="confirm-new-password-error">{passwordForm.formState.errors.confirmPassword.message}</p>}
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
              disabled={passwordForm.formState.isSubmitting || !turnstileToken || !isTurnstileConfigured}
            >
              {passwordForm.formState.isSubmitting ? 'Updating…' : 'Update password'}
            </button>
          </form>
        </section>
      </div>

      <section className="danger-zone">
        <div className="danger-copy">
          <div className="danger-icon"><AlertTriangle size={18} aria-hidden="true" /></div>
          <div><h2>Delete account</h2><p>Deleting your account is permanent and cannot be undone.</p></div>
        </div>
        <button type="button" className="button button-danger" onClick={deleteAccount} disabled={deleting}>
          {deleting ? 'Deleting…' : 'Delete my account'}
        </button>
        {deleteError && <p className="field-error danger-error" role="alert">{deleteError}</p>}
      </section>
    </section>
  );
}
