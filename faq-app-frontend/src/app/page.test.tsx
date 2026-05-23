import { render, screen } from "@testing-library/react";
import HomePage from "./page";

describe("HomePage", () => {
  test("トップページのタイトルを表示する", () => {
    render(<HomePage />);

    expect(
      screen.getByRole("heading", {
        level: 1,
        name: "社内ナレッジ検索アプリ",
      })
    ).toBeInTheDocument();
  });

  test("アプリ概要を表示する", () => {
    render(<HomePage />);

    expect(
      screen.getByText(
        "手順書・FAQ・障害対応メモを登録し、通常検索とAI検索の両方から必要な情報を探せるアプリです。"
      )
    ).toBeInTheDocument();
  });

  test("通常FAQ検索へのリンクを表示する", () => {
    render(<HomePage />);

    const link = screen.getByRole("link", {
      name: "通常FAQ検索を使う",
    });

    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute("href", "/faqs");
  });

  test("AI FAQ検索へのリンクを表示する", () => {
    render(<HomePage />);

    const link = screen.getByRole("link", {
      name: "AI FAQ検索を使う",
    });

    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute("href", "/ai-search");
  });

  test("通常FAQ検索カードを表示する", () => {
    render(<HomePage />);

    expect(
      screen.getByRole("heading", {
        level: 2,
        name: "通常FAQ検索",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        "キーワード・カテゴリ・タグをもとに、登録済みFAQを検索します。"
      )
    ).toBeInTheDocument();
  });

  test("AI FAQ検索カードを表示する", () => {
    render(<HomePage />);

    expect(
      screen.getByRole("heading", {
        level: 2,
        name: "AI FAQ検索",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        "関連FAQの上位5件をもとに、外部AI APIで回答を生成し、参照元FAQも表示します。"
      )
    ).toBeInTheDocument();
  });

test("ページ内リンクのhrefが正しい", () => {
  render(<HomePage />);

  const links = screen.getAllByRole("link");

  expect(
    links.filter((link) => link.getAttribute("href") === "/faqs")
  ).toHaveLength(2);

  expect(
    links.filter((link) => link.getAttribute("href") === "/ai-search")
  ).toHaveLength(2);
});
});