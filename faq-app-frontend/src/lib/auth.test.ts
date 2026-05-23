import {
  saveToken,
  getToken,
  removeToken,
  isLoggedIn,
  AUTH_CHANGED_EVENT,
} from "./auth";

describe("auth", () => {
  beforeEach(() => {
    localStorage.clear();
    jest.clearAllMocks();
  });

  test("saveToken は localStorage にトークンを保存する", () => {
    saveToken("test-token");

    expect(localStorage.getItem("faq_app_access_token")).toBe("test-token");
  });

  test("getToken は localStorage のトークンを取得する", () => {
    localStorage.setItem("faq_app_access_token", "stored-token");

    expect(getToken()).toBe("stored-token");
  });

  test("getToken はトークンがない場合 null を返す", () => {
    expect(getToken()).toBeNull();
  });

  test("removeToken は localStorage からトークンを削除する", () => {
    localStorage.setItem("faq_app_access_token", "test-token");

    removeToken();

    expect(localStorage.getItem("faq_app_access_token")).toBeNull();
  });

  test("isLoggedIn はトークンがある場合 true を返す", () => {
    localStorage.setItem("faq_app_access_token", "test-token");

    expect(isLoggedIn()).toBe(true);
  });

  test("isLoggedIn はトークンがない場合 false を返す", () => {
    expect(isLoggedIn()).toBe(false);
  });

  test("saveToken は認証変更イベントを発火する", () => {
    const eventHandler = jest.fn();

    window.addEventListener(AUTH_CHANGED_EVENT, eventHandler);

    saveToken("test-token");

    expect(eventHandler).toHaveBeenCalledTimes(1);

    window.removeEventListener(AUTH_CHANGED_EVENT, eventHandler);
  });

  test("removeToken は認証変更イベントを発火する", () => {
    const eventHandler = jest.fn();

    window.addEventListener(AUTH_CHANGED_EVENT, eventHandler);

    removeToken();

    expect(eventHandler).toHaveBeenCalledTimes(1);

    window.removeEventListener(AUTH_CHANGED_EVENT, eventHandler);
  });

  test("AUTH_CHANGED_EVENT は authChanged である", () => {
    expect(AUTH_CHANGED_EVENT).toBe("authChanged");
  });
});