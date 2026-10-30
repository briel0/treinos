using Treinos.Domain.Workouts;

namespace Treinos.Application.Workouts;

/// <summary>Escrita: carrega e guarda treinos inteiros (com exercícios e séries) como entidades do domínio.</summary>
public interface IWorkoutRepository {
    /// <summary>O treino com os exercícios e as séries, se existir e for do usuário.</summary>
    Task<Workout?> FindAsync(Guid userId, Guid id, CancellationToken ct);

    /// <summary>Se existe um treino com esse id, de qualquer usuário.</summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    void Add(Workout workout);

    void Remove(Workout workout);
}
