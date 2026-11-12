using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treinos.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Rosca alternada com halteres e rosca concentrada na máquina, que faltavam no catálogo. Mesmos dados de
    /// docs/exercise-catalog.json (a fonte).
    /// </summary>
    public partial class AddAlternatingAndConcentrationCurls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Rosca alternada com halteres', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('biceps', 'primary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['rosca alternada', 'rosca alternada com halter', 'alternating dumbbell curl']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Rosca concentrada na máquina', 'weight_reps', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('biceps', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['rosca concentrada', 'rosca concentrada máquina', 'machine concentration curl', 'concentration curl']) AS t (term);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Apelidos e grupos musculares saem em cascata; falha se algum treino já usar o exercício.
            migrationBuilder.Sql("""
                DELETE FROM treinos.exercises
                WHERE user_id IS NULL AND name IN ('Rosca alternada com halteres', 'Rosca concentrada na máquina');
                """);
        }
    }
}
