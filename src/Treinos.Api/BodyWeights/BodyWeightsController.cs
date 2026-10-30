using Microsoft.AspNetCore.Mvc;
using Treinos.Application.BodyWeights;

namespace Treinos.Api.BodyWeights;

/// <param name="WeightKg">Peso em kg, até duas casas decimais.</param>
public sealed record RecordBodyWeightRequest(decimal WeightKg);

/// <summary>O peso corporal do usuário atual, no máximo um registro por dia.</summary>
[ApiController]
[Route("api/body-weights")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class BodyWeightsController(BodyWeightUseCases bodyWeights) : ControllerBase {
    /// <summary>Os registros entre os dias since e until (inclusive), do mais recente para o mais antigo.</summary>
    [HttpGet(Name = "ListBodyWeights")]
    public Task<IReadOnlyList<BodyWeightResponse>> List(
        [FromQuery] DateOnly? since, [FromQuery] DateOnly? until, CancellationToken ct) =>
        bodyWeights.ListAsync(since, until, ct);

    /// <summary>Grava o peso do dia (<c>2026-10-30</c>): cria ou substitui. Repetir o pedido dá no mesmo.</summary>
    [HttpPut("{date}", Name = "RecordBodyWeight")]
    [ProducesResponseType<BodyWeightResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<BodyWeightResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BodyWeightResponse>> Record(
        DateOnly date, RecordBodyWeightRequest request, CancellationToken ct) {
        var (bodyWeight, created) = await bodyWeights.RecordAsync(date, request.WeightKg, ct);
        return created ? StatusCode(StatusCodes.Status201Created, bodyWeight) : Ok(bodyWeight);
    }

    [HttpDelete("{date}", Name = "DeleteBodyWeight")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(DateOnly date, CancellationToken ct) {
        await bodyWeights.DeleteAsync(date, ct);
        return NoContent();
    }
}
