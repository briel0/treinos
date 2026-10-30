using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Treinos.Infrastructure.Persistence.Configurations;

/// <summary>
/// Listas de valores fixos viram texto em snake_case (<c>WeightReps</c> ↔ <c>'weight_reps'</c>), protegido por
/// um CHECK (decisão 9). É o mesmo formato do JSON da API.
/// </summary>
internal sealed class SnakeCaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    value => SnakeCaseEnum.Names<TEnum>()[value],
    text => SnakeCaseEnum.Values<TEnum>()[text])
    where TEnum : struct, Enum;

internal static class SnakeCaseEnum {
    public static Dictionary<TEnum, string> Names<TEnum>() where TEnum : struct, Enum => Cache<TEnum>.Names;

    public static Dictionary<string, TEnum> Values<TEnum>() where TEnum : struct, Enum => Cache<TEnum>.Values;

    /// <summary>A condição do CHECK: <c>column IN ('a', 'b')</c>.</summary>
    public static string CheckIn<TEnum>(string column) where TEnum : struct, Enum =>
        $"{column} IN ({string.Join(", ", Cache<TEnum>.Names.Values.Select(name => $"'{name}'"))})";

    private static class Cache<TEnum> where TEnum : struct, Enum {
        public static readonly Dictionary<TEnum, string> Names = Enum.GetValues<TEnum>()
            .ToDictionary(value => value, value => JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString()));

        public static readonly Dictionary<string, TEnum> Values =
            Names.ToDictionary(pair => pair.Value, pair => pair.Key);
    }
}
