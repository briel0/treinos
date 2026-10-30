using Treinos.Domain.Exercises;
using Treinos.Domain.Workouts;

namespace Treinos.Domain.Tests.Workouts;

public class WorkoutTests {
    private static readonly Guid UserId = Guid.CreateVersion7();

    private static readonly Exercise Bench = Exercise.Create(
        Guid.CreateVersion7(), userId: null, "Supino reto com barra", MeasurementType.WeightReps, isUnilateral: false,
        [new(new MuscleGroup("chest"), MuscleRole.Primary)]);

    private static readonly Exercise Run = Exercise.Create(
        Guid.CreateVersion7(), userId: null, "Corrida", MeasurementType.DistanceTime, isUnilateral: false, []);

    private static Workout Create(int? durationSec = null, string? note = null) =>
        Workout.Create(Guid.CreateVersion7(), UserId, DateTimeOffset.UtcNow, durationSec, note);

    private static NewSet BenchSet(int reps = 8, decimal load = 60) =>
        new(Guid.CreateVersion7(), Reps: reps, LoadKg: load);

    private static NewSet RunSet() => new(Guid.CreateVersion7(), DistanceM: 5000, DurationSec: 1680);

    [Fact]
    public void New_workout_has_no_exercises() {
        Assert.Empty(Create().Exercises);
    }

    [Fact]
    public void Create_rejects_ids_that_are_not_uuid_version_7() {
        var error = Assert.Throws<DomainException>(() => Workout.Create(
            Guid.NewGuid(), UserId, DateTimeOffset.UtcNow, durationSec: null, note: null)); // versão 4

        Assert.Equal("invalid-id", error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-60)]
    public void Duration_must_be_more_than_zero(int durationSec) {
        var error = Assert.Throws<DomainException>(() => Create(durationSec));

        Assert.Equal("workout.invalid-duration", error.Code);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  treino leve  ", "treino leve")]
    public void Note_is_trimmed_and_empty_becomes_absent(string? note, string? expected) {
        Assert.Equal(expected, Create(note: note).Note);
    }

    [Fact]
    public void Note_has_at_most_2000_characters() {
        var error = Assert.Throws<DomainException>(() => Create(note: new string('a', 2001)));

        Assert.Equal("workout.invalid-note", error.Code);
    }

    [Fact]
    public void AddExercise_appends_in_order_and_numbers_the_sets() {
        var workout = Create();

        workout.AddExercise(Guid.CreateVersion7(), Bench, [BenchSet(), BenchSet(reps: 6, load: 65)]);
        workout.AddExercise(Guid.CreateVersion7(), Run, [RunSet()]);

        Assert.Equal([0, 1], workout.Exercises.Select(entry => entry.SortOrder));
        Assert.Equal([0, 1], workout.Exercises[0].Sets.Select(set => set.SortOrder));
        Assert.Equal(MeasurementType.DistanceTime, workout.Exercises[1].MeasurementType);
    }

    [Fact]
    public void The_same_exercise_can_appear_twice() {
        var workout = Create();

        workout.AddExercise(Guid.CreateVersion7(), Bench, [BenchSet()]);
        workout.AddExercise(Guid.CreateVersion7(), Bench, [BenchSet()]);

        Assert.Equal(2, workout.Exercises.Count);
    }

    [Fact]
    public void An_exercise_in_a_workout_needs_at_least_one_set() {
        var error = Assert.Throws<DomainException>(() => Create().AddExercise(Guid.CreateVersion7(), Bench, []));

        Assert.Equal("workout.exercise-without-sets", error.Code);
    }

    [Fact]
    public void Each_set_needs_its_own_id() {
        var set = BenchSet();

        var error = Assert.Throws<DomainException>(() => Create().AddExercise(Guid.CreateVersion7(), Bench, [set, set]));

        Assert.Equal("workout.duplicate-set-id", error.Code);
    }

    [Fact]
    public void Each_exercise_in_a_workout_needs_its_own_id() {
        var workout = Create();
        var id = Guid.CreateVersion7();
        workout.AddExercise(id, Bench, [BenchSet()]);

        var error = Assert.Throws<DomainException>(() => workout.AddExercise(id, Bench, [BenchSet()]));

        Assert.Equal("workout.duplicate-exercise-id", error.Code);
    }

    [Fact]
    public void Workout_uses_system_exercises_and_its_owners_exercises_only() {
        var mine = Exercise.Create(Guid.CreateVersion7(), UserId, "Meu", MeasurementType.WeightReps, false, []);
        var theirs = Exercise.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), "Dele", MeasurementType.WeightReps, false, []);
        var workout = Create();

        workout.AddExercise(Guid.CreateVersion7(), mine, [BenchSet()]);
        var error = Assert.Throws<DomainException>(
            () => workout.AddExercise(Guid.CreateVersion7(), theirs, [BenchSet()]));

        Assert.Equal("workout.exercise-not-visible", error.Code);
    }

    [Fact]
    public void ReplaceExercise_keeps_the_position_and_swaps_exercise_and_sets() {
        var workout = Create();
        var first = Guid.CreateVersion7();
        workout.AddExercise(first, Bench, [BenchSet()]);
        workout.AddExercise(Guid.CreateVersion7(), Bench, [BenchSet()]);

        workout.ReplaceExercise(first, Run, [RunSet()]);

        Assert.Equal(Run.Id, workout.Exercises[0].ExerciseId);
        Assert.Equal(0, workout.Exercises[0].SortOrder);
        Assert.Equal(5000, workout.Exercises[0].Sets[0].DistanceM);
    }

    [Fact]
    public void RemoveExercise_renumbers_the_rest() {
        var workout = Create();
        var first = Guid.CreateVersion7();
        workout.AddExercise(first, Bench, [BenchSet()]);
        workout.AddExercise(Guid.CreateVersion7(), Run, [RunSet()]);

        workout.RemoveExercise(first);

        Assert.Equal(0, Assert.Single(workout.Exercises).SortOrder);
    }

    [Fact]
    public void ReorderExercises_applies_the_new_order() {
        var workout = Create();
        var bench = Guid.CreateVersion7();
        var run = Guid.CreateVersion7();
        workout.AddExercise(bench, Bench, [BenchSet()]);
        workout.AddExercise(run, Run, [RunSet()]);

        workout.ReorderExercises([run, bench]);

        Assert.Equal([run, bench], workout.Exercises.Select(entry => entry.Id));
        Assert.Equal([0, 1], workout.Exercises.Select(entry => entry.SortOrder));
    }

    [Fact]
    public void ReorderExercises_needs_each_exercise_exactly_once() {
        var workout = Create();
        var bench = Guid.CreateVersion7();
        workout.AddExercise(bench, Bench, [BenchSet()]);
        workout.AddExercise(Guid.CreateVersion7(), Run, [RunSet()]);

        var error = Assert.Throws<DomainException>(() => workout.ReorderExercises([bench, bench]));

        Assert.Equal("workout.invalid-order", error.Code);
    }

    [Fact]
    public void Unknown_exercise_ids_are_rejected() {
        var error = Assert.Throws<DomainException>(() => Create().RemoveExercise(Guid.CreateVersion7()));

        Assert.Equal("workout.exercise-not-found", error.Code);
    }
}
