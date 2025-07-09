using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizSite.Migrations
{
    /// <inheritdoc />
    public partial class AddBlankCountUserToQuizResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlankCount",
                table: "UserQuizResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlankCount",
                table: "UserQuizResults");
        }
    }
}
