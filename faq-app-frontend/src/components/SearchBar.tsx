"use client";

import { FormEvent, useState } from "react";

type SearchBarProps = {
  onSearch: (keyword?: string) => void | Promise<void>;
};

export default function SearchBar({ onSearch }: SearchBarProps) {
  const [keyword, setKeyword] = useState("");

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    await onSearch(keyword);
  };

  const handleClear = async () => {
    setKeyword("");

    await onSearch("");
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-3xl border border-white/10 bg-slate-900/80 p-4 shadow-xl shadow-slate-950/30"
    >
      <div className="flex flex-col gap-3 sm:flex-row">
        <input
          type="text"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          placeholder="キーワードでFAQを検索"
          className="min-w-0 flex-1 rounded-xl border border-white/10 bg-slate-950/60 px-4 py-3 text-sm text-white placeholder:text-slate-500 focus:border-blue-400/60 focus:outline-none"
        />

        <button
          type="submit"
          className="rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
        >
          検索
        </button>

        <button
          type="button"
          onClick={handleClear}
          className="rounded-xl border border-white/10 bg-white/10 px-5 py-3 text-sm font-semibold text-slate-100 transition hover:bg-white/15"
        >
          クリア
        </button>
      </div>
    </form>
  );
}