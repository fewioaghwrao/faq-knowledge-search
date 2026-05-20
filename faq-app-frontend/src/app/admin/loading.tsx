import Loading from "@/components/Loading";

export default function AdminLoading() {
  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <Loading
        title="管理画面を読み込み中..."
        message="FAQ・ユーザー情報を取得しています。"
      />
    </div>
  );
}