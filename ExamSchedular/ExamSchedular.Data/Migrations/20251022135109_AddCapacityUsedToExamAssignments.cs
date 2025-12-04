using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamSchedular.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacityUsedToExamAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CapacityUsed",
                table: "ExamAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacityUsed",
                table: "ExamAssignments");
        }
    }
}
