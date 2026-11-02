using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treinos.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Rosca direta com halteres e rosca Scott na máquina, que faltavam no catálogo. Mesmos dados de
    /// docs/exercise-catalog.json (a fonte).
    /// </summary>
    public partial class AddBicepsCurlsToCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Rosca direta com halteres', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('biceps', 'primary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['rosca com halteres', 'rosca direta com halter', 'rosca com halter', 'dumbbell curl']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Rosca Scott na máquina', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('biceps', 'primary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['rosca scott', 'scott', 'rosca scott máquina', 'machine preacher curl', 'preacher curl']) AS t (term);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Apelidos e grupos musculares saem em cascata; falha se algum treino já usar o exercício.
            migrationBuilder.Sql("""
                DELETE FROM treinos.exercises
                WHERE user_id IS NULL AND name IN ('Rosca direta com halteres', 'Rosca Scott na máquina');
                """);
        }
    }
}
