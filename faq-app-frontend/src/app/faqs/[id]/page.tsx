"use client";

import { getFaqById } from "@/lib/api";
import { FaqListItem } from "@/types/faq";
import Link from "next/link";
import { useParams } from "next/navigation";
import ReactMarkdown from "react-markdown";
import { useEffect, useState } from "react";

export default function FaqDetailPage() {
  const params = useParams();
  const id = Number(params.id);

  const [faq, setFaq] = useState<FaqListItem | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const load = async () => {
      try {
        const result = await getFaqById(id);
        setFaq(result);
      } catch (e) {
        setError("FAQ詳細の取得に失敗しました。");
        console.error(e);
      }
    };

    load();
  }, [id]);

  if (error) {
    return (
      <div className="rounded-xl border border-red-700 bg-red-950 p-4 text-red-200">
        {error}
      </div>
    );
  }

  if (!faq) {
    return <div className="text-slate-300">読み込み中...</div>;
  }

  return (
    <article className="rounded-3xl border border-slate-700 bg-slate-900 p-8">
      <div className="mb-6">
        <Link href="/" className="text-sm text-blue-400 hover:underline">
          ← FAQ一覧へ戻る
        </Link>
      </div>

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300">
          {faq.categoryName}
        </span>

        {faq.tags.map((tag) => (
          <span
            key={tag}
            className="rounded-full bg-blue-950 px-3 py-1 text-xs text-blue-200"
          >
            #{tag}
          </span>
        ))}
      </div>

      <h1 className="text-3xl font-bold text-white">{faq.title}</h1>

      <div className="mt-2 text-sm text-slate-500">
        閲覧数: {faq.viewCount}
      </div>

      <div className="prose prose-invert mt-8 max-w-none">
        <ReactMarkdown>{faq.body}</ReactMarkdown>
      </div>
    </article>
  );
}