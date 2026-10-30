using Microsoft.EntityFrameworkCore;
using Treinos.Application.Exercises;
using Treinos.Domain.Exercises;

namespace Treinos.Infrastructure.Persistence.Exercises;

internal sealed class ExerciseRepository(TreinosDbContext db) : IExerciseRepository {
    public Task<Exercise?> FindVisibleAsync(Guid userId, Guid id, CancellationToken ct) =>
        db.Exercises.SingleOrDefaultAsync(e => e.Id == id && (e.UserId == null || e.UserId == userId), ct);
}
