import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import SearchBar from "./SearchBar";

describe("SearchBar", () => {
  test("検索フォームが表示される", () => {
    render(<SearchBar onSearch={jest.fn()} />);

    expect(
      screen.getByPlaceholderText("キーワードでFAQを検索")
    ).toBeInTheDocument();

    expect(screen.getByRole("button", { name: "検索" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "クリア" })).toBeInTheDocument();
  });

  test("キーワードを入力して検索すると onSearch が入力値付きで呼ばれる", async () => {
    const user = userEvent.setup();
    const onSearch = jest.fn();

    render(<SearchBar onSearch={onSearch} />);

    const input = screen.getByPlaceholderText("キーワードでFAQを検索");

    await user.type(input, "ログイン");
    await user.click(screen.getByRole("button", { name: "検索" }));

    expect(onSearch).toHaveBeenCalledTimes(1);
    expect(onSearch).toHaveBeenCalledWith("ログイン");
  });

  test("Enterキーで検索できる", async () => {
    const user = userEvent.setup();
    const onSearch = jest.fn();

    render(<SearchBar onSearch={onSearch} />);

    const input = screen.getByPlaceholderText("キーワードでFAQを検索");

    await user.type(input, "パスワード{enter}");

    expect(onSearch).toHaveBeenCalledTimes(1);
    expect(onSearch).toHaveBeenCalledWith("パスワード");
  });

  test("クリアを押すと入力欄が空になり onSearch に空文字が渡される", async () => {
    const user = userEvent.setup();
    const onSearch = jest.fn();

    render(<SearchBar onSearch={onSearch} />);

    const input = screen.getByPlaceholderText("キーワードでFAQを検索");

    await user.type(input, "エラー");
    expect(input).toHaveValue("エラー");

    await user.click(screen.getByRole("button", { name: "クリア" }));

    expect(input).toHaveValue("");
    expect(onSearch).toHaveBeenCalledTimes(1);
    expect(onSearch).toHaveBeenCalledWith("");
  });

  test("空欄のまま検索すると空文字で onSearch が呼ばれる", async () => {
    const user = userEvent.setup();
    const onSearch = jest.fn();

    render(<SearchBar onSearch={onSearch} />);

    await user.click(screen.getByRole("button", { name: "検索" }));

    expect(onSearch).toHaveBeenCalledTimes(1);
    expect(onSearch).toHaveBeenCalledWith("");
  });
});