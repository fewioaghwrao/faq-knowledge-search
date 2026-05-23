import { render, screen } from "@testing-library/react";
import FaqCard from "./FaqCard";
import { FaqListItem } from "@/types/faq";

const baseFaq: FaqListItem = {
  id: 1,
  title: "ログインできない場合の対処",
  titleHighlighted: null,
  body: "ログインできない場合は、メールアドレスとパスワードを確認してください。",
  bodyExcerpt: null,
  categoryName: "アカウント",
  tags: ["login", "password"],
  isPublished: true,
  viewCount: 12,
  updatedAt: "2026-05-23T00:00:00",
  score: 0,
};

describe("FaqCard", () => {
  test("FAQの基本情報が表示される", () => {
    render(<FaqCard faq={baseFaq} />);

    expect(screen.getByText("アカウント")).toBeInTheDocument();
    expect(screen.getByText("ログインできない場合の対処")).toBeInTheDocument();
    expect(
      screen.getByText(
        "ログインできない場合は、メールアドレスとパスワードを確認してください。"
      )
    ).toBeInTheDocument();

    expect(screen.getByText("#login")).toBeInTheDocument();
    expect(screen.getByText("#password")).toBeInTheDocument();
    expect(screen.getByText("12")).toBeInTheDocument();
    expect(screen.getByText("詳細を見る →")).toBeInTheDocument();
  });

  test("詳細ページへのリンクが設定される", () => {
    render(<FaqCard faq={baseFaq} />);

    const link = screen.getByRole("link");

    expect(link).toHaveAttribute("href", "/faqs/1");
  });

  test("非公開FAQの場合は非公開ラベルが表示される", () => {
    render(
      <FaqCard
        faq={{
          ...baseFaq,
          isPublished: false,
        }}
      />
    );

    expect(screen.getByText("非公開")).toBeInTheDocument();
  });

  test("scoreが0より大きい場合はscoreが表示される", () => {
    render(
      <FaqCard
        faq={{
          ...baseFaq,
          score: 8,
        }}
      />
    );

    expect(screen.getByText("score: 8")).toBeInTheDocument();
  });

  test("scoreが0の場合はscoreが表示されない", () => {
    render(<FaqCard faq={baseFaq} />);

    expect(screen.queryByText(/score:/)).not.toBeInTheDocument();
  });

  test("bodyExcerptがある場合はbodyExcerptを優先表示する", () => {
    render(
      <FaqCard
        faq={{
          ...baseFaq,
          body: "通常本文です。",
          bodyExcerpt: "検索結果用の抜粋です。",
        }}
      />
    );

    expect(screen.getByText("検索結果用の抜粋です。")).toBeInTheDocument();
    expect(screen.queryByText("通常本文です。")).not.toBeInTheDocument();
  });

  test("タグが空の場合はタグなしが表示される", () => {
    render(
      <FaqCard
        faq={{
          ...baseFaq,
          tags: [],
        }}
      />
    );

    expect(screen.getByText("タグなし")).toBeInTheDocument();
  });

  test("titleHighlightedがある場合はハイライトHTMLを表示する", () => {
    render(
      <FaqCard
        faq={{
          ...baseFaq,
          title: "ログインできない場合の対処",
          titleHighlighted: "<mark>ログイン</mark>できない場合の対処",
        }}
      />
    );

    expect(screen.getByText("ログイン")).toBeInTheDocument();
    expect(screen.getByText("ログイン").tagName).toBe("MARK");
  });
});