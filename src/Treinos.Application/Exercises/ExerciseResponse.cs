using Treinos.Domain.Exercises;

namespace Treinos.Application.Exercises;

/// <summary>Um exercício como a API o devolve, com os grupos musculares e os apelidos que o usuário enxerga.</summary>
/// <param name="IsSystem">Do catálogo do sistema (true) ou criado pelo usuário (false).</param>
public sealed record ExerciseResponse(
    Guid Id,
    string Name,
    MeasurementType MeasurementType,
    bool IsUnilateral,
    bool IsSystem,
    IReadOnlyList<ExerciseMuscleResponse> Muscles,
    IReadOnlyList<string> Aliases);

/// <param name="MuscleGroup">Código do grupo muscular (<c>chest</c>, <c>triceps</c>…).</param>
public sealed record ExerciseMuscleResponse(string MuscleGroup, MuscleRole Role);
