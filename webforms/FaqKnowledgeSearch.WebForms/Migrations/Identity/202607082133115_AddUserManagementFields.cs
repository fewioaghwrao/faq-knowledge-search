namespace FaqKnowledgeSearch.WebForms.Migrations.Identity
{
    using System.Data.Entity.Migrations;

    public partial class AddUserManagementFields : DbMigration
    {
        public override void Up()
        {
            // 既存ユーザーが存在するため、最初はNULLを許可する
            AddColumn(
                "AspNetUsers",
                "DisplayName",
                c => c.String(
                    nullable: true,
                    maxLength: 100,
                    storeType: "nvarchar"));

            AddColumn(
                "AspNetUsers",
                "IsActive",
                c => c.Boolean(
                    nullable: true));

            AddColumn(
                "AspNetUsers",
                "CreatedAt",
                c => c.DateTime(
                    nullable: true,
                    precision: 0));

            // 既存ユーザーの初期値を設定する
            Sql(@"
                UPDATE `AspNetUsers`
                SET
                    `DisplayName` =
                        CASE
                            WHEN `DisplayName` IS NULL
                                 OR TRIM(`DisplayName`) = ''
                            THEN '未設定'
                            ELSE `DisplayName`
                        END,
                    `IsActive` =
                        COALESCE(`IsActive`, 1),
                    `CreatedAt` =
                        COALESCE(`CreatedAt`, UTC_TIMESTAMP());
            ");

            // データ補完後、モデル定義に合わせてNULL不可にする
            AlterColumn(
                "AspNetUsers",
                "DisplayName",
                c => c.String(
                    nullable: false,
                    maxLength: 100,
                    storeType: "nvarchar"));

            AlterColumn(
                "AspNetUsers",
                "IsActive",
                c => c.Boolean(
                    nullable: false));

            AlterColumn(
                "AspNetUsers",
                "CreatedAt",
                c => c.DateTime(
                    nullable: false,
                    precision: 0));
        }

        public override void Down()
        {
            DropColumn("AspNetUsers", "CreatedAt");
            DropColumn("AspNetUsers", "IsActive");
            DropColumn("AspNetUsers", "DisplayName");
        }
    }
}