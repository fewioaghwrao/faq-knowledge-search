import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import GlobalError from "./global-error";

describe("GlobalError", () => {
  test("グローバルエラー画面の基本メッセージが表示される", () => {
    render(
      <GlobalError
        error={new Error("Test critical error")}
        reset={jest.fn()}
      />
    );

    expect(screen.getByText("Critical Error")).toBeInTheDocument();

    expect(
      screen.getByText("アプリケーションでエラーが発生しました")
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        "アプリ全体の表示中に問題が発生しました。再試行しても解消しない場合は、時間をおいて再度お試しください。"
      )
    ).toBeInTheDocument();
  });

  test("再試行するボタンを押すと reset が呼ばれる", async () => {
    const user = userEvent.setup();
    const reset = jest.fn();

    render(
      <GlobalError
        error={new Error("Test critical error")}
        reset={reset}
      />
    );

    await user.click(screen.getByRole("button", { name: "再試行する" }));

    expect(reset).toHaveBeenCalledTimes(1);
  });

  test("ホームへ戻るリンクが表示される", () => {
    render(
      <GlobalError
        error={new Error("Test critical error")}
        reset={jest.fn()}
      />
    );

    const link = screen.getByRole("link", { name: "ホームへ戻る" });

    expect(link).toHaveAttribute("href", "/");
  });


  test("test環境では開発用エラー詳細を表示しない", () => {
    render(
      <GlobalError
        error={new Error("テスト用クリティカルエラー")}
        reset={jest.fn()}
      />
    );

    expect(screen.queryByText("Development Error")).not.toBeInTheDocument();
    expect(screen.queryByText("テスト用クリティカルエラー")).not.toBeInTheDocument();
  });
});