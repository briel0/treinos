using Treinos.Application.Common;

namespace Treinos.Application.Exercises;

/// <summary>Os casos de uso do catálogo de exercícios, do ponto de vista do usuário atual.</summary>
public sealed class ExerciseUseCases(ICurrentUser user, IExerciseQueries queries) {
    public Task<IReadOnlyList<ExerciseResponse>> ListAsync(CancellationToken ct) => queries.ListAsync(user.Id, ct);

    public async Task<ExerciseResponse> GetAsync(Guid id, CancellationToken ct) =>
        await queries.FindAsync(user.Id, id, ct) ?? throw UseCaseException.NotFound("Exercise");

    /// <summary>
    /// O exercício que o termo identifica ("paralela" → Mergulho nas paralelas). Quem registra pelo Telegram usa
    /// isto para achar o exercício certo; se nada bater, a resposta é 404 e o agente pergunta ao usuário em vez
    /// de criar um exercício novo.
    /// </summary>
    public async Task<ExerciseResponse> ResolveAsync(string term, CancellationToken ct) {
        var normalized = string.Join(' ', (term ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if(normalized.Length == 0 || await queries.ResolveTermAsync(user.Id, normalized, ct) is not { } id) {
            throw new UseCaseException(
                UseCaseErrorKind.NotFound, "exercise.term-not-found", $"No exercise is known as '{term}'.");
        }
        return await GetAsync(id, ct);
    }
}
