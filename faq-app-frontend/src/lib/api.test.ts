import {
  loginApi,
  searchFaqs,
  getFaqById,
  createFaq,
  updateFaq,
  deleteFaq,
  searchAi,
  getAiSearchHistories,
  getAiSearchHistoryDetail,
  sendAiSearchFeedback,
  getUsers,
  updateUserRole,
  updateUserStatus,
} from "./api";
import { getToken } from "./auth";

jest.mock("./auth", () => ({
  getToken: jest.fn(),
}));

const mockedGetToken = getToken as jest.Mock;

const API_BASE_URL = "http://localhost:5000";

function mockFetchResponse<T>(
  data: T,
  options?: {
    status?: number;
    ok?: boolean;
    text?: string;
  }
) {
  const status = options?.status ?? 200;
  const ok = options?.ok ?? (status >= 200 && status < 300);

  (global.fetch as jest.Mock).mockResolvedValue({
    ok,
    status,
    json: jest.fn().mockResolvedValue(data),
    text: jest.fn().mockResolvedValue(options?.text ?? ""),
  });
}

function getFetchCall() {
  const fetchMock = global.fetch as jest.Mock;
  const [url, options] = fetchMock.mock.calls[0];

  return {
    url: url as string,
    options: options as RequestInit & {
      headers: Record<string, string>;
    },
  };
}

