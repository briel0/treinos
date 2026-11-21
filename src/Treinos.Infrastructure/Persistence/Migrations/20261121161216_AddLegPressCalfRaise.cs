using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treinos.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Panturrilha no leg press (flexão plantar na máquina de leg press), que faltava no catálogo. Mesmos dados de
    /// docs/exercise-catalog.json (a fonte).
    /// </summary>
    public partial class AddLegPressCalfRaise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Panturrilha no leg press', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('calves', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['panturrilha no leg', 'gêmeos no leg press', 'leg press calf raise']) AS t (term);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Apelidos e grupos musculares saem em cascata; falha se algum treino já usar o exercício.
            migrationBuilder.Sql("""
                DELETE FROM treinos.exercises
                WHERE user_id IS NULL AND name = 'Panturrilha no leg press';
                """);
        }
    }
}
