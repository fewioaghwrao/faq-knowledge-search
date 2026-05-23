import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ErrorPage from "./error";

describe("ErrorPage", () => {
  test("エラー画面の基本メッセージが表示される", () => {
    render(
      <ErrorPage
        error={new Error("Test error")}
        reset={jest.fn()}
      />
    );

    expect(screen.getByText("Error")).toBeInTheDocument();
    expect(screen.getByText("エラーが発生しました")).toBeInTheDocument();

    expect(
      screen.getByText(
        "画面の表示中に問題が発生しました。再読み込みしても解消しない場合は、時間をおいて再度お試しください。"
      )
    ).toBeInTheDocument();
  });

  test("再試行するボタンを押すと reset が呼ばれる", async () => {
    const user = userEvent.setup();
    const reset = jest.fn();

    render(
      <ErrorPage
        error={new Error("Test error")}
        reset={reset}
      />
    );

    await user.click(screen.getByRole("button", { name: "再試行する" }));

    expect(reset).toHaveBeenCalledTimes(1);
  });

  test("ホームへ戻るリンクが表示される", () => {
    render(
      <ErrorPage
        error={new Error("Test error")}
        reset={jest.fn()}
      />
    );

    const link = screen.getByRole("link", { name: "ホームへ戻る" });

    expect(link).toHaveAttribute("href", "/");
  });

  test("test環境では開発用エラー詳細を表示しない", () => {
    render(
      <ErrorPage
        error={new Error("テスト用エラー詳細")}
        reset={jest.fn()}
      />
    );

    expect(screen.queryByText("Development Error")).not.toBeInTheDocument();
    expect(screen.queryByText("テスト用エラー詳細")).not.toBeInTheDocument();
  });
});