namespace Treinos.Domain;

/// <summary>
/// Regra comum a valores em kg: no máximo duas casas decimais (cobre anilhas de 1,25 kg e balanças) e um teto que
/// acompanha o tipo da coluna no banco (<c>numeric(6,2)</c> ou <c>numeric(5,2)</c>).
/// </summary>
internal static class Kilograms {
    public static decimal Require(decimal value, decimal max, bool allowZero, string errorCode, string description) {
        var min = allowZero ? "zero or more" : "more than zero";
        if(value < 0 || (!allowZero && value == 0) || value > max || decimal.Round(value, 2) != value) {
            throw new DomainException(
                errorCode, $"{description} must be {min}, at most {max} kg, with up to two decimal places.");
        }
        return value;
    }
}
