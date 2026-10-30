using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treinos.Domain.Exercises;
using Treinos.Domain.Users;
using Treinos.Domain.Workouts;

namespace Treinos.Infrastructure.Persistence.Configurations;

internal sealed class WorkoutConfiguration : IEntityTypeConfiguration<Workout> {
    public void Configure(EntityTypeBuilder<Workout> builder) {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasDefaultValueSql("uuidv7()");

        builder.HasOne<User>().WithMany().HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);

        // O Npgsql só grava instantes em UTC em timestamptz; o instante é o mesmo, só muda a representação.
        builder.Property(w => w.StartedAt).HasConversion(value => value.ToUniversalTime(), value => value);

        builder.HasMany(w => w.Exercises)
            .WithOne()
            .HasForeignKey("WorkoutId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.UserId, w.StartedAt });

        builder.ToTable(table => {
            table.HasCheckConstraint("ck_workouts_duration_sec", "duration_sec > 0");
            table.HasCheckConstraint("ck_workouts_note", $"length(note) <= {Workout.NoteMaxLength}");
        });

        builder.HasTimestamps();
    }
}

internal sealed class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise> {
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder) {
        builder.ToTable("workout_exercises", table =>
            table.HasCheckConstraint("ck_workout_exercises_sort_order", "sort_order >= 0"));

        // Ids vêm sempre do cliente: sem geração pelo EF, o que faz um exercício novo num treino já carregado
        // ser tratado como inserção. O valor padrão no banco fica para quem escrever direto em SQL.
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedNever();

        // Cópia do tipo de medição, garantida pela FK composta (exercise_id, measurement_type) → exercises
        // (id, measurement_type), que também impede mudar o tipo de um exercício em uso (decisões 6 e 8). NO
        // ACTION: apagar um exercício em uso falha, mas apagar o usuário (que leva treinos e exercícios juntos)
        // não.
        builder.Property(e => e.MeasurementType).HasConversion(new SnakeCaseEnumConverter<MeasurementType>());
        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(e => new { e.ExerciseId, e.MeasurementType })
            .HasPrincipalKey(exercise => new { exercise.Id, exercise.MeasurementType })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(e => e.ExerciseId);

        // Alvo da FK composta das séries; as séries recebem a cópia do tipo daqui, sem o domínio saber.
        builder.HasAlternateKey(e => new { e.Id, e.MeasurementType });
        builder.HasMany(e => e.Sets)
            .WithOne()
            .HasForeignKey("WorkoutExerciseId", "MeasurementType")
            .HasPrincipalKey(e => new { e.Id, e.MeasurementType })
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // A unicidade (workout_id, sort_order), adiável, é criada na migration: o EF Core não gera restrições
        // adiáveis. O índice dela também serve a cascata a partir de workouts.
    }
}

internal sealed class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet> {
    public void Configure(EntityTypeBuilder<WorkoutSet> builder) {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedNever();

        // Propriedades sem setter não são mapeadas por convenção: o EF Core as preenche pelos campos.
        builder.Property(s => s.SortOrder);
        builder.Property(s => s.IsFailed);
        builder.Property(s => s.Reps);
        builder.Property(s => s.DurationSec);
        builder.Property(s => s.DistanceM);
        builder.Property<MeasurementType>("MeasurementType").HasConversion(new SnakeCaseEnumConverter<MeasurementType>());
        builder.Property(s => s.Type).HasColumnName("set_type").HasConversion(new SnakeCaseEnumConverter<SetType>());
        builder.Property(s => s.Side).HasConversion(new SnakeCaseEnumConverter<Side>());
        builder.Property(s => s.LoadKg).HasPrecision(6, 2);
        builder.Property(s => s.AssistanceKg).HasPrecision(6, 2);

        builder.ToTable("workout_sets", table => {
            table.HasCheckConstraint("ck_workout_sets_sort_order", "sort_order >= 0");
            table.HasCheckConstraint("ck_workout_sets_measurement_type", SnakeCaseEnum.CheckIn<MeasurementType>("measurement_type"));
            table.HasCheckConstraint("ck_workout_sets_set_type", SnakeCaseEnum.CheckIn<SetType>("set_type"));
            table.HasCheckConstraint("ck_workout_sets_side", SnakeCaseEnum.CheckIn<Side>("side"));
            table.HasCheckConstraint("ck_workout_sets_reps", "reps >= 0");
            table.HasCheckConstraint("ck_workout_sets_load_kg", "load_kg >= 0");
            table.HasCheckConstraint("ck_workout_sets_assistance_kg", "assistance_kg > 0");
            table.HasCheckConstraint("ck_workout_sets_duration_sec", "duration_sec > 0");
            table.HasCheckConstraint("ck_workout_sets_distance_m", "distance_m > 0");
            table.HasCheckConstraint("ck_workout_sets_measurements", MeasurementsByType);
        });

        // A unicidade (workout_exercise_id, sort_order), adiável, é criada na migration.
    }

    /// <summary>Quais medidas cada tipo preenche (decisões 3, 4 e 8), conferido na própria linha.</summary>
    private const string MeasurementsByType = """
        (measurement_type = 'weight_reps'
            AND reps IS NOT NULL AND load_kg IS NOT NULL
            AND assistance_kg IS NULL AND duration_sec IS NULL AND distance_m IS NULL)
        OR (measurement_type = 'bodyweight'
            AND reps IS NOT NULL
            AND NOT (load_kg IS NOT NULL AND assistance_kg IS NOT NULL)
            AND duration_sec IS NULL AND distance_m IS NULL)
        OR (measurement_type = 'time'
            AND duration_sec IS NOT NULL
            AND reps IS NULL AND load_kg IS NULL AND assistance_kg IS NULL AND distance_m IS NULL)
        OR (measurement_type = 'distance_time'
            AND distance_m IS NOT NULL AND duration_sec IS NOT NULL
            AND reps IS NULL AND load_kg IS NULL AND assistance_kg IS NULL)
        """;
}
