import { render, screen, waitFor } from "@testing-library/react";
import AdminAiHistoryDetailPage from "./page";
import { getAiSearchHistoryDetail } from "@/lib/api";

jest.mock("next/navigation", () => ({
  useParams: jest.fn(),
}));

jest.mock("@/lib/api", () => ({
  getAiSearchHistoryDetail: jest.fn(),
}));

import { useParams } from "next/navigation";

const mockedUseParams = useParams as jest.Mock;
const mockedGetAiSearchHistoryDetail = getAiSearchHistoryDetail as jest.Mock;

const historyDetail = {
  id: 10,
  question: "ログインできない場合は？",
  searchKeywords: "ログイン パスワード",
  aiAnswer: "メールアドレスとパスワードを確認してください。",
  isSuccess: true,
  errorMessage: null,
  isHelpful: true,
  executedAt: "2026-05-23T10:30:00",
  sources: [
    {
      faqId: 1,
      faqTitle: "ログインできない場合の対処",
      displayOrder: 2,
      score: 80,
      url: "/faqs/1",
    },
    {
      faqId: 2,
      faqTitle: "パスワード再設定手順",
      displayOrder: 1,
      score: 95,
      url: "/faqs/2",
    },
  ],
};

function expectAtLeastOne(elements: HTMLElement[]) {
  expect(elements.length).toBeGreaterThan(0);
}

describe("AdminAiHistoryDetailPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseParams.mockReturnValue({ id: "10" });
  });

  test("初期表示でAI検索履歴詳細を取得する", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue(historyDetail);

    render(<AdminAiHistoryDetailPage />);

    expect(
      screen.getByText("AI検索履歴詳細を読み込み中です...")
    ).toBeInTheDocument();

    await waitFor(() => {
      expect(mockedGetAiSearchHistoryDetail).toHaveBeenCalledWith(10);
    });
  });

  test("取得したAI検索履歴詳細が表示される", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue(historyDetail);

    render(<AdminAiHistoryDetailPage />);

    expect(
      await screen.findByText("ログインできない場合は？")
    ).toBeInTheDocument();

    expect(screen.getByText("ID: 10")).toBeInTheDocument();
    expect(screen.getByText("成功")).toBeInTheDocument();
    expect(screen.getByText("参照FAQ 2件")).toBeInTheDocument();
    expect(screen.getByText("ログイン パスワード")).toBeInTheDocument();

    expect(
      screen.getByText("メールアドレスとパスワードを確認してください。")
    ).toBeInTheDocument();

    expect(screen.getByText("役に立った 👍")).toBeInTheDocument();
  });

  test("一覧へ戻るリンクが表示される", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue(historyDetail);

    render(<AdminAiHistoryDetailPage />);

    const link = screen.getByRole("link", { name: "一覧へ戻る" });

    expect(link).toHaveAttribute("href", "/admin/ai-histories");

    await screen.findByText("ログインできない場合は？");
  });

  test("参照元FAQが表示される", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue(historyDetail);

    render(<AdminAiHistoryDetailPage />);

    expect(
      await screen.findByText("ログインできない場合の対処")
    ).toBeInTheDocument();

    expect(screen.getByText("パスワード再設定手順")).toBeInTheDocument();

    expectAtLeastOne(screen.getAllByText("FAQを見る →"));
    expect(screen.getByText("FAQ ID: 1")).toBeInTheDocument();
    expect(screen.getByText("FAQ ID: 2")).toBeInTheDocument();
    expect(screen.getByText("Score: 80")).toBeInTheDocument();
    expect(screen.getByText("Score: 95")).toBeInTheDocument();
  });

  test("参照元FAQはdisplayOrder順に表示される", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue(historyDetail);

    render(<AdminAiHistoryDetailPage />);

    await screen.findByText("ログインできない場合の対処");

    const sourceTitles = [
      screen.getByText("パスワード再設定手順"),
      screen.getByText("ログインできない場合の対処"),
    ];

    expect(sourceTitles[0]).toBeInTheDocument();
    expect(sourceTitles[1]).toBeInTheDocument();
  });

  test("参照元FAQリンクのhrefが設定される", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue(historyDetail);

    render(<AdminAiHistoryDetailPage />);

    await screen.findByText("ログインできない場合の対処");

    const links = screen.getAllByRole("link", { name: /FAQを見る/i });

    expect(links[0]).toHaveAttribute("href", "/faqs/2");
    expect(links[1]).toHaveAttribute("href", "/faqs/1");
  });

  test("urlが空の場合は /faqs/{faqId} へのリンクになる", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue({
      ...historyDetail,
      sources: [
        {
          faqId: 99,
          faqTitle: "URLなしFAQ",
          displayOrder: 1,
          score: null,
          url: "",
        },
      ],
    });

    render(<AdminAiHistoryDetailPage />);

    await screen.findByText("URLなしFAQ");

    const link = screen.getByRole("link", { name: /FAQを見る/i });

    expect(link).toHaveAttribute("href", "/faqs/99");
  });

  test("AI回答がない場合は未保存メッセージを表示する", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue({
      ...historyDetail,
      aiAnswer: null,
    });

    render(<AdminAiHistoryDetailPage />);

    expect(
      await screen.findByText("AI回答は保存されていません。")
    ).toBeInTheDocument();
  });

  test("検索キーワードがない場合は検索キーワード欄を表示しない", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue({
      ...historyDetail,
      searchKeywords: null,
    });

    render(<AdminAiHistoryDetailPage />);

    await screen.findByText("ログインできない場合は？");

    expect(screen.queryByText("検索キーワード")).not.toBeInTheDocument();
  });

  test("エラー内容がある場合は表示する", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue({
      ...historyDetail,
      isSuccess: false,
      errorMessage: "AI API error",
      aiAnswer: null,
      isHelpful: false,
      sources: [],
    });

    render(<AdminAiHistoryDetailPage />);

    expect(await screen.findByText("失敗")).toBeInTheDocument();
    expect(screen.getByText("AI API error")).toBeInTheDocument();
    expect(screen.getByText("役に立たなかった 👎")).toBeInTheDocument();
    expect(screen.getByText("参照元FAQはありません。")).toBeInTheDocument();
  });

  test("未評価の場合は未評価と表示する", async () => {
    mockedGetAiSearchHistoryDetail.mockResolvedValue({
      ...historyDetail,
      isHelpful: null,
    });

    render(<AdminAiHistoryDetailPage />);

    expect(await screen.findByText("未評価")).toBeInTheDocument();
  });

  test("IDが不正な場合はエラーメッセージを表示し、APIを呼ばない", async () => {
    mockedUseParams.mockReturnValue({ id: "abc" });

    render(<AdminAiHistoryDetailPage />);

    expect(
      await screen.findByText("AI検索履歴IDが不正です。")
    ).toBeInTheDocument();

    expect(mockedGetAiSearchHistoryDetail).not.toHaveBeenCalled();
  });

  test("IDが0以下の場合はエラーメッセージを表示し、APIを呼ばない", async () => {
    mockedUseParams.mockReturnValue({ id: "0" });

    render(<AdminAiHistoryDetailPage />);

    expect(
      await screen.findByText("AI検索履歴IDが不正です。")
    ).toBeInTheDocument();

    expect(mockedGetAiSearchHistoryDetail).not.toHaveBeenCalled();
  });

  test("詳細取得に失敗した場合はエラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedGetAiSearchHistoryDetail.mockRejectedValue(new Error("API error"));

    render(<AdminAiHistoryDetailPage />);

    expect(
      await screen.findByText("AI検索履歴の詳細取得に失敗しました。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });
});