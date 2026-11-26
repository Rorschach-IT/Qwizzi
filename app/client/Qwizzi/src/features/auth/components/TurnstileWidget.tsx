import { useEffect, useRef, useState } from 'react';
import { turnstileSiteKey } from '../api/turnstileConfig';

interface TurnstileOptions {
  sitekey: string;
  action: string;
  callback: (token: string) => void;
  'expired-callback': () => void;
  'error-callback': () => void;
}

interface TurnstileApi {
  render: (container: HTMLElement, options: TurnstileOptions) => string;
  remove: (widgetId: string) => void;
}

declare global {
  interface Window {
    turnstile?: TurnstileApi;
  }
}

interface TurnstileWidgetProps {
  action: 'login' | 'register' | 'password-change';
  resetKey: number;
  onTokenChange: (token: string) => void;
}

const scriptUrl = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';

export function TurnstileWidget({
  action,
  resetKey,
  onTokenChange,
}: TurnstileWidgetProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [loadError, setLoadError] = useState(false);

  useEffect(() => {
    if (!turnstileSiteKey || !containerRef.current) {
      return undefined;
    }

    let widgetId: string | undefined;
    let cancelled = false;
    const renderWidget = () => {
      if (cancelled || !window.turnstile || !containerRef.current) {
        return;
      }

      widgetId = window.turnstile.render(containerRef.current, {
        sitekey: turnstileSiteKey,
        action,
        callback: onTokenChange,
        'expired-callback': () => onTokenChange(''),
        'error-callback': () => onTokenChange(''),
      });
    };

    let script = document.querySelector<HTMLScriptElement>('script[data-qwizzi-turnstile]');
    const ownsScript = !script;
    if (!script) {
      script = document.createElement('script');
      script.src = scriptUrl;
      script.async = true;
      script.defer = true;
      script.dataset.qwizziTurnstile = 'true';
    }

    const handleScriptError = () => setLoadError(true);
    script.addEventListener('load', renderWidget);
    script.addEventListener('error', handleScriptError);
    if (window.turnstile) {
      renderWidget();
    } else if (ownsScript) {
      document.head.appendChild(script);
    }

    return () => {
      cancelled = true;
      script?.removeEventListener('load', renderWidget);
      script?.removeEventListener('error', handleScriptError);
      if (widgetId && window.turnstile) {
        window.turnstile.remove(widgetId);
      }
    };
  }, [action, onTokenChange, resetKey]);

  return (
    <div className="turnstile-field">
      {turnstileSiteKey
        ? <>
          <div ref={containerRef} />
          {loadError && <p className="turnstile-unavailable" role="alert">
            The security check could not load. Check your connection and try again.
          </p>}
        </>
        : <p className="turnstile-unavailable" role="alert">
          Security verification is not configured. Please contact the site administrator.
        </p>}
    </div>
  );
}
