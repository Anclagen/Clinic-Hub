import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AdminLoginResponse } from "@/api/services/adminAuthService";

type AdminAuthState = {
  token: string | null;
  username: string | null;
  hydrated: boolean;
  rehydrateError: string | null;
  setHydrated: (hydrated: boolean) => void;
  setRehydrateError: (error: string | null) => void;
  setLogin: (details: AdminLoginResponse) => void;
  logout: () => void;
};

export const useAdminAuthStore = create<AdminAuthState>()(
  persist(
    (set) => ({
      token: null,
      username: null,
      hydrated: false,
      rehydrateError: null,

      setHydrated: (hydrated) => set({ hydrated }),
      setRehydrateError: (rehydrateError) => set({ rehydrateError }),
      setLogin: (details) =>
        set({
          token: details.token,
          username: details.username,
        }),
      logout: () =>
        set({
          token: null,
          username: null,
        }),
    }),
    {
      name: "admin-auth-store",
      partialize: (state) => ({
        token: state.token,
        username: state.username,
      }),
      onRehydrateStorage: () => (state, error) => {
        state?.setHydrated(true);
        state?.setRehydrateError(error ? String(error) : null);
      },
    },
  ),
);
