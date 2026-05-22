"use client";

import Link from "next/link";

type ErrorPageProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function ErrorPage({ error, reset }: ErrorPageProps) {
  return (
    <main className="min-h-[calc(100vh-80px)] bg-slate-950 px-4 py-10 text-white">
      <div className="mx-auto flex max-w-3xl items-center justify-center py-24">
        <div className="w-full overflow-hidden rounded-3xl border border-red-500/20 bg-slate-900/80 shadow-2xl shadow-slate-950/40">
          <div className="h-1 bg-gradient-to-r from-red-500 via-orange-500 to-yellow-400" />

          <div className="p-8">
            <div className="mb-5 inline-flex rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-2 text-sm font-semibold text-red-100">
              Error
            </div>

            <h1 className="text-2xl font-bold text-white">
              エラーが発生しました
            </h1>

            <p className="mt-3 text-sm leading-6 text-slate-300">
              画面の表示中に問題が発生しました。再読み込みしても解消しない場合は、時間をおいて再度お試しください。
            </p>

            {process.env.NODE_ENV === "development" && (
              <div className="mt-6 rounded-2xl border border-white/10 bg-slate-950/80 p-4">
                <div className="text-xs font-semibold text-slate-400">
                  Development Error
                </div>
                <pre className="mt-2 max-h-40 overflow-auto whitespace-pre-wrap break-words text-xs text-red-200">
                  {error.message}
                </pre>
              </div>
            )}

            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <button
                type="button"
                onClick={reset}
                className="rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
              >
                再試行する
              </button>

              <Link
                href="/"
                className="rounded-xl border border-white/10 bg-white/10 px-5 py-3 text-center text-sm font-semibold text-white transition hover:bg-white/15"
              >
                ホームへ戻る
              </Link>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}