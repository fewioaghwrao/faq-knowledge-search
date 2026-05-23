import { render, screen } from "@testing-library/react";
import AdminLoading from "./loading";

describe("AdminLoading", () => {
  test("管理画面用のローディング文言が表示される", () => {
    render(<AdminLoading />);

    expect(screen.getByText("管理画面を読み込み中...")).toBeInTheDocument();
    expect(
      screen.getByText("FAQ・ユーザー情報を取得しています。")
    ).toBeInTheDocument();
  });
});