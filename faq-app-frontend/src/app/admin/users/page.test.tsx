import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import AdminUsersPage from "./page";
import { getUsers, updateUserStatus } from "@/lib/api";

jest.mock("@/lib/api", () => ({
  getUsers: jest.fn(),
  updateUserStatus: jest.fn(),
}));

const mockedGetUsers = getUsers as jest.Mock;
const mockedUpdateUserStatus = updateUserStatus as jest.Mock;

const users = [
  {
    id: "admin-1",
    email: "admin@example.com",
    displayName: "管理者ユーザー",
    role: "Admin",
    isActive: true,
    createdAt: "2026-05-23T10:30:00",
  },
  {
    id: "editor-1",
    email: "editor@example.com",
    displayName: "編集者ユーザー",
    role: "Editor",
    isActive: true,
    createdAt: "2026-05-23T11:00:00",
  },
  {
    id: "user-1",
    email: "user@example.com",
    displayName: "",
    role: "User",
    isActive: false,
    createdAt: "2026-05-23T12:00:00",
  },
];

function expectAtLeastOne(elements: HTMLElement[]) {
  expect(elements.length).toBeGreaterThan(0);
}

describe("AdminUsersPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedGetUsers.mockResolvedValue(users);
    mockedUpdateUserStatus.mockResolvedValue(undefined);

    window.alert = jest.fn();
  });

  test("初期表示でユーザー一覧を取得する", async () => {
    render(<AdminUsersPage />);

    expect(screen.getByText("ユーザー情報を読み込み中...")).toBeInTheDocument();

    await waitFor(() => {
      expect(mockedGetUsers).toHaveBeenCalledTimes(1);
    });
  });

  test("取得したユーザー一覧が表示される", async () => {
    render(<AdminUsersPage />);

    expectAtLeastOne(await screen.findAllByText("管理者ユーザー"));
    expectAtLeastOne(screen.getAllByText("編集者ユーザー"));
    expectAtLeastOne(screen.getAllByText("admin@example.com"));
    expectAtLeastOne(screen.getAllByText("editor@example.com"));
    expectAtLeastOne(screen.getAllByText("User Management"));
    expect(screen.getByText("ユーザー管理")).toBeInTheDocument();
  });

