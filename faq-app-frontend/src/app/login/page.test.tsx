import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import LoginPage from "./page";
import { loginApi } from "@/lib/api";
import { saveToken } from "@/lib/auth";
import { useRouter } from "next/navigation";

const pushMock = jest.fn();
const refreshMock = jest.fn();

jest.mock("@/lib/api", () => ({
  loginApi: jest.fn(),
}));

jest.mock("@/lib/auth", () => ({
  saveToken: jest.fn(),
}));

jest.mock("next/navigation", () => ({
  useRouter: jest.fn(),
}));

const mockedLoginApi = loginApi as jest.Mock;
const mockedSaveToken = saveToken as jest.Mock;
const mockedUseRouter = useRouter as jest.Mock;

describe("LoginPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedUseRouter.mockReturnValue({
      push: pushMock,
      refresh: refreshMock,
    });

    mockedLoginApi.mockResolvedValue({
      accessToken: "test-access-token",
      tokenType: "Bearer",
      expiresIn: 3600,
    });
  });

  test("ログイン画面の基本要素が表示される", () => {
    render(<LoginPage />);

    expect(screen.getByText("管理者ログイン")).toBeInTheDocument();
    expect(
      screen.getByText("FAQの登録・編集を行う管理画面にログインします。")
    ).toBeInTheDocument();

    expect(
      screen.getByPlaceholderText("メールアドレスを入力")
    ).toBeInTheDocument();

    expect(
      screen.getByPlaceholderText("パスワードを入力")
    ).toBeInTheDocument();

    expect(screen.getByRole("button", { name: "ログイン" })).toBeInTheDocument();
  });

  test("メールアドレスとパスワードを入力できる", async () => {
    const user = userEvent.setup();

    render(<LoginPage />);

    const emailInput = screen.getByPlaceholderText("メールアドレスを入力");
    const passwordInput = screen.getByPlaceholderText("パスワードを入力");

    await user.type(emailInput, "admin@example.com");
    await user.type(passwordInput, "Password123!");

    expect(emailInput).toHaveValue("admin@example.com");
    expect(passwordInput).toHaveValue("Password123!");
  });

  test("パスワード表示ボタンでtypeを切り替えられる", async () => {
    const user = userEvent.setup();

    render(<LoginPage />);

    const passwordInput = screen.getByPlaceholderText("パスワードを入力");

    expect(passwordInput).toHaveAttribute("type", "password");

    await user.click(
      screen.getByRole("button", { name: "パスワードを表示する" })
    );

    expect(passwordInput).toHaveAttribute("type", "text");
    expect(
      screen.getByRole("button", { name: "パスワードを非表示にする" })
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole("button", { name: "パスワードを非表示にする" })
    );

    expect(passwordInput).toHaveAttribute("type", "password");
  });

  test("ログイン成功時にloginApi、saveToken、router.push、router.refreshを呼ぶ", async () => {
    const user = userEvent.setup();

    render(<LoginPage />);

    await user.type(
      screen.getByPlaceholderText("メールアドレスを入力"),
      "  admin@example.com  "
    );

    await user.type(
      screen.getByPlaceholderText("パスワードを入力"),
      "Password123!"
    );

    await user.click(screen.getByRole("button", { name: "ログイン" }));

    await waitFor(() => {
      expect(mockedLoginApi).toHaveBeenCalledWith({
        email: "admin@example.com",
        password: "Password123!",
      });
    });

    expect(mockedSaveToken).toHaveBeenCalledWith("test-access-token");
    expect(pushMock).toHaveBeenCalledWith("/admin");
    expect(refreshMock).toHaveBeenCalledTimes(1);
  });

  test("送信中はボタンがログイン中表示になり無効化される", async () => {
    const user = userEvent.setup();

    let resolveLogin: (value: { accessToken: string }) => void;

    mockedLoginApi.mockReturnValue(
      new Promise((resolve) => {
        resolveLogin = resolve;
      })
    );

    render(<LoginPage />);

    await user.type(
      screen.getByPlaceholderText("メールアドレスを入力"),
      "admin@example.com"
    );

    await user.type(
      screen.getByPlaceholderText("パスワードを入力"),
      "Password123!"
    );

    await user.click(screen.getByRole("button", { name: "ログイン" }));

    expect(screen.getByRole("button", { name: "ログイン中..." })).toBeDisabled();

    resolveLogin!({ accessToken: "test-access-token" });

    await waitFor(() => {
      expect(pushMock).toHaveBeenCalledWith("/admin");
    });
  });

  test("ログイン失敗時はエラーメッセージを表示する", async () => {
    const user = userEvent.setup();

    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedLoginApi.mockRejectedValue(new Error("login failed"));

    render(<LoginPage />);

    await user.type(
      screen.getByPlaceholderText("メールアドレスを入力"),
      "admin@example.com"
    );

    await user.type(
      screen.getByPlaceholderText("パスワードを入力"),
      "wrong-password"
    );

    await user.click(screen.getByRole("button", { name: "ログイン" }));

    expect(
      await screen.findByText(
        "ログインに失敗しました。メールアドレスまたはパスワードを確認してください。"
      )
    ).toBeInTheDocument();

    expect(mockedSaveToken).not.toHaveBeenCalled();
    expect(pushMock).not.toHaveBeenCalled();
    expect(refreshMock).not.toHaveBeenCalled();

    consoleErrorMock.mockRestore();
  });
});