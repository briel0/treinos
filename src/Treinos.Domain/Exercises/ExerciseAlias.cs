namespace Treinos.Domain.Exercises;

/// <summary>
/// Outro jeito de chamar um exercício ("paralela" para o Mergulho nas paralelas). Pode ser do sistema (visível
/// para todos) ou de um usuário (visível só para ele), inclusive em exercícios do sistema.
/// <para>
/// A unicidade do termo dentro do escopo depende dos outros apelidos, e por isso é verificada fora do domínio
/// (Application e índice do banco). Entre escopos, o termo do usuário tem precedência na busca (decisão 7).
/// </para>
/// </summary>
public sealed class ExerciseAlias {
    private ExerciseAlias(Guid id, Guid exerciseId, Guid? userId, string term) {
        Id = id;
        ExerciseId = exerciseId;
        UserId = userId;
        Term = term;
    }

    public Guid Id { get; }

    public Guid ExerciseId { get; }

    /// <summary>Dono do apelido; vazio nos apelidos do sistema.</summary>
    public Guid? UserId { get; }

    public string Term { get; private set; }

    /// <param name="id">Escolhido pelo cliente (UUID v7), o que torna a criação idempotente.</param>
    /// <param name="exercise">
    /// O exercício apelidado. Um apelido do sistema só aponta para exercícios do sistema; o de um usuário, para
    /// exercícios que ele enxerga.
    /// </param>
    /// <param name="userId">Dono do apelido, ou vazio para um apelido do sistema.</param>
    public static ExerciseAlias Create(Guid id, Exercise exercise, Guid? userId, string term) {
        ArgumentNullException.ThrowIfNull(exercise);
        var visible = userId is { } owner ? exercise.IsVisibleTo(owner) : exercise.IsSystem;
        if(!visible) {
            throw new DomainException(
                "alias.exercise-not-visible", "An alias can only point to an exercise its owner can see.");
        }
        return new(EntityId.Require(id), exercise.Id, userId, NormalizeTerm(term));
    }

    public void ChangeTerm(string term) => Term = NormalizeTerm(term);

    private static string NormalizeTerm(string term) =>
        SearchTerm.Normalize(term, "alias.invalid-term", "An alias");
}
