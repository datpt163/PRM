using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssueTableV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_issues_issues_parentIssueId",
                table: "issues");

            migrationBuilder.AddForeignKey(
                name: "FK_issues_issues_parentIssueId",
                table: "issues",
                column: "parentIssueId",
                principalTable: "issues",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_issues_issues_parentIssueId",
                table: "issues");

            migrationBuilder.AddForeignKey(
                name: "FK_issues_issues_parentIssueId",
                table: "issues",
                column: "parentIssueId",
                principalTable: "issues",
                principalColumn: "id");
        }
    }
}
