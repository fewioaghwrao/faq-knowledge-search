import { render, screen } from "@testing-library/react";
import RootLayout, { metadata } from "./layout";

jest.mock("@/components/Header", () => {
  return function MockHeader() {
    return <header data-testid="mock-header">Header</header>;
  };
});

describe("RootLayout", () => {
  test("metadataが設定されている", () => {
    expect(metadata.title).toBe("社内FAQ検索アプリ");
    expect(metadata.description).toBe(
      "社内FAQ検索アプリです。AIを活用して社内ナレッジを簡単に検索できます。"
    );
  });

  test("Headerを表示する", () => {
    render(
      <RootLayout>
        <div>テスト本文</div>
      </RootLayout>
    );

    expect(screen.getByTestId("mock-header")).toBeInTheDocument();
  });

  test("childrenをmain内に表示する", () => {
    render(
      <RootLayout>
        <div>テスト本文</div>
      </RootLayout>
    );

    const main = screen.getByRole("main");

    expect(main).toBeInTheDocument();
    expect(main).toHaveTextContent("テスト本文");
  });

  test("mainにレイアウト用クラスが設定されている", () => {
    render(
      <RootLayout>
        <div>テスト本文</div>
      </RootLayout>
    );

    const main = screen.getByRole("main");

    expect(main).toHaveClass("mx-auto");
    expect(main).toHaveClass("max-w-6xl");
    expect(main).toHaveClass("px-4");
    expect(main).toHaveClass("py-8");
  });
});