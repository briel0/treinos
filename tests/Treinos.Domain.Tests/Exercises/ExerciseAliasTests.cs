using Treinos.Domain.Exercises;

namespace Treinos.Domain.Tests.Exercises;

public class ExerciseAliasTests {
    private static readonly Guid UserId = Guid.CreateVersion7();

    private static readonly Exercise SystemExercise = Exercise.Create(
        Guid.CreateVersion7(), userId: null, "Mergulho nas paralelas", MeasurementType.Bodyweight, isUnilateral: false,
        [new(new MuscleGroup("chest"), MuscleRole.Primary)]);

    private static readonly Exercise UserExercise = Exercise.Create(
        Guid.CreateVersion7(), UserId, "Meu exercício", MeasurementType.WeightReps, isUnilateral: false, []);

    [Fact]
    public void Create_collapses_and_trims_spaces_in_the_term() {
        var alias = ExerciseAlias.Create(Guid.CreateVersion7(), SystemExercise, userId: null, "  barra   paralela ");

        Assert.Equal("barra paralela", alias.Term);
        Assert.Equal(SystemExercise.Id, alias.ExerciseId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_term(string term) {
        var error = Assert.Throws<DomainException>(
            () => ExerciseAlias.Create(Guid.CreateVersion7(), SystemExercise, userId: null, term));

        Assert.Equal("alias.invalid-term", error.Code);
    }

    [Fact]
    public void Users_can_alias_system_exercises_and_their_own() {
        ExerciseAlias.Create(Guid.CreateVersion7(), SystemExercise, UserId, "paralela");
        ExerciseAlias.Create(Guid.CreateVersion7(), UserExercise, UserId, "meu");
    }

    [Fact]
    public void Users_cannot_alias_exercises_of_other_users() {
        var error = Assert.Throws<DomainException>(
            () => ExerciseAlias.Create(Guid.CreateVersion7(), UserExercise, Guid.CreateVersion7(), "dele"));

        Assert.Equal("alias.exercise-not-visible", error.Code);
    }

    [Fact]
    public void System_aliases_only_point_to_system_exercises() {
        var error = Assert.Throws<DomainException>(
            () => ExerciseAlias.Create(Guid.CreateVersion7(), UserExercise, userId: null, "meu"));

        Assert.Equal("alias.exercise-not-visible", error.Code);
    }
}
