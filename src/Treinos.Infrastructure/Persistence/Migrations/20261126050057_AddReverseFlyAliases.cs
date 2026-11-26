using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treinos.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Apelidos com "invertido" para o Voador inverso: em 25/11/2026 "peck deck invertido" não foi reconhecido e as
    /// séries foram para o INBOX da ALICE. Mesmos dados de docs/exercise-catalog.json (a fonte).
    /// </summary>
    public partial class AddReverseFlyAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term
                FROM treinos.exercises AS e, unnest(ARRAY['peck deck invertido', 'voador invertido', 'crucifixo invertido']) AS t (term)
                WHERE e.user_id IS NULL AND e.name = 'Voador inverso';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM treinos.exercise_aliases
                WHERE term IN ('peck deck invertido', 'voador invertido', 'crucifixo invertido')
                  AND exercise_id IN (SELECT id FROM treinos.exercises WHERE user_id IS NULL AND name = 'Voador inverso');
                """);
        }
    }
}
