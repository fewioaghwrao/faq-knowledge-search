import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import AdminAiHistoriesPage from "./page";
import { getAiSearchHistories } from "@/lib/api";

jest.mock("@/lib/api", () => ({
  getAiSearchHistories: jest.fn(),
}));

const mockedGetAiSearchHistories = getAiSearchHistories as jest.Mock;

const histories = [
  {
    id: 1,
    question: "ログインできない場合は？",
    answerPreview: "メールアドレスとパスワードを確認してください。",
    isSuccess: true,
    errorMessage: null,
    isHelpful: true,
    sourceCount: 2,
    executedAt: "2026-05-23T10:30:00",
  },
  {
    id: 2,
    question: "AI検索でエラーになる",
    answerPreview: null,
    isSuccess: false,
    errorMessage: "AI API error",
    isHelpful: false,
    sourceCount: 0,
    executedAt: "2026-05-23T11:00:00",
  },
];

function expectAtLeastOne(elements: HTMLElement[]) {
  expect(elements.length).toBeGreaterThan(0);
}

describe("AdminAiHistoriesPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test("初期表示でAI検索履歴を取得する", async () => {
    mockedGetAiSearchHistories.mockResolvedValue(histories);

    render(<AdminAiHistoriesPage />);

    expectAtLeastOne(
      screen.getAllByText("AI検索履歴を読み込み中です...")
    );

    await waitFor(() => {
      expect(mockedGetAiSearchHistories).toHaveBeenCalledWith({
        keyword: "",
        isSuccess: "",
        isHelpful: "",
        page: "1",
        pageSize: "5",
      });
    });

    await screen.findAllByText("ログインできない場合は？");
  });

  test("取得したAI検索履歴が表示される", async () => {
    mockedGetAiSearchHistories.mockResolvedValue(histories);

    render(<AdminAiHistoriesPage />);

    expectAtLeastOne(
      await screen.findAllByText("ログインできない場合は？")
    );

    expectAtLeastOne(
      screen.getAllByText("AI検索でエラーになる")
    );

    expectAtLeastOne(
      screen.getAllByText("メールアドレスとパスワードを確認してください。")
    );

    expectAtLeastOne(
      screen.getAllByText("AI API error")
    );
  });

  test("履歴が0件の場合は空メッセージを表示する", async () => {
    mockedGetAiSearchHistories.mockResolvedValue([]);

    render(<AdminAiHistoriesPage />);

    expectAtLeastOne(
      await screen.findAllByText("AI検索履歴はまだありません。")
    );
  });

  test("取得失敗時はエラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedGetAiSearchHistories.mockRejectedValue(new Error("API error"));

    render(<AdminAiHistoriesPage />);

    expectAtLeastOne(
      await screen.findAllByText("AI検索履歴の取得に失敗しました。")
    );

    consoleErrorMock.mockRestore();
  });

  test("キーワード入力で検索条件付きAPIが呼ばれる", async () => {
    const user = userEvent.setup();

    mockedGetAiSearchHistories.mockResolvedValue(histories);

    render(<AdminAiHistoriesPage />);

    await screen.findAllByText("ログインできない場合は？");

    const input = screen.getByPlaceholderText("質問・回答・エラー内容で検索");

    await user.type(input, "ログイン");

    await waitFor(() => {
      expect(mockedGetAiSearchHistories).toHaveBeenLastCalledWith({
        keyword: "ログイン",
        isSuccess: "",
        isHelpful: "",
        page: "1",
        pageSize: "5",
      });
    });
  });

  test("成功/失敗フィルタ変更でAPIが呼ばれる", async () => {
    const user = userEvent.setup();

    mockedGetAiSearchHistories.mockResolvedValue(histories);

    render(<AdminAiHistoriesPage />);

    await screen.findAllByText("ログインできない場合は？");

    const selects = screen.getAllByRole("combobox");
    const successSelect = selects[0];

    await user.selectOptions(successSelect, "true");

    await waitFor(() => {
      expect(mockedGetAiSearchHistories).toHaveBeenLastCalledWith({
        keyword: "",
        isSuccess: "true",
        isHelpful: "",
        page: "1",
        pageSize: "5",
      });
    });
  });

  test("フィードバックフィルタ変更でAPIが呼ばれる", async () => {
    const user = userEvent.setup();

    mockedGetAiSearchHistories.mockResolvedValue(histories);

    render(<AdminAiHistoriesPage />);

    await screen.findAllByText("ログインできない場合は？");

    const selects = screen.getAllByRole("combobox");
    const helpfulSelect = selects[1];

    await user.selectOptions(helpfulSelect, "false");

    await waitFor(() => {
      expect(mockedGetAiSearchHistories).toHaveBeenLastCalledWith({
        keyword: "",
        isSuccess: "",
        isHelpful: "false",
        page: "1",
        pageSize: "5",
      });
    });
  });

  test("5件取得時は次へボタンが有効になり、クリックで2ページ目を取得する", async () => {
    const user = userEvent.setup();

    const fiveHistories = Array.from({ length: 5 }, (_, index) => ({
      id: index + 1,
      question: `質問${index + 1}`,
      answerPreview: `回答${index + 1}`,
      isSuccess: true,
      errorMessage: null,
      isHelpful: null,
      sourceCount: 1,
      executedAt: "2026-05-23T10:30:00",
    }));

    mockedGetAiSearchHistories.mockResolvedValue(fiveHistories);

    render(<AdminAiHistoriesPage />);

    expectAtLeastOne(await screen.findAllByText("質問1"));

    const nextButton = screen.getByRole("button", { name: "次へ" });

    expect(nextButton).toBeEnabled();

    await user.click(nextButton);

    await waitFor(() => {
      expect(mockedGetAiSearchHistories).toHaveBeenLastCalledWith({
        keyword: "",
        isSuccess: "",
        isHelpful: "",
        page: "2",
        pageSize: "5",
      });
    });

    expectAtLeastOne(screen.getAllByText("2ページ目"));
  });

  test("1ページ目では前へボタンが無効", async () => {
    mockedGetAiSearchHistories.mockResolvedValue(histories);

    render(<AdminAiHistoriesPage />);

    await screen.findAllByText("ログインできない場合は？");

    expect(screen.getByRole("button", { name: "前へ" })).toBeDisabled();
  });

  test("5件未満の場合は次へボタンが無効", async () => {
    mockedGetAiSearchHistories.mockResolvedValue(histories);

    render(<AdminAiHistoriesPage />);

    await screen.findAllByText("ログインできない場合は？");

    expect(screen.getByRole("button", { name: "次へ" })).toBeDisabled();
  });

  test("管理トップへ戻るリンクが表示される", async () => {
    mockedGetAiSearchHistories.mockResolvedValue([]);

    render(<AdminAiHistoriesPage />);

    const link = screen.getByRole("link", { name: "管理トップへ戻る" });

    expect(link).toHaveAttribute("href", "/admin");

    await screen.findAllByText("AI検索履歴はまだありません。");
  });
});