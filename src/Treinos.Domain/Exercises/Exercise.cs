namespace Treinos.Domain.Exercises;

/// <summary>
/// Um exercício do catálogo do sistema (sem dono) ou criado por um usuário (visível só para ele).
/// <para>
/// O tipo de medição não muda depois de criado: as séries já registradas deixariam de bater com ele. Corrigir um
/// tipo errado é criar outro exercício (decisão 6 do AGENTS.md).
/// </para>
/// <para>
/// A unicidade do nome dentro do escopo e a proibição de um exercício do usuário ter o nome de um do sistema
/// dependem dos outros exercícios, e por isso são verificadas fora do domínio (Application e índice do banco).
/// </para>
/// </summary>
public sealed class Exercise {
    private List<ExerciseMuscle> _muscles;

    private Exercise(
        Guid id, Guid? userId, string name, MeasurementType measurementType, bool isUnilateral,
        List<ExerciseMuscle> muscles) {
        Id = id;
        UserId = userId;
        Name = name;
        MeasurementType = measurementType;
        IsUnilateral = isUnilateral;
        _muscles = muscles;
    }

    public Guid Id { get; }

    /// <summary>Dono do exercício; vazio nos exercícios do sistema.</summary>
    public Guid? UserId { get; }

    public bool IsSystem => UserId is null;

    public string Name { get; private set; }

    public MeasurementType MeasurementType { get; }

    /// <summary>Cada lado é treinado separadamente; só nesses exercícios as séries registram o lado.</summary>
    public bool IsUnilateral { get; private set; }

    public IReadOnlyList<ExerciseMuscle> Muscles => _muscles;

    /// <param name="id">Escolhido pelo cliente (UUID v7), o que torna a criação idempotente.</param>
    /// <param name="userId">Dono do exercício, ou vazio para um exercício do sistema.</param>
    public static Exercise Create(
        Guid id, Guid? userId, string name, MeasurementType measurementType, bool isUnilateral,
        IEnumerable<ExerciseMuscle> muscles) {
        ArgumentNullException.ThrowIfNull(muscles);
        return new(
            EntityId.Require(id), userId, NormalizeName(name), measurementType, isUnilateral,
            ValidateMuscles(userId is null, measurementType, muscles));
    }

    /// <summary>Exercícios do sistema são visíveis para todos; os de um usuário, só para ele.</summary>
    public bool IsVisibleTo(Guid userId) => UserId is null || UserId == userId;

    public void Rename(string name) => Name = NormalizeName(name);

    public void ChangeUnilateral(bool isUnilateral) => IsUnilateral = isUnilateral;

    public void ReplaceMuscles(IEnumerable<ExerciseMuscle> muscles) {
        ArgumentNullException.ThrowIfNull(muscles);
        _muscles = ValidateMuscles(IsSystem, MeasurementType, muscles);
    }

    private static string NormalizeName(string name) =>
        SearchTerm.Normalize(name, "exercise.invalid-name", "An exercise name");

    /// <summary>
    /// Um grupo aparece no máximo uma vez. Exercícios do sistema listam ao menos um grupo principal, exceto o
    /// cardio, que não tem grupo nenhum para não inflar as análises por grupo (decisão 13 do AGENTS.md). Nos
    /// exercícios do usuário os grupos são opcionais.
    /// </summary>
    private static List<ExerciseMuscle> ValidateMuscles(
        bool isSystem, MeasurementType measurementType, IEnumerable<ExerciseMuscle> muscles) {
        var list = muscles.ToList();
        if(list.DistinctBy(muscle => muscle.MuscleGroup).Count() != list.Count) {
            throw new DomainException(
                "exercise.duplicate-muscle-group", "A muscle group can appear only once per exercise.");
        }
        if(isSystem && measurementType == MeasurementType.DistanceTime && list.Count > 0) {
            throw new DomainException(
                "exercise.cardio-with-muscle-groups", "System cardio exercises have no muscle groups.");
        }
        if(isSystem && measurementType != MeasurementType.DistanceTime
           && !list.Any(muscle => muscle.Role == MuscleRole.Primary)) {
            throw new DomainException(
                "exercise.missing-primary-muscle-group", "System exercises need at least one primary muscle group.");
        }
        return list;
    }
}
