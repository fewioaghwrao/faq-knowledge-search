import Loading from "@/components/Loading";

export default function AppLoading() {
  return (
    <main className="min-h-[calc(100vh-80px)] bg-slate-950 px-4 py-10 text-white">
      <div className="mx-auto flex max-w-6xl items-center justify-center py-24">
        <Loading message="画面を読み込み中です..." />
      </div>
    </main>
  );
}