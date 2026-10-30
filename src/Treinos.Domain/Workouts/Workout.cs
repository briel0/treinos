using Treinos.Domain.Exercises;

namespace Treinos.Domain.Workouts;

/// <summary>
/// Um treino de um usuário: quando começou, quanto durou e os exercícios feitos, na ordem. Não há fichas nem
/// modelos; o treino só registra o que foi feito. Pode ficar sem exercícios enquanto é registrado série a série.
/// </summary>
public sealed class Workout {
    public const int NoteMaxLength = 2000;

    private readonly List<WorkoutExercise> _exercises = [];

    private Workout(Guid id, Guid userId, DateTimeOffset startedAt, int? durationSec, string? note) {
        Id = id;
        UserId = userId;
        StartedAt = startedAt;
        DurationSec = durationSec;
        Note = note;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public DateTimeOffset StartedAt { get; private set; }

    public int? DurationSec { get; private set; }

    public string? Note { get; private set; }

    public IReadOnlyList<WorkoutExercise> Exercises => _exercises;

    /// <param name="id">Escolhido pelo cliente (UUID v7), o que torna a criação idempotente.</param>
    public static Workout Create(Guid id, Guid userId, DateTimeOffset startedAt, int? durationSec, string? note) =>
        new(EntityId.Require(id), userId, startedAt, ValidateDuration(durationSec), NormalizeNote(note));

    public void Reschedule(DateTimeOffset startedAt) => StartedAt = startedAt;

    public void ChangeDuration(int? durationSec) => DurationSec = ValidateDuration(durationSec);

    public void ChangeNote(string? note) => Note = NormalizeNote(note);

    /// <summary>Acrescenta um exercício no fim do treino, com as suas séries.</summary>
    public WorkoutExercise AddExercise(Guid workoutExerciseId, Exercise exercise, IReadOnlyList<NewSet> sets) {
        RequireVisible(exercise);
        if(_exercises.Any(entry => entry.Id == workoutExerciseId)) {
            throw new DomainException("workout.duplicate-exercise-id", "Each exercise in a workout needs its own id.");
        }
        var entry = WorkoutExercise.Create(workoutExerciseId, exercise, sets, _exercises.Count);
        _exercises.Add(entry);
        return entry;
    }

    /// <summary>
    /// Troca o exercício e as séries de uma entrada, mantendo a posição. Exercício e séries mudam juntos, porque
    /// as séries dependem do tipo de medição do exercício.
    /// </summary>
    public WorkoutExercise ReplaceExercise(Guid workoutExerciseId, Exercise exercise, IReadOnlyList<NewSet> sets) {
        RequireVisible(exercise);
        var index = IndexOf(workoutExerciseId);
        var entry = WorkoutExercise.Create(workoutExerciseId, exercise, sets, index);
        _exercises[index] = entry;
        return entry;
    }

    public void RemoveExercise(Guid workoutExerciseId) {
        _exercises.RemoveAt(IndexOf(workoutExerciseId));
        Renumber();
    }

    /// <summary>Reordena os exercícios; a lista precisa ter exatamente os exercícios do treino.</summary>
    public void ReorderExercises(IReadOnlyList<Guid> workoutExerciseIds) {
        ArgumentNullException.ThrowIfNull(workoutExerciseIds);
        var current = _exercises.Select(entry => entry.Id).ToHashSet();
        if(workoutExerciseIds.Count != current.Count || !current.SetEquals(workoutExerciseIds)) {
            throw new DomainException(
                "workout.invalid-order", "The new order must list each exercise of the workout exactly once.");
        }
        var byId = _exercises.ToDictionary(entry => entry.Id);
        _exercises.Clear();
        _exercises.AddRange(workoutExerciseIds.Select(id => byId[id]));
        Renumber();
    }

    /// <summary>Só exercícios do sistema ou do dono do treino podem entrar nele.</summary>
    private void RequireVisible(Exercise exercise) {
        ArgumentNullException.ThrowIfNull(exercise);
        if(!exercise.IsVisibleTo(UserId)) {
            throw new DomainException(
                "workout.exercise-not-visible", "A workout can only use system exercises or its owner's exercises.");
        }
    }

    private int IndexOf(Guid workoutExerciseId) {
        var index = _exercises.FindIndex(entry => entry.Id == workoutExerciseId);
        if(index < 0) {
            throw new DomainException(
                "workout.exercise-not-found", $"The workout has no exercise with id '{workoutExerciseId}'.");
        }
        return index;
    }

    private void Renumber() {
        for(var i = 0; i < _exercises.Count; i++) {
            _exercises[i].SortOrder = i;
        }
    }

    private static int? ValidateDuration(int? durationSec) {
        if(durationSec <= 0) {
            throw new DomainException("workout.invalid-duration", "The duration must be more than zero seconds.");
        }
        return durationSec;
    }

    /// <summary>Observação sem espaços nas pontas; vazia vira ausente.</summary>
    private static string? NormalizeNote(string? note) {
        var trimmed = note?.Trim();
        if(string.IsNullOrEmpty(trimmed)) {
            return null;
        }
        if(trimmed.Length > NoteMaxLength) {
            throw new DomainException(
                "workout.invalid-note", $"A workout note must have at most {NoteMaxLength} characters.");
        }
        return trimmed;
    }
}
