using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizSite.Migrations
{
    /// <inheritdoc />
    public partial class QuestionCorrectAnswerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectOptionIndex",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectOptionIndex",
                table: "Questions");
        }
    }
}
