import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import HomePage from "./page";
import { searchFaqs } from "@/lib/api";

jest.mock("@/lib/api", () => ({
  searchFaqs: jest.fn(),
}));

const mockedSearchFaqs = searchFaqs as jest.Mock;

const faqs = [
  {
    id: 1,
    title: "ログインできない場合の対応",
    titleHighlighted: null,
    body: "メールアドレスとパスワードを確認してください。",
    bodyExcerpt: null,
    categoryName: "ログイン",
    tags: ["初期対応", "FAQ"],
    isPublished: true,
    viewCount: 12,
    updatedAt: "2026-05-23T10:30:00",
    score: 0,
  },
  {
    id: 2,
    title: "CSV取込エラーの対応",
    titleHighlighted: null,
    body: "CSVの文字コードと列数を確認してください。",
    bodyExcerpt: null,
    categoryName: "エラー対応",
    tags: ["CSV"],
    isPublished: true,
    viewCount: 5,
    updatedAt: "2026-05-23T11:00:00",
    score: 0,
  },
];

function expectAtLeastOne(elements: HTMLElement[]) {
  expect(elements.length).toBeGreaterThan(0);
}

describe("HomePage", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedSearchFaqs.mockResolvedValue(faqs);
  });

  test("トップページの基本要素が表示される", async () => {
    render(<HomePage />);

    expect(screen.getByText("Phase 1 / FAQ Search")).toBeInTheDocument();
    expect(screen.getByText("社内ナレッジをすばやく検索")).toBeInTheDocument();

    expect(
      screen.getByText(
        "手順書・FAQ・障害対応メモを登録し、キーワードから必要な情報を探せる社内FAQ検索アプリです。"
      )
    ).toBeInTheDocument();

    expect(screen.getByText("FAQ検索")).toBeInTheDocument();
    expect(screen.getByText("根拠表示")).toBeInTheDocument();
    expect(screen.getByText("管理者登録")).toBeInTheDocument();

    expect(
      await screen.findByText("ログインできない場合の対応")
    ).toBeInTheDocument();
  });

  test("初期表示でsearchFaqsを空文字で呼ぶ", async () => {
    render(<HomePage />);

    await waitFor(() => {
      expect(mockedSearchFaqs).toHaveBeenCalledWith("");
    });

    expect(mockedSearchFaqs).toHaveBeenCalledTimes(1);
  });

  test("読み込み中はLoadingを表示する", () => {
    render(<HomePage />);

    expect(screen.getByText("読み込み中...")).toBeInTheDocument();
    expect(screen.getByText("FAQデータを取得しています。")).toBeInTheDocument();
  });

  test("取得したFAQ一覧を表示する", async () => {
    render(<HomePage />);

    expect(
      await screen.findByText("ログインできない場合の対応")
    ).toBeInTheDocument();

    expect(screen.getByText("CSV取込エラーの対応")).toBeInTheDocument();
    expect(screen.getByText("登録済みFAQ")).toBeInTheDocument();
    expect(screen.getByText("2件")).toBeInTheDocument();
  });

  test("検索キーワードでFAQ検索できる", async () => {
    const user = userEvent.setup();

    mockedSearchFaqs
      .mockResolvedValueOnce(faqs)
      .mockResolvedValueOnce([faqs[0]]);

    render(<HomePage />);

    await screen.findByText("ログインできない場合の対応");

    const input = screen.getByPlaceholderText("キーワードでFAQを検索");

    await user.type(input, "ログイン");
    await user.click(screen.getByRole("button", { name: "検索" }));

    await waitFor(() => {
      expect(mockedSearchFaqs).toHaveBeenLastCalledWith("ログイン");
    });

    expect(screen.getByText("「ログイン」の検索結果")).toBeInTheDocument();
  });

  test("検索キーワードはtrimされる", async () => {
    const user = userEvent.setup();

    mockedSearchFaqs
      .mockResolvedValueOnce(faqs)
      .mockResolvedValueOnce([faqs[0]]);

    render(<HomePage />);

    await screen.findByText("ログインできない場合の対応");

    const input = screen.getByPlaceholderText("キーワードでFAQを検索");

    await user.type(input, "  ログイン  ");
    await user.click(screen.getByRole("button", { name: "検索" }));

    await waitFor(() => {
      expect(mockedSearchFaqs).toHaveBeenLastCalledWith("ログイン");
    });

    expect(screen.getByText("「ログイン」の検索結果")).toBeInTheDocument();
  });

  test("検索結果が0件の場合は0件メッセージを表示する", async () => {
    mockedSearchFaqs.mockResolvedValue([]);

    render(<HomePage />);

    expect(
      await screen.findByText("FAQが見つかりませんでした")
    ).toBeInTheDocument();

    expect(
      screen.getByText("キーワードを変えるか、管理画面からFAQを登録してください。")
    ).toBeInTheDocument();

    expect(screen.getByText("0件")).toBeInTheDocument();
  });

  test("FAQ取得に失敗した場合はエラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedSearchFaqs.mockRejectedValue(new Error("API error"));

    render(<HomePage />);

    expect(await screen.findByText("取得エラー")).toBeInTheDocument();

    expect(
      screen.getByText("FAQの取得に失敗しました。APIの起動状態を確認してください。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("6件以上の場合はページングが表示される", async () => {
    const manyFaqs = Array.from({ length: 6 }, (_, index) => ({
      id: index + 1,
      title: `FAQタイトル${index + 1}`,
      titleHighlighted: null,
      body: `FAQ本文${index + 1}`,
      bodyExcerpt: null,
      categoryName: "ログイン",
      tags: [],
      isPublished: true,
      viewCount: index,
      updatedAt: "2026-05-23T10:30:00",
      score: 0,
    }));

    mockedSearchFaqs.mockResolvedValue(manyFaqs);

    render(<HomePage />);

    expectAtLeastOne(await screen.findAllByText("FAQタイトル1"));

    expect(screen.getByText("1 / 2")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "前へ" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "次へ" })).toBeEnabled();
  });

  test("次へボタンで2ページ目を表示する", async () => {
    const user = userEvent.setup();

    const manyFaqs = Array.from({ length: 6 }, (_, index) => ({
      id: index + 1,
      title: `FAQタイトル${index + 1}`,
      titleHighlighted: null,
      body: `FAQ本文${index + 1}`,
      bodyExcerpt: null,
      categoryName: "ログイン",
      tags: [],
      isPublished: true,
      viewCount: index,
      updatedAt: "2026-05-23T10:30:00",
      score: 0,
    }));

    mockedSearchFaqs.mockResolvedValue(manyFaqs);

    render(<HomePage />);

    await screen.findByText("FAQタイトル1");

    await user.click(screen.getByRole("button", { name: "次へ" }));

    expect(screen.getByText("2 / 2")).toBeInTheDocument();
    expect(screen.getByText("FAQタイトル6")).toBeInTheDocument();

    expect(screen.getByRole("button", { name: "次へ" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "前へ" })).toBeEnabled();
  });

  test("前へボタンで1ページ目に戻る", async () => {
    const user = userEvent.setup();

    const manyFaqs = Array.from({ length: 6 }, (_, index) => ({
      id: index + 1,
      title: `FAQタイトル${index + 1}`,
      titleHighlighted: null,
      body: `FAQ本文${index + 1}`,
      bodyExcerpt: null,
      categoryName: "ログイン",
      tags: [],
      isPublished: true,
      viewCount: index,
      updatedAt: "2026-05-23T10:30:00",
      score: 0,
    }));

    mockedSearchFaqs.mockResolvedValue(manyFaqs);

    render(<HomePage />);

    await screen.findByText("FAQタイトル1");

    await user.click(screen.getByRole("button", { name: "次へ" }));
    await user.click(screen.getByRole("button", { name: "前へ" }));

    expect(screen.getByText("1 / 2")).toBeInTheDocument();
    expect(screen.getByText("FAQタイトル1")).toBeInTheDocument();
  });

  test("5件以下の場合はページングを表示しない", async () => {
    render(<HomePage />);

    await screen.findByText("ログインできない場合の対応");

    expect(screen.queryByRole("button", { name: "前へ" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "次へ" })).not.toBeInTheDocument();
  });

  test("検索するとページ番号が1ページ目に戻る", async () => {
    const user = userEvent.setup();

    const manyFaqs = Array.from({ length: 6 }, (_, index) => ({
      id: index + 1,
      title: `FAQタイトル${index + 1}`,
      titleHighlighted: null,
      body: `FAQ本文${index + 1}`,
      bodyExcerpt: null,
      categoryName: "ログイン",
      tags: [],
      isPublished: true,
      viewCount: index,
      updatedAt: "2026-05-23T10:30:00",
      score: 0,
    }));

    mockedSearchFaqs
      .mockResolvedValueOnce(manyFaqs)
      .mockResolvedValueOnce([manyFaqs[0]]);

    render(<HomePage />);

    await screen.findByText("FAQタイトル1");

    await user.click(screen.getByRole("button", { name: "次へ" }));

    expect(screen.getByText("2 / 2")).toBeInTheDocument();

    const input = screen.getByPlaceholderText("キーワードでFAQを検索");

    await user.type(input, "FAQタイトル1");
    await user.click(screen.getByRole("button", { name: "検索" }));

    await waitFor(() => {
      expect(mockedSearchFaqs).toHaveBeenLastCalledWith("FAQタイトル1");
    });

    expect(screen.queryByText("2 / 2")).not.toBeInTheDocument();
    expect(screen.getByText("FAQタイトル1")).toBeInTheDocument();
  });
});