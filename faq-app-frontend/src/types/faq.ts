export type FaqListItem = {
  id: number;
  title: string;
  body: string;

  // フェーズ2: ハイライト済みタイトル
  titleHighlighted?: string | null;

  // フェーズ2: 本文抜粋
  bodyExcerpt?: string | null;

  categoryName: string;
  tags: string[];
  isPublished: boolean;
  viewCount: number;

  // フェーズ2: 簡易スコア
  score: number;

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