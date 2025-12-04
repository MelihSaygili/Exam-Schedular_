using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamSchedular.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacityUsedToExamAssignments1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql(@"
        ALTER TABLE ""ExamAssignments""
            ADD COLUMN IF NOT EXISTS ""CapacityUsed"" integer;

        UPDATE ""ExamAssignments"" SET ""CapacityUsed"" = COALESCE(""CapacityUsed"", 0);

        ALTER TABLE ""ExamAssignments""
            ALTER COLUMN ""CapacityUsed"" SET DEFAULT 0,
            ALTER COLUMN ""CapacityUsed"" SET NOT NULL;
    ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
