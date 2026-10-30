namespace Treinos.Domain.Exercises;

/// <summary>
/// Grupo muscular do catálogo fixo do sistema, identificado por um código legível (<c>chest</c>), que é o que os
/// clientes enviam. Usuários escolhem desta lista, mas não criam grupos novos.
/// </summary>
public sealed record MuscleGroup {
    private static readonly HashSet<string> Codes = [
        "chest", "back", "shoulders", "biceps", "triceps", "forearms",
        "quadriceps", "hamstrings", "glutes", "calves", "abs",
    ];

    public MuscleGroup(string code) {
        ArgumentNullException.ThrowIfNull(code);
        if(!Codes.Contains(code)) {
            throw new DomainException("muscle-group.unknown", $"'{code}' is not a known muscle group.");
        }
        Code = code;
    }

    public string Code { get; }

    public static IReadOnlyCollection<string> AllCodes => Codes;

    public override string ToString() => Code;
}
