export type HeaderValue = string | undefined;

export type ApiConfig = Readonly<{
  baseUrl: string;
  timeoutMs: number;
  defaultHeaders: Record<string, string>;
  getAuthToken?: () => string | null;
}>;

export class ApiConfigBuilder {
  private _baseUrl = "";
  private _timeoutMs = 15_000;
  private _defaultHeaders: Record<string, string> = { Accept: "application/json" };
  private _getAuthToken?: () => string | null;

  baseUrl(url: string) {
    this._baseUrl = url.replace(/\/+$/, "");
    return this;
  }

  timeoutMs(ms: number) {
    this._timeoutMs = ms;
    return this;
  }

  header(key: string, value: string) {
    this._defaultHeaders[key] = value;
    return this;
  }

  auth(getToken: () => string | null) {
    this._getAuthToken = getToken;
    return this;
  }

  build(): ApiConfig {
    if (!this._baseUrl) throw new Error("ApiConfigBuilder: baseUrl is required");
    return Object.freeze({
      baseUrl: this._baseUrl,
      timeoutMs: this._timeoutMs,
      defaultHeaders: { ...this._defaultHeaders },
      getAuthToken: this._getAuthToken,
    });
  }
}
