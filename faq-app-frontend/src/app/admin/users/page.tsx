"use client";

import { useEffect, useState } from "react";
import {
  getUsers,
  updateUserStatus,
} from "@/lib/api";
import type { UserListItem } from "@/types/user";

export default function AdminUsersPage() {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [updatingUserId, setUpdatingUserId] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);

  const pageSize = 5;
  const totalPages = Math.ceil(users.length / pageSize);

  const displayedUsers = users.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getUsers();

      setUsers(data);
      setCurrentPage(1);
    } catch (error) {
      console.error("ユーザー一覧取得エラー:", error);

      setError(
        error instanceof Error
          ? error.message
          : "ユーザー一覧の取得に失敗しました。"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const handleStatusChange = async (userId: string, isActive: boolean) => {
    try {
      setUpdatingUserId(userId);
      await updateUserStatus(userId, { isActive });
      await loadUsers();
    } catch (error) {
      console.error("有効/無効変更エラー:", error);
      alert("有効/無効の変更に失敗しました。");
    } finally {
      setUpdatingUserId(null);
    }
  };

  const getRoleBadgeClass = (role: string) => {
    if (role === "Admin") {
      return "border border-violet-400/30 bg-violet-500/10 text-violet-200";
    }

    if (role === "Editor") {
      return "border border-blue-400/30 bg-blue-500/10 text-blue-200";
    }

    return "border border-slate-500/30 bg-slate-500/10 text-slate-300";
  };

  const getStatusBadgeClass = (isActive: boolean) => {
    return isActive
      ? "border border-emerald-400/30 bg-emerald-500/10 text-emerald-200"
      : "border border-slate-500/30 bg-slate-500/10 text-slate-300";
  };

  if (loading) {
    return (
      <div className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
        <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />
        <div className="p-6 text-sm text-slate-300">
          ユーザー情報を読み込み中...
        </div>
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-3xl border border-white/10 bg-slate-900/80 shadow-xl shadow-slate-950/30">
      <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

      <div className="relative p-6">
        <div className="pointer-events-none absolute -right-24 -top-24 h-64 w-64 rounded-full bg-blue-500/20 blur-3xl" />
        <div className="pointer-events-none absolute -bottom-24 -left-24 h-64 w-64 rounded-full bg-violet-500/20 blur-3xl" />

        <div className="relative">
          <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
            <div>
              <span className="inline-flex rounded-full border border-cyan-400/30 bg-cyan-500/10 px-4 py-1 text-sm text-cyan-200">
                User Management
              </span>

              <h2 className="mt-4 text-2xl font-bold text-white">
                ユーザー管理
              </h2>

              <p className="mt-2 text-sm text-slate-400">
                登録ユーザーのロール確認、Admin権限を除く有効/無効の切り替えを行います。
              </p>
            </div>

            <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm text-slate-300">
              登録ユーザー数{" "}
              <span className="ml-2 font-semibold text-white">
                {users.length}
              </span>
              件
            </div>
          </div>

          {error && (
            <div className="mb-6 overflow-hidden rounded-2xl border border-red-500/30 bg-red-950/50">
              <div className="h-1 bg-gradient-to-r from-red-500 to-orange-400" />
              <div className="p-4 text-sm text-red-100">{error}</div>
            </div>
          )}

          {users.length === 0 ? (
            <div className="rounded-2xl border border-white/10 bg-white/5 p-8 text-center">
              <p className="text-sm text-slate-300">
                表示できるユーザーがありません。
              </p>
            </div>
          ) : (
            <>
<>
  {/* Mobile / Tablet: card layout */}
  <div className="grid gap-4 lg:hidden">
    {displayedUsers.map((user) => {
      const isUpdating = updatingUserId === user.id;
      const isAdmin = user.role === "Admin";

      return (
        <article
          key={user.id}
          className="overflow-hidden rounded-2xl border border-white/10 bg-slate-950/40 shadow-lg shadow-slate-950/20"
        >
          <div className="h-1 bg-gradient-to-r from-blue-500 via-violet-500 to-cyan-400" />

          <div className="space-y-4 p-4">
            <div className="flex flex-wrap items-center gap-2">
              <span
                className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getRoleBadgeClass(
                  user.role
                )}`}
                title="デモ環境のため、ロール変更は無効化しています。"
              >
                {user.role}
              </span>

              {isAdmin ? (
                <span
                  className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getStatusBadgeClass(
                    user.isActive
                  )}`}
                  title="デモ環境のため、Adminユーザーの有効/無効は変更できません。"
                >
                  {user.isActive ? "有効" : "無効"}
                </span>
              ) : (
                <button
                  type="button"
                  disabled={isUpdating}
                  onClick={() =>
                    handleStatusChange(user.id, !user.isActive)
                  }
                  className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold transition disabled:cursor-not-allowed disabled:opacity-60 ${
                    user.isActive
                      ? "border border-emerald-400/30 bg-emerald-500/10 text-emerald-200 hover:bg-emerald-500/20"
                      : "border border-slate-500/30 bg-slate-500/10 text-slate-300 hover:bg-slate-500/20"
                  }`}
                >
                  {user.isActive ? "有効" : "無効"}
                </button>
              )}
            </div>

            <div>
              <div className="text-base font-bold text-white">
                {user.displayName || "未設定"}
              </div>

              <div className="mt-2 break-all text-sm text-slate-300">
                {user.email}
              </div>

              <div className="mt-2 break-all text-xs text-slate-500">
                ID: {user.id}
              </div>
            </div>

            <div className="border-t border-white/10 pt-4 text-xs text-slate-400">
              作成日: {new Date(user.createdAt).toLocaleString("ja-JP")}
            </div>
          </div>
        </article>
      );
    })}
  </div>

  {/* Desktop: table layout */}
  <div className="hidden overflow-hidden rounded-2xl border border-white/10 bg-white/5 lg:block">
    <div className="overflow-x-auto">
      <table className="min-w-full text-sm">
        <thead className="bg-white/10 text-slate-300">
          <tr>
            <th className="px-5 py-4 text-left font-medium">
              表示名
            </th>
            <th className="px-5 py-4 text-left font-medium">
              メールアドレス
            </th>
            <th className="px-5 py-4 text-left font-medium">
              ロール
            </th>
            <th className="px-5 py-4 text-left font-medium">
              状態
            </th>
            <th className="px-5 py-4 text-left font-medium">
              作成日
            </th>
          </tr>
        </thead>

        <tbody className="divide-y divide-white/10">
          {displayedUsers.map((user) => {
            const isUpdating = updatingUserId === user.id;
            const isAdmin = user.role === "Admin";

            return (
              <tr
                key={user.id}
                className="transition hover:bg-white/[0.04]"
              >
                <td className="px-5 py-4">
                  <div className="font-medium text-white">
                    {user.displayName || "未設定"}
                  </div>
                  <div className="mt-1 break-all text-xs text-slate-500">
                    ID: {user.id}
                  </div>
                </td>

                <td className="px-5 py-4 break-all text-slate-300">
                  {user.email}
                </td>

                <td className="px-5 py-4">
                  <span
                    className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getRoleBadgeClass(
                      user.role
                    )}`}
                    title="デモ環境のため、ロール変更は無効化しています。"
                  >
                    {user.role}
                  </span>
                </td>

                <td className="px-5 py-4">
                  {isAdmin ? (
                    <span
                      className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getStatusBadgeClass(
                        user.isActive
                      )}`}
                      title="デモ環境のため、Adminユーザーの有効/無効は変更できません。"
                    >
                      {user.isActive ? "有効" : "無効"}
                    </span>
                  ) : (
                    <button
                      type="button"
                      disabled={isUpdating}
                      onClick={() =>
                        handleStatusChange(user.id, !user.isActive)
                      }
                      className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold transition disabled:cursor-not-allowed disabled:opacity-60 ${
                        user.isActive
                          ? "border border-emerald-400/30 bg-emerald-500/10 text-emerald-200 hover:bg-emerald-500/20"
                          : "border border-slate-500/30 bg-slate-500/10 text-slate-300 hover:bg-slate-500/20"
                      }`}
                    >
                      {user.isActive ? "有効" : "無効"}
                    </button>
                  )}
                </td>

                <td className="px-5 py-4 text-slate-400">
                  {new Date(user.createdAt).toLocaleString("ja-JP")}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  </div>
</>

              {totalPages > 1 && (
                <div className="mt-6 flex items-center justify-center gap-3">
                  <button
                    type="button"
                    onClick={() =>
                      setCurrentPage((page) => Math.max(1, page - 1))
                    }
                    disabled={currentPage === 1}
                    className="rounded-full border border-white/10 bg-slate-900 px-4 py-2 text-sm text-slate-200 transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-slate-900"
                  >
                    前へ
                  </button>

                  <span className="text-sm text-slate-400">
                    {currentPage} / {totalPages}
                  </span>

                  <button
                    type="button"
                    onClick={() =>
                      setCurrentPage((page) =>
                        Math.min(totalPages, page + 1)
                      )
                    }
                    disabled={currentPage === totalPages}
                    className="rounded-full border border-white/10 bg-slate-900 px-4 py-2 text-sm text-slate-200 transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-slate-900"
                  >
                    次へ
                  </button>
                </div>
              )}
            </>
          )}

          <div className="mt-4 text-xs text-slate-500">
            ※ デモ環境のため、ロール変更は無効化しています。Adminユーザーの有効/無効も変更できません。
          </div>
        </div>
      </div>
    </div>
  );
}