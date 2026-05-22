import {
  FaqCreateRequest,
  FaqListItem,
  FaqUpdateRequest,
  LoginRequest,
  LoginResponse,
} from "@/types/faq";
import { getToken } from "./auth";
import type {
  AiSearchResponse,
  AiSearchHistoryListItem,
  AiSearchHistoryDetail,
  AiSearchFeedbackRequest,
} from "@/types/ai";
import type {
  UserListItem,
  UpdateUserRoleRequest,
  UpdateUserStatusRequest,
} from "@/types/user";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

if (!API_BASE_URL) {
  throw new Error("NEXT_PUBLIC_API_BASE_URL is not defined.");
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (options.headers) {
    Object.assign(headers, options.headers);
  }

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
    cache: "no-store",
  });

if (!response.ok) {
  if (response.status === 429) {
    throw new Error(
      "AI検索の実行回数が上限に達しました。1分ほど待ってから再実行してください。"
    );
  }

  const text = await response.text();

  throw new Error(
    text || `API error: ${response.status}`
  );
}

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}
export async function loginApi(requestBody: LoginRequest) {
  return request<LoginResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify(requestBody),
  });
}

export async function searchFaqs(keyword?: string) {
  const params = new URLSearchParams();

  const normalizedKeyword = keyword?.trim() ?? "";

  if (normalizedKeyword) {
    params.set("keyword", normalizedKeyword);
    params.set("highlight", "true");
    params.set("sort", "score");
  }

  const query = params.toString();

  return request<FaqListItem[]>(`/api/faqs${query ? `?${query}` : ""}`);
}

export async function getFaqById(id: number) {
  return request<FaqListItem>(`/api/faqs/${id}`);
}

export async function createFaq(requestBody: FaqCreateRequest) {
  return request<FaqListItem>("/api/faqs", {
    method: "POST",
    body: JSON.stringify(requestBody),
  });
}

export async function updateFaq(id: number, requestBody: FaqUpdateRequest) {
  return request<FaqListItem>(`/api/faqs/${id}`, {
    method: "PUT",
    body: JSON.stringify(requestBody),
  });
}

export async function deleteFaq(id: number) {
  return request<void>(`/api/faqs/${id}`, {
    method: "DELETE",
  });
}

export async function searchAi(question: string) {
  return request<AiSearchResponse>("/api/ai/search", {
    method: "POST",
    body: JSON.stringify({ question }),
  });
}

export type AiSearchHistoryQuery = {
  keyword?: string;
  isSuccess?: string;
  isHelpful?: string;
  page?: string;
  pageSize?: string;
};

export async function getAiSearchHistories(params?: AiSearchHistoryQuery) {
  const searchParams = new URLSearchParams();

  if (params?.keyword) searchParams.set("keyword", params.keyword);
  if (params?.isSuccess) searchParams.set("isSuccess", params.isSuccess);
  if (params?.isHelpful) searchParams.set("isHelpful", params.isHelpful);
  if (params?.page) searchParams.set("page", params.page);
  if (params?.pageSize) searchParams.set("pageSize", params.pageSize);

  const query = searchParams.toString();

  return request<AiSearchHistoryListItem[]>(
    `/api/ai/histories${query ? `?${query}` : ""}`
  );
}

export async function getAiSearchHistoryDetail(id: number) {
  return request<AiSearchHistoryDetail>(`/api/ai/histories/${id}`);
}

export async function sendAiSearchFeedback(
  id: number,
  requestBody: AiSearchFeedbackRequest
) {
  return request<void>(`/api/ai/histories/${id}/feedback`, {
    method: "POST",
    body: JSON.stringify(requestBody),
  });
}

export async function getUsers() {
  return request<UserListItem[]>("/api/users");
}

export async function updateUserRole(
  userId: string,
  requestBody: UpdateUserRoleRequest
) {
  return request<void>(`/api/users/${userId}/role`, {
    method: "PUT",
    body: JSON.stringify(requestBody),
  });
}

export async function updateUserStatus(
  userId: string,
  requestBody: UpdateUserStatusRequest
) {
  return request<void>(`/api/users/${userId}/status`, {
    method: "PUT",
    body: JSON.stringify(requestBody),
  });
}