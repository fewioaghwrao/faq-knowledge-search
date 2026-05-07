"use client";

import { createFaq } from "@/lib/api";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function NewFaqPage() {
  const router = useRouter();

  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [categoryId, setCategoryId] = useState(1);
  const [tagIdsText, setTagIdsText] = useState("1,2");
  const [isPublished, setIsPublished] = useState(true);
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      setSubmitting(true);
      setError("");

      const tagIds = tagIdsText
        .split(",")
        .map((x) => Number(x.trim()))
        .filter((x) => !Number.isNaN(x));

      await createFaq({
        title: title.trim(),
        body: body.trim(),
        categoryId,
        tagIds,
        isPublished,
      });

      router.push("/admin");
    } catch (e) {
      setError("FAQ登録に失敗しました。入力内容またはAPIを確認してください。");
      console.error(e);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
      <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

      <div className="relative p-6">
        <div className="pointer-events-none absolute -right-24 -top-24 h-64 w-64 rounded-full bg-blue-500/20 blur-3xl" />
        <div className="pointer-events-none absolute -bottom-24 -left-24 h-64 w-64 rounded-full bg-violet-500/20 blur-3xl" />

        <div className="relative">
          <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
            <div>
              <span className="inline-flex rounded-full border border-blue-400/30 bg-blue-500/10 px-4 py-1 text-sm text-blue-200">
                New FAQ
              </span>

              <h2 className="mt-4 text-2xl font-bold text-white">
                FAQ新規登録
              </h2>

              <p className="mt-2 text-sm text-slate-400">
                社内FAQ・手順書・障害対応メモを登録します。
              </p>
            </div>

            <Link
              href="/admin"
              className="rounded-xl border border-white/10 bg-white/10 px-4 py-2 text-sm font-medium text-white transition hover:bg-white/15"
            >
              一覧へ戻る
            </Link>
          </div>

          {error && (
            <div className="mb-6 overflow-hidden rounded-2xl border border-red-500/30 bg-red-950/50">
              <div className="h-1 bg-gradient-to-r from-red-500 to-orange-400" />
              <div className="p-4 text-sm text-red-100">{error}</div>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <div className="mb-2 flex items-center justify-between gap-3">
                <label className="block text-sm font-medium text-slate-300">
                  タイトル
                </label>
                <span className="text-xs text-slate-500">
                  {title.length}/100
                </span>
              </div>

              <input
                className="w-full rounded-xl border border-white/10 bg-slate-800 px-4 py-3 text-white placeholder:text-slate-500 outline-none transition focus:border-blue-400 focus:ring-2 focus:ring-blue-500/20"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                maxLength={100}
                placeholder="例：ログインできない場合の対応"
                required
              />
            </div>

            <div>
              <div className="mb-2 flex items-center justify-between gap-3">
                <label className="block text-sm font-medium text-slate-300">
                  本文
                </label>
                <span className="text-xs text-slate-500">
                  {body.length}文字
                </span>
              </div>

              <textarea
                className="min-h-56 w-full rounded-xl border border-white/10 bg-slate-800 px-4 py-3 text-white placeholder:text-slate-500 outline-none transition focus:border-blue-400 focus:ring-2 focus:ring-blue-500/20"
                value={body}
                onChange={(e) => setBody(e.target.value)}
                placeholder="対応手順、確認ポイント、参照元などを記載してください。Markdown形式も利用できます。"
                required
              />
            </div>

            <div className="grid gap-5 md:grid-cols-2">
              <div>
                <label className="mb-2 block text-sm font-medium text-slate-300">
                  カテゴリ
                </label>

                <select
                  className="w-full rounded-xl border border-white/10 bg-slate-800 px-4 py-3 text-white outline-none transition focus:border-blue-400 focus:ring-2 focus:ring-blue-500/20"
                  value={categoryId}
                  onChange={(e) => setCategoryId(Number(e.target.value))}
                >
                  <option value={1}>ログイン</option>
                  <option value={2}>請求</option>
                  <option value={3}>エラー対応</option>
                </select>
              </div>

              <div>
                <label className="mb-2 block text-sm font-medium text-slate-300">
                  タグID
                </label>

                <input
                  className="w-full rounded-xl border border-white/10 bg-slate-800 px-4 py-3 text-white placeholder:text-slate-500 outline-none transition focus:border-blue-400 focus:ring-2 focus:ring-blue-500/20"
                  value={tagIdsText}
                  onChange={(e) => setTagIdsText(e.target.value)}
                  placeholder="1,2"
                />

                <p className="mt-2 text-xs text-slate-500">
                  例：1,2 / 1: 初期対応、2: FAQ、3: 障害対応
                </p>
              </div>
            </div>

            <div className="rounded-2xl border border-white/10 bg-white/5 p-4">
              <label className="flex items-center justify-between gap-4">
                <div>
                  <div className="text-sm font-medium text-white">
                    公開状態
                  </div>
                  <p className="mt-1 text-xs text-slate-400">
                    ONにすると利用者向けのFAQ一覧・検索結果に表示されます。
                  </p>
                </div>

                <input
                  type="checkbox"
                  checked={isPublished}
                  onChange={(e) => setIsPublished(e.target.checked)}
                  className="h-5 w-5 accent-blue-500"
                />
              </label>
            </div>

            <div className="flex flex-wrap justify-end gap-3 border-t border-white/10 pt-6">
              <Link
                href="/admin"
                className="rounded-xl border border-white/10 bg-white/10 px-5 py-3 text-sm font-medium text-white transition hover:bg-white/15"
              >
                キャンセル
              </Link>

              <button
                type="submit"
                disabled={submitting}
                className="rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {submitting ? "登録中..." : "登録する"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}