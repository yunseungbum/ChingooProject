using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chingoo.Migrations
{
    /// <inheritdoc />
    public partial class UnifyComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BoardType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OldBoardType = table.Column<string>(type: "varchar(30)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldCommentId = table.Column<int>(type: "int", nullable: true),
                    OldParentCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
                INSERT INTO `Comments` (`BoardType`, `BoardId`, `UserId`, `ParentCommentId`, `Content`, `CreatedAt`, `OldBoardType`, `OldCommentId`, `OldParentCommentId`)
                SELECT 'Post', `PostId`, `UserId`, NULL, `Content`, `CreatedAt`, 'Post', `Id`, `ParentCommentId`
                FROM `PostComments`;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO `Comments` (`BoardType`, `BoardId`, `UserId`, `ParentCommentId`, `Content`, `CreatedAt`, `OldBoardType`, `OldCommentId`, `OldParentCommentId`)
                SELECT 'Notice', `NoticeId`, `UserId`, NULL, `Content`, `CreatedAt`, 'Notice', `Id`, `ParentCommentId`
                FROM `NoticeComments`;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO `Comments` (`BoardType`, `BoardId`, `UserId`, `ParentCommentId`, `Content`, `CreatedAt`, `OldBoardType`, `OldCommentId`, `OldParentCommentId`)
                SELECT 'Community', `CommunityPostId`, `UserId`, NULL, `Content`, `CreatedAt`, 'Community', `Id`, `ParentCommentId`
                FROM `CommunityComments`;
            ");

            migrationBuilder.Sql(@"
                UPDATE `Comments` AS child
                INNER JOIN `Comments` AS parent
                    ON parent.`OldBoardType` = child.`OldBoardType`
                    AND parent.`OldCommentId` = child.`OldParentCommentId`
                SET child.`ParentCommentId` = parent.`Id`
                WHERE child.`OldParentCommentId` IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "OldBoardType",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "OldCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "OldParentCommentId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "CommunityComments");

            migrationBuilder.DropTable(
                name: "NoticeComments");

            migrationBuilder.DropTable(
                name: "PostComments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BoardType_BoardId",
                table: "Comments",
                columns: new[] { "BoardType", "BoardId" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.CreateTable(
                name: "CommunityComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommunityPostId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityComments_CommunityComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "CommunityComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommunityComments_CommunityPosts_CommunityPostId",
                        column: x => x.CommunityPostId,
                        principalTable: "CommunityPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NoticeComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NoticeId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeComments_NoticeComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "NoticeComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NoticeComments_Notices_NoticeId",
                        column: x => x.NoticeId,
                        principalTable: "Notices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoticeComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PostComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostComments_PostComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "PostComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostComments_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_CommunityPostId",
                table: "CommunityComments",
                column: "CommunityPostId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_ParentCommentId",
                table: "CommunityComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_UserId",
                table: "CommunityComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeComments_NoticeId",
                table: "NoticeComments",
                column: "NoticeId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeComments_ParentCommentId",
                table: "NoticeComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeComments_UserId",
                table: "NoticeComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_ParentCommentId",
                table: "PostComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostId",
                table: "PostComments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_UserId",
                table: "PostComments",
                column: "UserId");
        }
    }
}
