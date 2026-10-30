using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Treinos.Domain.BodyWeights;
using Treinos.Domain.Exercises;
using Treinos.Domain.Users;
using Treinos.Domain.Workouts;

namespace Treinos.Infrastructure.Persistence;

/// <summary>
/// O banco visto pelo C#: cada <see cref="DbSet{TEntity}"/> é uma tabela. O mapeamento de cada entidade fica
/// em <c>Configurations/</c>; o modelo físico de referência é docs/data-modeling/04-physical-model.md.
/// </summary>
public sealed class TreinosDbContext(DbContextOptions<TreinosDbContext> options) : DbContext(options) {
    /// <summary>Todo objeto do produto vive neste schema, inclusive o histórico de migrations.</summary>
    public const string Schema = "treinos";

    /// <summary>Collation que ignora maiúsculas e acentos, usada nos termos de busca (decisão 10).</summary>
    public const string IgnoreCaseAccents = "ignore_case_accents";

    public DbSet<User> Users => Set<User>();

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<ExerciseAlias> ExerciseAliases => Set<ExerciseAlias>();

    public DbSet<Workout> Workouts => Set<Workout>();

    public DbSet<BodyWeight> BodyWeights => Set<BodyWeight>();

    /// <summary>
    /// Sem índices automáticos em chaves estrangeiras: cada índice é declarado de propósito, a partir das
    /// consultas reais (ver a seção Indexes do modelo físico).
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasCollation(
            Schema, IgnoreCaseAccents, locale: "und-u-ks-level1", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TreinosDbContext).Assembly);
    }
}
