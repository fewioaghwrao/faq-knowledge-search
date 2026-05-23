import { render, screen } from "@testing-library/react";
import AppLoading from "./loading";

jest.mock("@/components/Loading", () => {
  return function MockLoading({ message }: { message: string }) {
    return <div data-testid="mock-loading">{message}</div>;
  };
});

describe("AppLoading", () => {
  test("読み込み中メッセージを表示する", () => {
    render(<AppLoading />);

    expect(screen.getByTestId("mock-loading")).toHaveTextContent(
      "画面を読み込み中です..."
    );
  });

  test("main要素を表示する", () => {
    render(<AppLoading />);

    const main = screen.getByRole("main");

    expect(main).toBeInTheDocument();
  });

  test("loading画面用のレイアウトクラスが設定されている", () => {
    render(<AppLoading />);

    const main = screen.getByRole("main");

    expect(main).toHaveClass("min-h-[calc(100vh-80px)]");
    expect(main).toHaveClass("bg-slate-950");
    expect(main).toHaveClass("px-4");
    expect(main).toHaveClass("py-10");
    expect(main).toHaveClass("text-white");
  });
});