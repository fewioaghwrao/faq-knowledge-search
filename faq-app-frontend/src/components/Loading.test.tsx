import { render, screen } from "@testing-library/react";
import Loading from "./Loading";

describe("Loading", () => {
  test("デフォルトのタイトルとメッセージが表示される", () => {
    render(<Loading />);

    expect(screen.getByText("読み込み中...")).toBeInTheDocument();
    expect(screen.getByText("FAQデータを取得しています。")).toBeInTheDocument();
  });

  test("指定したタイトルとメッセージが表示される", () => {
    render(
      <Loading
        title="検索中..."
        message="AI検索結果を取得しています。"
      />
    );

    expect(screen.getByText("検索中...")).toBeInTheDocument();
    expect(screen.getByText("AI検索結果を取得しています。")).toBeInTheDocument();
  });

  test("ローディングスピナーが表示される", () => {
    const { container } = render(<Loading />);

    const spinner = container.querySelector(".animate-spin");

    expect(spinner).toBeInTheDocument();
  });
});