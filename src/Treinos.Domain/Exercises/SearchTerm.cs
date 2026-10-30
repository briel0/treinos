namespace Treinos.Domain.Exercises;

/// <summary>
/// Regra comum a nomes de exercício e apelidos, que são termos de busca: além das regras de texto obrigatório,
/// espaços repetidos viram um só, porque o banco ignora maiúsculas e acentos, mas não espaços.
/// </summary>
internal static class SearchTerm {
    public const int MaxLength = 100;

    public static string Normalize(string value, string errorCode, string description) {
        ArgumentNullException.ThrowIfNull(value);
        var collapsed = string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return RequiredText.Normalize(collapsed, MaxLength, errorCode, description);
    }
}
