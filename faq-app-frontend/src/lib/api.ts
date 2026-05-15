import {
  FaqCreateRequest,
  FaqListItem,
  FaqUpdateRequest,
  LoginRequest,
  LoginResponse,
} from "@/types/faq";
import { getToken } from "./auth";
import type { AiSearchResponse } from "@/types/ai";

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
    const text = await response.text();
    throw new Error(text || `API error: ${response.status}`);
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