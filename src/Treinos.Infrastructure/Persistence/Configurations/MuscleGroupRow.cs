using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Treinos.Infrastructure.Persistence.Configurations;

/// <summary>
/// Uma linha do catálogo fixo de grupos musculares. Não é uma entidade do domínio (lá, o grupo é só o código);
/// existe para a tabela <c>muscle_groups</c>, alvo da chave estrangeira e fonte dos nomes de exibição.
/// </summary>
internal sealed class MuscleGroupRow {
    public required string Code { get; init; }

    public required string Name { get; init; }
}

internal sealed class MuscleGroupConfiguration : IEntityTypeConfiguration<MuscleGroupRow> {
    public void Configure(EntityTypeBuilder<MuscleGroupRow> builder) {
        builder.ToTable("muscle_groups", table => {
            table.HasCheckConstraint("ck_muscle_groups_code", "code ~ '^[a-z_]{1,30}$'");
            table.HasCheckConstraint("ck_muscle_groups_name", Checks.RequiredText("name", 50));
        });
        builder.HasKey(group => group.Code);

        // Criados pela migration; usuários nunca mudam este catálogo.
        builder.HasData(
            new MuscleGroupRow { Code = "chest", Name = "Chest" },
            new MuscleGroupRow { Code = "back", Name = "Back" },
            new MuscleGroupRow { Code = "shoulders", Name = "Shoulders" },
            new MuscleGroupRow { Code = "biceps", Name = "Biceps" },
            new MuscleGroupRow { Code = "triceps", Name = "Triceps" },
            new MuscleGroupRow { Code = "forearms", Name = "Forearms" },
            new MuscleGroupRow { Code = "quadriceps", Name = "Quadriceps" },
            new MuscleGroupRow { Code = "hamstrings", Name = "Hamstrings" },
            new MuscleGroupRow { Code = "glutes", Name = "Glutes" },
            new MuscleGroupRow { Code = "calves", Name = "Calves" },
            new MuscleGroupRow { Code = "abs", Name = "Abs" });
    }
}
