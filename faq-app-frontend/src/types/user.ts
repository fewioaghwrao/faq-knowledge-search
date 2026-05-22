export type UserListItem = {
  id: string;
  email: string;
  displayName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
};

export type UpdateUserRoleRequest = {
  role: string;
};

export type UpdateUserStatusRequest = {
  isActive: boolean;
};