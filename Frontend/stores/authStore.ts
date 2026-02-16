import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { LoginResponse } from "@/api/services/authService";

type AuthState = {
  token: string | null;
  firstname: string | null;
  lastname: string | null;
  email: string | null;
  dateOfBirth: string | null;
  hydrated: boolean;
  rehydrateError: string | null;
  setHydrated: (hydrated: boolean) => void;
  setLogin: (details: LoginResponse) => void;
  logout: () => void;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      firstname: null,
      lastname: null,
      email: null,
      dateOfBirth: null,
      hydrated: false,
      rehydrateError: null,

      setHydrated: (hydrated) => set({ hydrated }),

      setLogin: (details) =>
        set({
          token: details.token,
          firstname: details.firstname,
          lastname: details.lastname,
          email: details.email,
          dateOfBirth: details.dateOfBirth,
        }),

      logout: () =>
        set({
          token: null,
          firstname: null,
          lastname: null,
          email: null,
          dateOfBirth: null,
        }),
    }),
    {
      name: "auth-store",

      partialize: (state) => ({
        token: state.token,
        firstname: state.firstname,
        lastname: state.lastname,
        email: state.email,
        dateOfBirth: state.dateOfBirth,
      }),

      onRehydrateStorage: () => (state, error) => {
        useAuthStore.setState({
          hydrated: true,
          rehydrateError: error ? String(error) : null,
        });
      },
    },
  ),
);
