using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Treinos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "treinos");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:treinos.ignore_case_accents", "und-u-ks-level1,und-u-ks-level1,icu,False");

            migrationBuilder.CreateTable(
                name: "muscle_groups",
                schema: "treinos",
                columns: table => new
                {
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_muscle_groups", x => x.code);
                    table.CheckConstraint("ck_muscle_groups_code", "code ~ '^[a-z_]{1,30}$'");
                    table.CheckConstraint("ck_muscle_groups_name", "name = btrim(name) AND length(name) BETWEEN 1 AND 50");
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "treinos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_email", "length(email) <= 254 AND email = lower(email)");
                    table.CheckConstraint("ck_users_name", "name = btrim(name) AND length(name) BETWEEN 1 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "body_weights",
                schema: "treinos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    measured_on = table.Column<DateOnly>(type: "date", nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_body_weights", x => x.id);
                    table.CheckConstraint("ck_body_weights_weight_kg", "weight_kg > 0");
                    table.ForeignKey(
                        name: "fk_body_weights_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "treinos",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercises",
                schema: "treinos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false, collation: "ignore_case_accents"),
                    measurement_type = table.Column<string>(type: "text", nullable: false),
                    is_unilateral = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exercises", x => x.id);
                    table.UniqueConstraint("ak_exercises_id_measurement_type", x => new { x.id, x.measurement_type });
                    table.CheckConstraint("ck_exercises_measurement_type", "measurement_type IN ('weight_reps', 'bodyweight', 'time', 'distance_time')");
                    table.CheckConstraint("ck_exercises_name", "name = btrim(name) AND length(name) BETWEEN 1 AND 100");
                    table.ForeignKey(
                        name: "fk_exercises_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "treinos",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workouts",
                schema: "treinos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    duration_sec = table.Column<int>(type: "integer", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workouts", x => x.id);
                    table.CheckConstraint("ck_workouts_duration_sec", "duration_sec > 0");
                    table.CheckConstraint("ck_workouts_note", "length(note) <= 2000");
                    table.ForeignKey(
                        name: "fk_workouts_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "treinos",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_aliases",
                schema: "treinos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    term = table.Column<string>(type: "text", nullable: false, collation: "ignore_case_accents"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exercise_aliases", x => x.id);
                    table.CheckConstraint("ck_exercise_aliases_term", "term = btrim(term) AND length(term) BETWEEN 1 AND 100");
                    table.ForeignKey(
                        name: "fk_exercise_aliases_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalSchema: "treinos",
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exercise_aliases_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "treinos",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_muscle_groups",
                schema: "treinos",
                columns: table => new
                {
                    muscle_group_code = table.Column<string>(type: "text", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exercise_muscle_groups", x => new { x.exercise_id, x.muscle_group_code });
                    table.CheckConstraint("ck_exercise_muscle_groups_role", "role IN ('primary', 'secondary')");
                    table.ForeignKey(
                        name: "fk_exercise_muscle_groups_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalSchema: "treinos",
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_exercises",
                schema: "treinos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    measurement_type = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    workout_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workout_exercises", x => x.id);
                    table.UniqueConstraint("ak_workout_exercises_id_measurement_type", x => new { x.id, x.measurement_type });
                    table.CheckConstraint("ck_workout_exercises_sort_order", "sort_order >= 0");
                    table.ForeignKey(
                        name: "fk_workout_exercises_exercises_exercise_id_measurement_type",
                        columns: x => new { x.exercise_id, x.measurement_type },
                        principalSchema: "treinos",
                        principalTable: "exercises",
                        principalColumns: new[] { "id", "measurement_type" });
                    table.ForeignKey(
                        name: "fk_workout_exercises_workouts_workout_id",
                        column: x => x.workout_id,
                        principalSchema: "treinos",
                        principalTable: "workouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_sets",
                schema: "treinos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    set_type = table.Column<string>(type: "text", nullable: false),
                    is_failed = table.Column<bool>(type: "boolean", nullable: false),
                    reps = table.Column<int>(type: "integer", nullable: true),
                    load_kg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    assistance_kg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    duration_sec = table.Column<int>(type: "integer", nullable: true),
                    distance_m = table.Column<int>(type: "integer", nullable: true),
                    side = table.Column<string>(type: "text", nullable: true),
                    measurement_type = table.Column<string>(type: "text", nullable: false),
                    workout_exercise_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workout_sets", x => x.id);
                    table.CheckConstraint("ck_workout_sets_assistance_kg", "assistance_kg > 0");
                    table.CheckConstraint("ck_workout_sets_distance_m", "distance_m > 0");
                    table.CheckConstraint("ck_workout_sets_duration_sec", "duration_sec > 0");
                    table.CheckConstraint("ck_workout_sets_load_kg", "load_kg >= 0");
                    table.CheckConstraint("ck_workout_sets_measurement_type", "measurement_type IN ('weight_reps', 'bodyweight', 'time', 'distance_time')");
                    table.CheckConstraint("ck_workout_sets_measurements", "(measurement_type = 'weight_reps'\n    AND reps IS NOT NULL AND load_kg IS NOT NULL\n    AND assistance_kg IS NULL AND duration_sec IS NULL AND distance_m IS NULL)\nOR (measurement_type = 'bodyweight'\n    AND reps IS NOT NULL\n    AND NOT (load_kg IS NOT NULL AND assistance_kg IS NOT NULL)\n    AND duration_sec IS NULL AND distance_m IS NULL)\nOR (measurement_type = 'time'\n    AND duration_sec IS NOT NULL\n    AND reps IS NULL AND load_kg IS NULL AND assistance_kg IS NULL AND distance_m IS NULL)\nOR (measurement_type = 'distance_time'\n    AND distance_m IS NOT NULL AND duration_sec IS NOT NULL\n    AND reps IS NULL AND load_kg IS NULL AND assistance_kg IS NULL)");
                    table.CheckConstraint("ck_workout_sets_reps", "reps >= 0");
                    table.CheckConstraint("ck_workout_sets_set_type", "set_type IN ('warmup', 'working')");
                    table.CheckConstraint("ck_workout_sets_side", "side IN ('left', 'right')");
                    table.CheckConstraint("ck_workout_sets_sort_order", "sort_order >= 0");
                    table.ForeignKey(
                        name: "fk_workout_sets_workout_exercises_workout_exercise_id_measurem",
                        columns: x => new { x.workout_exercise_id, x.measurement_type },
                        principalSchema: "treinos",
                        principalTable: "workout_exercises",
                        principalColumns: new[] { "id", "measurement_type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "treinos",
                table: "muscle_groups",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "abs", "Abs" },
                    { "back", "Back" },
                    { "biceps", "Biceps" },
                    { "calves", "Calves" },
                    { "chest", "Chest" },
                    { "forearms", "Forearms" },
                    { "glutes", "Glutes" },
                    { "hamstrings", "Hamstrings" },
                    { "quadriceps", "Quadriceps" },
                    { "shoulders", "Shoulders" },
                    { "triceps", "Triceps" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_body_weights_user_id_measured_on",
                schema: "treinos",
                table: "body_weights",
                columns: new[] { "user_id", "measured_on" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exercise_aliases_exercise_id",
                schema: "treinos",
                table: "exercise_aliases",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "ix_exercise_aliases_user_id_term",
                schema: "treinos",
                table: "exercise_aliases",
                columns: new[] { "user_id", "term" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_exercises_user_id_name",
                schema: "treinos",
                table: "exercises",
                columns: new[] { "user_id", "name" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "treinos",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workout_exercises_exercise_id",
                schema: "treinos",
                table: "workout_exercises",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "ix_workouts_user_id_started_at",
                schema: "treinos",
                table: "workouts",
                columns: new[] { "user_id", "started_at" });

            // O que o EF Core não gera (ver docs/data-modeling/04-physical-model.md).

            // Grupo muscular de um exercício precisa existir no catálogo (o código é um objeto de valor no domínio).
            migrationBuilder.Sql("""
                ALTER TABLE treinos.exercise_muscle_groups ADD CONSTRAINT fk_exercise_muscle_groups_muscle_groups
                    FOREIGN KEY (muscle_group_code) REFERENCES treinos.muscle_groups (code);
                """);

            // Ordem única dentro do dono, conferida só no fim da transação: remover um exercício e renumerar os
            // seguintes passa por duplicatas no meio do caminho.
            migrationBuilder.Sql("""
                ALTER TABLE treinos.workout_exercises ADD CONSTRAINT ux_workout_exercises_workout_id_sort_order
                    UNIQUE (workout_id, sort_order) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE treinos.workout_sets ADD CONSTRAINT ux_workout_sets_workout_exercise_id_sort_order
                    UNIQUE (workout_exercise_id, sort_order) DEFERRABLE INITIALLY DEFERRED;
                """);

            // updated_at = agora, em toda alteração de linha.
            migrationBuilder.Sql("""
                CREATE FUNCTION treinos.set_updated_at() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    NEW.updated_at = now();
                    RETURN NEW;
                END
                $$;

                CREATE TRIGGER trg_users_updated_at BEFORE UPDATE ON treinos.users
                    FOR EACH ROW EXECUTE FUNCTION treinos.set_updated_at();
                CREATE TRIGGER trg_exercises_updated_at BEFORE UPDATE ON treinos.exercises
                    FOR EACH ROW EXECUTE FUNCTION treinos.set_updated_at();
                CREATE TRIGGER trg_exercise_aliases_updated_at BEFORE UPDATE ON treinos.exercise_aliases
                    FOR EACH ROW EXECUTE FUNCTION treinos.set_updated_at();
                CREATE TRIGGER trg_workouts_updated_at BEFORE UPDATE ON treinos.workouts
                    FOR EACH ROW EXECUTE FUNCTION treinos.set_updated_at();
                CREATE TRIGGER trg_body_weights_updated_at BEFORE UPDATE ON treinos.body_weights
                    FOR EACH ROW EXECUTE FUNCTION treinos.set_updated_at();
                """);

            // Mudar exercícios ou séries de um treino conta como mudar o treino ("o que mudou desde…").
            migrationBuilder.Sql("""
                CREATE FUNCTION treinos.touch_workout_from_entry() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE treinos.workouts SET updated_at = now()
                    WHERE id = CASE WHEN TG_OP = 'DELETE' THEN OLD.workout_id ELSE NEW.workout_id END;
                    RETURN NULL;
                END
                $$;

                CREATE FUNCTION treinos.touch_workout_from_set() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE treinos.workouts SET updated_at = now()
                    WHERE id = (
                        SELECT workout_id FROM treinos.workout_exercises
                        WHERE id = CASE WHEN TG_OP = 'DELETE' THEN OLD.workout_exercise_id ELSE NEW.workout_exercise_id END);
                    RETURN NULL;
                END
                $$;

                CREATE TRIGGER trg_workout_exercises_touch_workout AFTER INSERT OR UPDATE OR DELETE ON treinos.workout_exercises
                    FOR EACH ROW EXECUTE FUNCTION treinos.touch_workout_from_entry();
                CREATE TRIGGER trg_workout_sets_touch_workout AFTER INSERT OR UPDATE OR DELETE ON treinos.workout_sets
                    FOR EACH ROW EXECUTE FUNCTION treinos.touch_workout_from_set();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER trg_workout_sets_touch_workout ON treinos.workout_sets;
                DROP TRIGGER trg_workout_exercises_touch_workout ON treinos.workout_exercises;
                DROP FUNCTION treinos.touch_workout_from_set();
                DROP FUNCTION treinos.touch_workout_from_entry();
                DROP TRIGGER trg_users_updated_at ON treinos.users;
                DROP TRIGGER trg_exercises_updated_at ON treinos.exercises;
                DROP TRIGGER trg_exercise_aliases_updated_at ON treinos.exercise_aliases;
                DROP TRIGGER trg_workouts_updated_at ON treinos.workouts;
                DROP TRIGGER trg_body_weights_updated_at ON treinos.body_weights;
                DROP FUNCTION treinos.set_updated_at();
                """);

            migrationBuilder.DropTable(
                name: "body_weights",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "exercise_aliases",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "exercise_muscle_groups",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "muscle_groups",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "workout_sets",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "workout_exercises",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "exercises",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "workouts",
                schema: "treinos");

            migrationBuilder.DropTable(
                name: "users",
                schema: "treinos");
        }
    }
}
