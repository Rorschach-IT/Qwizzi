# Qwizzi web client

Qwizzi is a responsive React single-page application built with Vite, TypeScript, Tailwind CSS, React Router, React Hook Form, and Zod.

## Local development

From this directory:

```powershell
npm install
npm run dev
```

The Vite development server proxies `/api` requests to `https://localhost:7275`. Start the ASP.NET Core API with its HTTPS development profile as well. To use a different API base URL, set `VITE_API_BASE_URL` before building; `/api` is the default.

Run the frontend checks with:

```powershell
npm run build
npm run lint
```

## Cloudflare Pages

The project includes a Pages Functions proxy for `/api/*` and an SPA fallback in `public/_redirects`.

1. Create a Cloudflare Pages project from the repository and set the project root to `app/client/Qwizzi`.
2. Set the build command to `npm run build` and the build output directory to `dist`.
3. Add `VITE_API_BASE_URL=/api` as a build environment variable.
4. Add `API_ORIGIN` as a Pages Functions runtime variable, for example `https://api.example.com`. Set the origin only, without an `/api` path. Configure it for both Production and Preview environments.
5. Deploy. Pages serves the SPA and the Function forwards same-origin `/api/*` requests, including the authentication and CSRF cookies, to `API_ORIGIN`.

Use an HTTPS API origin reachable from Cloudflare. Do not put credentials in the Vite environment variables: values prefixed with `VITE_` are included in browser code.

## Backend integration notes

- The API exposes authentication, account, and administrator user-management endpoints. Its current source does not expose quiz, quiz-attempt/statistics, or password-recovery endpoints, so those views intentionally show a clear unavailable/empty state rather than issuing requests to nonexistent routes.
- The login form sends `rememberMe`, but the current API ignores it and always issues a non-persistent, 30-minute cookie. Persistent sign-in requires corresponding backend support.
- The forms include the `website` honeypot field and return a silent client-side success if it is filled. The current API DTOs do not validate this field, so server-side honeypot handling is still required for effective bot protection.
