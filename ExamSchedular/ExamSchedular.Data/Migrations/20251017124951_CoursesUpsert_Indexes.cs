using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamSchedular.Data.Migrations
{
    /// <inheritdoc />
    public partial class CoursesUpsert_Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courses_Code",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_DepartmentId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_Code",
                table: "Classrooms");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DepartmentId_Code",
                table: "Courses",
                columns: new[] { "DepartmentId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courses_DepartmentId_Code",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Code",
                table: "Courses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DepartmentId",
                table: "Courses",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_Code",
                table: "Classrooms",
                column: "Code",
                unique: true);
        }
    }
}
