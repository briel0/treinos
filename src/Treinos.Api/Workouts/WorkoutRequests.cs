using Treinos.Application.Workouts;
using Treinos.Domain.Workouts;

namespace Treinos.Api.Workouts;

/// <param name="Id">Gerado pelo cliente (UUID v7): repetir o pedido com o mesmo id não duplica o treino.</param>
/// <param name="StartedAt">Quando o treino começou, com fuso (ex.: <c>2026-10-30T18:30:00-03:00</c>).</param>
/// <param name="DurationSec">Duração total em segundos (opcional).</param>
/// <param name="Note">Observação livre, até 2000 caracteres (opcional).</param>
/// <param name="Exercises">Os exercícios na ordem em que foram feitos; vazio para registrar série a série.</param>
public sealed record CreateWorkoutRequest(
    Guid Id,
    DateTimeOffset StartedAt,
    int? DurationSec,
    string? Note,
    IReadOnlyList<WorkoutExerciseRequest>? Exercises);

/// <param name="Id">Gerado pelo cliente (UUID v7): id deste exercício dentro do treino.</param>
/// <param name="ExerciseId">O exercício do catálogo (veja <c>GET /api/exercises/resolve?term=</c>).</param>
/// <param name="Sets">As séries na ordem em que foram feitas; pelo menos uma.</param>
public sealed record WorkoutExerciseRequest(Guid Id, Guid ExerciseId, IReadOnlyList<WorkoutSetRequest> Sets) {
    public NewWorkoutExercise ToNew() => new(Id, ExerciseId, [.. Sets.Select(set => set.ToNew())]);
}

/// <summary>
/// Uma série. Quais medidas preencher depende do tipo de medição do exercício: <c>weight_reps</c> → reps e
/// loadKg; <c>bodyweight</c> → reps, e loadKg (carga adicional) ou assistanceKg; <c>time</c> → durationSec;
/// <c>distance_time</c> → distanceM e durationSec. As demais ficam ausentes.
/// </summary>
/// <param name="Id">Gerado pelo cliente (UUID v7).</param>
/// <param name="Type">Aquecimento ou trabalho (padrão: trabalho).</param>
/// <param name="IsFailed">Uma repetição foi tentada e não completada.</param>
/// <param name="Reps">Repetições (0 só numa série falhada).</param>
/// <param name="LoadKg">Carga em kg, até duas casas; nos exercícios de peso corporal, a carga adicional.</param>
/// <param name="AssistanceKg">Peso retirado por máquina ou elástico, em kg (só peso corporal).</param>
/// <param name="DurationSec">Duração em segundos.</param>
/// <param name="DistanceM">Distância em metros.</param>
/// <param name="Side">Lado treinado, só em exercícios unilaterais.</param>
public sealed record WorkoutSetRequest(
    Guid Id,
    SetType? Type,
    bool? IsFailed,
    int? Reps,
    decimal? LoadKg,
    decimal? AssistanceKg,
    int? DurationSec,
    int? DistanceM,
    Side? Side) {
    public NewSet ToNew() => new(
        Id, Type ?? SetType.Working, IsFailed ?? false, Reps, LoadKg, AssistanceKg, DurationSec, DistanceM, Side);
}

/// <summary>Só os campos enviados são alterados. Para apagar a duração ou a observação, use os campos clear*.</summary>
public sealed record UpdateWorkoutRequest {
    public DateTimeOffset? StartedAt { get; init; }

    public int? DurationSec { get; init; }

    public bool ClearDuration { get; init; }

    public string? Note { get; init; }

    public bool ClearNote { get; init; }
}
