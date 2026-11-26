using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qwizzi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizSecurityAndImageAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "lockout_level",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_until",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "must_change_password",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "temporary_password_expires_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "security_audit_state",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    last_sequence = table.Column<long>(type: "bigint", nullable: false),
                    last_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_audit_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "security_events",
                columns: table => new
                {
                    sequence = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    previous_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entry_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_events", x => x.sequence);
                });

            migrationBuilder.InsertData(
                table: "security_audit_state",
                columns: new[] { "id", "last_sequence", "last_hash" },
                values: new object[] { 1, 0L, "" });

            migrationBuilder.Sql("""
                CREATE FUNCTION reject_security_event_mutation() RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'security events are append-only';
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER security_events_are_append_only
                    BEFORE UPDATE OR DELETE ON security_events
                    FOR EACH ROW EXECUTE FUNCTION reject_security_event_mutation();
                CREATE FUNCTION protect_security_audit_state() RETURNS trigger AS $$
                BEGIN
                    IF TG_OP = 'DELETE' OR current_setting('qwizzi.audit_write', true) IS DISTINCT FROM 'on' THEN
                        RAISE EXCEPTION 'security audit state is protected';
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER security_audit_state_is_protected
                    BEFORE UPDATE OR DELETE ON security_audit_state
                    FOR EACH ROW EXECUTE FUNCTION protect_security_audit_state();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS security_audit_state_is_protected ON security_audit_state;
                DROP TRIGGER IF EXISTS security_events_are_append_only ON security_events;
                DROP FUNCTION IF EXISTS protect_security_audit_state();
                DROP FUNCTION IF EXISTS reject_security_event_mutation();
                """);

            migrationBuilder.DropTable(
                name: "security_audit_state");

            migrationBuilder.DropTable(
                name: "security_events");

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_level",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_until",
                table: "users");

            migrationBuilder.DropColumn(
                name: "must_change_password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "temporary_password_expires_at",
                table: "users");
        }
    }
}
