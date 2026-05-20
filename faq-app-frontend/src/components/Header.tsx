"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { AUTH_CHANGED_EVENT, isLoggedIn, removeToken } from "@/lib/auth";
import { useEffect, useState } from "react";


export default function Header() {
  const router = useRouter();
  const [loggedIn, setLoggedIn] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

useEffect(() => {
  setLoggedIn(isLoggedIn());
}, []);

useEffect(() => {
  const handleAuthChange = () => {
    setLoggedIn(isLoggedIn());
  };

  window.addEventListener(AUTH_CHANGED_EVENT, handleAuthChange);

  return () => {
    window.removeEventListener(AUTH_CHANGED_EVENT, handleAuthChange);
  };
}, []);

  const handleLogout = () => {
    removeToken();
    setLoggedIn(false);
    setMenuOpen(false);
    setLogoutConfirmOpen(false);
    router.push("/");
  };

  const closeMenu = () => {
    setMenuOpen(false);
  };

  const [logoutConfirmOpen, setLogoutConfirmOpen] = useState(false);

  return (
    <header className="sticky top-0 z-50 border-b border-white/10 bg-slate-950/85 backdrop-blur">
      <div className="mx-auto max-w-6xl px-4">
        <div className="flex items-center justify-between py-4">
          <Link
            href="/"
            onClick={closeMenu}
            className="flex min-w-0 items-center gap-3"
          >
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-violet-500 font-bold text-white shadow-lg shadow-blue-500/20">
              Q
            </div>

            <div className="min-w-0">
              <div className="truncate text-base font-bold text-white sm:text-lg">
                Knowledge FAQ
              </div>
              <div className="truncate text-xs text-slate-400">
                社内ナレッジ検索
              </div>
            </div>
          </Link>

          {/* Desktop navigation */}
          <nav className="hidden items-center gap-2 text-sm md:flex">
            <Link
              href="/"
              className="rounded-lg px-3 py-2 text-slate-300 transition hover:bg-white/10 hover:text-white"
            >
              ホーム
            </Link>

            <Link
              href="/faqs"
              className="rounded-lg px-3 py-2 text-slate-300 transition hover:bg-white/10 hover:text-white"
            >
              FAQ検索
            </Link>

            <Link
              href="/ai-search"
              className="rounded-lg px-3 py-2 text-slate-300 transition hover:bg-white/10 hover:text-white"
            >
              AI検索
            </Link>

            {loggedIn && (
              <Link
                href="/admin"
                className="rounded-lg px-3 py-2 text-slate-300 transition hover:bg-white/10 hover:text-white"
              >
                管理画面
              </Link>
            )}

            {!loggedIn ? (
              <Link
                href="/login"
                className="ml-1 rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-4 py-2 font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
              >
                ログイン
              </Link>
            ) : (
<button
  type="button"
  onClick={() => setLogoutConfirmOpen(true)}
  className="ml-1 rounded-xl border border-white/10 bg-white/10 px-4 py-2 text-white transition hover:bg-white/15"
>
  ログアウト
</button>
            )}
          </nav>

          {/* Mobile menu button */}
          <button
            type="button"
            onClick={() => setMenuOpen((prev) => !prev)}
            className="inline-flex items-center justify-center rounded-xl border border-white/10 bg-white/10 px-3 py-2 text-sm font-semibold text-white transition hover:bg-white/15 md:hidden"
            aria-expanded={menuOpen}
            aria-label="メニューを開閉"
          >
            {menuOpen ? "閉じる" : "メニュー"}
          </button>
        </div>

        {/* Mobile navigation */}
        {menuOpen && (
          <nav className="space-y-2 border-t border-white/10 pb-4 pt-3 text-sm md:hidden">
            <Link
              href="/"
              onClick={closeMenu}
              className="block rounded-xl px-4 py-3 text-slate-300 transition hover:bg-white/10 hover:text-white"
            >
              ホーム
            </Link>

            <Link
              href="/faqs"
              onClick={closeMenu}
              className="block rounded-xl px-4 py-3 text-slate-300 transition hover:bg-white/10 hover:text-white"
            >
              FAQ検索
            </Link>

            <Link
              href="/ai-search"
              onClick={closeMenu}
              className="block rounded-xl px-4 py-3 text-slate-300 transition hover:bg-white/10 hover:text-white"
            >
              AI検索
            </Link>

            {loggedIn && (
              <Link
                href="/admin"
                onClick={closeMenu}
                className="block rounded-xl px-4 py-3 text-slate-300 transition hover:bg-white/10 hover:text-white"
              >
                管理画面
              </Link>
            )}

            <div className="pt-2">
              {!loggedIn ? (
                <Link
                  href="/login"
                  onClick={closeMenu}
                  className="block rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-4 py-3 text-center font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
                >
                  ログイン
                </Link>
              ) : (
<button
  type="button"
  onClick={() => setLogoutConfirmOpen(true)}
  className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 font-semibold text-white transition hover:bg-white/15"
>
  ログアウト
</button>
              )}
            </div>
          </nav>
        )}
      </div>
{logoutConfirmOpen && (
  <div className="fixed inset-0 z-[100] flex items-start justify-center bg-black/60 px-4 pt-32">
    <div className="w-full max-w-sm overflow-hidden rounded-3xl border border-white/10 bg-slate-900 shadow-2xl shadow-slate-950/60">
      <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

      <div className="p-6">
        <h2 className="text-lg font-bold text-white">
          ログアウトしますか？
        </h2>

        <p className="mt-3 text-sm leading-6 text-slate-300">
          管理画面からログアウトします。再度利用する場合は、もう一度ログインしてください。
        </p>

        <div className="mt-6 flex gap-3">
          <button
            type="button"
            onClick={() => setLogoutConfirmOpen(false)}
            className="flex-1 rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-sm font-semibold text-white transition hover:bg-white/15"
          >
            キャンセル
          </button>

          <button
            type="button"
            onClick={handleLogout}
            className="flex-1 rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
          >
            ログアウトする
          </button>
        </div>
      </div>
    </div>
  </div>
)}
    </header>
  );
}