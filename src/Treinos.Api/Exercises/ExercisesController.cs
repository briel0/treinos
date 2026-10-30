using Microsoft.AspNetCore.Mvc;
using Treinos.Application.Exercises;

namespace Treinos.Api.Exercises;

/// <summary>O catálogo de exercícios que o usuário atual enxerga: os do sistema e os dele.</summary>
[ApiController]
[Route("api/exercises")]
[Produces("application/json")]
public sealed class ExercisesController(ExerciseUseCases exercises) : ControllerBase {
    /// <summary>Todos os exercícios visíveis, em ordem alfabética, com grupos musculares e apelidos.</summary>
    [HttpGet(Name = "ListExercises")]
    public Task<IReadOnlyList<ExerciseResponse>> List(CancellationToken ct) => exercises.ListAsync(ct);

    [HttpGet("{id:guid}", Name = "GetExercise")]
    [ProducesResponseType<ExerciseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ExerciseResponse> Get(Guid id, CancellationToken ct) => exercises.GetAsync(id, ct);

    /// <summary>
    /// O exercício que um termo identifica, pelo nome ou por um apelido, ignorando maiúsculas e acentos
    /// ("paralela" → Mergulho nas paralelas). 404 quando nada bate: pergunte ao usuário em vez de adivinhar.
    /// </summary>
    [HttpGet("resolve", Name = "ResolveExercise")]
    [ProducesResponseType<ExerciseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ExerciseResponse> Resolve([FromQuery] string term, CancellationToken ct) =>
        exercises.ResolveAsync(term, ct);
}
