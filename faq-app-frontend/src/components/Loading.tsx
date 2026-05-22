type LoadingProps = {
  title?: string;
  message?: string;
};

export default function Loading({
  title = "読み込み中...",
  message = "FAQデータを取得しています。",
}: LoadingProps) {
  return (
    <div className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
      <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

      <div className="flex items-center gap-4 p-6">
        <div className="h-5 w-5 animate-spin rounded-full border-2 border-blue-400 border-t-transparent" />

        <div>
          <div className="font-semibold text-white">{title}</div>
          <div className="mt-1 text-sm text-slate-400">
            {message}
          </div>
        </div>
      </div>
    </div>
  );
}