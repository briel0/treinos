using Treinos.Domain.Exercises;

namespace Treinos.Domain.Tests.Exercises;

public class ExerciseTests {
    private static readonly Guid UserId = Guid.CreateVersion7();

    private static ExerciseMuscle Primary(string code) => new(new MuscleGroup(code), MuscleRole.Primary);

    private static ExerciseMuscle Secondary(string code) => new(new MuscleGroup(code), MuscleRole.Secondary);

    private static Exercise CreateSystem(
        string name = "Supino reto com barra", MeasurementType type = MeasurementType.WeightReps,
        params ExerciseMuscle[] muscles) =>
        Exercise.Create(Guid.CreateVersion7(), userId: null, name, type, isUnilateral: false,
            muscles.Length > 0 || type == MeasurementType.DistanceTime ? muscles : [Primary("chest")]);

    private static Exercise CreateForUser(MeasurementType type = MeasurementType.WeightReps, params ExerciseMuscle[] muscles) =>
        Exercise.Create(Guid.CreateVersion7(), UserId, "Meu exercício", type, isUnilateral: false, muscles);

    [Fact]
    public void Create_collapses_and_trims_spaces_in_the_name() {
        var exercise = CreateSystem("  Supino   reto  com barra ");

        Assert.Equal("Supino reto com barra", exercise.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_name(string name) {
        var error = Assert.Throws<DomainException>(() => CreateSystem(name));

        Assert.Equal("exercise.invalid-name", error.Code);
    }

    [Fact]
    public void Create_rejects_name_longer_than_100_characters() {
        var error = Assert.Throws<DomainException>(() => CreateSystem(new string('a', 101)));

        Assert.Equal("exercise.invalid-name", error.Code);
    }

    [Fact]
    public void Create_rejects_ids_that_are_not_uuid_version_7() {
        var error = Assert.Throws<DomainException>(() => Exercise.Create(
            Guid.NewGuid(), UserId, "Supino", MeasurementType.WeightReps, isUnilateral: false, [])); // versão 4

        Assert.Equal("invalid-id", error.Code);
    }

    [Fact]
    public void Exercise_without_owner_is_a_system_exercise() {
        Assert.True(CreateSystem().IsSystem);
        Assert.False(CreateForUser().IsSystem);
    }

    [Fact]
    public void System_exercises_are_visible_to_everyone_and_user_exercises_only_to_their_owner() {
        var someoneElse = Guid.CreateVersion7();

        Assert.True(CreateSystem().IsVisibleTo(someoneElse));
        Assert.True(CreateForUser().IsVisibleTo(UserId));
        Assert.False(CreateForUser().IsVisibleTo(someoneElse));
    }

    [Fact]
    public void Create_rejects_the_same_muscle_group_twice() {
        var error = Assert.Throws<DomainException>(() => CreateForUser(
            MeasurementType.WeightReps, Primary("chest"), Secondary("chest")));

        Assert.Equal("exercise.duplicate-muscle-group", error.Code);
    }

    [Fact]
    public void System_exercise_needs_a_primary_muscle_group() {
        var error = Assert.Throws<DomainException>(() => CreateSystem(
            "Supino", MeasurementType.WeightReps, Secondary("chest")));

        Assert.Equal("exercise.missing-primary-muscle-group", error.Code);
    }

    [Fact]
    public void System_cardio_has_no_muscle_groups() {
        var cardio = CreateSystem("Corrida", MeasurementType.DistanceTime);

        Assert.Empty(cardio.Muscles);
    }

    [Fact]
    public void System_cardio_rejects_muscle_groups() {
        var error = Assert.Throws<DomainException>(() => CreateSystem(
            "Corrida", MeasurementType.DistanceTime, Primary("quadriceps")));

        Assert.Equal("exercise.cardio-with-muscle-groups", error.Code);
    }

    [Fact]
    public void Muscle_groups_are_optional_in_user_exercises() {
        Assert.Empty(CreateForUser().Muscles);
        Assert.Single(CreateForUser(MeasurementType.DistanceTime, Primary("quadriceps")).Muscles);
    }

    [Fact]
    public void ReplaceMuscles_applies_the_same_rules() {
        var exercise = CreateSystem();

        var error = Assert.Throws<DomainException>(() => exercise.ReplaceMuscles([Secondary("triceps")]));

        Assert.Equal("exercise.missing-primary-muscle-group", error.Code);
        exercise.ReplaceMuscles([Primary("chest"), Secondary("triceps")]);
        Assert.Equal(2, exercise.Muscles.Count);
    }

    [Fact]
    public void Rename_applies_the_same_name_rules() {
        var exercise = CreateSystem();

        exercise.Rename("  Supino  inclinado ");

        Assert.Equal("Supino inclinado", exercise.Name);
    }
}