describe("api", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedGetToken.mockReturnValue("test-token");

    global.fetch = jest.fn();
  });

  describe("共通request処理", () => {
    test("AuthorizationヘッダーにBearerトークンを付与する", async () => {
      mockFetchResponse([]);

      await searchFaqs();

      const { options } = getFetchCall();

      expect(options.headers).toEqual(
        expect.objectContaining({
          "Content-Type": "application/json",
          Authorization: "Bearer test-token",
        })
      );
    });

    test("トークンがない場合はAuthorizationヘッダーを付与しない", async () => {
      mockedGetToken.mockReturnValue(null);
      mockFetchResponse([]);

      await searchFaqs();

      const { options } = getFetchCall();

      expect(options.headers).toEqual(
        expect.objectContaining({
          "Content-Type": "application/json",
        })
      );
      expect(options.headers.Authorization).toBeUndefined();
    });

    test("全リクエストでcache no-storeを指定する", async () => {
      mockFetchResponse([]);

      await searchFaqs();

      const { options } = getFetchCall();

      expect(options.cache).toBe("no-store");
    });

    test("429の場合はAI検索上限の専用エラーメッセージを投げる", async () => {
      mockFetchResponse(null, {
        status: 429,
        ok: false,
      });

      await expect(searchAi("ログインできない")).rejects.toThrow(
        "AI検索の実行回数が上限に達しました。1分ほど待ってから再実行してください。"
      );
    });

    test("APIエラー時にレスポンス本文があればその内容をErrorにする", async () => {
      mockFetchResponse(null, {
        status: 400,
        ok: false,
        text: "入力内容が不正です。",
      });

      await expect(searchFaqs()).rejects.toThrow("入力内容が不正です。");
    });

    test("APIエラー時にレスポンス本文が空ならステータス付きErrorにする", async () => {
      mockFetchResponse(null, {
        status: 500,
        ok: false,
        text: "",
      });

      await expect(searchFaqs()).rejects.toThrow("API error: 500");
    });

    test("204 No Contentの場合はundefinedを返す", async () => {
      mockFetchResponse(null, {
        status: 204,
        ok: true,
      });

      await expect(deleteFaq(1)).resolves.toBeUndefined();
    });
  });

  describe("loginApi", () => {
    test("POST /api/auth/login にログイン情報を送信する", async () => {
      const response = {
        token: "login-token",
        email: "admin@faq-app.local",
        role: "Admin",
      };

      mockFetchResponse(response);

      const requestBody = {
        email: "admin@faq-app.local",
        password: "Password123!",
      };

      const result = await loginApi(requestBody);

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/auth/login`);
      expect(options.method).toBe("POST");
      expect(options.body).toBe(JSON.stringify(requestBody));
      expect(result).toEqual(response);
    });
  });

  describe("searchFaqs", () => {
    test("キーワードなしの場合は /api/faqs を呼び出す", async () => {
      mockFetchResponse([]);

      await searchFaqs();

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/faqs`);
      expect(options.method).toBeUndefined();
    });

    test("空白のみの場合はクエリなしで /api/faqs を呼び出す", async () => {
      mockFetchResponse([]);

      await searchFaqs("   ");

      const { url } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/faqs`);
    });

    test("キーワードありの場合はhighlightとsort付きで検索する", async () => {
      mockFetchResponse([]);

      await searchFaqs("ログイン");

      const { url } = getFetchCall();

      expect(decodeURIComponent(url)).toBe(
        `${API_BASE_URL}/api/faqs?keyword=ログイン&highlight=true&sort=score`
      );
    });

    test("前後の空白を除去して検索する", async () => {
      mockFetchResponse([]);

      await searchFaqs("  パスワード  ");

      const { url } = getFetchCall();

      expect(decodeURIComponent(url)).toBe(
        `${API_BASE_URL}/api/faqs?keyword=パスワード&highlight=true&sort=score`
      );
    });
  });

  describe("getFaqById", () => {
    test("GET /api/faqs/{id} を呼び出す", async () => {
      const faq = {
        id: 1,
        title: "ログインできない場合",
      };

      mockFetchResponse(faq);

      const result = await getFaqById(1);

      const { url } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/faqs/1`);
      expect(result).toEqual(faq);
    });
  });

  describe("createFaq", () => {
    test("POST /api/faqs にFAQ作成リクエストを送信する", async () => {
      const requestBody = {
        title: "新しいFAQ",
        body: "FAQ本文です。",
        categoryId: 1,
        tags: ["login", "error"],
        isPublished: true,
      };

      const response = {
        id: 10,
        ...requestBody,
      };

      mockFetchResponse(response);

      const result = await createFaq(requestBody as any);

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/faqs`);
      expect(options.method).toBe("POST");
      expect(options.body).toBe(JSON.stringify(requestBody));
      expect(result).toEqual(response);
    });
  });

  describe("updateFaq", () => {
    test("PUT /api/faqs/{id} にFAQ更新リクエストを送信する", async () => {
      const requestBody = {
        title: "更新後FAQ",
        body: "更新後本文です。",
        categoryId: 2,
        tags: ["updated"],
        isPublished: false,
      };

      const response = {
        id: 3,
        ...requestBody,
      };

      mockFetchResponse(response);

      const result = await updateFaq(3, requestBody as any);

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/faqs/3`);
      expect(options.method).toBe("PUT");
      expect(options.body).toBe(JSON.stringify(requestBody));
      expect(result).toEqual(response);
    });
  });

  describe("deleteFaq", () => {
    test("DELETE /api/faqs/{id} を呼び出す", async () => {
      mockFetchResponse(null, {
        status: 204,
        ok: true,
      });

      await deleteFaq(5);

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/faqs/5`);
      expect(options.method).toBe("DELETE");
    });
  });

  describe("searchAi", () => {
    test("POST /api/ai/search に質問を送信する", async () => {
      const response = {
        answer: "ログイン情報を確認してください。",
        disclaimer: "詳細は参照元FAQをご確認ください。",
        sources: [
          {
            id: 1,
            title: "ログインできない場合",
            url: "/faqs/1",
          },
        ],
        message: null,
        aiHistoryId: 100,
      };

      mockFetchResponse(response);

      const result = await searchAi("ログインできない");

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/ai/search`);
      expect(options.method).toBe("POST");
      expect(options.body).toBe(
        JSON.stringify({
          question: "ログインできない",
        })
      );
      expect(result).toEqual(response);
    });
  });

  describe("getAiSearchHistories", () => {
    test("パラメータなしの場合は /api/ai/histories を呼び出す", async () => {
      mockFetchResponse([]);

      await getAiSearchHistories();

      const { url } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/ai/histories`);
    });

    test("指定された検索条件をクエリ文字列にする", async () => {
      mockFetchResponse([]);

      await getAiSearchHistories({
        keyword: "ログイン",
        isSuccess: "true",
        isHelpful: "false",
        page: "2",
        pageSize: "20",
      });

      const { url } = getFetchCall();

      expect(decodeURIComponent(url)).toBe(
        `${API_BASE_URL}/api/ai/histories?keyword=ログイン&isSuccess=true&isHelpful=false&page=2&pageSize=20`
      );
    });

    test("未指定の検索条件はクエリに含めない", async () => {
      mockFetchResponse([]);

      await getAiSearchHistories({
        keyword: "エラー",
        page: "1",
      });

      const { url } = getFetchCall();

      expect(decodeURIComponent(url)).toBe(
        `${API_BASE_URL}/api/ai/histories?keyword=エラー&page=1`
      );
    });
  });

  describe("getAiSearchHistoryDetail", () => {
    test("GET /api/ai/histories/{id} を呼び出す", async () => {
      const response = {
        id: 7,
        question: "ログインできない",
        answer: "ログイン情報を確認してください。",
        isSuccess: true,
        errorMessage: null,
        isHelpful: null,
        executedAt: "2026-05-23T00:00:00",
        sources: [],
      };

      mockFetchResponse(response);

      const result = await getAiSearchHistoryDetail(7);

      const { url } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/ai/histories/7`);
      expect(result).toEqual(response);
    });
  });

  describe("sendAiSearchFeedback", () => {
    test("POST /api/ai/histories/{id}/feedback にフィードバックを送信する", async () => {
      const requestBody = {
        isHelpful: true,
      };

      mockFetchResponse(null, {
        status: 204,
        ok: true,
      });

      await sendAiSearchFeedback(11, requestBody as any);

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/ai/histories/11/feedback`);
      expect(options.method).toBe("POST");
      expect(options.body).toBe(JSON.stringify(requestBody));
    });
  });

  describe("getUsers", () => {
    test("GET /api/users を呼び出す", async () => {
      const response = [
        {
          id: "user-1",
          email: "admin@faq-app.local",
          role: "Admin",
          isActive: true,
        },
      ];

      mockFetchResponse(response);

      const result = await getUsers();

      const { url } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/users`);
      expect(result).toEqual(response);
    });
  });

  describe("updateUserRole", () => {
    test("PUT /api/users/{userId}/role にロール更新リクエストを送信する", async () => {
      const requestBody = {
        role: "Admin",
      };

      mockFetchResponse(null, {
        status: 204,
        ok: true,
      });

      await updateUserRole("user-1", requestBody as any);

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/users/user-1/role`);
      expect(options.method).toBe("PUT");
      expect(options.body).toBe(JSON.stringify(requestBody));
    });
  });

  describe("updateUserStatus", () => {
    test("PUT /api/users/{userId}/status にステータス更新リクエストを送信する", async () => {
      const requestBody = {
        isActive: false,
      };

      mockFetchResponse(null, {
        status: 204,
        ok: true,
      });

      await updateUserStatus("user-2", requestBody as any);

      const { url, options } = getFetchCall();

      expect(url).toBe(`${API_BASE_URL}/api/users/user-2/status`);
      expect(options.method).toBe("PUT");
      expect(options.body).toBe(JSON.stringify(requestBody));
    });
  });
});