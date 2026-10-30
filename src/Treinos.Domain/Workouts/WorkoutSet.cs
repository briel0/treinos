using Treinos.Domain.Exercises;

namespace Treinos.Domain.Workouts;

public enum SetType {
    Warmup,
    Working,
}

public enum Side {
    Left,
    Right,
}

/// <summary>
/// Os dados de uma série nova, como chegam do cliente. A ordem não vem aqui: é a posição na lista de séries.
/// </summary>
/// <param name="Id">Escolhido pelo cliente (UUID v7).</param>
public sealed record NewSet(
    Guid Id,
    SetType Type = SetType.Working,
    bool IsFailed = false,
    int? Reps = null,
    decimal? LoadKg = null,
    decimal? AssistanceKg = null,
    int? DurationSec = null,
    int? DistanceM = null,
    Side? Side = null);

/// <summary>
/// Uma série de um exercício dentro do treino. O tipo de medição do exercício decide quais medidas ela preenche;
/// as que não se aplicam ficam vazias (decisões 3 e 4 do AGENTS.md).
/// </summary>
public sealed class WorkoutSet {
    public const decimal MaxLoadKg = 9999.99m;

    private WorkoutSet(NewSet data, int sortOrder) {
        Id = data.Id;
        SortOrder = sortOrder;
        Type = data.Type;
        IsFailed = data.IsFailed;
        Reps = data.Reps;
        LoadKg = data.LoadKg;
        AssistanceKg = data.AssistanceKg;
        DurationSec = data.DurationSec;
        DistanceM = data.DistanceM;
        Side = data.Side;
    }

    public Guid Id { get; }

    /// <summary>Posição dentro do exercício, a partir de 0.</summary>
    public int SortOrder { get; }

    public SetType Type { get; }

    /// <summary>Uma repetição foi tentada e não completada.</summary>
    public bool IsFailed { get; }

    public int? Reps { get; }

    /// <summary>Carga levantada; nos exercícios de peso corporal, a carga adicional.</summary>
    public decimal? LoadKg { get; }

    /// <summary>Peso retirado por máquina ou elástico, só nos exercícios de peso corporal.</summary>
    public decimal? AssistanceKg { get; }

    public int? DurationSec { get; }

    public int? DistanceM { get; }

    /// <summary>Lado treinado, só nos exercícios unilaterais.</summary>
    public Side? Side { get; }

    internal static WorkoutSet Create(NewSet data, int sortOrder, Exercise exercise) {
        ArgumentNullException.ThrowIfNull(data);
        EntityId.Require(data.Id);
        RequireMeasurementsFor(exercise.MeasurementType, data);
        RequireRanges(data);
        if(data.Side is not null && !exercise.IsUnilateral) {
            throw new DomainException("set.side-not-unilateral", "Only sets of unilateral exercises record a side.");
        }
        return new(data, sortOrder);
    }

    private static void RequireMeasurementsFor(MeasurementType type, NewSet set) {
        var valid = type switch {
            MeasurementType.WeightReps =>
                set is { Reps: not null, LoadKg: not null, AssistanceKg: null, DurationSec: null, DistanceM: null },
            MeasurementType.Bodyweight =>
                set is { Reps: not null, DurationSec: null, DistanceM: null }
                && (set.LoadKg is null || set.AssistanceKg is null),
            MeasurementType.Time =>
                set is { DurationSec: not null, Reps: null, LoadKg: null, AssistanceKg: null, DistanceM: null },
            MeasurementType.DistanceTime =>
                set is { DistanceM: not null, DurationSec: not null, Reps: null, LoadKg: null, AssistanceKg: null },
            _ => false,
        };
        if(!valid) {
            throw new DomainException(
                "set.invalid-measurements", $"The set's measurements do not match the measurement type {type}.");
        }
    }

    private static void RequireRanges(NewSet set) {
        if(set.Reps < 0) {
            throw new DomainException("set.invalid-reps", "Repetitions cannot be negative.");
        }
        if(set.Reps == 0 && !set.IsFailed) {
            throw new DomainException("set.invalid-reps", "Zero repetitions only make sense in a failed set.");
        }
        if(set.LoadKg is { } load) {
            Kilograms.Require(load, MaxLoadKg, allowZero: true, "set.invalid-load", "The load");
        }
        if(set.AssistanceKg is { } assistance) {
            Kilograms.Require(assistance, MaxLoadKg, allowZero: false, "set.invalid-assistance", "The assistance");
        }
        if(set.DurationSec <= 0) {
            throw new DomainException("set.invalid-duration", "The duration must be more than zero seconds.");
        }
        if(set.DistanceM <= 0) {
            throw new DomainException("set.invalid-distance", "The distance must be more than zero meters.");
        }
    }
}
