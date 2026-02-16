import { ApiConfig } from "./config";
import { ApiError } from "./errors";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export type RequestOptions = {
  method?: HttpMethod;
  path: string; // "/doctors"
  query?: Record<string, string | number | boolean | null | undefined>;
  headers?: Record<string, string>;
  body?: unknown; // will be JSON.stringify unless it's FormData/Blob/etc.
  auth?: boolean; // default true
  signal?: AbortSignal;
};

function buildQueryString(query?: RequestOptions["query"]) {
  if (!query) return "";
  const params = new URLSearchParams();
  for (const [k, v] of Object.entries(query)) {
    if (v === null || v === undefined || v === "") continue;
    params.set(k, String(v));
  }
  const s = params.toString();
  return s ? `?${s}` : "";
}

function isBodyInit(x: unknown): x is BodyInit {
  return (
    x instanceof FormData ||
    x instanceof Blob ||
    x instanceof ArrayBuffer ||
    x instanceof URLSearchParams ||
    typeof x === "string"
  );
}

async function safeParseJson(res: Response) {
  const contentType = res.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) return undefined;
  try {
    return await res.json();
  } catch {
    return undefined;
  }
}

export function createHttpClient(config: ApiConfig) {
  return async function request<T>(opts: RequestOptions): Promise<T> {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), config.timeoutMs);

    const url =
      config.baseUrl +
      (opts.path.startsWith("/") ? opts.path : `/${opts.path}`) +
      buildQueryString(opts.query);

    const headers: Record<string, string> = {
      ...config.defaultHeaders,
      ...(opts.headers ?? {}),
    };

    const wantsAuth = opts.auth ?? true;
    if (wantsAuth && config.getAuthToken) {
      const token = config.getAuthToken();
      if (token) headers.Authorization = `Bearer ${token}`;
    }

    let body: BodyInit | undefined = undefined;

    if (opts.body !== undefined) {
      if (isBodyInit(opts.body)) {
        body = opts.body;
        // If user passed FormData etc, don't force JSON content-type.
      } else {
        body = JSON.stringify(opts.body);
        headers["Content-Type"] ??= "application/json";
      }
    }

    try {
      const res = await fetch(url, {
        method: opts.method ?? "GET",
        headers,
        body,
        signal: opts.signal ?? controller.signal,
      });

      const payload = await safeParseJson(res);

      if (!res.ok) {
        const message =
          (payload as any).message ||
          (typeof payload === "string" ? payload : undefined) ||
          `${res.status} ${res.statusText}`;

        throw new ApiError({ status: res.status, url, message, payload });
      }

      // No content
      if (res.status === 204) return undefined as T;

      // Prefer JSON, fallback to text
      if ((res.headers.get("content-type") ?? "").includes("application/json")) {
        return (payload ?? (await res.json())) as T;
      }

      return (await res.text()) as unknown as T;
    } finally {
      clearTimeout(timeout);
    }
  };
}
