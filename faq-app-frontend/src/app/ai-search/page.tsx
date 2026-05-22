"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { searchAi, sendAiSearchFeedback } from "@/lib/api";
import type { AiSearchResponse } from "@/types/ai";

export default function AiSearchPage() {
  const [question, setQuestion] = useState("");
  const [result, setResult] = useState<AiSearchResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [feedbackMessage, setFeedbackMessage] = useState("");
const [feedbackLoading, setFeedbackLoading] = useState(false);

const exampleQuestions = [
  "ログインできない 初期対応",
  "CSV取込 エラー",
  "PDF出力 失敗",
];
  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const normalizedQuestion = question.trim();

    if (!normalizedQuestion) {
      setError("質問・検索キーワードを入力してください。");
      setResult(null);
      return;
    }

    try {
setLoading(true);
setError("");
setResult(null);
setFeedbackMessage("");

      const response = await searchAi(normalizedQuestion);
      setResult(response);
} catch (e) {
  const message =
    e instanceof Error
      ? e.message
      : "AI検索の実行に失敗しました。APIの起動状態やAI API設定を確認してください。";

  if (message.includes("実行回数が上限")) {
    console.warn(message);
  } else {
    console.error(e);
  }

  setError(message);
} finally {
  setLoading(false);
}
  };

