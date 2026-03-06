import { adminApi } from "@/api";

export type AdminAccount = {
  id: string;
  username: string;
  email: string;
};

export type CreateAdminPayload = {
  username: string;
  email: string;
  password: string;
};

export type UpdateAdminPayload = Partial<CreateAdminPayload>;

export const AdminsService = {
  all: () => adminApi<AdminAccount[]>({ path: "/admins" }),
  byId: (id: string) => adminApi<AdminAccount>({ path: `/admins/${id}` }),
  create: (payload: CreateAdminPayload) =>
    adminApi<AdminAccount>({ method: "POST", path: "/admins", body: payload }),
  update: (id: string, payload: UpdateAdminPayload) =>
    adminApi<AdminAccount>({ method: "PATCH", path: `/admins/${id}`, body: payload }),
  remove: (id: string) => adminApi<void>({ method: "DELETE", path: `/admins/${id}` }),
};
