using Treinos.Domain.Exercises;
using Treinos.Domain.Workouts;

namespace Treinos.Domain.Tests.Workouts;

/// <summary>As medidas de cada série seguem o tipo de medição do exercício (decisões 3 e 4 do AGENTS.md).</summary>
public class WorkoutSetTests {
    private static readonly Guid UserId = Guid.CreateVersion7();

    private static Exercise ExerciseOf(MeasurementType type, bool isUnilateral = false) =>
        Exercise.Create(Guid.CreateVersion7(), UserId, "Exercício", type, isUnilateral, []);

    /// <summary>Registra a série num treino novo, que é o único caminho para criar uma série.</summary>
    private static WorkoutSet Record(Exercise exercise, NewSet set) {
        var workout = Workout.Create(Guid.CreateVersion7(), UserId, DateTimeOffset.UtcNow, durationSec: null, note: null);
        return workout.AddExercise(Guid.CreateVersion7(), exercise, [set]).Sets[0];
    }

    private static string RecordError(Exercise exercise, NewSet set) =>
        Assert.Throws<DomainException>(() => Record(exercise, set)).Code;

    private static NewSet Set(
        int? reps = null, decimal? loadKg = null, decimal? assistanceKg = null, int? durationSec = null,
        int? distanceM = null, bool isFailed = false, Side? side = null) =>
        new(Guid.CreateVersion7(), SetType.Working, isFailed, reps, loadKg, assistanceKg, durationSec, distanceM, side);

    [Fact]
    public void Weight_and_reps_records_reps_and_load() {
        var set = Record(ExerciseOf(MeasurementType.WeightReps), Set(reps: 8, loadKg: 60));

        Assert.Equal(8, set.Reps);
        Assert.Equal(60, set.LoadKg);
    }

    [Theory]
    [InlineData(8, null, null, null)] // falta a carga
    [InlineData(null, 60.0, null, null)] // faltam as repetições
    [InlineData(8, 60.0, 10.0, null)] // assistência não se aplica
    [InlineData(8, 60.0, null, 30)] // duração não se aplica
    public void Weight_and_reps_rejects_other_combinations(int? reps, double? load, double? assistance, int? duration) {
        var code = RecordError(
            ExerciseOf(MeasurementType.WeightReps),
            Set(reps, (decimal?)load, (decimal?)assistance, duration));

        Assert.Equal("set.invalid-measurements", code);
    }

    [Fact]
    public void Bodyweight_records_reps_with_optional_added_load_or_assistance() {
        var exercise = ExerciseOf(MeasurementType.Bodyweight);

        Assert.Null(Record(exercise, Set(reps: 10)).LoadKg);
        Assert.Equal(10, Record(exercise, Set(reps: 8, loadKg: 10)).LoadKg);
        Assert.Equal(20, Record(exercise, Set(reps: 8, assistanceKg: 20)).AssistanceKg);
    }

    [Fact]
    public void Bodyweight_rejects_added_load_and_assistance_together() {
        var code = RecordError(ExerciseOf(MeasurementType.Bodyweight), Set(reps: 8, loadKg: 10, assistanceKg: 20));

        Assert.Equal("set.invalid-measurements", code);
    }

    [Fact]
    public void Time_records_only_the_duration() {
        var exercise = ExerciseOf(MeasurementType.Time);

        Assert.Equal(60, Record(exercise, Set(durationSec: 60)).DurationSec);
        Assert.Equal("set.invalid-measurements", RecordError(exercise, Set(durationSec: 60, reps: 1)));
        Assert.Equal("set.invalid-measurements", RecordError(exercise, Set()));
    }

    [Fact]
    public void Distance_and_time_records_both() {
        var exercise = ExerciseOf(MeasurementType.DistanceTime);

        var set = Record(exercise, Set(distanceM: 5000, durationSec: 1680));

        Assert.Equal(5000, set.DistanceM);
        Assert.Equal(1680, set.DurationSec);
        Assert.Equal("set.invalid-measurements", RecordError(exercise, Set(distanceM: 5000)));
    }

    [Fact]
    public void Zero_reps_only_in_a_failed_set() {
        var exercise = ExerciseOf(MeasurementType.WeightReps);

        Assert.Equal(0, Record(exercise, Set(reps: 0, loadKg: 100, isFailed: true)).Reps);
        Assert.Equal("set.invalid-reps", RecordError(exercise, Set(reps: 0, loadKg: 100)));
        Assert.Equal("set.invalid-reps", RecordError(exercise, Set(reps: -1, loadKg: 100)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10000)]
    [InlineData(1.255)] // três casas decimais
    public void Load_must_be_non_negative_up_to_9999_99_with_two_decimals(double load) {
        var code = RecordError(ExerciseOf(MeasurementType.WeightReps), Set(reps: 5, loadKg: (decimal)load));

        Assert.Equal("set.invalid-load", code);
    }

    [Fact]
    public void Load_accepts_zero_and_plate_fractions() {
        var exercise = ExerciseOf(MeasurementType.WeightReps);

        Assert.Equal(0, Record(exercise, Set(reps: 10, loadKg: 0)).LoadKg);
        Assert.Equal(21.25m, Record(exercise, Set(reps: 10, loadKg: 21.25m)).LoadKg);
    }

    [Fact]
    public void Assistance_must_be_more_than_zero() {
        var code = RecordError(ExerciseOf(MeasurementType.Bodyweight), Set(reps: 5, assistanceKg: 0));

        Assert.Equal("set.invalid-assistance", code);
    }

    [Fact]
    public void Duration_and_distance_must_be_more_than_zero() {
        var exercise = ExerciseOf(MeasurementType.DistanceTime);

        Assert.Equal("set.invalid-duration", RecordError(exercise, Set(distanceM: 100, durationSec: 0)));
        Assert.Equal("set.invalid-distance", RecordError(exercise, Set(distanceM: 0, durationSec: 60)));
    }

    [Fact]
    public void Side_only_on_unilateral_exercises() {
        var set = Set(reps: 10, loadKg: 12, side: Side.Left);

        Assert.Equal(Side.Left, Record(ExerciseOf(MeasurementType.WeightReps, isUnilateral: true), set).Side);
        Assert.Equal("set.side-not-unilateral", RecordError(ExerciseOf(MeasurementType.WeightReps), set));
    }

    [Fact]
    public void Set_rejects_ids_that_are_not_uuid_version_7() {
        var set = Set(reps: 8, loadKg: 60) with { Id = Guid.NewGuid() }; // versão 4

        Assert.Equal("invalid-id", RecordError(ExerciseOf(MeasurementType.WeightReps), set));
    }
}
