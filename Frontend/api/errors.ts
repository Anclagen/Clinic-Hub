export type ApiErrorPayload =
  | { message?: string; errors?: Record<string, string>; statusCode?: number }
  | unknown;

export class ApiError extends Error {
  readonly status: number;
  readonly url: string;
  readonly payload?: ApiErrorPayload;

  constructor(args: { status: number; url: string; message: string; payload?: ApiErrorPayload }) {
    super(args.message);
    this.name = "ApiError";
    this.status = args.status;
    this.url = args.url;
    this.payload = args.payload;
  }
}

export function isApiError(err: unknown): err is ApiError {
  return err instanceof ApiError;
}

export function getUnknownMessage(err: unknown): string {
  if (err instanceof Error) return err.message;
  if (typeof err === "string") return err;
  return "Something went wrong while booking, please refresh the page and try again.";
}
