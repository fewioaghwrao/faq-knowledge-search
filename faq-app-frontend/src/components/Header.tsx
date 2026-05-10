"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { isLoggedIn, removeToken } from "@/lib/auth";
import { useEffect, useState } from "react";

export default function Header() {
  const router = useRouter();
  const [loggedIn, setLoggedIn] = useState(false);

  useEffect(() => {
    setLoggedIn(isLoggedIn());
  }, []);

  const handleLogout = () => {
    removeToken();
    setLoggedIn(false);
    router.push("/");
  };

  return (
    <header className="sticky top-0 z-50 border-b border-white/10 bg-slate-950/80 backdrop-blur">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4">
        <Link href="/" className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-violet-500 font-bold text-white shadow-lg shadow-blue-500/20">
            Q
          </div>
          <div>
            <div className="text-lg font-bold text-white">
              Knowledge FAQ
            </div>
            <div className="text-xs text-slate-400">
              社内ナレッジ検索
            </div>
          </div>
        </Link>

        <nav className="flex items-center gap-3 text-sm">
          <Link href="/" className="rounded-lg px-3 py-2 text-slate-300 hover:bg-white/10 hover:text-white">
            FAQ一覧
          </Link>

          {loggedIn && (
            <Link href="/admin" className="rounded-lg px-3 py-2 text-slate-300 hover:bg-white/10 hover:text-white">
              管理画面
            </Link>
          )}

          <Link
  href="/ai-search"
  className="text-sm text-slate-300 transition hover:text-white"
>
  AI検索
</Link>

          {!loggedIn ? (
            <Link
              href="/login"
              className="rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-4 py-2 font-semibold text-white shadow-lg shadow-blue-500/20 hover:from-blue-500 hover:to-violet-500"
            >
              ログイン
            </Link>
          ) : (
            <button
              onClick={handleLogout}
              className="rounded-xl border border-white/10 bg-white/10 px-4 py-2 text-white hover:bg-white/15"
            >
              ログアウト
            </button>
          )}
        </nav>
      </div>
    </header>
  );
}