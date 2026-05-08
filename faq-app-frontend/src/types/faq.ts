export type FaqListItem = {
  id: number;
  title: string;
  body: string;
  categoryName: string;
  tags: string[];
  isPublished: boolean;
  viewCount: number;
  updatedAt: string;
};

export type FaqCreateRequest = {
  title: string;
  body: string;
  categoryId: number;
  tagIds: number[];
  isPublished: boolean;
};

export type FaqUpdateRequest = FaqCreateRequest;

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  role: string;
};