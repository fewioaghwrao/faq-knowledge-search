import Link from "next/link";

export default function NotFound() {
  return (
    <main className="min-h-[calc(100vh-80px)] bg-slate-950 px-4 py-10 text-white">
      <div className="mx-auto flex max-w-3xl items-center justify-center py-24">
        <div className="w-full overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-2xl shadow-slate-950/40">
          <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

          <div className="p-8">
            <div className="mb-5 inline-flex rounded-2xl border border-blue-500/30 bg-blue-500/10 px-4 py-2 text-sm font-semibold text-blue-100">
              404 Not Found
            </div>

            <h1 className="text-2xl font-bold text-white">
              ページが見つかりません
            </h1>

            <p className="mt-3 text-sm leading-6 text-slate-300">
              指定されたページは存在しないか、移動または削除された可能性があります。
            </p>

            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <Link
                href="/"
                className="rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-5 py-3 text-center text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
              >
                ホームへ戻る
              </Link>

              <Link
                href="/faqs"
                className="rounded-xl border border-white/10 bg-white/10 px-5 py-3 text-center text-sm font-semibold text-white transition hover:bg-white/15"
              >
                FAQ検索へ
              </Link>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}