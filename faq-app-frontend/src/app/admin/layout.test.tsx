import { render, screen, waitFor } from "@testing-library/react";
import AdminLayout from "./layout";
import { isLoggedIn } from "@/lib/auth";
import { useRouter } from "next/navigation";

const pushMock = jest.fn();

jest.mock("next/navigation", () => ({
  useRouter: jest.fn(),
}));

jest.mock("@/lib/auth", () => ({
  isLoggedIn: jest.fn(),
}));

const mockedUseRouter = useRouter as jest.Mock;
const mockedIsLoggedIn = isLoggedIn as jest.Mock;

describe("AdminLayout", () => {
  beforeEach(() => {
    jest.clearAllMocks();

    mockedUseRouter.mockReturnValue({
      push: pushMock,
    });

    mockedIsLoggedIn.mockReturnValue(true);
  });

  test("管理画面レイアウトの基本表示が表示される", () => {
    render(
      <AdminLayout>
        <div>子コンテンツ</div>
      </AdminLayout>
    );

    expect(screen.getByText("Admin Console")).toBeInTheDocument();
    expect(screen.getByText("FAQ管理画面")).toBeInTheDocument();
    expect(
      screen.getByText("社内FAQの登録・編集・削除を行います。")
    ).toBeInTheDocument();

    expect(screen.getByText("子コンテンツ")).toBeInTheDocument();
  });

  test("管理画面内リンクが表示される", () => {
    render(
      <AdminLayout>
        <div>子コンテンツ</div>
      </AdminLayout>
    );

    expect(screen.getByRole("link", { name: "FAQ管理" })).toHaveAttribute(
      "href",
      "/admin"
    );

    expect(screen.getByRole("link", { name: "新規登録" })).toHaveAttribute(
      "href",
      "/admin/faqs/new"
    );

    expect(screen.getByRole("link", { name: "ユーザー管理" })).toHaveAttribute(
      "href",
      "/admin/users"
    );
  });

  test("ログイン済みの場合はログイン画面へリダイレクトしない", async () => {
    mockedIsLoggedIn.mockReturnValue(true);

    render(
      <AdminLayout>
        <div>子コンテンツ</div>
      </AdminLayout>
    );

    await waitFor(() => {
      expect(mockedIsLoggedIn).toHaveBeenCalledTimes(1);
    });

    expect(pushMock).not.toHaveBeenCalled();
  });

  test("未ログインの場合はログイン画面へリダイレクトする", async () => {
    mockedIsLoggedIn.mockReturnValue(false);

    render(
      <AdminLayout>
        <div>子コンテンツ</div>
      </AdminLayout>
    );

    await waitFor(() => {
      expect(pushMock).toHaveBeenCalledWith("/login");
    });
  });
});