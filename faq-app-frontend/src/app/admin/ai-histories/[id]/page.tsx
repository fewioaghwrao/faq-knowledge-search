"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { getAiSearchHistoryDetail } from "@/lib/api";
import type { AiSearchHistoryDetail } from "@/types/ai";

export default function AdminAiHistoryDetailPage() {
  const params = useParams();
  const id = Number(params.id);

  const [history, setHistory] = useState<AiSearchHistoryDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    const loadHistory = async () => {
      if (!Number.isFinite(id) || id <= 0) {
        setErrorMessage("AI検索履歴IDが不正です。");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setErrorMessage(null);

        const result = await getAiSearchHistoryDetail(id);
        setHistory(result);
      } catch (error) {
        console.error(error);
        setErrorMessage("AI検索履歴の詳細取得に失敗しました。");
      } finally {
        setLoading(false);
      }
    };

    loadHistory();
  }, [id]);

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-8">
      <div className="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-semibold text-cyan-300">Admin</p>
          <h1 className="mt-1 text-2xl font-bold text-white">
            AI検索履歴詳細
          </h1>
          <p className="mt-2 text-sm text-slate-400">
            AI検索時の質問、回答、成功/失敗、参照元FAQを確認します。
          </p>
        </div>

        <Link
          href="/admin/ai-histories"
          className="inline-flex items-center justify-center rounded-xl border border-white/10 bg-white/10 px-4 py-2 text-sm font-medium text-slate-100 transition hover:bg-white/15"
        >
          一覧へ戻る
        </Link>
      </div>

      {loading && (
        <div className="rounded-3xl border border-white/10 bg-slate-900/80 p-6 text-sm text-slate-300 shadow-xl shadow-slate-950/30">
          AI検索履歴詳細を読み込み中です...
        </div>
      )}

      {errorMessage && (
        <div className="rounded-3xl border border-red-500/30 bg-red-950/50 p-6 text-sm text-red-100">
          {errorMessage}
        </div>
      )}

      {!loading && !errorMessage && history && (
        <>
          <section className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
            <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

            <div className="space-y-6 p-5 sm:p-6">
              <div className="flex flex-wrap items-center gap-2">
                <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-300">
                  ID: {history.id}
                </span>

                {history.isSuccess ? (
                  <span className="rounded-full border border-emerald-400/30 bg-emerald-500/10 px-3 py-1 text-xs text-emerald-300">
                    成功
                  </span>
                ) : (
                  <span className="rounded-full border border-red-400/30 bg-red-500/10 px-3 py-1 text-xs text-red-300">
                    失敗
                  </span>
                )}

                <span className="rounded-full border border-cyan-400/30 bg-cyan-500/10 px-3 py-1 text-xs text-cyan-200">
                  参照FAQ {history.sources.length}件
                </span>

                <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-300">
                  {formatDateTime(history.executedAt)}
                </span>
              </div>

              <div>
                <h2 className="text-sm font-semibold text-cyan-200">
                  質問文
                </h2>
                <div className="mt-3 whitespace-pre-wrap rounded-2xl border border-white/10 bg-slate-950/60 p-4 text-sm leading-7 text-slate-200">
                  {history.question}
                </div>
              </div>

              {history.searchKeywords && (
                <div>
                  <h2 className="text-sm font-semibold text-cyan-200">
                    検索キーワード
                  </h2>
                  <div className="mt-3 rounded-2xl border border-white/10 bg-slate-950/60 p-4 text-sm text-slate-300">
                    {history.searchKeywords}
                  </div>
                </div>
              )}

              <div>
                <h2 className="text-sm font-semibold text-cyan-200">
                  AI回答
                </h2>

                {history.aiAnswer ? (
                  <div className="mt-3 whitespace-pre-wrap rounded-2xl border border-white/10 bg-slate-950/60 p-4 text-sm leading-7 text-slate-200">
                    {history.aiAnswer}
                  </div>
                ) : (
                  <div className="mt-3 rounded-2xl border border-white/10 bg-slate-950/60 p-4 text-sm text-slate-500">
                    AI回答は保存されていません。
                  </div>
                )}
              </div>

              {history.errorMessage && (
                <div>
                  <h2 className="text-sm font-semibold text-red-300">
                    エラー内容
                  </h2>
                  <div className="mt-3 whitespace-pre-wrap rounded-2xl border border-red-500/30 bg-red-950/40 p-4 text-sm leading-7 text-red-100">
                    {history.errorMessage}
                  </div>
                </div>
              )}

              <div>
                <h2 className="text-sm font-semibold text-cyan-200">
                  フィードバック
                </h2>
                <div className="mt-3 rounded-2xl border border-white/10 bg-slate-950/60 p-4 text-sm text-slate-300">
                  {formatHelpful(history.isHelpful)}
                </div>
              </div>
            </div>
          </section>

          <section className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
            <div className="h-1 bg-gradient-to-r from-cyan-400 via-blue-500 to-violet-500" />

            <div className="p-5 sm:p-6">
              <h2 className="text-lg font-bold text-white">参照元FAQ</h2>
              <p className="mt-1 text-sm text-slate-400">
                AI回答生成時に参照したFAQです。
              </p>

              {history.sources.length === 0 ? (
                <div className="mt-5 rounded-2xl border border-white/10 bg-slate-950/60 p-5 text-sm text-slate-400">
                  参照元FAQはありません。
                </div>
              ) : (
                <div className="mt-5 grid gap-3">
                  {history.sources
                    .slice()
                    .sort((a, b) => a.displayOrder - b.displayOrder)
                    .map((source) => (
                      <Link
                        key={`${source.faqId}-${source.displayOrder}`}
                        href={source.url || `/faqs/${source.faqId}`}
                        className="group rounded-2xl border border-white/10 bg-slate-950/50 p-4 transition hover:border-cyan-400/50 hover:bg-slate-900"
                      >
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                          <div className="min-w-0">
                            <div className="flex flex-wrap items-center gap-2">
                              <span className="rounded-full border border-cyan-400/30 bg-cyan-500/10 px-3 py-1 text-xs text-cyan-200">
                                #{source.displayOrder}
                              </span>
                              <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-300">
                                FAQ ID: {source.faqId}
                              </span>
                              {source.score !== null &&
                                source.score !== undefined && (
                                  <span className="rounded-full border border-violet-400/30 bg-violet-500/10 px-3 py-1 text-xs text-violet-200">
                                    Score: {source.score}
                                  </span>
                                )}
                            </div>

                            <div className="mt-3 break-words text-sm font-semibold text-white group-hover:text-cyan-200">
                              {source.faqTitle}
                            </div>
                          </div>

                          <span className="shrink-0 text-sm font-semibold text-cyan-300 transition group-hover:translate-x-1">
                            FAQを見る →
                          </span>
                        </div>
                      </Link>
                    ))}
                </div>
              )}
            </div>
          </section>
        </>
      )}
    </div>
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
  if (value === true) return "役に立った 👍";
  if (value === false) return "役に立たなかった 👎";
  return "未評価";
}