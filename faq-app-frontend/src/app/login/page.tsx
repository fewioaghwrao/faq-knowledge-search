"use client";

import { loginApi } from "@/lib/api";
import { saveToken } from "@/lib/auth";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function LoginPage() {
  const router = useRouter();

  const [email, setEmail] = useState("admin@faq-app.local");
  const [password, setPassword] = useState("Password123!");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      setSubmitting(true);
      setError("");

      const result = await loginApi({
        email: email.trim(),
        password,
      });

      saveToken(result.accessToken);
      router.push("/admin");
      router.refresh();
    } catch (e) {
      setError(
        "ログインに失敗しました。メールアドレスまたはパスワードを確認してください。"
      );
      console.error(e);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="mx-auto max-w-md">
      <div className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-2xl shadow-slate-950/40">
        <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

        <div className="relative p-8">
          <div className="pointer-events-none absolute -right-24 -top-24 h-56 w-56 rounded-full bg-blue-500/20 blur-3xl" />
          <div className="pointer-events-none absolute -bottom-24 -left-24 h-56 w-56 rounded-full bg-violet-500/20 blur-3xl" />

          <div className="relative">
            <div className="mb-6 flex items-center gap-3">
              <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-violet-500 text-lg font-bold text-white shadow-lg shadow-blue-500/20">
                A
              </div>

              <div>
                <h1 className="text-2xl font-bold text-white">
                  管理者ログイン
                </h1>
                <p className="mt-1 text-sm text-slate-400">
                  FAQの登録・編集を行う管理画面にログインします。
                </p>
              </div>
            </div>

            <form onSubmit={handleLogin} className="space-y-5">
              <div>
                <label className="mb-2 block text-sm font-medium text-slate-300">
                  メールアドレス
                </label>
                <input
                  type="email"
                  className="w-full rounded-xl border border-white/10 bg-slate-800 px-4 py-3 text-white placeholder:text-slate-500 outline-none transition focus:border-blue-400 focus:ring-2 focus:ring-blue-500/20"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  autoComplete="email"
                  required
                />
              </div>

              <div>
                <label className="mb-2 block text-sm font-medium text-slate-300">
                  パスワード
                </label>

                <div className="relative">
                  <input
                    type={showPassword ? "text" : "password"}
                    className="w-full rounded-xl border border-white/10 bg-slate-800 px-4 py-3 pr-24 text-white placeholder:text-slate-500 outline-none transition focus:border-blue-400 focus:ring-2 focus:ring-blue-500/20"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    autoComplete="current-password"
                    required
                  />

                  <button
                    type="button"
                    onClick={() => setShowPassword((current) => !current)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 rounded-lg px-3 py-1 text-xs font-medium text-slate-300 transition hover:bg-white/10 hover:text-white"
                    aria-label={
                      showPassword
                        ? "パスワードを非表示にする"
                        : "パスワードを表示する"
                    }
                  >
                    {showPassword ? "非表示" : "表示"}
                  </button>
                </div>
              </div>

              {error && (
                <div className="overflow-hidden rounded-2xl border border-red-500/30 bg-red-950/50">
                  <div className="h-1 bg-gradient-to-r from-red-500 to-orange-400" />
                  <div className="p-4 text-sm text-red-100">{error}</div>
                </div>
              )}

              <button
                type="submit"
                disabled={submitting}
                className="w-full rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-4 py-3 font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {submitting ? "ログイン中..." : "ログイン"}
              </button>
            </form>

            <div className="mt-6 rounded-2xl border border-white/10 bg-white/5 p-4 text-xs text-slate-400">
              <div className="font-semibold text-slate-300">Demo Account</div>
              <div className="mt-1">admin@faq-app.local</div>
              <div>Password123!</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}