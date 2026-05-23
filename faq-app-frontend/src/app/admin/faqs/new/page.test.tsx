import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import NewFaqPage from "./page";
import { createFaq } from "@/lib/api";
import { useRouter } from "next/navigation";

const pushMock = jest.fn();

jest.mock("next/navigation", () => ({
  useRouter: jest.fn(),
}));

jest.mock("@/lib/api", () => ({
  createFaq: jest.fn(),
}));

const mockedUseRouter = useRouter as jest.Mock;
const mockedCreateFaq = createFaq as jest.Mock;

describe("NewFaqPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedUseRouter.mockReturnValue({
      push: pushMock,
    });

    mockedCreateFaq.mockResolvedValue({
      id: 1,
      title: "登録タイトル",
      body: "登録本文",
      categoryName: "ログイン",
      tags: ["初期対応", "FAQ"],
      isPublished: true,
      viewCount: 0,
      updatedAt: "2026-05-23T10:30:00",
      score: 0,
      titleHighlighted: null,
      bodyExcerpt: null,
    });
  });

  test("新規登録画面の基本要素が表示される", () => {
    render(<NewFaqPage />);

    expect(screen.getByText("New FAQ")).toBeInTheDocument();
    expect(screen.getByText("FAQ新規登録")).toBeInTheDocument();
    expect(
      screen.getByText("社内FAQ・手順書・障害対応メモを登録します。")
    ).toBeInTheDocument();

    expect(
      screen.getByPlaceholderText("例：ログインできない場合の対応")
    ).toBeInTheDocument();

    expect(
      screen.getByPlaceholderText(
        "対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
      )
    ).toBeInTheDocument();

    expect(screen.getByRole("button", { name: "登録する" })).toBeInTheDocument();
  });

  test("一覧へ戻るリンクが表示される", () => {
    render(<NewFaqPage />);

    const link = screen.getByRole("link", { name: "一覧へ戻る" });

    expect(link).toHaveAttribute("href", "/admin");
  });

  test("キャンセルリンクが表示される", () => {
    render(<NewFaqPage />);

    const link = screen.getByRole("link", { name: "キャンセル" });

    expect(link).toHaveAttribute("href", "/admin");
  });

  test("初期値が表示される", () => {
    render(<NewFaqPage />);

    expect(screen.getByDisplayValue("1,2")).toBeInTheDocument();
    expect(screen.getByDisplayValue("ログイン")).toBeInTheDocument();

    const checkbox = screen.getByRole("checkbox");

    expect(checkbox).toBeChecked();
  });

  test("タイトル入力時に文字数が更新される", async () => {
    const user = userEvent.setup();

    render(<NewFaqPage />);

    const titleInput = screen.getByPlaceholderText(
      "例：ログインできない場合の対応"
    );

    expect(screen.getByText("0/100")).toBeInTheDocument();

    await user.type(titleInput, "ログインできない");

    expect(screen.getByText("8/100")).toBeInTheDocument();
  });

  test("本文入力時に文字数が更新される", async () => {
    const user = userEvent.setup();

    render(<NewFaqPage />);

    const bodyInput = screen.getByPlaceholderText(
      "対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
    );

    expect(screen.getByText("0文字")).toBeInTheDocument();

    await user.type(bodyInput, "本文テスト");

    expect(screen.getByText("5文字")).toBeInTheDocument();
  });

  test("フォームを入力してFAQを登録できる", async () => {
    const user = userEvent.setup();

    render(<NewFaqPage />);

    const titleInput = screen.getByPlaceholderText(
      "例：ログインできない場合の対応"
    );

    const bodyInput = screen.getByPlaceholderText(
      "対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
    );

    const categorySelect = screen.getByDisplayValue("ログイン");
    const tagIdsInput = screen.getByDisplayValue("1,2");
    const publishedCheckbox = screen.getByRole("checkbox");

    await user.type(titleInput, "新規FAQタイトル");
    await user.type(bodyInput, "新規FAQ本文です。");

    await user.selectOptions(categorySelect, "2");

    await user.clear(tagIdsInput);
    await user.type(tagIdsInput, "1, 3, abc");

    await user.click(publishedCheckbox);

    await user.click(screen.getByRole("button", { name: "登録する" }));

    await waitFor(() => {
      expect(mockedCreateFaq).toHaveBeenCalledWith({
        title: "新規FAQタイトル",
        body: "新規FAQ本文です。",
        categoryId: 2,
        tagIds: [1, 3],
        isPublished: false,
      });
    });

    expect(pushMock).toHaveBeenCalledWith("/admin");
  });

  test("タイトルと本文はtrimされて登録される", async () => {
    const user = userEvent.setup();

    render(<NewFaqPage />);

    const titleInput = screen.getByPlaceholderText(
      "例：ログインできない場合の対応"
    );

    const bodyInput = screen.getByPlaceholderText(
      "対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
    );

    await user.type(titleInput, "  前後空白タイトル  ");
    await user.type(bodyInput, "  前後空白本文  ");

    await user.click(screen.getByRole("button", { name: "登録する" }));

    await waitFor(() => {
      expect(mockedCreateFaq).toHaveBeenCalledWith(
        expect.objectContaining({
          title: "前後空白タイトル",
          body: "前後空白本文",
        })
      );
    });
  });

  test("タグIDは数値化され、不正な値は除外される", async () => {
    const user = userEvent.setup();

    render(<NewFaqPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合の対応"),
      "タグテスト"
    );

    await user.type(
      screen.getByPlaceholderText(
        "対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
      ),
      "タグ本文"
    );

    const tagIdsInput = screen.getByDisplayValue("1,2");

    await user.clear(tagIdsInput);
    await user.type(tagIdsInput, "1, 2, abc, 3");

    await user.click(screen.getByRole("button", { name: "登録する" }));

    await waitFor(() => {
      expect(mockedCreateFaq).toHaveBeenCalledWith(
        expect.objectContaining({
          tagIds: [1, 2, 3],
        })
      );
    });
  });

  test("登録中はボタンが無効になり表示が登録中になる", async () => {
    const user = userEvent.setup();

    let resolveCreate: () => void;

    mockedCreateFaq.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = resolve;
      })
    );

    render(<NewFaqPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合の対応"),
      "登録中テスト"
    );

    await user.type(
      screen.getByPlaceholderText(
        "対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
      ),
      "登録中本文"
    );

    await user.click(screen.getByRole("button", { name: "登録する" }));

    expect(screen.getByRole("button", { name: "登録中..." })).toBeDisabled();

    resolveCreate!();

    await waitFor(() => {
      expect(pushMock).toHaveBeenCalledWith("/admin");
    });
  });

  test("FAQ登録に失敗した場合はエラーメッセージを表示する", async () => {
    const user = userEvent.setup();

    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedCreateFaq.mockRejectedValue(new Error("API error"));

    render(<NewFaqPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合の対応"),
      "失敗テスト"
    );

    await user.type(
      screen.getByPlaceholderText(
        "対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
      ),
      "失敗本文"
    );

    await user.click(screen.getByRole("button", { name: "登録する" }));

    expect(
      await screen.findByText("FAQ登録に失敗しました。入力内容またはAPIを確認してください。")
    ).toBeInTheDocument();

    expect(pushMock).not.toHaveBeenCalled();

    consoleErrorMock.mockRestore();
  });
});