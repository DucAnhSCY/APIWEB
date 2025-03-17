using Microsoft.EntityFrameworkCore.Migrations;

namespace diendan2.Migrations;

public partial class AddIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add indexes for frequently queried columns
        migrationBuilder.CreateIndex(
            name: "IX_Posts_ThreadId",
            table: "Post",
            column: "thread_id");

        migrationBuilder.CreateIndex(
            name: "IX_Posts_UserId",
            table: "Post",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_PostId",
            table: "Comment",
            column: "post_id");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_UserId",
            table: "Comment",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Likes_PostId",
            table: "Likes",
            column: "post_id");

        migrationBuilder.CreateIndex(
            name: "IX_Likes_UserId",
            table: "Likes",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Threads_CategoryId",
            table: "Thread",
            column: "category_ID");

        migrationBuilder.CreateIndex(
            name: "IX_Threads_UserId",
            table: "Thread",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Threads_CreatedAt",
            table: "Thread",
            column: "createdAt");

        migrationBuilder.CreateIndex(
            name: "IX_Posts_CreatedAt",
            table: "Post",
            column: "createdAt");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_CreatedAt",
            table: "Comment",
            column: "createdAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove indexes in reverse order
        migrationBuilder.DropIndex(
            name: "IX_Comments_CreatedAt",
            table: "Comment");

        migrationBuilder.DropIndex(
            name: "IX_Posts_CreatedAt",
            table: "Post");

        migrationBuilder.DropIndex(
            name: "IX_Threads_CreatedAt",
            table: "Thread");

        migrationBuilder.DropIndex(
            name: "IX_Threads_UserId",
            table: "Thread");

        migrationBuilder.DropIndex(
            name: "IX_Threads_CategoryId",
            table: "Thread");

        migrationBuilder.DropIndex(
            name: "IX_Likes_UserId",
            table: "Likes");

        migrationBuilder.DropIndex(
            name: "IX_Likes_PostId",
            table: "Likes");

        migrationBuilder.DropIndex(
            name: "IX_Comments_UserId",
            table: "Comment");

        migrationBuilder.DropIndex(
            name: "IX_Comments_PostId",
            table: "Comment");

        migrationBuilder.DropIndex(
            name: "IX_Posts_UserId",
            table: "Post");

        migrationBuilder.DropIndex(
            name: "IX_Posts_ThreadId",
            table: "Post");
    }
} 