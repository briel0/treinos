namespace Treinos.Application.Exercises;

/// <summary>Leitura: exercícios visíveis para o usuário (os do sistema e os dele), no formato da resposta.</summary>
public interface IExerciseQueries {
    /// <summary>Todos os exercícios visíveis, em ordem alfabética.</summary>
    Task<IReadOnlyList<ExerciseResponse>> ListAsync(Guid userId, CancellationToken ct);

    Task<ExerciseResponse?> FindAsync(Guid userId, Guid id, CancellationToken ct);

    /// <summary>
    /// O id do exercício que o termo identifica, ignorando maiúsculas e acentos, nesta ordem de precedência: nomes
    /// dos exercícios do usuário, apelidos do usuário, nomes do sistema, apelidos do sistema (decisão 7).
    /// </summary>
    Task<Guid?> ResolveTermAsync(Guid userId, string term, CancellationToken ct);
}
