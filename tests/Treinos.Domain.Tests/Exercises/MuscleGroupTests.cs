using Treinos.Domain.Exercises;

namespace Treinos.Domain.Tests.Exercises;

public class MuscleGroupTests {
    [Fact]
    public void Catalog_has_the_eleven_muscle_groups() {
        Assert.Equal(11, MuscleGroup.AllCodes.Count);
    }

    [Theory]
    [InlineData("pecs")]
    [InlineData("Chest")]
    [InlineData("")]
    public void Rejects_codes_outside_the_catalog(string code) {
        var error = Assert.Throws<DomainException>(() => new MuscleGroup(code));

        Assert.Equal("muscle-group.unknown", error.Code);
    }

    [Fact]
    public void Muscle_groups_with_the_same_code_are_equal() {
        Assert.Equal(new MuscleGroup("chest"), new MuscleGroup("chest"));
    }
}
