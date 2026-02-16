import { create } from "zustand";
import { persist } from "zustand/middleware";
import { LoginResponse } from "@/types/dtos";

type AuthState = {
  token: string | null;
  user: string | null;
  username: string | null;
  hydrated: boolean;
  setLogin: (details: LoginResponse) => void;
  logout: () => void;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      username: null,
      hydrated: false,

      setLogin: (details) =>
        set({
          token: details.token,
          user: details.user,
          username: details.username,
        }),

      logout: () =>
        set({
          token: null,
          user: null,
          username: null,
        }),
    }),
    {
      name: "auth-store",
      onRehydrateStorage: () => (state, error) => {
        if (state) {
          state.hydrated = true;
        }
      },
      partialize: (state) => ({
        token: state.token,
        user: state.user,
        username: state.username,
      }),
    },
  ),
);
