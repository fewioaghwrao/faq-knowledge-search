import { render, screen } from "@testing-library/react";
import NotFound from "./not-found";

describe("NotFound", () => {
  test("404 Not Foundのラベルを表示する", () => {
    render(<NotFound />);

    expect(screen.getByText("404 Not Found")).toBeInTheDocument();
  });

  test("ページが見つからない旨の見出しを表示する", () => {
    render(<NotFound />);

    expect(
      screen.getByRole("heading", {
        name: "ページが見つかりません",
      })
    ).toBeInTheDocument();
  });

  test("説明文を表示する", () => {
    render(<NotFound />);

    expect(
      screen.getByText(
        "指定されたページは存在しないか、移動または削除された可能性があります。"
      )
    ).toBeInTheDocument();
  });

  test("ホームへのリンクを表示する", () => {
    render(<NotFound />);

    const link = screen.getByRole("link", {
      name: "ホームへ戻る",
    });

    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute("href", "/");
  });

  test("FAQ検索へのリンクを表示する", () => {
    render(<NotFound />);

    const link = screen.getByRole("link", {
      name: "FAQ検索へ",
    });

    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute("href", "/faqs");
  });

  test("mainにNotFound画面用のレイアウトクラスが設定されている", () => {
    render(<NotFound />);

    const main = screen.getByRole("main");

    expect(main).toHaveClass("min-h-[calc(100vh-80px)]");
    expect(main).toHaveClass("bg-slate-950");
    expect(main).toHaveClass("px-4");
    expect(main).toHaveClass("py-10");
    expect(main).toHaveClass("text-white");
  });
});