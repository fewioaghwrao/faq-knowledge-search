import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import AiSearchPage from "./page";
import { searchAi, sendAiSearchFeedback } from "@/lib/api";

jest.mock("@/lib/api", () => ({
  searchAi: jest.fn(),
  sendAiSearchFeedback: jest.fn(),
}));

const mockedSearchAi = searchAi as jest.Mock;
const mockedSendAiSearchFeedback = sendAiSearchFeedback as jest.Mock;

const aiResult = {
  answer: "ログインできない場合は、メールアドレスとパスワードを確認してください。",
  disclaimer: "詳細は参照元FAQをご確認ください。",
  message: null,
  aiHistoryId: 10,
  sources: [
    {
      id: 1,
      title: "ログインできない場合の対応",
      url: "/faqs/1",
    },
    {
      id: 2,
      title: "パスワード再設定手順",
      url: "/faqs/2",
    },
  ],
};

describe("AiSearchPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedSearchAi.mockResolvedValue(aiResult);
    mockedSendAiSearchFeedback.mockResolvedValue(undefined);
  });

  test("AI検索画面の基本要素が表示される", () => {
    render(<AiSearchPage />);

    expect(screen.getByText("AI FAQ Search")).toBeInTheDocument();
    expect(screen.getByText("FAQをもとにAI回答を生成")).toBeInTheDocument();

    expect(
      screen.getByText(
        "質問・検索キーワードから関連FAQを検索し、上位5件のFAQをもとにAI回答と参照元FAQを表示します。"
      )
    ).toBeInTheDocument();

    expect(screen.getByText("FAQ検索連携")).toBeInTheDocument();
    expect(screen.getByText("参照元表示")).toBeInTheDocument();
    expect(screen.getByText("外部AI API連携")).toBeInTheDocument();

    expect(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？")
    ).toBeInTheDocument();

    expect(screen.getByRole("button", { name: "AI検索する" })).toBeInTheDocument();
  });

  test("通常のFAQ検索を見るリンクが表示される", () => {
    render(<AiSearchPage />);

    const link = screen.getByRole("link", { name: "通常のFAQ検索を見る →" });

    expect(link).toHaveAttribute("href", "/faqs");
  });

