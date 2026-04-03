using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentorsAndStudents.Migrations
{
    /// <inheritdoc />
    public partial class AddedClassLetterAndBoolToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClass",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SchoolClassLetter",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClass",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SchoolClassLetter",
                table: "Courses");
        }
    }
}
