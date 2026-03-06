import { adminApi } from "@/api";
import { useAdminAuthStore } from "@/stores/adminAuthStore";

export type AdminLoginRequest = {
  username: string;
  password: string;
};

export type AdminLoginResponse = {
  token: string;
  username: string;
};

export const AdminAuthService = {
  login: async (payload: AdminLoginRequest) => {
    const response = await adminApi<AdminLoginResponse>({
      method: "POST",
      path: "/auth/admin/login",
      body: payload,
      auth: false,
    });
    useAdminAuthStore.getState().setLogin(response);
    return response;
  },
};
