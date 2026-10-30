using Microsoft.EntityFrameworkCore;
using Treinos.Application.Workouts;
using Treinos.Domain.Workouts;

namespace Treinos.Infrastructure.Persistence.Workouts;

internal sealed class WorkoutRepository(TreinosDbContext db) : IWorkoutRepository {
    /// <summary>
    /// O treino inteiro, com exercícios e séries na ordem: o domínio usa a posição na lista como ordem
    /// (decisão 15), então a lista precisa chegar ordenada.
    /// </summary>
    public Task<Workout?> FindAsync(Guid userId, Guid id, CancellationToken ct) =>
        db.Workouts
            .Include(w => w.Exercises.OrderBy(entry => entry.SortOrder))
            .ThenInclude(entry => entry.Sets.OrderBy(set => set.SortOrder))
            .AsSplitQuery()
            .SingleOrDefaultAsync(w => w.UserId == userId && w.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) => db.Workouts.AnyAsync(w => w.Id == id, ct);

    public void Add(Workout workout) => db.Workouts.Add(workout);

    public void Remove(Workout workout) => db.Workouts.Remove(workout);
}
