using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamSchedular.Data.Migrations
{
    /// <inheritdoc />
    public partial class RolesAndDepartments2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "Timeslots",
                newName: "DurationMin");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Timeslots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ExamType",
                table: "Exams",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Exams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeslotId",
                table: "Exams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Timeslots_DepartmentId",
                table: "Timeslots",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_DepartmentId",
                table: "Students",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_DepartmentId",
                table: "Exams",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_TimeslotId",
                table: "Exams",
                column: "TimeslotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Departments_DepartmentId",
                table: "Exams",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Timeslots_TimeslotId",
                table: "Exams",
                column: "TimeslotId",
                principalTable: "Timeslots",
                principalColumn: "TimeslotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Departments_DepartmentId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Timeslots_TimeslotId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Timeslots_Departments_DepartmentId",
                table: "Timeslots");

            migrationBuilder.DropIndex(
                name: "IX_Timeslots_DepartmentId",
                table: "Timeslots");

            migrationBuilder.DropIndex(
                name: "IX_Students_DepartmentId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Exams_DepartmentId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_TimeslotId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Timeslots");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "TimeslotId",
                table: "Exams");

            migrationBuilder.RenameColumn(
                name: "DurationMin",
                table: "Timeslots",
                newName: "DurationMinutes");

            migrationBuilder.AlterColumn<string>(
                name: "ExamType",
                table: "Exams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);
        }
    }
}
