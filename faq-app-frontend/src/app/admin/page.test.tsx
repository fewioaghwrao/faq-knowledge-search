import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import AdminPage from "./page";
import { deleteFaq, searchFaqs } from "@/lib/api";

jest.mock("@/lib/api", () => ({
  searchFaqs: jest.fn(),
  deleteFaq: jest.fn(),
}));

const mockedSearchFaqs = searchFaqs as jest.Mock;
const mockedDeleteFaq = deleteFaq as jest.Mock;

const faqs = [
  {
    id: 1,
    title: "ログインできない場合の対応",
    titleHighlighted: null,
    body: "メールアドレスとパスワードを確認してください。",
    bodyExcerpt: null,
    categoryName: "ログイン",
    tags: ["login"],
    isPublished: true,
    viewCount: 12,
    updatedAt: "2026-05-23T10:30:00",
    score: 0,
  },
  {
    id: 2,
    title: "請求書が表示されない",
    titleHighlighted: null,
    body: "",
    bodyExcerpt: null,
    categoryName: "請求",
    tags: ["billing"],
    isPublished: false,
    viewCount: 3,
    updatedAt: "2026-05-23T11:00:00",
    score: 0,
  },
];

function expectAtLeastOne(elements: HTMLElement[]) {
  expect(elements.length).toBeGreaterThan(0);
}

describe("AdminPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedSearchFaqs.mockResolvedValue(faqs);
    mockedDeleteFaq.mockResolvedValue(undefined);

    window.confirm = jest.fn();
    window.alert = jest.fn();
  });

  test("初期表示でFAQ一覧を取得する", async () => {
    render(<AdminPage />);

    expect(screen.getByText("FAQ一覧を読み込み中...")).toBeInTheDocument();

    await waitFor(() => {
      expect(mockedSearchFaqs).toHaveBeenCalledTimes(1);
    });
  });

  test("取得したFAQ一覧が表示される", async () => {
    render(<AdminPage />);

    expectAtLeastOne(
      await screen.findAllByText("ログインできない場合の対応")
    );

    expectAtLeastOne(screen.getAllByText("請求書が表示されない"));
    expectAtLeastOne(screen.getAllByText("ログイン"));
    expectAtLeastOne(screen.getAllByText("請求"));
    expectAtLeastOne(screen.getAllByText("公開"));
    expectAtLeastOne(screen.getAllByText("非公開"));
  });

  test("本文が空の場合は本文未登録メッセージを表示する", async () => {
    render(<AdminPage />);

    expectAtLeastOne(
      await screen.findAllByText("本文が登録されていません。")
    );
  });

  test("登録件数が表示される", async () => {
    render(<AdminPage />);

    await screen.findAllByText("ログインできない場合の対応");

    expect(
      screen.getByText((_, element) => {
        const text = element?.textContent?.replace(/\s+/g, "");
        return text === "登録件数2件";
      })
    ).toBeInTheDocument();
  });

  test("AI検索履歴を見るリンクが表示される", async () => {
    render(<AdminPage />);

    await screen.findAllByText("ログインできない場合の対応");

    const link = screen.getByRole("link", { name: "AI検索履歴を見る" });

    expect(link).toHaveAttribute("href", "/admin/ai-histories");
  });

  test("編集リンクがFAQ編集ページを向いている", async () => {
    render(<AdminPage />);

    await screen.findAllByText("ログインできない場合の対応");

    const editLinks = screen.getAllByRole("link", { name: "編集" });

    expect(editLinks[0]).toHaveAttribute("href", "/admin/faqs/1/edit");
  });

  test("FAQが0件の場合は空メッセージを表示する", async () => {
    mockedSearchFaqs.mockResolvedValue([]);

    render(<AdminPage />);

    expect(
      await screen.findByText("FAQがまだ登録されていません")
    ).toBeInTheDocument();

    expect(
      screen.getByText("新規登録ボタンからFAQを追加してください。")
    ).toBeInTheDocument();

    const link = screen.getByRole("link", { name: "FAQを登録する" });

    expect(link).toHaveAttribute("href", "/admin/faqs/new");
  });

  test("FAQ一覧取得に失敗した場合はエラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedSearchFaqs.mockRejectedValue(new Error("API error"));

    render(<AdminPage />);

    expect(
      await screen.findByText("FAQ一覧の取得に失敗しました。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("削除確認でキャンセルした場合はdeleteFaqを呼ばない", async () => {
    const user = userEvent.setup();

    (window.confirm as jest.Mock).mockReturnValue(false);

    render(<AdminPage />);

    await screen.findAllByText("ログインできない場合の対応");

    const deleteButtons = screen.getAllByRole("button", { name: "削除" });

    await user.click(deleteButtons[0]);

    expect(window.confirm).toHaveBeenCalledWith("このFAQを削除しますか？");
    expect(mockedDeleteFaq).not.toHaveBeenCalled();
  });

  test("削除確認でOKした場合はdeleteFaqを呼び、FAQ一覧を再取得する", async () => {
    const user = userEvent.setup();

    (window.confirm as jest.Mock).mockReturnValue(true);

    render(<AdminPage />);

    await screen.findAllByText("ログインできない場合の対応");

    const deleteButtons = screen.getAllByRole("button", { name: "削除" });

    await user.click(deleteButtons[0]);

    await waitFor(() => {
      expect(mockedDeleteFaq).toHaveBeenCalledWith(1);
    });

    await waitFor(() => {
      expect(mockedSearchFaqs).toHaveBeenCalledTimes(2);
    });
  });

  test("削除に失敗した場合はalertを表示する", async () => {
    const user = userEvent.setup();

    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    (window.confirm as jest.Mock).mockReturnValue(true);
    mockedDeleteFaq.mockRejectedValue(new Error("API error"));

    render(<AdminPage />);

    await screen.findAllByText("ログインできない場合の対応");

    const deleteButtons = screen.getAllByRole("button", { name: "削除" });

    await user.click(deleteButtons[0]);

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("削除に失敗しました。");
    });

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

    render(<AdminPage />);

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

    render(<AdminPage />);

    await screen.findAllByText("FAQタイトル1");

    await user.click(screen.getByRole("button", { name: "次へ" }));

    expect(screen.getByText("2 / 2")).toBeInTheDocument();
    expectAtLeastOne(screen.getAllByText("FAQタイトル6"));
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

    render(<AdminPage />);

    await screen.findAllByText("FAQタイトル1");

    await user.click(screen.getByRole("button", { name: "次へ" }));
    await user.click(screen.getByRole("button", { name: "前へ" }));

    expect(screen.getByText("1 / 2")).toBeInTheDocument();
    expectAtLeastOne(screen.getAllByText("FAQタイトル1"));
  });

  test("5件以下の場合はページングを表示しない", async () => {
    render(<AdminPage />);

    await screen.findAllByText("ログインできない場合の対応");

    expect(screen.queryByRole("button", { name: "前へ" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "次へ" })).not.toBeInTheDocument();
  });
});