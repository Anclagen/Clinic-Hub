import { api } from "@/api";
import { useAuthStore } from "@/stores/authStore";

export type LoginRequest = { email: string; password: string };
export type LoginResponse = {
  token: string;
  id: string;
  email: string;
  firstname: string;
  lastname: string;
  dateOfBirth: string;
};
export type RegisterRequest = {
  firstname: string;
  lastname: string;
  dateOfBirth: string;
  email: string;
  password: string;
};

export const AuthServices = {
  login: async (payload: LoginRequest) => {
    const res = await api<LoginResponse>({
      method: "POST",
      path: "/auth/login",
      body: payload,
      auth: false,
    });
    useAuthStore.getState().setLogin(res);
    return res;
  },
  register: (payload: RegisterRequest) => {
    return api<void>({
      method: "POST",
      path: "/auth/register",
      body: payload,
      auth: false,
    });
  },
};