const handleFeedback = async (isHelpful: boolean) => {
  if (!result?.aiHistoryId) {
    setFeedbackMessage("AI検索履歴IDが取得できないため、送信できません。");
    return;
  }

  try {
    setFeedbackLoading(true);
    setFeedbackMessage("");

    await sendAiSearchFeedback(result.aiHistoryId, {
      isHelpful,
    });

    setFeedbackMessage(
      isHelpful
        ? "フィードバックを送信しました：役に立った"
        : "フィードバックを送信しました：役に立たなかった"
    );
  } catch (error) {
    console.error(error);
    setFeedbackMessage("フィードバックの送信に失敗しました。");
  } finally {
    setFeedbackLoading(false);
  }
};

  return (
    <div className="space-y-8">
      <section className="relative overflow-hidden rounded-3xl border border-white/10 bg-slate-900 p-8 shadow-2xl shadow-slate-950/40">
        <div className="absolute inset-0 bg-gradient-to-br from-violet-600/20 via-blue-600/20 to-cyan-500/10" />
        <div className="absolute -right-24 -top-24 h-72 w-72 rounded-full bg-violet-500/20 blur-3xl" />
        <div className="absolute -bottom-24 -left-24 h-72 w-72 rounded-full bg-blue-500/20 blur-3xl" />

        <div className="relative">
          <span className="inline-flex rounded-full border border-violet-400/30 bg-violet-500/10 px-4 py-1 text-sm text-violet-200">
            AI FAQ Search
          </span>

          <h1 className="mt-5 text-4xl font-bold tracking-tight text-white">
            FAQをもとにAI回答を生成
          </h1>

          <p className="mt-4 max-w-2xl text-slate-300">
            質問・検索キーワードから関連FAQを検索し、上位5件のFAQをもとにAI回答と参照元FAQを表示します。
          </p>

          <div className="mt-6 flex flex-wrap gap-3 text-sm">
            <span className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-slate-200">
              FAQ検索連携
            </span>
            <span className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-slate-200">
              参照元表示
            </span>
            <span className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-slate-200">
              外部AI API連携
            </span>
          </div>
        </div>
      </section>

      <form
        onSubmit={handleSubmit}
        className="rounded-3xl border border-white/10 bg-slate-900/80 p-5 shadow-xl shadow-slate-950/30"
      >
        <label className="block text-sm font-semibold text-slate-200">
          質問・検索キーワード
        </label>

        <textarea
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          rows={5}
          maxLength={500}
          placeholder="例：ログインできない場合はどうすればいいですか？"
          className="mt-3 w-full resize-none rounded-2xl border border-white/10 bg-slate-800 px-4 py-3 text-white placeholder:text-slate-500 focus:border-violet-400 focus:outline-none"
        />

        <div className="mt-4">
          <div className="text-xs font-semibold text-slate-400">
            質問例
          </div>

          <div className="mt-2 flex flex-wrap gap-2">
            {exampleQuestions.map((example) => (
              <button
                key={example}
                type="button"
                onClick={() => {
setQuestion(example);
setError("");
setResult(null);
setFeedbackMessage("");
                }}
                className="rounded-full border border-white/10 bg-slate-800 px-4 py-2 text-xs text-slate-300 transition hover:border-violet-400/50 hover:bg-slate-700 hover:text-white"
              >
                {example}
              </button>
            ))}
          </div>
        </div>

        <div className="mt-3 flex items-center justify-between gap-4 text-xs text-slate-500">
          <span>FAQに登録された内容をもとに回答します。</span>
          <span>{question.length}/500</span>
        </div>

        <div className="mt-5 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <Link
            href="/faqs"
            className="text-sm font-semibold text-blue-300 transition hover:text-blue-200"
          >
            通常のFAQ検索を見る →
          </Link>

          <button
            type="submit"
            disabled={loading}
            className="rounded-xl bg-gradient-to-r from-violet-600 to-blue-600 px-6 py-3 font-semibold text-white shadow-lg shadow-violet-500/20 transition hover:from-violet-500 hover:to-blue-500 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {loading ? "AI回答を生成中..." : "AI検索する"}
          </button>
        </div>
      </form>

      {loading && (
        <div className="overflow-hidden rounded-3xl border border-blue-500/30 bg-blue-950/40 shadow-xl shadow-blue-950/20">
          <div className="h-1 bg-gradient-to-r from-violet-500 via-blue-500 to-cyan-400" />
          <div className="p-5 text-blue-100">
            <div className="font-semibold">AI回答を生成中です</div>
            <p className="mt-1 text-sm text-blue-200">
              関連FAQを検索し、FAQ本文をもとに回答を生成しています。
            </p>
          </div>
        </div>
      )}

      {error && (
        <div className="overflow-hidden rounded-3xl border border-red-500/30 bg-red-950/50 shadow-xl shadow-red-950/20">
          <div className="h-1 bg-gradient-to-r from-red-500 to-orange-400" />
          <div className="p-5 text-red-100">
            <div className="font-semibold">実行エラー</div>
            <p className="mt-1 text-sm text-red-200">{error}</p>
          </div>
        </div>
      )}

      {result?.message && (
        <div className="overflow-hidden rounded-3xl border border-yellow-500/30 bg-yellow-950/40 shadow-xl shadow-yellow-950/20">
          <div className="h-1 bg-gradient-to-r from-yellow-400 to-orange-400" />
          <div className="p-5 text-yellow-100">
            <div className="font-semibold">検索結果なし</div>
            <p className="mt-1 text-sm text-yellow-200">{result.message}</p>

            <div className="mt-4">
              <Link
                href="/faqs"
                className="inline-flex rounded-xl border border-yellow-400/30 bg-yellow-500/10 px-4 py-2 text-sm font-semibold text-yellow-100 transition hover:bg-yellow-500/20"
              >
                通常のFAQ検索で探す →
              </Link>
            </div>
          </div>
        </div>
      )}

      {result?.answer && (
        <section className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
          <div className="h-1 bg-gradient-to-r from-violet-500 via-blue-500 to-cyan-400" />

          <div className="space-y-6 p-6">
            <div>
              <div className="inline-flex rounded-full border border-violet-400/30 bg-violet-500/10 px-3 py-1 text-sm font-semibold text-violet-200">
                FAQ参照AI回答
              </div>

              <div className="mt-3 whitespace-pre-wrap rounded-2xl border border-white/10 bg-slate-950/60 p-5 text-sm leading-7 text-slate-200">
                {result.answer}
              </div>
            </div>

            {result.disclaimer && (
              <div className="rounded-2xl border border-cyan-400/20 bg-cyan-500/10 p-4 text-sm text-cyan-100">
                {result.disclaimer}
              </div>
            )}

<div className="rounded-2xl border border-white/10 bg-slate-950/50 p-4">
  <div className="text-sm font-semibold text-slate-200">
    このAI回答は役に立ちましたか？
  </div>

  <div className="mt-3 flex flex-wrap gap-2">
    <button
      type="button"
      onClick={() => handleFeedback(true)}
      disabled={feedbackLoading || Boolean(feedbackMessage)}
      className="rounded-xl border border-emerald-400/30 bg-emerald-500/10 px-4 py-2 text-sm font-semibold text-emerald-200 transition hover:bg-emerald-500/20 disabled:cursor-not-allowed disabled:opacity-60"
    >
      役に立った
    </button>

    <button
      type="button"
      onClick={() => handleFeedback(false)}
      disabled={feedbackLoading || Boolean(feedbackMessage)}
      className="rounded-xl border border-red-400/30 bg-red-500/10 px-4 py-2 text-sm font-semibold text-red-200 transition hover:bg-red-500/20 disabled:cursor-not-allowed disabled:opacity-60"
    >
      役に立たなかった
    </button>
  </div>

  {feedbackMessage && (
    <p className="mt-3 text-sm text-slate-300">
      {feedbackMessage}
    </p>
  )}
</div>

            <div>
              <div className="text-sm font-semibold text-slate-200">
                参照元FAQ
              </div>

              {result.sources.length > 0 ? (
                <div className="mt-3 grid gap-3">
                  {result.sources.map((source) => (
                    <Link
                      key={source.id}
                      href={source.url}
                      className="group rounded-2xl border border-white/10 bg-slate-950/50 p-4 transition hover:border-blue-400/50 hover:bg-slate-900"
                    >
                      <div className="flex items-center justify-between gap-4">
                        <div>
                          <div className="text-sm font-semibold text-white group-hover:text-blue-200">
                            {source.title}
                          </div>
                          <div className="mt-1 text-xs text-slate-500">
                            FAQ ID: {source.id}
                          </div>
                        </div>

                        <span className="shrink-0 text-sm text-blue-300 transition group-hover:translate-x-1">
                          詳細を見る →
                        </span>
                      </div>
                    </Link>
                  ))}
                </div>
              ) : (
                <div className="mt-3 rounded-2xl border border-white/10 bg-slate-950/40 p-4">
                  <p className="text-sm text-slate-400">
                    参照元FAQはありません。通常のFAQ検索もあわせて確認してください。
                  </p>

                  <Link
                    href="/faqs"
                    className="mt-3 inline-flex text-sm font-semibold text-blue-300 transition hover:text-blue-200"
                  >
                    通常のFAQ検索を見る →
                  </Link>
                </div>
              )}
            </div>
          </div>
        </section>
      )}
    </div>
  );
}