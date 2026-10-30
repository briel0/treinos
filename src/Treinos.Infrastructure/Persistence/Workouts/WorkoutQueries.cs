using Microsoft.EntityFrameworkCore;
using Treinos.Application.Workouts;
using Treinos.Domain.Workouts;
using Treinos.Infrastructure.Persistence.Configurations;

namespace Treinos.Infrastructure.Persistence.Workouts;

internal sealed class WorkoutQueries(TreinosDbContext db) : IWorkoutQueries {
    public async Task<IReadOnlyList<WorkoutResponse>> ListAsync(
        Guid userId, DateTimeOffset? since, DateTimeOffset? until, CancellationToken ct) {
        var workouts = db.Workouts.Where(w => w.UserId == userId);
        if(since is { } start) {
            var startUtc = start.ToUniversalTime();
            workouts = workouts.Where(w => w.StartedAt >= startUtc);
        }
        if(until is { } end) {
            var endUtc = end.ToUniversalTime();
            workouts = workouts.Where(w => w.StartedAt < endUtc);
        }
        return await Project(workouts.OrderByDescending(w => w.StartedAt)).ToListAsync(ct);
    }

    public Task<WorkoutResponse?> FindAsync(Guid userId, Guid id, CancellationToken ct) =>
        Project(db.Workouts.Where(w => w.UserId == userId && w.Id == id)).SingleOrDefaultAsync(ct);

    /// <summary>
    /// O treino direto no formato da resposta, numa consulta só: exercícios e séries na ordem, o nome de cada
    /// exercício e as datas, que só o banco tem.
    /// </summary>
    private IQueryable<WorkoutResponse> Project(IQueryable<Workout> workouts) =>
        workouts.AsNoTracking().AsSplitQuery().Select(w => new WorkoutResponse(
            w.Id,
            w.StartedAt,
            w.DurationSec,
            w.Note,
            w.Exercises.OrderBy(entry => entry.SortOrder).Select(entry => new WorkoutExerciseResponse(
                entry.Id,
                entry.ExerciseId,
                db.Exercises.Where(exercise => exercise.Id == entry.ExerciseId).Select(exercise => exercise.Name).First(),
                entry.MeasurementType,
                entry.Sets.OrderBy(set => set.SortOrder).Select(set => new WorkoutSetResponse(
                    set.Id,
                    set.Type,
                    set.IsFailed,
                    set.Reps,
                    set.LoadKg,
                    set.AssistanceKg,
                    set.DurationSec,
                    set.DistanceM,
                    set.Side)).ToList())).ToList(),
            EF.Property<DateTimeOffset>(w, Timestamps.CreatedAt),
            EF.Property<DateTimeOffset>(w, Timestamps.UpdatedAt)));
}
