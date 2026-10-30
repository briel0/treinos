using Microsoft.EntityFrameworkCore;
using Treinos.Application.Exercises;
using Treinos.Domain.Exercises;

namespace Treinos.Infrastructure.Persistence.Exercises;

internal sealed class ExerciseQueries(TreinosDbContext db) : IExerciseQueries {
    public async Task<IReadOnlyList<ExerciseResponse>> ListAsync(Guid userId, CancellationToken ct) {
        var exercises = await Visible(userId).OrderBy(e => e.Name).ToListAsync(ct);
        var aliases = await AliasesByExercise(userId, exerciseId: null, ct);
        return exercises.ConvertAll(exercise => ToResponse(exercise, aliases));
    }

    public async Task<ExerciseResponse?> FindAsync(Guid userId, Guid id, CancellationToken ct) {
        var exercise = await Visible(userId).SingleOrDefaultAsync(e => e.Id == id, ct);
        return exercise is null ? null : ToResponse(exercise, await AliasesByExercise(userId, id, ct));
    }

    /// <summary>
    /// Quatro consultas em ordem de precedência, parando na primeira que achar. A comparação usa a collation das
    /// colunas, que ignora maiúsculas e acentos ("paralela" = "Paralela" = "parálela").
    /// </summary>
    public async Task<Guid?> ResolveTermAsync(Guid userId, string term, CancellationToken ct) =>
        await db.Exercises.Where(e => e.UserId == userId && e.Name == term).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct)
        ?? await db.ExerciseAliases.Where(a => a.UserId == userId && a.Term == term)
            .Select(a => (Guid?)a.ExerciseId).FirstOrDefaultAsync(ct)
        ?? await db.Exercises.Where(e => e.UserId == null && e.Name == term).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct)
        ?? await db.ExerciseAliases.Where(a => a.UserId == null && a.Term == term)
            .Select(a => (Guid?)a.ExerciseId).FirstOrDefaultAsync(ct);

    /// <summary>Os exercícios do sistema e os do usuário, com os grupos musculares (tabela própria, carregada junto).</summary>
    private IQueryable<Exercise> Visible(Guid userId) =>
        db.Exercises.AsNoTracking().Where(e => e.UserId == null || e.UserId == userId);

    /// <summary>Os apelidos que o usuário enxerga (do sistema e os dele), agrupados por exercício.</summary>
    private async Task<ILookup<Guid, string>> AliasesByExercise(Guid userId, Guid? exerciseId, CancellationToken ct) {
        var aliases = db.ExerciseAliases.AsNoTracking().Where(a => a.UserId == null || a.UserId == userId);
        if(exerciseId is { } id) {
            aliases = aliases.Where(a => a.ExerciseId == id);
        }
        var rows = await aliases.OrderBy(a => a.Term).Select(a => new { a.ExerciseId, a.Term }).ToListAsync(ct);
        return rows.ToLookup(row => row.ExerciseId, row => row.Term);
    }

    private static ExerciseResponse ToResponse(Exercise exercise, ILookup<Guid, string> aliases) => new(
        exercise.Id,
        exercise.Name,
        exercise.MeasurementType,
        exercise.IsUnilateral,
        exercise.IsSystem,
        [.. exercise.Muscles
            .OrderBy(muscle => muscle.Role)
            .Select(muscle => new ExerciseMuscleResponse(muscle.MuscleGroup.Code, muscle.Role))],
        [.. aliases[exercise.Id]]);
}
