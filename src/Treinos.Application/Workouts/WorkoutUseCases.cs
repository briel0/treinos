using Treinos.Application.Common;
using Treinos.Application.Exercises;
using Treinos.Domain.Workouts;

namespace Treinos.Application.Workouts;

/// <summary>Um exercício novo num treino, com as séries na ordem em que foram feitas.</summary>
/// <param name="Id">Escolhido pelo cliente (UUID v7): id deste exercício dentro do treino.</param>
/// <param name="ExerciseId">O exercício do catálogo (do sistema ou do usuário).</param>
public sealed record NewWorkoutExercise(Guid Id, Guid ExerciseId, IReadOnlyList<NewSet> Sets);

/// <summary>Os casos de uso dos treinos, sempre do usuário atual.</summary>
public sealed class WorkoutUseCases(
    ICurrentUser user,
    IWorkoutRepository workouts,
    IWorkoutQueries queries,
    IExerciseRepository exercises,
    IUnitOfWork unitOfWork) {
    public Task<IReadOnlyList<WorkoutResponse>> ListAsync(DateTimeOffset? since, DateTimeOffset? until, CancellationToken ct) =>
        queries.ListAsync(user.Id, since, until, ct);

    public async Task<WorkoutResponse> GetAsync(Guid id, CancellationToken ct) =>
        await queries.FindAsync(user.Id, id, ct) ?? throw UseCaseException.NotFound("Workout");

    /// <summary>
    /// Registra um treino, já com os exercícios e as séries (ou vazio, para registrar série a série). Repetir o
    /// pedido com o mesmo id devolve o treino já criado (<c>Created</c> falso), em vez de duplicá-lo.
    /// </summary>
    public async Task<(WorkoutResponse Workout, bool Created)> CreateAsync(
        Guid id, DateTimeOffset startedAt, int? durationSec, string? note, IReadOnlyList<NewWorkoutExercise> entries,
        CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(entries);
        if(await queries.FindAsync(user.Id, id, ct) is { } existing) {
            return (existing, false);
        }
        if(await workouts.ExistsAsync(id, ct)) {
            throw UseCaseException.IdTaken();
        }

        var workout = Workout.Create(id, user.Id, startedAt, durationSec, note);
        foreach(var entry in entries) {
            workout.AddExercise(entry.Id, await FindExerciseAsync(entry.ExerciseId, ct), entry.Sets);
        }
        workouts.Add(workout);
        await unitOfWork.SaveChangesAsync(ct);
        return (await GetAsync(id, ct), true);
    }

    /// <summary>Muda só os campos enviados (início, duração, observação).</summary>
    /// <param name="clearDuration">Apaga a duração (em vez de deixá-la como está quando não enviada).</param>
    /// <param name="clearNote">Apaga a observação.</param>
    public async Task<WorkoutResponse> UpdateAsync(
        Guid id, DateTimeOffset? startedAt, int? durationSec, bool clearDuration, string? note, bool clearNote,
        CancellationToken ct) {
        var workout = await FindOwnAsync(id, ct);
        if(startedAt is { } start) {
            workout.Reschedule(start);
        }
        if(clearDuration || durationSec is not null) {
            workout.ChangeDuration(clearDuration ? null : durationSec);
        }
        if(clearNote || note is not null) {
            workout.ChangeNote(clearNote ? null : note);
        }
        await unitOfWork.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    /// <summary>
    /// Acrescenta um exercício no fim do treino. Repetir o pedido com o mesmo id não duplica o exercício.
    /// </summary>
    public async Task<WorkoutResponse> AddExerciseAsync(Guid workoutId, NewWorkoutExercise entry, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(entry);
        var workout = await FindOwnAsync(workoutId, ct);
        if(workout.Exercises.Any(existing => existing.Id == entry.Id)) {
            return await GetAsync(workoutId, ct);
        }
        workout.AddExercise(entry.Id, await FindExerciseAsync(entry.ExerciseId, ct), entry.Sets);
        await unitOfWork.SaveChangesAsync(ct);
        return await GetAsync(workoutId, ct);
    }

    public async Task<WorkoutResponse> RemoveExerciseAsync(Guid workoutId, Guid workoutExerciseId, CancellationToken ct) {
        var workout = await FindOwnAsync(workoutId, ct);
        if(workout.Exercises.All(entry => entry.Id != workoutExerciseId)) {
            throw UseCaseException.NotFound("Workout exercise");
        }
        workout.RemoveExercise(workoutExerciseId);
        await unitOfWork.SaveChangesAsync(ct);
        return await GetAsync(workoutId, ct);
    }

    /// <summary>Apaga o treino e, em cascata no banco, os exercícios e as séries dele.</summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct) {
        workouts.Remove(await FindOwnAsync(id, ct));
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Workout> FindOwnAsync(Guid id, CancellationToken ct) =>
        await workouts.FindAsync(user.Id, id, ct) ?? throw UseCaseException.NotFound("Workout");

    /// <summary>Um exercício que o usuário não enxerga responde como inexistente, para não revelar que existe.</summary>
    private async Task<Domain.Exercises.Exercise> FindExerciseAsync(Guid exerciseId, CancellationToken ct) =>
        await exercises.FindVisibleAsync(user.Id, exerciseId, ct)
        ?? throw new UseCaseException(
            UseCaseErrorKind.NotFound, "exercise.not-found", $"Exercise '{exerciseId}' not found.");
}
