import { render, screen, waitFor } from "@testing-library/react";
import FaqDetailPage from "./page";
import { getFaqById } from "@/lib/api";
import { useParams } from "next/navigation";

jest.mock("react-markdown", () => ({
  __esModule: true,
  default: ({ children }: { children: string }) => <div>{children}</div>,
}));

jest.mock("@/lib/api", () => ({
  getFaqById: jest.fn(),
}));

jest.mock("next/navigation", () => ({
  useParams: jest.fn(),
}));

const mockedGetFaqById = getFaqById as jest.Mock;
const mockedUseParams = useParams as jest.Mock;

const faq = {
  id: 1,
  title: "ログインできない場合の対応",
  titleHighlighted: null,
  body: "## 対応手順\n\nメールアドレスとパスワードを確認してください。",
  bodyExcerpt: null,
  categoryName: "ログイン",
  tags: ["初期対応", "FAQ"],
  isPublished: true,
  viewCount: 12,
  updatedAt: "2026-05-23T10:30:00",
  score: 0,
};

describe("FaqDetailPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedUseParams.mockReturnValue({
      id: "1",
    });

    mockedGetFaqById.mockResolvedValue(faq);
  });

  test("初期表示で読み込み中を表示する", () => {
    render(<FaqDetailPage />);

    expect(screen.getByText("読み込み中...")).toBeInTheDocument();
  });

  test("URLパラメータのidでFAQ詳細を取得する", async () => {
    render(<FaqDetailPage />);

    await waitFor(() => {
      expect(mockedGetFaqById).toHaveBeenCalledWith(1);
    });
  });

test("FAQ詳細が表示される", async () => {
  render(<FaqDetailPage />);

  expect(
    await screen.findByText("ログインできない場合の対応")
  ).toBeInTheDocument();

  expect(screen.getByText("ログイン")).toBeInTheDocument();
  expect(screen.getByText("#初期対応")).toBeInTheDocument();
  expect(screen.getByText("#FAQ")).toBeInTheDocument();
  expect(screen.getByText("閲覧数: 12")).toBeInTheDocument();

  expect(
    screen.getAllByText((_, element) => {
      const text = element?.textContent ?? "";
      return (
        text.includes("対応手順") &&
        text.includes("メールアドレスとパスワードを確認してください。")
      );
    }).length
  ).toBeGreaterThan(0);
});

  test("FAQ一覧へ戻るリンクが表示される", async () => {
    render(<FaqDetailPage />);

    await screen.findByText("ログインできない場合の対応");

    const link = screen.getByRole("link", { name: "← FAQ一覧へ戻る" });

    expect(link).toHaveAttribute("href", "/");
  });

  test("タグが0件でも詳細を表示できる", async () => {
    mockedGetFaqById.mockResolvedValue({
      ...faq,
      tags: [],
    });

    render(<FaqDetailPage />);

    expect(
      await screen.findByText("ログインできない場合の対応")
    ).toBeInTheDocument();

    expect(screen.queryByText("#初期対応")).not.toBeInTheDocument();
    expect(screen.queryByText("#FAQ")).not.toBeInTheDocument();
  });

  test("FAQ詳細取得に失敗した場合はエラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedGetFaqById.mockRejectedValue(new Error("API error"));

    render(<FaqDetailPage />);

    expect(
      await screen.findByText("FAQ詳細の取得に失敗しました。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("idは数値に変換されてgetFaqByIdに渡される", async () => {
    mockedUseParams.mockReturnValue({
      id: "123",
    });

    render(<FaqDetailPage />);

    await waitFor(() => {
      expect(mockedGetFaqById).toHaveBeenCalledWith(123);
    });
  });
});