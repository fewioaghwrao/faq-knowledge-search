import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import AdminErrorPage from "./error";

describe("AdminErrorPage", () => {
  test("管理画面エラーの基本メッセージが表示される", () => {
    render(
      <AdminErrorPage
        error={new Error("Test error")}
        reset={jest.fn()}
      />
    );

    expect(screen.getByText("Admin Error")).toBeInTheDocument();
    expect(screen.getByText("管理画面でエラーが発生しました")).toBeInTheDocument();
    expect(
      screen.getByText(
        "FAQ、AI検索履歴、ユーザー情報の取得または表示中に問題が発生しました。"
      )
    ).toBeInTheDocument();
  });

  test("再試行するボタンを押すと reset が呼ばれる", async () => {
    const user = userEvent.setup();
    const reset = jest.fn();

    render(
      <AdminErrorPage
        error={new Error("Test error")}
        reset={reset}
      />
    );

    await user.click(screen.getByRole("button", { name: "再試行する" }));

    expect(reset).toHaveBeenCalledTimes(1);
  });

  test("管理トップへ戻るリンクが表示される", () => {
    render(
      <AdminErrorPage
        error={new Error("Test error")}
        reset={jest.fn()}
      />
    );

    const link = screen.getByRole("link", { name: "管理トップへ戻る" });

    expect(link).toHaveAttribute("href", "/admin");
  });

  test("test環境では開発用エラー詳細を表示しない", () => {
    render(
      <AdminErrorPage
        error={new Error("テスト用エラー詳細")}
        reset={jest.fn()}
      />
    );

    expect(screen.queryByText("Development Error")).not.toBeInTheDocument();
    expect(screen.queryByText("テスト用エラー詳細")).not.toBeInTheDocument();
  });
});