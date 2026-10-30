using Treinos.Domain.BodyWeights;

namespace Treinos.Domain.Tests.BodyWeights;

public class BodyWeightTests {
    private static BodyWeight Create(decimal weightKg) =>
        BodyWeight.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), new DateOnly(2026, 10, 30), weightKg);

    [Fact]
    public void Create_keeps_the_day_and_weight() {
        var weight = Create(80.45m);

        Assert.Equal(new DateOnly(2026, 10, 30), weight.MeasuredOn);
        Assert.Equal(80.45m, weight.WeightKg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-70)]
    [InlineData(1000)]
    [InlineData(80.123)] // três casas decimais
    public void Weight_must_be_positive_up_to_999_99_with_two_decimals(double weightKg) {
        var error = Assert.Throws<DomainException>(() => Create((decimal)weightKg));

        Assert.Equal("body-weight.invalid-weight", error.Code);
    }

    [Fact]
    public void ChangeWeight_applies_the_same_rules() {
        var weight = Create(80);

        weight.ChangeWeight(79.5m);

        Assert.Equal(79.5m, weight.WeightKg);
        Assert.Throws<DomainException>(() => weight.ChangeWeight(0));
    }

    [Fact]
    public void Create_rejects_ids_that_are_not_uuid_version_7() {
        var error = Assert.Throws<DomainException>(() => BodyWeight.Create(
            Guid.NewGuid(), Guid.CreateVersion7(), new DateOnly(2026, 10, 30), 80)); // versão 4

        Assert.Equal("invalid-id", error.Code);
    }
}
