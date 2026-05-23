import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import Header from "./Header";
import { AUTH_CHANGED_EVENT, isLoggedIn, removeToken } from "@/lib/auth";
import { act } from "react";

const pushMock = jest.fn();

jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: pushMock,
  }),
}));

jest.mock("@/lib/auth", () => ({
  AUTH_CHANGED_EVENT: "auth-changed",
  isLoggedIn: jest.fn(),
  removeToken: jest.fn(),
}));

const mockedIsLoggedIn = isLoggedIn as jest.Mock;
const mockedRemoveToken = removeToken as jest.Mock;

describe("Header", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedIsLoggedIn.mockReturnValue(false);
  });

  test("サイト名と基本ナビゲーションが表示される", () => {
    render(<Header />);

    expect(screen.getByText("Knowledge FAQ")).toBeInTheDocument();
    expect(screen.getByText("社内ナレッジ検索")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /ホーム/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /FAQ検索/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /AI検索/i })).toBeInTheDocument();
  });

  test("未ログイン時はログインリンクが表示される", () => {
    mockedIsLoggedIn.mockReturnValue(false);

    render(<Header />);

    expect(screen.getByRole("link", { name: /ログイン/i })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: /管理画面/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "ログアウト" })).not.toBeInTheDocument();
  });

  test("ログイン時は管理画面リンクとログアウトボタンが表示される", () => {
    mockedIsLoggedIn.mockReturnValue(true);

    render(<Header />);

    expect(screen.getByRole("link", { name: /管理画面/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "ログアウト" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: /ログイン/i })).not.toBeInTheDocument();
  });

  test("モバイルメニューを開閉できる", async () => {
    const user = userEvent.setup();

    render(<Header />);

    const menuButton = screen.getByRole("button", { name: "メニューを開閉" });

    expect(menuButton).toHaveAttribute("aria-expanded", "false");

    await user.click(menuButton);

    expect(menuButton).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByText("閉じる")).toBeInTheDocument();

    await user.click(menuButton);

    expect(menuButton).toHaveAttribute("aria-expanded", "false");
  });

  test("ログアウトボタン押下で確認モーダルが表示される", async () => {
    const user = userEvent.setup();
    mockedIsLoggedIn.mockReturnValue(true);

    render(<Header />);

    await user.click(screen.getByRole("button", { name: "ログアウト" }));

    expect(screen.getByText("ログアウトしますか？")).toBeInTheDocument();
    expect(
      screen.getByText("管理画面からログアウトします。再度利用する場合は、もう一度ログインしてください。")
    ).toBeInTheDocument();
  });

  test("キャンセルを押すとログアウト確認モーダルが閉じる", async () => {
    const user = userEvent.setup();
    mockedIsLoggedIn.mockReturnValue(true);

    render(<Header />);

    await user.click(screen.getByRole("button", { name: "ログアウト" }));

    expect(screen.getByText("ログアウトしますか？")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "キャンセル" }));

    expect(screen.queryByText("ログアウトしますか？")).not.toBeInTheDocument();
  });

  test("ログアウトするを押すとトークン削除後にホームへ遷移する", async () => {
    const user = userEvent.setup();
    mockedIsLoggedIn.mockReturnValue(true);

    render(<Header />);

    await user.click(screen.getByRole("button", { name: "ログアウト" }));
    await user.click(screen.getByRole("button", { name: "ログアウトする" }));

    expect(mockedRemoveToken).toHaveBeenCalledTimes(1);
    expect(pushMock).toHaveBeenCalledWith("/");
  });

test("認証変更イベントを受け取るとログイン状態が更新される", () => {
  mockedIsLoggedIn.mockReturnValue(false);

  render(<Header />);

  expect(screen.queryByRole("link", { name: /管理画面/i })).not.toBeInTheDocument();

  mockedIsLoggedIn.mockReturnValue(true);

  act(() => {
    window.dispatchEvent(new Event(AUTH_CHANGED_EVENT));
  });

  expect(screen.getByRole("link", { name: /管理画面/i })).toBeInTheDocument();
});
});