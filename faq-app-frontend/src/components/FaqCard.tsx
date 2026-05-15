import Link from "next/link";
import { FaqListItem } from "@/types/faq";

type Props = {
  faq: FaqListItem;
};

export default function FaqCard({ faq }: Props) {
  const updatedAtText = new Date(faq.updatedAt).toLocaleDateString("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });

  const displayBody = faq.bodyExcerpt || faq.body;

  return (
    <Link
      href={`/faqs/${faq.id}`}
      className="group block overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30 transition hover:-translate-y-0.5 hover:border-blue-400/60 hover:shadow-blue-500/10"
    >
      <div className="relative p-5">
        <div className="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400 opacity-80" />

        <div className="mb-4 flex items-start justify-between gap-4">
          <div className="min-w-0">
            <div className="mb-2 flex flex-wrap items-center gap-2">
              <span className="rounded-full border border-blue-400/30 bg-blue-500/10 px-3 py-1 text-xs font-medium text-blue-200">
                {faq.categoryName}
              </span>

              {!faq.isPublished && (
                <span className="rounded-full border border-slate-500/40 bg-slate-800 px-3 py-1 text-xs text-slate-300">
                  非公開
                </span>
              )}

              {faq.score > 0 && (
                <span className="rounded-full border border-cyan-400/20 bg-cyan-500/10 px-3 py-1 text-xs text-cyan-200">
                  score: {faq.score}
                </span>
              )}
            </div>

            <h2 className="line-clamp-2 text-lg font-bold text-white transition group-hover:text-blue-200">
              {faq.titleHighlighted ? (
                <span
                  className="[&_mark]:rounded [&_mark]:bg-yellow-300 [&_mark]:px-1 [&_mark]:text-slate-950"
                  dangerouslySetInnerHTML={{ __html: faq.titleHighlighted }}
                />
              ) : (
                faq.title
              )}
            </h2>
          </div>

          <div className="shrink-0 rounded-2xl bg-gradient-to-br from-blue-600/20 to-violet-600/20 px-3 py-2 text-center">
            <div className="text-xs text-slate-400">Views</div>
            <div className="text-sm font-bold text-white">{faq.viewCount}</div>
          </div>
        </div>

        <p className="line-clamp-2 text-sm leading-6 text-slate-300">
          {displayBody}
        </p>

        <div className="mt-5 flex flex-wrap gap-2">
          {faq.tags.length > 0 ? (
            faq.tags.map((tag) => (
              <span
                key={tag}
                className="rounded-full border border-violet-400/20 bg-violet-500/10 px-3 py-1 text-xs text-violet-200"
              >
                #{tag}
              </span>
            ))
          ) : (
            <span className="text-xs text-slate-500">タグなし</span>
          )}
        </div>

        <div className="mt-5 flex items-center justify-between border-t border-white/10 pt-4 text-xs text-slate-500">
          <span>更新日: {updatedAtText}</span>
          <span className="text-blue-300 transition group-hover:translate-x-1">
            詳細を見る →
          </span>
        </div>
      </div>
    </Link>
  );
}