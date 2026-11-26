import { ArrowLeft, CircleAlert, Compass, ShieldX } from 'lucide-react';
import { Link } from 'react-router-dom';

export function ForbiddenPage() {
  return (
    <section className="system-page">
      <div className="system-icon forbidden-icon"><ShieldX size={29} aria-hidden="true" /></div>
      <span className="eyebrow">403 · ACCESS RESTRICTED</span>
      <h1>This area is for administrators.</h1>
      <p>Your account does not have permission to view this page.</p>
      <Link to="/" className="button button-primary"><ArrowLeft size={17} /> Back to home</Link>
    </section>
  );
}

export function NotFoundPage() {
  return (
    <section className="system-page">
      <div className="system-icon"><Compass size={29} aria-hidden="true" /></div>
      <span className="eyebrow">404 · PAGE NOT FOUND</span>
      <h1>Looks like you’ve wandered off the map.</h1>
      <p>The page you’re looking for may have moved or doesn’t exist.</p>
      <Link to="/" className="button button-primary"><ArrowLeft size={17} /> Back to home</Link>
    </section>
  );
}

export function ErrorPage() {
  return (
    <section className="system-page">
      <div className="system-icon error-icon"><CircleAlert size={29} aria-hidden="true" /></div>
      <span className="eyebrow">SOMETHING WENT WRONG</span>
      <h1>We couldn’t load this page.</h1>
      <p>Please try again. If the problem continues, come back in a little while.</p>
      <Link to="/" className="button button-primary"><ArrowLeft size={17} /> Back to home</Link>
    </section>
  );
}

export function LicensePage() {
  return (
    <section className="page-container legal-page">
      <span className="eyebrow">QWIZZI</span>
      <h1>License &amp; agreement</h1>
      <p>Information about the Qwizzi license and user agreement will be published here.</p>
      <p>By using Qwizzi, please use the service responsibly and respect the rights of other members.</p>
      <Link to="/" className="card-link">Back to home <ArrowLeft size={16} aria-hidden="true" /></Link>
    </section>
  );
}
