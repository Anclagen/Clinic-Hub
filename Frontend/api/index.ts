import { ApiConfigBuilder } from "./config";
import { createHttpClient } from "./http";
import { useAuthStore } from "@/stores/authStore";

const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "https://localhost:7071";

export const api = createHttpClient(
  new ApiConfigBuilder()
    .baseUrl(baseUrl)
    .timeoutMs(15_000)
    .auth(() => useAuthStore.getState().token) // <- read at request-time
    .build(),
);
