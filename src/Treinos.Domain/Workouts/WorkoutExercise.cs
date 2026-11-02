using Treinos.Domain.Exercises;

namespace Treinos.Domain.Workouts;

/// <summary>
/// Um exercício como foi feito num treino, com as suas séries na ordem. Guarda uma cópia do tipo de medição do
/// exercício, que o banco protege com uma chave estrangeira composta (decisão 8 do AGENTS.md).
/// </summary>
public sealed class WorkoutExercise {
    private readonly List<WorkoutSet> _sets;

    /// <summary>Usado pelo EF Core, que preenche as séries depois.</summary>
    private WorkoutExercise(Guid id, Guid exerciseId, MeasurementType measurementType, int sortOrder)
        : this(id, exerciseId, measurementType, sortOrder, []) { }

    private WorkoutExercise(
        Guid id, Guid exerciseId, MeasurementType measurementType, int sortOrder, List<WorkoutSet> sets) {
        Id = id;
        ExerciseId = exerciseId;
        MeasurementType = measurementType;
        SortOrder = sortOrder;
        _sets = sets;
    }

    public Guid Id { get; }

    public Guid ExerciseId { get; }

    public MeasurementType MeasurementType { get; }

    /// <summary>Posição dentro do treino, a partir de 0.</summary>
    public int SortOrder { get; internal set; }

    public IReadOnlyList<WorkoutSet> Sets => _sets;

    /// <summary>Um exercício no treino tem pelo menos uma série; a ordem delas é a da lista.</summary>
    internal static WorkoutExercise Create(Guid id, Exercise exercise, IReadOnlyList<NewSet> sets, int sortOrder) {
        var created = CreateSets(sets, firstSortOrder: 0, exercise, existingIds: []);
        return new(EntityId.Require(id), exercise.Id, exercise.MeasurementType, sortOrder, created);
    }

    /// <summary>Acrescenta séries depois das que já existem; <paramref name="exercise"/> é o desta entrada.</summary>
    internal void AddSets(Exercise exercise, IReadOnlyList<NewSet> sets) {
        ArgumentNullException.ThrowIfNull(exercise);
        if(exercise.Id != ExerciseId) {
            throw new ArgumentException("The exercise must be the one this entry refers to.", nameof(exercise));
        }
        _sets.AddRange(CreateSets(sets, _sets.Count, exercise, _sets.Select(set => set.Id)));
    }

    private static List<WorkoutSet> CreateSets(
        IReadOnlyList<NewSet> sets, int firstSortOrder, Exercise exercise, IEnumerable<Guid> existingIds) {
        ArgumentNullException.ThrowIfNull(sets);
        if(sets.Count == 0) {
            throw new DomainException("workout.exercise-without-sets", "An exercise in a workout needs at least one set.");
        }
        var ids = sets.Select(set => set.Id).Concat(existingIds).ToList();
        if(ids.Distinct().Count() != ids.Count) {
            throw new DomainException("workout.duplicate-set-id", "Each set needs its own id.");
        }
        return [.. sets.Select((set, index) => WorkoutSet.Create(set, firstSortOrder + index, exercise))];
    }
}
