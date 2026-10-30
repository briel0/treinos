using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treinos.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Catálogo inicial de exercícios do sistema, com grupos musculares e apelidos. Gerado a partir de
    /// docs/exercise-catalog.json (a fonte); mudanças no catálogo entram por novas migrations.
    /// </summary>
    public partial class SeedExerciseCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Supino reto com barra', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('triceps', 'secondary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['supino', 'supino reto', 'bench press', 'barbell bench press']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Supino reto com halteres', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('triceps', 'secondary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['supino com halteres', 'supino com halter', 'supino reto com halter', 'dumbbell bench press']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Supino inclinado com barra', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('shoulders', 'secondary'), ('triceps', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['supino inclinado', 'incline bench press']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Supino inclinado com halteres', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('shoulders', 'secondary'), ('triceps', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['supino inclinado com halter', 'incline dumbbell press']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Supino fechado', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('triceps', 'primary'), ('chest', 'primary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['supino pegada fechada', 'close-grip bench press']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Crossover na polia alta', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['crossover', 'cross over', 'cable crossover']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Crossover unilateral', 'weight_reps', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['single-arm cable crossover']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Crucifixo no cabo', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['crucifixo na polia', 'cable fly']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Peck deck', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['voador', 'pec deck', 'fly machine']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Peck deck unilateral', 'weight_reps', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['voador unilateral', 'single-arm pec deck']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Barra fixa pronada', 'bodyweight', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'secondary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['barra fixa', 'barra pronada', 'pull-up', 'pull up']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Barra fixa supinada', 'bodyweight', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'primary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['barra supinada', 'chin-up', 'chin up']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Puxada alta', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['puxada', 'puxada na frente', 'pulldown', 'lat pulldown']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Puxada alta com triângulo', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['puxada com triângulo', 'puxada triângulo', 'close-grip pulldown']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Remada curvada com barra', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'secondary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['remada curvada', 'barbell row', 'bent-over row']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Remada na máquina', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['remada máquina', 'remada sentada', 'machine row', 'seated row']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Remada cavalinho', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'secondary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['cavalinho', 't-bar row']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Remada unilateral com halter', 'weight_reps', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('biceps', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['serrote', 'remada unilateral', 'one-arm dumbbell row']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Levantamento terra', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('back', 'primary'), ('glutes', 'primary'), ('hamstrings', 'primary'), ('quadriceps', 'secondary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['terra', 'deadlift']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Desenvolvimento com halteres', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('shoulders', 'primary'), ('triceps', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['desenvolvimento', 'desenvolvimento com halter', 'dumbbell shoulder press']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Elevação lateral com halteres', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('shoulders', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['elevação lateral', 'elevação lateral com halter', 'lateral raise']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Elevação lateral na polia', 'weight_reps', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('shoulders', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['elevação lateral no cabo', 'cable lateral raise']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Face pull', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('shoulders', 'primary'), ('back', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['facepull', 'puxada para o rosto']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Rosca direta com barra', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('biceps', 'primary'), ('forearms', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['rosca direta', 'rosca w', 'rosca com barra w', 'barbell curl', 'ez bar curl']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Rosca martelo', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('biceps', 'primary'), ('forearms', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['rosca martelo com halteres', 'hammer curl']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Rosca inclinada com halteres', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('biceps', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['rosca inclinada', 'incline dumbbell curl']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Mergulho nas paralelas', 'bodyweight', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('chest', 'primary'), ('triceps', 'primary'), ('shoulders', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['paralela', 'paralelas', 'mergulho', 'dip', 'dips']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Tríceps na corda', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('triceps', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['tríceps corda', 'tríceps corda na polia', 'rope pushdown']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Tríceps francês', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('triceps', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['francês', 'overhead triceps extension']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Tríceps testa', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('triceps', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['testa', 'skull crusher', 'lying triceps extension']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Agachamento livre', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('quadriceps', 'primary'), ('glutes', 'primary'), ('hamstrings', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['agachamento', 'agachamento com barra', 'back squat', 'squat']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Agachamento búlgaro', 'weight_reps', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('quadriceps', 'primary'), ('glutes', 'primary'), ('hamstrings', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['búlgaro', 'agachamento búlgaro com halteres', 'bulgarian split squat']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Leg press 45°', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('quadriceps', 'primary'), ('glutes', 'primary'), ('hamstrings', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['leg press', 'leg 45', 'leg press 45']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Cadeira extensora', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('quadriceps', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['extensora', 'leg extension']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Cadeira extensora unilateral', 'weight_reps', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('quadriceps', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['extensora unilateral', 'single-leg extension']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Stiff', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('hamstrings', 'primary'), ('glutes', 'secondary'), ('back', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['stiff com barra', 'stiff-legged deadlift']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Levantamento terra romeno', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('hamstrings', 'primary'), ('glutes', 'primary'), ('back', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['terra romeno', 'rdl', 'romanian deadlift']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Mesa flexora', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('hamstrings', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['flexora', 'lying leg curl']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Hip thrust', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('glutes', 'primary'), ('hamstrings', 'secondary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['elevação pélvica', 'hip thrust com barra']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Panturrilha em pé', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('calves', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['panturrilha', 'gêmeos em pé', 'standing calf raise']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Panturrilha unilateral', 'bodyweight', true)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('calves', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['single-leg calf raise']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Abdominal no cabo', 'weight_reps', false)
                    RETURNING id
                ),
                m AS (
                    INSERT INTO treinos.exercise_muscle_groups (exercise_id, muscle_group_code, role)
                    SELECT e.id, v.code, v.role FROM e, (VALUES ('abs', 'primary')) AS v (code, role)
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['abdominal na polia', 'abdominal ajoelhado no cabo', 'cable crunch']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Corrida na esteira', 'distance_time', false)
                    RETURNING id
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['esteira', 'treadmill', 'treadmill run']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Corrida', 'distance_time', false)
                    RETURNING id
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['corrida na rua', 'run', 'running']) AS t (term);

                WITH e AS (
                    INSERT INTO treinos.exercises (name, measurement_type, is_unilateral)
                    VALUES ('Bicicleta ergométrica', 'distance_time', false)
                    RETURNING id
                )
                INSERT INTO treinos.exercise_aliases (exercise_id, term)
                SELECT e.id, t.term FROM e, unnest(ARRAY['bike', 'bicicleta', 'ergométrica', 'hiit na bike', 'hiit na bicicleta', 'stationary bike']) AS t (term);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Apelidos e grupos musculares saem em cascata.
            migrationBuilder.Sql("DELETE FROM treinos.exercises WHERE user_id IS NULL;");
        }
    }
}
