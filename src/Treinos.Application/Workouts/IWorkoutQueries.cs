namespace Treinos.Application.Workouts;

/// <summary>Leitura: treinos completos direto no formato da resposta.</summary>
public interface IWorkoutQueries {
    /// <summary>Os treinos do usuário que começaram no intervalo [since, until), do mais recente para o mais antigo.</summary>
    Task<IReadOnlyList<WorkoutResponse>> ListAsync(
        Guid userId, DateTimeOffset? since, DateTimeOffset? until, CancellationToken ct);

    Task<WorkoutResponse?> FindAsync(Guid userId, Guid id, CancellationToken ct);
}
