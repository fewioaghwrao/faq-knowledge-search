import Link from "next/link";

export default function HomePage() {
  return (
    <div className="space-y-8">
      <section className="relative overflow-hidden rounded-3xl border border-white/10 bg-slate-900 p-8 shadow-2xl shadow-slate-950/40">
        <div className="absolute inset-0 bg-gradient-to-br from-blue-600/20 via-violet-600/20 to-cyan-500/10" />
        <div className="absolute -right-24 -top-24 h-72 w-72 rounded-full bg-blue-500/20 blur-3xl" />
        <div className="absolute -bottom-24 -left-24 h-72 w-72 rounded-full bg-violet-500/20 blur-3xl" />

        <div className="relative">
          <span className="inline-flex rounded-full border border-blue-400/30 bg-blue-500/10 px-4 py-1 text-sm text-blue-200">
            FAQ Knowledge Search
          </span>

          <h1 className="mt-5 text-4xl font-bold tracking-tight text-white">
            社内ナレッジ検索アプリ
          </h1>

          <p className="mt-4 max-w-2xl text-slate-300">
            手順書・FAQ・障害対応メモを登録し、通常検索とAI検索の両方から必要な情報を探せるアプリです。
          </p>

          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <Link
              href="/faqs"
              className="rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-6 py-3 text-center font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
            >
              通常FAQ検索を使う
            </Link>

            <Link
              href="/ai-search"
              className="rounded-xl border border-white/10 bg-white/10 px-6 py-3 text-center font-semibold text-slate-100 transition hover:border-cyan-400/40 hover:bg-white/15"
            >
              AI FAQ検索を使う
            </Link>
          </div>
        </div>
      </section>

      <div className="grid gap-5 md:grid-cols-2">
        <Link
          href="/faqs"
          className="group rounded-3xl border border-white/10 bg-slate-900/80 p-6 shadow-xl shadow-slate-950/30 transition hover:-translate-y-0.5 hover:border-blue-400/60"
        >
          <div className="text-sm font-semibold text-blue-200">
            FAQ Search
          </div>

          <h2 className="mt-3 text-xl font-bold text-white group-hover:text-blue-200">
            通常FAQ検索
          </h2>

          <p className="mt-2 text-sm leading-6 text-slate-400">
            キーワード・カテゴリ・タグをもとに、登録済みFAQを検索します。
          </p>

          <div className="mt-5 text-sm font-semibold text-blue-300">
            検索画面へ →
          </div>
        </Link>

        <Link
          href="/ai-search"
          className="group rounded-3xl border border-white/10 bg-slate-900/80 p-6 shadow-xl shadow-slate-950/30 transition hover:-translate-y-0.5 hover:border-violet-400/60"
        >
          <div className="text-sm font-semibold text-violet-200">
            AI FAQ Search
          </div>

          <h2 className="mt-3 text-xl font-bold text-white group-hover:text-violet-200">
            AI FAQ検索
          </h2>

          <p className="mt-2 text-sm leading-6 text-slate-400">
            関連FAQの上位5件をもとに、外部AI APIで回答を生成し、参照元FAQも表示します。
          </p>

          <div className="mt-5 text-sm font-semibold text-violet-300">
            AI検索画面へ →
          </div>
        </Link>
      </div>
    </div>
  );
}