using Treinos.Application.Common;
using Treinos.Domain.BodyWeights;

namespace Treinos.Application.BodyWeights;

/// <summary>Um registro de peso corporal como a API o devolve.</summary>
public sealed record BodyWeightResponse(DateOnly MeasuredOn, decimal WeightKg, DateTimeOffset UpdatedAt);

/// <summary>Leitura: registros de peso no formato da resposta.</summary>
public interface IBodyWeightQueries {
    /// <summary>Os registros do usuário no intervalo [since, until] (dias), do mais recente para o mais antigo.</summary>
    Task<IReadOnlyList<BodyWeightResponse>> ListAsync(Guid userId, DateOnly? since, DateOnly? until, CancellationToken ct);

    Task<BodyWeightResponse?> FindAsync(Guid userId, DateOnly measuredOn, CancellationToken ct);
}

/// <summary>Escrita: registros de peso como entidades do domínio.</summary>
public interface IBodyWeightRepository {
    Task<BodyWeight?> FindAsync(Guid userId, DateOnly measuredOn, CancellationToken ct);

    void Add(BodyWeight bodyWeight);

    void Remove(BodyWeight bodyWeight);
}

/// <summary>Os casos de uso do peso corporal, sempre do usuário atual. No máximo um registro por dia.</summary>
public sealed class BodyWeightUseCases(
    ICurrentUser user,
    IBodyWeightRepository bodyWeights,
    IBodyWeightQueries queries,
    IUnitOfWork unitOfWork) {
    public Task<IReadOnlyList<BodyWeightResponse>> ListAsync(DateOnly? since, DateOnly? until, CancellationToken ct) =>
        queries.ListAsync(user.Id, since, until, ct);

    /// <summary>
    /// Grava o peso do dia: cria o registro ou substitui o valor do que já existe. Repetir o pedido dá no mesmo,
    /// por isso o dia (e não um id) identifica o registro.
    /// </summary>
    public async Task<(BodyWeightResponse BodyWeight, bool Created)> RecordAsync(
        DateOnly measuredOn, decimal weightKg, CancellationToken ct) {
        var existing = await bodyWeights.FindAsync(user.Id, measuredOn, ct);
        if(existing is null) {
            bodyWeights.Add(BodyWeight.Create(Guid.CreateVersion7(), user.Id, measuredOn, weightKg));
        }
        else {
            existing.ChangeWeight(weightKg);
        }
        await unitOfWork.SaveChangesAsync(ct);
        var saved = await queries.FindAsync(user.Id, measuredOn, ct)
            ?? throw new InvalidOperationException("The body weight was saved but could not be read back.");
        return (saved, existing is null);
    }

    public async Task DeleteAsync(DateOnly measuredOn, CancellationToken ct) {
        bodyWeights.Remove(
            await bodyWeights.FindAsync(user.Id, measuredOn, ct) ?? throw UseCaseException.NotFound("Body weight"));
        await unitOfWork.SaveChangesAsync(ct);
    }
}
