"use client";

import { deleteFaq, searchFaqs } from "@/lib/api";
import { FaqListItem } from "@/types/faq";
import Link from "next/link";
import { useEffect, useState } from "react";

export default function AdminPage() {
  const [faqs, setFaqs] = useState<FaqListItem[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  const loadFaqs = async () => {
    try {
      setLoading(true);
      setError("");

      const result = await searchFaqs();
      setFaqs(result);
    } catch (e) {
      setError("FAQ一覧の取得に失敗しました。");
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadFaqs();
  }, []);

  const handleDelete = async (id: number) => {
    const ok = window.confirm("このFAQを削除しますか？");

    if (!ok) return;

    try {
      setDeletingId(id);
      await deleteFaq(id);
      await loadFaqs();
    } catch (e) {
      alert("削除に失敗しました。");
      console.error(e);
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
      <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

      <div className="p-5">
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="text-xl font-bold text-white">FAQ一覧</h2>
            <p className="mt-1 text-sm text-slate-400">
              登録済みFAQの公開状態・編集・削除を管理します。
            </p>
          </div>

          <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-2 text-sm text-slate-300">
            登録件数{" "}
            <span className="font-bold text-white">{faqs.length}</span> 件
          </div>
        </div>

        {error && (
          <div className="mb-5 overflow-hidden rounded-2xl border border-red-500/30 bg-red-950/50">
            <div className="h-1 bg-gradient-to-r from-red-500 to-orange-400" />
            <div className="p-4 text-sm text-red-100">{error}</div>
          </div>
        )}

        {loading && (
          <div className="rounded-2xl border border-white/10 bg-white/5 p-5 text-sm text-slate-300">
            FAQ一覧を読み込み中...
          </div>
        )}

        {!loading && !error && faqs.length === 0 && (
          <div className="rounded-2xl border border-white/10 bg-white/5 p-8 text-center">
            <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-600/20 to-violet-600/20 text-2xl">
              ?
            </div>

            <h3 className="mt-4 text-lg font-bold text-white">
              FAQがまだ登録されていません
            </h3>

            <p className="mt-2 text-sm text-slate-400">
              新規登録ボタンからFAQを追加してください。
            </p>

            <Link
              href="/admin/faqs/new"
              className="mt-5 inline-flex rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition hover:from-blue-500 hover:to-violet-500"
            >
              FAQを登録する
            </Link>
          </div>
        )}

        {!loading && !error && faqs.length > 0 && (
          <div className="overflow-x-auto rounded-2xl border border-white/10">
            <table className="w-full min-w-[760px] text-left text-sm">
              <thead className="bg-white/5 text-slate-400">
                <tr>
                  <th className="px-4 py-3 font-medium">ID</th>
                  <th className="px-4 py-3 font-medium">タイトル</th>
                  <th className="px-4 py-3 font-medium">カテゴリ</th>
                  <th className="px-4 py-3 font-medium">公開状態</th>
                  <th className="px-4 py-3 font-medium">閲覧数</th>
                  <th className="px-4 py-3 font-medium">操作</th>
                </tr>
              </thead>

              <tbody>
                {faqs.map((faq) => (
                  <tr
                    key={faq.id}
                    className="border-t border-white/10 transition hover:bg-white/[0.03]"
                  >
                    <td className="px-4 py-4 text-slate-400">
                      #{faq.id}
                    </td>

                    <td className="px-4 py-4">
                      <div className="font-semibold text-white">
                        {faq.title}
                      </div>
                      <div className="mt-1 line-clamp-1 text-xs text-slate-500">
                        {faq.body}
                      </div>
                    </td>

                    <td className="px-4 py-4">
                      <span className="rounded-full border border-blue-400/30 bg-blue-500/10 px-3 py-1 text-xs text-blue-200">
                        {faq.categoryName}
                      </span>
                    </td>

                    <td className="px-4 py-4">
                      {faq.isPublished ? (
                        <span className="rounded-full border border-emerald-400/30 bg-emerald-500/10 px-3 py-1 text-xs text-emerald-300">
                          公開
                        </span>
                      ) : (
                        <span className="rounded-full border border-slate-500/40 bg-slate-800 px-3 py-1 text-xs text-slate-300">
                          非公開
                        </span>
                      )}
                    </td>

                    <td className="px-4 py-4 text-slate-300">
                      {faq.viewCount}
                    </td>

                    <td className="px-4 py-4">
                      <div className="flex gap-2">
                        <Link
                          href={`/admin/faqs/${faq.id}/edit`}
                          className="rounded-lg border border-white/10 bg-white/10 px-3 py-2 text-xs font-medium text-white transition hover:bg-white/15"
                        >
                          編集
                        </Link>

                        <button
                          onClick={() => handleDelete(faq.id)}
                          disabled={deletingId === faq.id}
                          className="rounded-lg bg-red-600 px-3 py-2 text-xs font-medium text-white transition hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-60"
                        >
                          {deletingId === faq.id ? "削除中..." : "削除"}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}