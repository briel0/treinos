using Treinos.Domain.Exercises;

namespace Treinos.Application.Exercises;

/// <summary>Escrita: carrega exercícios como entidades do domínio.</summary>
public interface IExerciseRepository {
    /// <summary>O exercício, se existir e for visível para o usuário (do sistema ou dele).</summary>
    Task<Exercise?> FindVisibleAsync(Guid userId, Guid id, CancellationToken ct);
}
