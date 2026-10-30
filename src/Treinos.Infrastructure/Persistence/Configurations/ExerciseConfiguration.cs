using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treinos.Domain.Exercises;
using Treinos.Domain.Users;

namespace Treinos.Infrastructure.Persistence.Configurations;

internal sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise> {
    public void Configure(EntityTypeBuilder<Exercise> builder) {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("uuidv7()");

        builder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);

        // Nome único dentro do escopo (sistema, ou um usuário), ignorando maiúsculas e acentos (decisões 7 e 10).
        builder.Property(e => e.Name).UseCollation(TreinosDbContext.IgnoreCaseAccents);
        builder.HasIndex(e => new { e.UserId, e.Name }).IsUnique().AreNullsDistinct(false);

        // Alvo da chave estrangeira composta que mantém a cópia do tipo nos treinos (decisão 8). O tipo nunca
        // muda no domínio, por isso pode ser parte de uma chave alternativa.
        builder.Property(e => e.MeasurementType).HasConversion(new SnakeCaseEnumConverter<MeasurementType>());
        builder.HasAlternateKey(e => new { e.Id, e.MeasurementType });

        // Grupos musculares ↔ tabela exercise_muscle_groups, com chave (exercise_id, muscle_group_code). A FK para
        // muscle_groups é criada na migration (o código é um objeto de valor, não uma entidade).
        builder.OwnsMany(e => e.Muscles, muscles => {
            muscles.ToTable("exercise_muscle_groups", table =>
                table.HasCheckConstraint("ck_exercise_muscle_groups_role", SnakeCaseEnum.CheckIn<MuscleRole>("role")));
            muscles.WithOwner().HasForeignKey("ExerciseId");
            muscles.Property(m => m.MuscleGroup)
                .HasColumnName("muscle_group_code")
                .HasConversion(group => group.Code, code => new MuscleGroup(code));
            muscles.Property(m => m.Role).HasConversion(new SnakeCaseEnumConverter<MuscleRole>());
            muscles.HasKey("ExerciseId", nameof(ExerciseMuscle.MuscleGroup));
        });

        builder.ToTable(table => {
            table.HasCheckConstraint("ck_exercises_name", Checks.RequiredText("name", 100));
            table.HasCheckConstraint(
                "ck_exercises_measurement_type", SnakeCaseEnum.CheckIn<MeasurementType>("measurement_type"));
        });

        builder.HasTimestamps();
    }
}

internal sealed class ExerciseAliasConfiguration : IEntityTypeConfiguration<ExerciseAlias> {
    public void Configure(EntityTypeBuilder<ExerciseAlias> builder) {
        builder.ToTable("exercise_aliases", table =>
            table.HasCheckConstraint("ck_exercise_aliases_term", Checks.RequiredText("term", 100)));
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("uuidv7()");

        builder.HasOne<Exercise>().WithMany().HasForeignKey(a => a.ExerciseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Term).UseCollation(TreinosDbContext.IgnoreCaseAccents);
        builder.HasIndex(a => new { a.UserId, a.Term }).IsUnique().AreNullsDistinct(false);
        builder.HasIndex(a => a.ExerciseId);

        builder.HasTimestamps();
    }
}