test("質問例ボタンを押すと入力欄に反映される", async () => {
  const user = userEvent.setup();

  render(<AiSearchPage />);

  const textarea = screen.getByPlaceholderText(
    "例：ログインできない場合はどうすればいいですか？"
  );

  await user.click(screen.getByRole("button", { name: "ログインできない 初期対応" }));

  expect(textarea).toHaveValue("ログインできない 初期対応");

  expect(
    screen.getByText((_, element) => {
      const text = element?.textContent?.replace(/\s+/g, "");
      return text === "13/500";
    })
  ).toBeInTheDocument();
});

  test("未入力で検索するとエラーメッセージを表示し、APIを呼ばない", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    expect(screen.getByText("実行エラー")).toBeInTheDocument();
    expect(
      screen.getByText("質問・検索キーワードを入力してください。")
    ).toBeInTheDocument();

    expect(mockedSearchAi).not.toHaveBeenCalled();
  });

  test("質問を入力してAI検索するとsearchAiを呼び、結果を表示する", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    const textarea = screen.getByPlaceholderText(
      "例：ログインできない場合はどうすればいいですか？"
    );

    await user.type(textarea, "ログインできない場合は？");
    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await waitFor(() => {
      expect(mockedSearchAi).toHaveBeenCalledWith("ログインできない場合は？");
    });

    expect(
      await screen.findByText(
        "ログインできない場合は、メールアドレスとパスワードを確認してください。"
      )
    ).toBeInTheDocument();

    expect(screen.getByText("詳細は参照元FAQをご確認ください。")).toBeInTheDocument();
    expect(screen.getByText("ログインできない場合の対応")).toBeInTheDocument();
    expect(screen.getByText("パスワード再設定手順")).toBeInTheDocument();
  });

  test("質問はtrimされてsearchAiに渡される", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "  CSV取込 エラー  "
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await waitFor(() => {
      expect(mockedSearchAi).toHaveBeenCalledWith("CSV取込 エラー");
    });
  });

  test("検索中はローディング表示とボタン無効化が行われる", async () => {
    const user = userEvent.setup();

    let resolveSearch: (value: typeof aiResult) => void;

    mockedSearchAi.mockReturnValue(
      new Promise((resolve) => {
        resolveSearch = resolve;
      })
    );

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    expect(screen.getByRole("button", { name: "AI回答を生成中..." })).toBeDisabled();
    expect(screen.getByText("AI回答を生成中です")).toBeInTheDocument();
    expect(
      screen.getByText("関連FAQを検索し、FAQ本文をもとに回答を生成しています。")
    ).toBeInTheDocument();

    resolveSearch!(aiResult);

    expect(
      await screen.findByText(
        "ログインできない場合は、メールアドレスとパスワードを確認してください。"
      )
    ).toBeInTheDocument();
  });

  test("AI検索に失敗した場合はエラーメッセージを表示する", async () => {
    const user = userEvent.setup();

    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedSearchAi.mockRejectedValue(new Error("AI API error"));

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "PDF出力 失敗"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    expect(await screen.findByText("実行エラー")).toBeInTheDocument();
    expect(screen.getByText("AI API error")).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("AI検索上限エラーの場合はconsole.warnを呼ぶ", async () => {
    const user = userEvent.setup();

    const consoleWarnMock = jest
      .spyOn(console, "warn")
      .mockImplementation(() => {});

    mockedSearchAi.mockRejectedValue(
      new Error("AI検索の実行回数が上限に達しました。1分ほど待ってから再実行してください。")
    );

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    expect(
      await screen.findByText(
        "AI検索の実行回数が上限に達しました。1分ほど待ってから再実行してください。"
      )
    ).toBeInTheDocument();

    expect(consoleWarnMock).toHaveBeenCalled();

    consoleWarnMock.mockRestore();
  });

  test("検索結果なしメッセージを表示する", async () => {
    const user = userEvent.setup();

    mockedSearchAi.mockResolvedValue({
      answer: null,
      disclaimer: null,
      message: "関連するFAQが見つかりませんでした。",
      aiHistoryId: 20,
      sources: [],
    });

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "存在しない質問"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    expect(await screen.findByText("検索結果なし")).toBeInTheDocument();
    expect(screen.getByText("関連するFAQが見つかりませんでした。")).toBeInTheDocument();

    const link = screen.getByRole("link", { name: "通常のFAQ検索で探す →" });
    expect(link).toHaveAttribute("href", "/faqs");
  });

  test("参照元FAQがない回答では参照元なしメッセージを表示する", async () => {
    const user = userEvent.setup();

    mockedSearchAi.mockResolvedValue({
      ...aiResult,
      sources: [],
    });

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    expect(
      await screen.findByText(
        "ログインできない場合は、メールアドレスとパスワードを確認してください。"
      )
    ).toBeInTheDocument();

    expect(
      screen.getByText("参照元FAQはありません。通常のFAQ検索もあわせて確認してください。")
    ).toBeInTheDocument();
  });

  test("参照元FAQリンクのhrefが設定される", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await screen.findByText("ログインできない場合の対応");

    const sourceLink = screen.getByRole("link", {
      name: /ログインできない場合の対応/i,
    });

    expect(sourceLink).toHaveAttribute("href", "/faqs/1");
  });

  test("役に立ったフィードバックを送信できる", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await screen.findByText("このAI回答は役に立ちましたか？");

    await user.click(screen.getByRole("button", { name: "役に立った" }));

    await waitFor(() => {
      expect(mockedSendAiSearchFeedback).toHaveBeenCalledWith(10, {
        isHelpful: true,
      });
    });

    expect(
      screen.getByText("フィードバックを送信しました：役に立った")
    ).toBeInTheDocument();
  });

  test("役に立たなかったフィードバックを送信できる", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await screen.findByText("このAI回答は役に立ちましたか？");

    await user.click(screen.getByRole("button", { name: "役に立たなかった" }));

    await waitFor(() => {
      expect(mockedSendAiSearchFeedback).toHaveBeenCalledWith(10, {
        isHelpful: false,
      });
    });

    expect(
      screen.getByText("フィードバックを送信しました：役に立たなかった")
    ).toBeInTheDocument();
  });

  test("フィードバック送信後はボタンが無効になる", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await screen.findByText("このAI回答は役に立ちましたか？");

    await user.click(screen.getByRole("button", { name: "役に立った" }));

    expect(
      await screen.findByText("フィードバックを送信しました：役に立った")
    ).toBeInTheDocument();

    expect(screen.getByRole("button", { name: "役に立った" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "役に立たなかった" })).toBeDisabled();
  });

  test("フィードバック送信に失敗した場合はメッセージを表示する", async () => {
    const user = userEvent.setup();

    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedSendAiSearchFeedback.mockRejectedValue(new Error("feedback error"));

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await screen.findByText("このAI回答は役に立ちましたか？");

    await user.click(screen.getByRole("button", { name: "役に立った" }));

    expect(
      await screen.findByText("フィードバックの送信に失敗しました。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("aiHistoryIdがない場合はフィードバック送信できない", async () => {
    const user = userEvent.setup();

    mockedSearchAi.mockResolvedValue({
      ...aiResult,
      aiHistoryId: 0,
    });

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await screen.findByText("このAI回答は役に立ちましたか？");

    await user.click(screen.getByRole("button", { name: "役に立った" }));

    expect(
      screen.getByText("AI検索履歴IDが取得できないため、送信できません。")
    ).toBeInTheDocument();

    expect(mockedSendAiSearchFeedback).not.toHaveBeenCalled();
  });

  test("新しい質問例を押すと前回の結果とフィードバックメッセージがクリアされる", async () => {
    const user = userEvent.setup();

    render(<AiSearchPage />);

    await user.type(
      screen.getByPlaceholderText("例：ログインできない場合はどうすればいいですか？"),
      "ログインできない"
    );

    await user.click(screen.getByRole("button", { name: "AI検索する" }));

    await screen.findByText("このAI回答は役に立ちましたか？");

    await user.click(screen.getByRole("button", { name: "役に立った" }));

    expect(
      await screen.findByText("フィードバックを送信しました：役に立った")
    ).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "CSV取込 エラー" }));

    expect(
      screen.queryByText(
        "ログインできない場合は、メールアドレスとパスワードを確認してください。"
      )
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText("フィードバックを送信しました：役に立った")
    ).not.toBeInTheDocument();
  });
});