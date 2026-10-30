using Microsoft.EntityFrameworkCore;
using Npgsql;
using Treinos.Application.Common;

namespace Treinos.Infrastructure.Persistence;

/// <summary>Salva as mudanças e traduz os conflitos do banco em erros dos casos de uso.</summary>
internal sealed class UnitOfWork(TreinosDbContext db) : IUnitOfWork {
    /// <summary>Restrições de unicidade e o erro que cada uma representa (cobre corridas entre dois pedidos).</summary>
    private static readonly Dictionary<string, (string Code, string Message)> UniqueViolations = new() {
        ["pk_workouts"] = ("id-taken", "This id is already in use."),
        ["pk_workout_exercises"] = ("id-taken", "This id is already in use."),
        ["pk_workout_sets"] = ("id-taken", "This id is already in use."),
        ["ix_body_weights_user_id_measured_on"] =
            ("body-weight.day-taken", "There is already a body weight for this day."),
    };

    public async Task SaveChangesAsync(CancellationToken ct) {
        try {
            await db.SaveChangesAsync(ct);
        } catch(DbUpdateException error) when(error.InnerException is PostgresException {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: { } constraint,
        } && UniqueViolations.ContainsKey(constraint)) {
            var (code, message) = UniqueViolations[constraint];
            throw new UseCaseException(UseCaseErrorKind.Conflict, code, message);
        }
    }
}
