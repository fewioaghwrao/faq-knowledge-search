import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import EditFaqPage from "./page";
import { getFaqById, updateFaq } from "@/lib/api";
import { useParams, useRouter } from "next/navigation";

const pushMock = jest.fn();

jest.mock("next/navigation", () => ({
  useParams: jest.fn(),
  useRouter: jest.fn(),
}));

jest.mock("@/lib/api", () => ({
  getFaqById: jest.fn(),
  updateFaq: jest.fn(),
}));

const mockedUseParams = useParams as jest.Mock;
const mockedUseRouter = useRouter as jest.Mock;
const mockedGetFaqById = getFaqById as jest.Mock;
const mockedUpdateFaq = updateFaq as jest.Mock;

const faq = {
  id: 3,
  title: "ログインできない場合の対応",
  body: "メールアドレスとパスワードを確認してください。",
  categoryName: "ログイン",
  tags: ["初期対応", "FAQ"],
  isPublished: true,
  viewCount: 10,
  updatedAt: "2026-05-23T10:30:00",
  score: 0,
  titleHighlighted: null,
  bodyExcerpt: null,
};

describe("EditFaqPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedUseParams.mockReturnValue({ id: "3" });
    mockedUseRouter.mockReturnValue({
      push: pushMock,
    });

    mockedGetFaqById.mockResolvedValue(faq);
    mockedUpdateFaq.mockResolvedValue(undefined);
  });

  test("初期表示でFAQ情報を取得する", async () => {
    render(<EditFaqPage />);

    expect(screen.getByText("FAQ情報を読み込み中...")).toBeInTheDocument();

    await waitFor(() => {
      expect(mockedGetFaqById).toHaveBeenCalledWith(3);
    });
  });

  test("取得したFAQ情報がフォームに表示される", async () => {
    render(<EditFaqPage />);

    expect(
      await screen.findByDisplayValue("ログインできない場合の対応")
    ).toBeInTheDocument();

    expect(
      screen.getByDisplayValue("メールアドレスとパスワードを確認してください。")
    ).toBeInTheDocument();

    expect(screen.getByDisplayValue("1,2")).toBeInTheDocument();
    expect(screen.getByText("FAQ ID: #3")).toBeInTheDocument();
  });

  test("カテゴリ名がログインの場合 categoryId は1になる", async () => {
    render(<EditFaqPage />);

    await screen.findByDisplayValue("ログインできない場合の対応");

    const categorySelect = screen.getByDisplayValue("ログイン");

    expect(categorySelect).toBeInTheDocument();
  });

  test("カテゴリ名が請求の場合 categoryId は2になる", async () => {
    mockedGetFaqById.mockResolvedValue({
      ...faq,
      categoryName: "請求",
    });

    render(<EditFaqPage />);

    await screen.findByDisplayValue("ログインできない場合の対応");

    const categorySelect = screen.getByDisplayValue("請求");

    expect(categorySelect).toBeInTheDocument();
  });

  test("カテゴリ名がエラー対応の場合 categoryId は3になる", async () => {
    mockedGetFaqById.mockResolvedValue({
      ...faq,
      categoryName: "エラー対応",
    });

    render(<EditFaqPage />);

    await screen.findByDisplayValue("ログインできない場合の対応");

    const categorySelect = screen.getByDisplayValue("エラー対応");

    expect(categorySelect).toBeInTheDocument();
  });

  test("一覧へ戻るリンクが表示される", async () => {
    render(<EditFaqPage />);

    await screen.findByText("FAQ編集");

    const links = screen.getAllByRole("link", { name: "一覧へ戻る" });

    expect(links[0]).toHaveAttribute("href", "/admin");
  });

  test("キャンセルリンクが表示される", async () => {
    render(<EditFaqPage />);

    await screen.findByText("FAQ編集");

    const link = screen.getByRole("link", { name: "キャンセル" });

    expect(link).toHaveAttribute("href", "/admin");
  });

  test("フォームを編集して更新できる", async () => {
    const user = userEvent.setup();

    render(<EditFaqPage />);

    const titleInput = await screen.findByDisplayValue(
      "ログインできない場合の対応"
    );

    const bodyInput = screen.getByDisplayValue(
      "メールアドレスとパスワードを確認してください。"
    );

    const tagIdsInput = screen.getByDisplayValue("1,2");
    const categorySelect = screen.getByDisplayValue("ログイン");
    const publishedCheckbox = screen.getByRole("checkbox");

    await user.clear(titleInput);
    await user.type(titleInput, "更新後タイトル");

    await user.clear(bodyInput);
    await user.type(bodyInput, "更新後本文です。");

    await user.selectOptions(categorySelect, "2");

    await user.clear(tagIdsInput);
    await user.type(tagIdsInput, "1, 3, abc");

    await user.click(publishedCheckbox);

    await user.click(screen.getByRole("button", { name: "更新する" }));

    await waitFor(() => {
      expect(mockedUpdateFaq).toHaveBeenCalledWith(3, {
        title: "更新後タイトル",
        body: "更新後本文です。",
        categoryId: 2,
        tagIds: [1, 3],
        isPublished: false,
      });
    });

    expect(pushMock).toHaveBeenCalledWith("/admin");
  });

  test("タイトルと本文はtrimされて更新される", async () => {
    const user = userEvent.setup();

    render(<EditFaqPage />);

    const titleInput = await screen.findByDisplayValue(
      "ログインできない場合の対応"
    );

    const bodyInput = screen.getByDisplayValue(
      "メールアドレスとパスワードを確認してください。"
    );

    await user.clear(titleInput);
    await user.type(titleInput, "  タイトル前後空白  ");

    await user.clear(bodyInput);
    await user.type(bodyInput, "  本文前後空白  ");

    await user.click(screen.getByRole("button", { name: "更新する" }));

    await waitFor(() => {
      expect(mockedUpdateFaq).toHaveBeenCalledWith(
        3,
        expect.objectContaining({
          title: "タイトル前後空白",
          body: "本文前後空白",
        })
      );
    });
  });

  test("更新中はボタン表示が更新中になる", async () => {
    const user = userEvent.setup();

    let resolveUpdate: () => void;

    mockedUpdateFaq.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveUpdate = resolve;
      })
    );

    render(<EditFaqPage />);

    await screen.findByDisplayValue("ログインできない場合の対応");

    await user.click(screen.getByRole("button", { name: "更新する" }));

    expect(screen.getByRole("button", { name: "更新中..." })).toBeDisabled();

    resolveUpdate!();

    await waitFor(() => {
      expect(pushMock).toHaveBeenCalledWith("/admin");
    });
  });

  test("FAQ取得に失敗した場合はエラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedGetFaqById.mockRejectedValue(new Error("API error"));

    render(<EditFaqPage />);

    expect(
      await screen.findByText("FAQの取得に失敗しました。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("FAQ IDが不正な場合はエラーメッセージを表示し、APIを呼ばない", async () => {
    mockedUseParams.mockReturnValue({ id: "abc" });

    render(<EditFaqPage />);

    expect(await screen.findByText("FAQ IDが不正です。")).toBeInTheDocument();

    expect(mockedGetFaqById).not.toHaveBeenCalled();
  });

  test("更新に失敗した場合はエラーメッセージを表示する", async () => {
    const user = userEvent.setup();

    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedUpdateFaq.mockRejectedValue(new Error("API error"));

    render(<EditFaqPage />);

    await screen.findByDisplayValue("ログインできない場合の対応");

    await user.click(screen.getByRole("button", { name: "更新する" }));

    expect(
      await screen.findByText("FAQ更新に失敗しました。入力内容またはAPIを確認してください。")
    ).toBeInTheDocument();

    expect(pushMock).not.toHaveBeenCalled();

    consoleErrorMock.mockRestore();
  });
});