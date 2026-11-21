using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Qwizzi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quiz_categories",
                columns: table => new
                {
                    quiz_category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_categories", x => x.quiz_category_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "image_assets",
                columns: table => new
                {
                    image_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    uploaded_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_assets", x => x.image_asset_id);
                    table.ForeignKey(
                        name: "FK_image_assets_users_uploaded_by_id",
                        column: x => x.uploaded_by_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quizzes",
                columns: table => new
                {
                    quiz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    cover_image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizzes", x => x.quiz_id);
                    table.ForeignKey(
                        name: "FK_quizzes_image_assets_cover_image_id",
                        column: x => x.cover_image_id,
                        principalTable: "image_assets",
                        principalColumn: "image_asset_id");
                    table.ForeignKey(
                        name: "FK_quizzes_quiz_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "quiz_categories",
                        principalColumn: "quiz_category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quizzes_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quizzes_users_verified_by_id",
                        column: x => x.verified_by_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    question_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quiz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.question_id);
                    table.ForeignKey(
                        name: "FK_questions_image_assets_image_id",
                        column: x => x.image_id,
                        principalTable: "image_assets",
                        principalColumn: "image_asset_id");
                    table.ForeignKey(
                        name: "FK_questions_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    quiz_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quiz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_attempts", x => x.quiz_attempt_id);
                    table.ForeignKey(
                        name: "FK_quiz_attempts_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "answers",
                columns: table => new
                {
                    answer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answers", x => x.answer_id);
                    table.ForeignKey(
                        name: "FK_answers_image_assets_image_id",
                        column: x => x.image_id,
                        principalTable: "image_assets",
                        principalColumn: "image_asset_id");
                    table.ForeignKey(
                        name: "FK_answers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_answers",
                columns: table => new
                {
                    user_answer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quiz_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_answer_id = table.Column<int>(type: "integer", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_answers", x => x.user_answer_id);
                    table.ForeignKey(
                        name: "FK_user_answers_answers_selected_answer_id",
                        column: x => x.selected_answer_id,
                        principalTable: "answers",
                        principalColumn: "answer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_answers_quiz_attempts_quiz_attempt_id",
                        column: x => x.quiz_attempt_id,
                        principalTable: "quiz_attempts",
                        principalColumn: "quiz_attempt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_answers_image_id",
                table: "answers",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "IX_answers_question_id",
                table: "answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_image_assets_storage_key",
                table: "image_assets",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_image_assets_uploaded_by_id",
                table: "image_assets",
                column: "uploaded_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_image_id",
                table: "questions",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_quiz_id",
                table: "questions",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_quiz_id",
                table: "quiz_attempts",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_user_id",
                table: "quiz_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_categories_name",
                table: "quiz_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_author_id",
                table: "quizzes",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_category_id",
                table: "quizzes",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_cover_image_id",
                table: "quizzes",
                column: "cover_image_id");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_status",
                table: "quizzes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_verified_by_id",
                table: "quizzes",
                column: "verified_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_quiz_attempt_id",
                table: "user_answers",
                column: "quiz_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_selected_answer_id",
                table: "user_answers",
                column: "selected_answer_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_answers");

            migrationBuilder.DropTable(
                name: "answers");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "quizzes");

            migrationBuilder.DropTable(
                name: "image_assets");

            migrationBuilder.DropTable(
                name: "quiz_categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
