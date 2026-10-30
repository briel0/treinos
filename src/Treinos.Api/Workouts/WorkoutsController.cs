using Microsoft.AspNetCore.Mvc;
using Treinos.Application.Workouts;

namespace Treinos.Api.Workouts;

/// <summary>Os treinos do usuário atual, com exercícios e séries.</summary>
[ApiController]
[Route("api/workouts")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class WorkoutsController(WorkoutUseCases workouts) : ControllerBase {
    /// <summary>Os treinos que começaram no intervalo [since, until), do mais recente para o mais antigo.</summary>
    [HttpGet(Name = "ListWorkouts")]
    public Task<IReadOnlyList<WorkoutResponse>> List(
        [FromQuery] DateTimeOffset? since, [FromQuery] DateTimeOffset? until, CancellationToken ct) =>
        workouts.ListAsync(since, until, ct);

    [HttpGet("{id:guid}", Name = "GetWorkout")]
    [ProducesResponseType<WorkoutResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<WorkoutResponse> Get(Guid id, CancellationToken ct) => workouts.GetAsync(id, ct);

    /// <summary>Registra um treino. Idempotente: repetir com o mesmo id devolve o treino existente (200).</summary>
    [HttpPost(Name = "CreateWorkout")]
    [ProducesResponseType<WorkoutResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<WorkoutResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkoutResponse>> Create(CreateWorkoutRequest request, CancellationToken ct) {
        var (workout, created) = await workouts.CreateAsync(
            request.Id, request.StartedAt, request.DurationSec, request.Note,
            [.. (request.Exercises ?? []).Select(entry => entry.ToNew())], ct);
        return created ? CreatedAtRoute("GetWorkout", new { id = workout.Id }, workout) : Ok(workout);
    }

    [HttpPatch("{id:guid}", Name = "UpdateWorkout")]
    [ProducesResponseType<WorkoutResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<WorkoutResponse> Update(Guid id, UpdateWorkoutRequest request, CancellationToken ct) =>
        workouts.UpdateAsync(
            id, request.StartedAt, request.DurationSec, request.ClearDuration, request.Note, request.ClearNote, ct);

    /// <summary>Apaga o treino com todos os exercícios e séries.</summary>
    [HttpDelete("{id:guid}", Name = "DeleteWorkout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) {
        await workouts.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Acrescenta um exercício, com as séries, no fim do treino. Idempotente pelo id do exercício no treino.
    /// </summary>
    [HttpPost("{id:guid}/exercises", Name = "AddWorkoutExercise")]
    [ProducesResponseType<WorkoutResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<WorkoutResponse> AddExercise(Guid id, WorkoutExerciseRequest request, CancellationToken ct) =>
        workouts.AddExerciseAsync(id, request.ToNew(), ct);

    /// <summary>Tira um exercício (com as séries) do treino; os seguintes sobem uma posição.</summary>
    [HttpDelete("{id:guid}/exercises/{workoutExerciseId:guid}", Name = "RemoveWorkoutExercise")]
    [ProducesResponseType<WorkoutResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<WorkoutResponse> RemoveExercise(Guid id, Guid workoutExerciseId, CancellationToken ct) =>
        workouts.RemoveExerciseAsync(id, workoutExerciseId, ct);
}
