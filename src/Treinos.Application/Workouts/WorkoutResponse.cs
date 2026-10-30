using Treinos.Domain.Exercises;
using Treinos.Domain.Workouts;

namespace Treinos.Application.Workouts;

/// <summary>Um treino como a API o devolve, com os exercícios e as séries na ordem em que foram feitos.</summary>
public sealed record WorkoutResponse(
    Guid Id,
    DateTimeOffset StartedAt,
    int? DurationSec,
    string? Note,
    IReadOnlyList<WorkoutExerciseResponse> Exercises,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <param name="Id">Id deste exercício dentro do treino (não o do exercício do catálogo).</param>
/// <param name="ExerciseId">O exercício do catálogo.</param>
/// <param name="ExerciseName">Nome do exercício, para leitura direta.</param>
public sealed record WorkoutExerciseResponse(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    MeasurementType MeasurementType,
    IReadOnlyList<WorkoutSetResponse> Sets);

public sealed record WorkoutSetResponse(
    Guid Id,
    SetType Type,
    bool IsFailed,
    int? Reps,
    decimal? LoadKg,
    decimal? AssistanceKg,
    int? DurationSec,
    int? DistanceM,
    Side? Side);
