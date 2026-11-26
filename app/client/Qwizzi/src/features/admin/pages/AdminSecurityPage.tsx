import { useEffect, useState } from 'react';
import { AlertTriangle, CheckCircle2, Fingerprint, KeyRound, LoaderCircle, RefreshCw, ShieldCheck } from 'lucide-react';
import { ApiError } from '../../../core/api/apiError';
import { useAuth } from '../../auth/context/useAuth';
import { adminSecurityService, type AuditVerification, type SecurityEvent } from '../api/adminSecurityService';

function formatDate(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? '—'
    : new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(date);
}

export function AdminSecurityPage() {
  const { user, updateUser } = useAuth();
  const [events, setEvents] = useState<SecurityEvent[]>([]);
  const [verification, setVerification] = useState<AuditVerification | null>(null);
  const [passwordMessage, setPasswordMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [checking, setChecking] = useState(false);
  const [enforcing, setEnforcing] = useState(false);

  async function loadEvents(showLoading = true) {
    if (showLoading) setLoading(true);
    try {
      setEvents(await adminSecurityService.listEvents());
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'Unable to load the security log.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    let active = true;
    void adminSecurityService.listEvents()
      .then((result) => { if (active) setEvents(result); })
      .catch((requestError: unknown) => {
        if (active) setError(requestError instanceof ApiError ? requestError.message : 'Unable to load the security log.');
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  async function verifyEvents() {
    setChecking(true);
    setVerification(null);
    setError('');
    try {
      setVerification(await adminSecurityService.verifyEvents());
      await loadEvents();
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to verify the security log.');
    } finally {
      setChecking(false);
    }
  }

  async function enforcePolicy() {
    if (!window.confirm('Require every active account to change its password on the next sign-in?')) return;
    setEnforcing(true);
    setPasswordMessage('');
    setError('');
    try {
      const result = await adminSecurityService.enforcePasswordPolicy();
      setPasswordMessage(`Password change required for ${result.affectedUsers} active account${result.affectedUsers === 1 ? '' : 's'}.`);
      if (user && result.affectedUsers > 0) updateUser({ ...user, mustChangePassword: true });
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to enforce the password policy.');
    } finally {
      setEnforcing(false);
    }
  }

  return (
    <section className="page-container admin-page">
      <div className="page-heading">
        <span className="eyebrow">ADMINISTRATION</span>
        <h1>Security &amp; audit</h1>
        <p>Enforce account security and review the integrity-protected event log.</p>
      </div>

      <div className="security-action-grid">
        <section className="security-action-card" aria-labelledby="password-policy-heading">
          <div className="security-action-icon"><KeyRound size={20} aria-hidden="true" /></div>
          <div className="security-action-content">
            <h2 id="password-policy-heading">Require new passwords</h2>
            <p>All active users, including administrators, will be asked to change their password before using the app again.</p>
            <button type="button" className="button button-primary" onClick={() => void enforcePolicy()} disabled={enforcing}>
              {enforcing ? <LoaderCircle className="spin" size={17} /> : <KeyRound size={17} />}
              {enforcing ? 'Applying…' : 'Enforce password change'}
            </button>
            {passwordMessage && <p className="notice success-notice security-inline-message" role="status">{passwordMessage}</p>}
          </div>
        </section>

        <section className="security-action-card" aria-labelledby="audit-check-heading">
          <div className="security-action-icon security-action-icon-blue"><Fingerprint size={21} aria-hidden="true" /></div>
          <div className="security-action-content">
            <h2 id="audit-check-heading">Verify the security log</h2>
            <p>Check the event chain and its protected checkpoint for changed or removed records.</p>
            <button type="button" className="button button-outline" onClick={() => void verifyEvents()} disabled={checking}>
              {checking ? <LoaderCircle className="spin" size={17} /> : <ShieldCheck size={17} />}
              {checking ? 'Checking log…' : 'Verify log integrity'}
            </button>
          </div>
        </section>
      </div>

      {verification && (
        <div className={`notice audit-result ${verification.isIntact ? 'success-notice' : 'error-notice'}`} role={verification.isIntact ? 'status' : 'alert'}>
          {verification.isIntact ? <CheckCircle2 size={20} /> : <AlertTriangle size={20} />}
          <div>
            <strong>{verification.isIntact ? 'No integrity incidents detected' : 'Possible security-log tampering detected'}</strong>
            <p>{verification.isIntact
              ? `${verification.verifiedEntries} event${verification.verifiedEntries === 1 ? '' : 's'} verified.`
              : `The first inconsistent entry is ${verification.firstInvalidSequence ?? 'unknown'}. ${verification.verifiedEntries} entries were verified before it.`}</p>
          </div>
        </div>
      )}
      {error && <div className="notice error-notice" role="alert">{error}</div>}

      <section className="table-card security-log-card" aria-labelledby="security-log-title">
        <div className="table-toolbar">
          <div><h2 id="security-log-title">Recent security events</h2><p>Latest 100 records; entries cannot be edited or deleted through the API.</p></div>
          <button type="button" className="icon-button refresh-button" aria-label="Refresh security events" onClick={() => { setError(''); void loadEvents(); }} disabled={loading}>
            <RefreshCw size={17} aria-hidden="true" />
          </button>
        </div>
        {loading ? (
          <div className="table-state" role="status"><LoaderCircle className="spin" size={23} /> Loading security events…</div>
        ) : events.length === 0 ? (
          <div className="table-state">No security events have been recorded.</div>
        ) : (
          <div className="table-scroll">
            <table className="security-events-table">
              <thead><tr><th>Time</th><th>User</th><th>Action</th><th>Source IP</th><th>Result</th><th>Description</th></tr></thead>
              <tbody>
                {[...events].reverse().map((event) => (
                  <tr key={event.sequence}>
                    <td className="date-cell">{formatDate(event.occurredAt)}</td>
                    <td>{event.username ?? 'Unknown'}<small className="event-sequence">#{event.sequence}</small></td>
                    <td><span className="event-action">{event.action}</span></td>
                    <td>{event.ipAddress ?? '—'}</td>
                    <td><span className={`status-pill${event.succeeded ? ' status-active' : ' status-inactive'}`}><i />{event.succeeded ? 'Success' : 'Failed'}</span></td>
                    <td className="event-description">{event.description}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </section>
  );
}
