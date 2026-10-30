namespace Treinos.Domain.BodyWeights;

/// <summary>
/// O peso corporal de um usuário num dia. Com ele, a carga real dos exercícios de peso corporal sai das
/// consultas. O limite de um registro por dia depende dos outros registros, e por isso é verificado fora do
/// domínio (Application e índice único do banco).
/// </summary>
public sealed class BodyWeight {
    public const decimal MaxWeightKg = 999.99m;

    private BodyWeight(Guid id, Guid userId, DateOnly measuredOn, decimal weightKg) {
        Id = id;
        UserId = userId;
        MeasuredOn = measuredOn;
        WeightKg = weightKg;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    /// <summary>Dia do calendário, sem hora nem fuso: "pesei no dia 30" vale onde o usuário estiver.</summary>
    public DateOnly MeasuredOn { get; }

    public decimal WeightKg { get; private set; }

    /// <param name="id">Escolhido pelo cliente (UUID v7), o que torna a criação idempotente.</param>
    public static BodyWeight Create(Guid id, Guid userId, DateOnly measuredOn, decimal weightKg) =>
        new(EntityId.Require(id), userId, measuredOn, ValidateWeight(weightKg));

    public void ChangeWeight(decimal weightKg) => WeightKg = ValidateWeight(weightKg);

    private static decimal ValidateWeight(decimal weightKg) =>
        Kilograms.Require(weightKg, MaxWeightKg, allowZero: false, "body-weight.invalid-weight", "A body weight");
}
