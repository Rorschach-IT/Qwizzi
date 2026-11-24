interface CloudflareEnv {
  API_ORIGIN: string;
}

interface PagesFunctionContext {
  request: Request;
  env: CloudflareEnv;
}

export async function onRequest({
  request,
  env,
}: PagesFunctionContext): Promise<Response> {
  if (!env.API_ORIGIN) {
    return new Response('The API_ORIGIN environment variable is not configured.', {
      status: 502,
    });
  }

  let apiOrigin: URL;
  try {
    apiOrigin = new URL(env.API_ORIGIN);
  } catch {
    return new Response('API_ORIGIN must be a valid HTTPS origin.', {
      status: 500,
    });
  }

  if (
    apiOrigin.protocol !== 'https:' ||
    apiOrigin.pathname !== '/' ||
    apiOrigin.search ||
    apiOrigin.hash ||
    apiOrigin.username ||
    apiOrigin.password
  ) {
    return new Response('API_ORIGIN must be an HTTPS origin without a path.', {
      status: 500,
    });
  }

  const requestUrl = new URL(request.url);
  const upstreamUrl = new URL(
    `${requestUrl.pathname}${requestUrl.search}`,
    apiOrigin,
  );
  const headers = new Headers(request.headers);
  headers.delete('host');

  return fetch(
    new Request(upstreamUrl, {
      method: request.method,
      headers,
      body: ['GET', 'HEAD'].includes(request.method) ? undefined : request.body,
      redirect: 'manual',
    }),
  );
}
