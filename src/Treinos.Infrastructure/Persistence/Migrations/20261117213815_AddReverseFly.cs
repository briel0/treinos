using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treinos.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Voador inverso (peck deck inverso, deltoide posterior), que faltava no catálogo. Mesmos dados de
    /// docs/exercise-catalog.json (a fonte).
    /// </summary>
    public partial class AddReverseFly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Voador inverso', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('shoulders', 'primary'), ('back', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['peck deck inverso', 'crucifixo inverso na máquina', 'reverse pec deck', 'reverse fly machine']) AS t (term);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Apelidos e grupos musculares saem em cascata; falha se algum treino já usar o exercício.
            migrationBuilder.Sql("""
                DELETE FROM treinos.exercises
                WHERE user_id IS NULL AND name = 'Voador inverso';
                """);
        }
    }
}
