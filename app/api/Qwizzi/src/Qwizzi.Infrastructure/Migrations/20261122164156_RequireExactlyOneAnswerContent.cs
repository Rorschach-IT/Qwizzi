using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qwizzi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequireExactlyOneAnswerContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "text",
                table: "answers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddCheckConstraint(
                name: "ck_answers_exactly_one_text_or_image",
                table: "answers",
                sql: "(NULLIF(BTRIM(text), '') IS NOT NULL) <> (image_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_answers_exactly_one_text_or_image",
                table: "answers");

            migrationBuilder.AlterColumn<string>(
                name: "text",
                table: "answers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
