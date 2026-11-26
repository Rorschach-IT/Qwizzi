import { ArrowLeft, Mail } from 'lucide-react';
import { Link } from 'react-router-dom';

export function ForgotPasswordPage() {
  return (
    <div className="auth-page">
      <div className="auth-topbar">
        <Link className="auth-back-link" to="/login">
          <ArrowLeft size={17} aria-hidden="true" /> Back to sign in
        </Link>
        <Link className="brand auth-brand" to="/" aria-label="Qwizzi home">
          <img src="/favicons/logo-design.png" alt="Qwizzi" />
        </Link>
      </div>
      <main className="auth-card" aria-labelledby="forgot-title">
        <div className="auth-card-heading">
          <span className="eyebrow">ACCOUNT RECOVERY</span>
          <h1 id="forgot-title">Reset your password.</h1>
          <p>Enter your email and we’ll help you get back to learning.</p>
        </div>
        <div className="notice info-notice" role="status">
          <Mail size={18} aria-hidden="true" />
          Password recovery is not available yet. Please contact the Qwizzi team for help.
        </div>
        <Link to="/login" className="button button-primary auth-submit">Return to sign in</Link>
      </main>
    </div>
  );
}
