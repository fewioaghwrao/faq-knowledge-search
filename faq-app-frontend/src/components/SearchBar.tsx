"use client";

import { useState } from "react";

type Props = {
  onSearch: (keyword: string) => void;
};

export default function SearchBar({ onSearch }: Props) {
  const [keyword, setKeyword] = useState("");

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        onSearch(keyword);
      }}
      className="rounded-2xl border border-white/10 bg-slate-900/80 p-3 shadow-xl"
    >
      <div className="flex gap-3">
        <input
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          placeholder="例：ログインできない、請求書、エラー対応"
          className="w-full rounded-xl border border-white/10 bg-slate-800 px-4 py-3 text-white placeholder:text-slate-500 focus:border-blue-400 focus:outline-none"
        />

        <button
          type="submit"
          className="rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-6 py-3 font-semibold text-white shadow-lg shadow-blue-500/20 hover:from-blue-500 hover:to-violet-500"
        >
          検索
        </button>
      </div>
    </form>
  );
}