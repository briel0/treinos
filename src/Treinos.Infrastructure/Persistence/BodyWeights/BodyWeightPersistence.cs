using Microsoft.EntityFrameworkCore;
using Treinos.Application.BodyWeights;
using Treinos.Domain.BodyWeights;
using Treinos.Infrastructure.Persistence.Configurations;

namespace Treinos.Infrastructure.Persistence.BodyWeights;

internal sealed class BodyWeightRepository(TreinosDbContext db) : IBodyWeightRepository {
    public Task<BodyWeight?> FindAsync(Guid userId, DateOnly measuredOn, CancellationToken ct) =>
        db.BodyWeights.SingleOrDefaultAsync(b => b.UserId == userId && b.MeasuredOn == measuredOn, ct);

    public void Add(BodyWeight bodyWeight) => db.BodyWeights.Add(bodyWeight);

    public void Remove(BodyWeight bodyWeight) => db.BodyWeights.Remove(bodyWeight);
}

internal sealed class BodyWeightQueries(TreinosDbContext db) : IBodyWeightQueries {
    public async Task<IReadOnlyList<BodyWeightResponse>> ListAsync(
        Guid userId, DateOnly? since, DateOnly? until, CancellationToken ct) {
        var weights = db.BodyWeights.Where(b => b.UserId == userId);
        if(since is { } start) {
            weights = weights.Where(b => b.MeasuredOn >= start);
        }
        if(until is { } end) {
            weights = weights.Where(b => b.MeasuredOn <= end);
        }
        return await Project(weights.OrderByDescending(b => b.MeasuredOn)).ToListAsync(ct);
    }

    public Task<BodyWeightResponse?> FindAsync(Guid userId, DateOnly measuredOn, CancellationToken ct) =>
        Project(db.BodyWeights.Where(b => b.UserId == userId && b.MeasuredOn == measuredOn)).SingleOrDefaultAsync(ct);

    private static IQueryable<BodyWeightResponse> Project(IQueryable<BodyWeight> weights) =>
        weights.AsNoTracking().Select(b => new BodyWeightResponse(
            b.MeasuredOn, b.WeightKg, EF.Property<DateTimeOffset>(b, Timestamps.UpdatedAt)));
}
