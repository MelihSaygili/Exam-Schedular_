using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamSchedular.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacityUsedToExamAssignments2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Departments_DepartmentId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Timeslots_Departments_DepartmentId",
                table: "Timeslots");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId1",
                table: "Timeslots",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Timeslots_DepartmentId1",
                table: "Timeslots",
                column: "DepartmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Departments_DepartmentId",
                table: "Exams",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Timeslots_Departments_DepartmentId",
                table: "Timeslots",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Timeslots_Departments_DepartmentId1",
                table: "Timeslots",
                column: "DepartmentId1",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Departments_DepartmentId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Timeslots_Departments_DepartmentId",
                table: "Timeslots");

            migrationBuilder.DropForeignKey(
                name: "FK_Timeslots_Departments_DepartmentId1",
                table: "Timeslots");

            migrationBuilder.DropIndex(
                name: "IX_Timeslots_DepartmentId1",
                table: "Timeslots");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "Timeslots");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Departments_DepartmentId",
                table: "Exams",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Timeslots_Departments_DepartmentId",
                table: "Timeslots",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
