"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { getAiSearchHistories } from "@/lib/api";
import type { AiSearchHistoryListItem } from "@/types/ai";

export default function AdminAiHistoriesPage() {
  const [histories, setHistories] = useState<AiSearchHistoryListItem[]>([]);
  const [keyword, setKeyword] = useState("");
  const [isSuccess, setIsSuccess] = useState("");
  const [isHelpful, setIsHelpful] = useState("");
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);

  const pageSize = 5;

  const queryParams = useMemo(
    () => ({
      keyword,
      isSuccess,
      isHelpful,
      page: currentPage.toString(),
      pageSize: pageSize.toString(),
    }),
    [keyword, isSuccess, isHelpful, currentPage]
  );

  useEffect(() => {
    setCurrentPage(1);
  }, [keyword, isSuccess, isHelpful]);

  useEffect(() => {
    const loadHistories = async () => {
      setLoading(true);
      setErrorMessage(null);

      try {
        const result = await getAiSearchHistories(queryParams);
        setHistories(result);
      } catch (error) {
        console.error(error);
        setErrorMessage("AI検索履歴の取得に失敗しました。");
      } finally {
        setLoading(false);
      }
    };

    loadHistories();
  }, [queryParams]);

  return (
    <main className="mx-auto max-w-6xl px-4 py-8">
      <div className="mb-6 flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-semibold text-cyan-300">Admin</p>
          <h1 className="mt-1 text-2xl font-bold text-white">
            AI検索履歴
          </h1>
          <p className="mt-2 text-sm text-slate-400">
            AI検索で実行された質問、成功/失敗、参照FAQ件数を確認できます。
          </p>
        </div>

        <Link
          href="/admin"
          className="inline-flex items-center justify-center rounded-xl border border-white/10 bg-white/10 px-4 py-2 text-sm font-medium text-slate-100 transition hover:bg-white/15"
        >
          管理トップへ戻る
        </Link>
      </div>

      <section className="mb-6 rounded-3xl border border-white/10 bg-slate-900/80 p-4 shadow-xl shadow-slate-950/30">
        <div className="grid gap-4 md:grid-cols-3">
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-300">
              キーワード
            </label>
            <input
              type="text"
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="質問・回答・エラー内容で検索"
              className="w-full rounded-xl border border-white/10 bg-slate-950/60 px-3 py-2 text-sm text-white placeholder:text-slate-500 focus:border-cyan-400/60 focus:outline-none"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-300">
              成功/失敗
            </label>
            <select
              value={isSuccess}
              onChange={(e) => setIsSuccess(e.target.value)}
              className="w-full rounded-xl border border-white/10 bg-slate-950/60 px-3 py-2 text-sm text-white focus:border-cyan-400/60 focus:outline-none"
            >
              <option value="">すべて</option>
              <option value="true">成功</option>
              <option value="false">失敗</option>
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-300">
              フィードバック
            </label>
            <select
              value={isHelpful}
              onChange={(e) => setIsHelpful(e.target.value)}
              className="w-full rounded-xl border border-white/10 bg-slate-950/60 px-3 py-2 text-sm text-white focus:border-cyan-400/60 focus:outline-none"
            >
              <option value="">すべて</option>
              <option value="true">役に立った</option>
              <option value="false">役に立たなかった</option>
            </select>
          </div>
        </div>
      </section>

      {loading && (
        <div className="rounded-3xl border border-white/10 bg-slate-900/80 p-6 text-sm text-slate-300 shadow-xl shadow-slate-950/30">
          AI検索履歴を読み込み中です...
        </div>
      )}

      {errorMessage && (
        <div className="rounded-3xl border border-red-500/30 bg-red-950/50 p-6 text-sm text-red-100">
          {errorMessage}
        </div>
      )}

      {!loading && !errorMessage && histories.length === 0 && (
        <div className="rounded-3xl border border-white/10 bg-slate-900/80 p-8 text-center text-sm text-slate-300 shadow-xl shadow-slate-950/30">
          AI検索履歴はまだありません。
        </div>
      )}

      {!loading && !errorMessage && histories.length > 0 && (
        <>
          {/* Mobile / Tablet: card layout */}
          <div className="grid gap-4 lg:hidden">
            {histories.map((history) => (
              <article
                key={history.id}
                className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30"
              >
                <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

                <div className="space-y-4 p-4">
                  <div className="flex flex-wrap items-center gap-2">
                    {history.isSuccess ? (
                      <span className="rounded-full border border-emerald-400/30 bg-emerald-500/10 px-3 py-1 text-xs text-emerald-300">
                        成功
                      </span>
                    ) : (
                      <span className="rounded-full border border-red-400/30 bg-red-500/10 px-3 py-1 text-xs text-red-300">
                        失敗
                      </span>
                    )}

                    <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-300">
                      参照FAQ: {history.sourceCount}件
                    </span>

                    <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-300">
                      FB: {formatHelpful(history.isHelpful)}
                    </span>
                  </div>

                  <div>
                    <div className="text-xs text-slate-500">
                      {formatDateTime(history.executedAt)}
                    </div>

                    <h2 className="mt-3 break-words text-base font-bold leading-7 text-white">
                      {history.question}
                    </h2>

                    {history.answerPreview ? (
                      <p className="mt-2 line-clamp-4 break-words text-sm leading-6 text-slate-400">
                        {history.answerPreview}
                      </p>
                    ) : history.errorMessage ? (
                      <p className="mt-2 line-clamp-4 break-words text-sm leading-6 text-red-300">
                        {history.errorMessage}
                      </p>
                    ) : (
                      <p className="mt-2 text-sm text-slate-500">-</p>
                    )}
                  </div>

                  <div className="border-t border-white/10 pt-4">
                    <Link
                      href={`/admin/ai-histories/${history.id}`}
                      className="inline-flex w-full items-center justify-center rounded-xl border border-cyan-400/30 bg-cyan-500/10 px-4 py-2 text-sm font-semibold text-cyan-200 transition hover:border-cyan-300/60 hover:bg-cyan-500/20"
                    >
                      詳細を見る
                    </Link>
                  </div>
                </div>
              </article>
            ))}
          </div>

          {/* Desktop: table layout */}
          <section className="hidden overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30 lg:block">
            <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

            <div className="overflow-x-auto">
              <table className="min-w-full table-fixed text-left text-sm">
                <thead className="bg-white/5 text-slate-400">
                  <tr>
                    <th className="w-44 px-4 py-3 font-medium">実行日時</th>
                    <th className="w-64 px-4 py-3 font-medium">質問</th>
                    <th className="px-4 py-3 font-medium">回答プレビュー</th>
                    <th className="w-24 px-4 py-3 font-medium">結果</th>
                    <th className="w-24 px-4 py-3 font-medium">参照FAQ</th>
                    <th className="w-20 px-4 py-3 font-medium">FB</th>
                    <th className="w-24 px-4 py-3 font-medium">詳細</th>
                  </tr>
                </thead>

                <tbody>
                  {histories.map((history) => (
                    <tr
                      key={history.id}
                      className="border-t border-white/10 transition hover:bg-white/[0.03]"
                    >
                      <td className="px-4 py-4 align-top text-slate-400">
                        {formatDateTime(history.executedAt)}
                      </td>

                      <td className="px-4 py-4 align-top">
                        <p className="line-clamp-2 break-words font-semibold leading-6 text-white">
                          {history.question}
                        </p>
                      </td>

                      <td className="px-4 py-4 align-top text-slate-400">
                        {history.answerPreview ? (
                          <p className="line-clamp-2 break-words leading-6">
                            {history.answerPreview}
                          </p>
                        ) : history.errorMessage ? (
                          <p className="line-clamp-2 break-words leading-6 text-red-300">
                            {history.errorMessage}
                          </p>
                        ) : (
                          <span className="text-slate-500">-</span>
                        )}
                      </td>

                      <td className="px-4 py-4 align-top">
                        {history.isSuccess ? (
                          <span className="rounded-full border border-emerald-400/30 bg-emerald-500/10 px-3 py-1 text-xs text-emerald-300">
                            成功
                          </span>
                        ) : (
                          <span className="rounded-full border border-red-400/30 bg-red-500/10 px-3 py-1 text-xs text-red-300">
                            失敗
                          </span>
                        )}
                      </td>

                      <td className="px-4 py-4 align-top text-slate-300">
                        {history.sourceCount}件
                      </td>

                      <td className="px-4 py-4 align-top text-slate-300">
                        {formatHelpful(history.isHelpful)}
                      </td>

                      <td className="px-4 py-4 align-top">
                        <Link
                          href={`/admin/ai-histories/${history.id}`}
                          className="rounded-lg border border-cyan-400/30 bg-cyan-500/10 px-3 py-2 text-xs font-semibold text-cyan-200 transition hover:border-cyan-300/60 hover:bg-cyan-500/20"
                        >
                          詳細
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <div className="mt-6 flex items-center justify-center gap-3">
            <button
              type="button"
              onClick={() => setCurrentPage((page) => Math.max(1, page - 1))}
              disabled={currentPage === 1 || loading}
              className="rounded-full border border-white/10 bg-slate-900 px-4 py-2 text-sm text-slate-200 transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-slate-900"
            >
              前へ
            </button>

            <span className="text-sm text-slate-400">
              {currentPage}ページ目
            </span>

            <button
              type="button"
              onClick={() => setCurrentPage((page) => page + 1)}
              disabled={histories.length < pageSize || loading}
              className="rounded-full border border-white/10 bg-slate-900 px-4 py-2 text-sm text-slate-200 transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-slate-900"
            >
              次へ
            </button>
          </div>
        </>
      )}
    </main>
  );
}

function formatDateTime(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function formatHelpful(value: boolean | null | undefined) {
  if (value === true) return "👍";
  if (value === false) return "👎";
  return "-";
}