"use client";

import { useEffect, useState } from "react";
import FaqCard from "@/components/FaqCard";
import Loading from "@/components/Loading";
import SearchBar from "@/components/SearchBar";
import { searchFaqs } from "@/lib/api";
import { FaqListItem } from "@/types/faq";

export default function HomePage() {
  const [faqs, setFaqs] = useState<FaqListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [searchedKeyword, setSearchedKeyword] = useState("");

  const loadFaqs = async (keyword?: string) => {
    try {
      setLoading(true);
      setError("");

      const normalizedKeyword = keyword?.trim() ?? "";
      setSearchedKeyword(normalizedKeyword);

      const result = await searchFaqs(normalizedKeyword);
      setFaqs(result);
    } catch (e) {
      setError("FAQの取得に失敗しました。APIの起動状態を確認してください。");
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadFaqs();
  }, []);

  return (
    <div className="space-y-8">
      <section className="relative overflow-hidden rounded-3xl border border-white/10 bg-slate-900 p-8 shadow-2xl shadow-slate-950/40">
        <div className="absolute inset-0 bg-gradient-to-br from-blue-600/20 via-violet-600/20 to-cyan-500/10" />
        <div className="absolute -right-24 -top-24 h-72 w-72 rounded-full bg-blue-500/20 blur-3xl" />
        <div className="absolute -bottom-24 -left-24 h-72 w-72 rounded-full bg-violet-500/20 blur-3xl" />

        <div className="relative">
          <span className="inline-flex rounded-full border border-blue-400/30 bg-blue-500/10 px-4 py-1 text-sm text-blue-200">
            Phase 1 / FAQ Search
          </span>

          <h1 className="mt-5 text-4xl font-bold tracking-tight text-white">
            社内ナレッジをすばやく検索
          </h1>

          <p className="mt-4 max-w-2xl text-slate-300">
            手順書・FAQ・障害対応メモを登録し、キーワードから必要な情報を探せる社内FAQ検索アプリです。
          </p>

          <div className="mt-6 flex flex-wrap gap-3 text-sm">
            <span className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-slate-200">
              FAQ検索
            </span>
            <span className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-slate-200">
              根拠表示
            </span>
            <span className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-slate-200">
              管理者登録
            </span>
          </div>
        </div>
      </section>

      <SearchBar onSearch={loadFaqs} />

      {!loading && !error && (
        <div className="flex items-center justify-between text-sm text-slate-400">
          <span>
            {searchedKeyword
              ? `「${searchedKeyword}」の検索結果`
              : "登録済みFAQ"}
          </span>
          <span>{faqs.length}件</span>
        </div>
      )}

      {loading && <Loading />}

      {error && (
        <div className="overflow-hidden rounded-3xl border border-red-500/30 bg-red-950/50 shadow-xl shadow-red-950/20">
          <div className="h-1 bg-gradient-to-r from-red-500 to-orange-400" />
          <div className="p-5 text-red-100">
            <div className="font-semibold">取得エラー</div>
            <p className="mt-1 text-sm text-red-200">{error}</p>
          </div>
        </div>
      )}

      {!loading && !error && faqs.length === 0 && (
        <div className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
          <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

          <div className="p-8 text-center">
            <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-600/20 to-violet-600/20 text-2xl">
              ?
            </div>

            <h2 className="mt-4 text-lg font-bold text-white">
              FAQが見つかりませんでした
            </h2>

            <p className="mt-2 text-sm text-slate-400">
              キーワードを変えるか、管理画面からFAQを登録してください。
            </p>
          </div>
        </div>
      )}

      {!loading && !error && faqs.length > 0 && (
        <div className="grid gap-5">
          {faqs.map((faq) => (
            <FaqCard key={faq.id} faq={faq} />
          ))}
        </div>
      )}
    </div>
  );
}