test("登録ユーザー数が表示される", async () => {
  render(<AdminUsersPage />);

  await screen.findAllByText("管理者ユーザー");

  expect(
    screen.getByText((_, element) => {
      const text = element?.textContent?.replace(/\s+/g, "");
      return text === "登録ユーザー数3件";
    })
  ).toBeInTheDocument();
});

  test("表示名が空の場合は未設定と表示される", async () => {
    render(<AdminUsersPage />);

    expectAtLeastOne(await screen.findAllByText("未設定"));
  });

  test("ユーザーが0件の場合は空メッセージを表示する", async () => {
    mockedGetUsers.mockResolvedValue([]);

    render(<AdminUsersPage />);

    expect(
      await screen.findByText("表示できるユーザーがありません。")
    ).toBeInTheDocument();
  });

  test("ユーザー一覧取得に失敗した場合はエラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedGetUsers.mockRejectedValue(new Error("ユーザー一覧の取得に失敗しました。"));

    render(<AdminUsersPage />);

    expect(
      await screen.findByText("ユーザー一覧の取得に失敗しました。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("非Errorの例外の場合は汎用エラーメッセージを表示する", async () => {
    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedGetUsers.mockRejectedValue("unknown error");

    render(<AdminUsersPage />);

    expect(
      await screen.findByText("ユーザー一覧の取得に失敗しました。")
    ).toBeInTheDocument();

    consoleErrorMock.mockRestore();
  });

  test("Adminユーザーの状態はボタンではなくspan表示になる", async () => {
    render(<AdminUsersPage />);

    await screen.findAllByText("管理者ユーザー");

    const adminStatusLabels = screen.getAllByTitle(
      "デモ環境のため、Adminユーザーの有効/無効は変更できません。"
    );

    expect(adminStatusLabels.length).toBeGreaterThan(0);
  });

  test("Editor/Userユーザーの状態ボタンを押すと有効/無効を更新する", async () => {
    const user = userEvent.setup();

    render(<AdminUsersPage />);

    await screen.findAllByText("編集者ユーザー");

    const statusButtons = screen.getAllByRole("button", { name: "有効" });

    await user.click(statusButtons[0]);

    await waitFor(() => {
      expect(mockedUpdateUserStatus).toHaveBeenCalledWith("editor-1", {
        isActive: false,
      });
    });

    await waitFor(() => {
      expect(mockedGetUsers).toHaveBeenCalledTimes(2);
    });
  });

  test("無効ユーザーの状態ボタンを押すと有効化する", async () => {
    const user = userEvent.setup();

    render(<AdminUsersPage />);

    await screen.findAllByText("未設定");

    const disabledButtons = screen.getAllByRole("button", { name: "無効" });

    await user.click(disabledButtons[0]);

    await waitFor(() => {
      expect(mockedUpdateUserStatus).toHaveBeenCalledWith("user-1", {
        isActive: true,
      });
    });
  });

  test("状態更新に失敗した場合はalertを表示する", async () => {
    const user = userEvent.setup();

    const consoleErrorMock = jest
      .spyOn(console, "error")
      .mockImplementation(() => {});

    mockedUpdateUserStatus.mockRejectedValue(new Error("API error"));

    render(<AdminUsersPage />);

    await screen.findAllByText("編集者ユーザー");

    const statusButtons = screen.getAllByRole("button", { name: "有効" });

    await user.click(statusButtons[0]);

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("有効/無効の変更に失敗しました。");
    });

    consoleErrorMock.mockRestore();
  });

  test("6件以上の場合はページングが表示される", async () => {
    const manyUsers = Array.from({ length: 6 }, (_, index) => ({
      id: `user-${index + 1}`,
      email: `user${index + 1}@example.com`,
      displayName: `ユーザー${index + 1}`,
      role: "User",
      isActive: true,
      createdAt: "2026-05-23T10:30:00",
    }));

    mockedGetUsers.mockResolvedValue(manyUsers);

    render(<AdminUsersPage />);

    expectAtLeastOne(await screen.findAllByText("ユーザー1"));

    expect(screen.getByText("1 / 2")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "前へ" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "次へ" })).toBeEnabled();
  });

  test("次へボタンで2ページ目を表示する", async () => {
    const user = userEvent.setup();

    const manyUsers = Array.from({ length: 6 }, (_, index) => ({
      id: `user-${index + 1}`,
      email: `user${index + 1}@example.com`,
      displayName: `ユーザー${index + 1}`,
      role: "User",
      isActive: true,
      createdAt: "2026-05-23T10:30:00",
    }));

    mockedGetUsers.mockResolvedValue(manyUsers);

    render(<AdminUsersPage />);

    await screen.findAllByText("ユーザー1");

    await user.click(screen.getByRole("button", { name: "次へ" }));

    expect(screen.getByText("2 / 2")).toBeInTheDocument();
    expectAtLeastOne(screen.getAllByText("ユーザー6"));
    expect(screen.getByRole("button", { name: "次へ" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "前へ" })).toBeEnabled();
  });

  test("前へボタンで1ページ目に戻る", async () => {
    const user = userEvent.setup();

    const manyUsers = Array.from({ length: 6 }, (_, index) => ({
      id: `user-${index + 1}`,
      email: `user${index + 1}@example.com`,
      displayName: `ユーザー${index + 1}`,
      role: "User",
      isActive: true,
      createdAt: "2026-05-23T10:30:00",
    }));

    mockedGetUsers.mockResolvedValue(manyUsers);

    render(<AdminUsersPage />);

    await screen.findAllByText("ユーザー1");

    await user.click(screen.getByRole("button", { name: "次へ" }));
    await user.click(screen.getByRole("button", { name: "前へ" }));

    expect(screen.getByText("1 / 2")).toBeInTheDocument();
    expectAtLeastOne(screen.getAllByText("ユーザー1"));
  });

  test("5件以下の場合はページングを表示しない", async () => {
    render(<AdminUsersPage />);

    await screen.findAllByText("管理者ユーザー");

    expect(screen.queryByRole("button", { name: "前へ" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "次へ" })).not.toBeInTheDocument();
  });

  test("注意書きが表示される", async () => {
    render(<AdminUsersPage />);

    await screen.findAllByText("管理者ユーザー");

    expect(
      screen.getByText(
        "※ デモ環境のため、ロール変更は無効化しています。Adminユーザーの有効/無効も変更できません。"
      )
    ).toBeInTheDocument();
  });
});