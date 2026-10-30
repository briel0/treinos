using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treinos.Domain.BodyWeights;
using Treinos.Domain.Users;

namespace Treinos.Infrastructure.Persistence.Configurations;

internal sealed class BodyWeightConfiguration : IEntityTypeConfiguration<BodyWeight> {
    public void Configure(EntityTypeBuilder<BodyWeight> builder) {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasDefaultValueSql("uuidv7()");

        builder.HasOne<User>().WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(b => b.WeightKg).HasPrecision(5, 2);

        // Um registro por dia; o mesmo índice serve "o peso mais recente antes de uma data".
        builder.HasIndex(b => new { b.UserId, b.MeasuredOn }).IsUnique();

        builder.ToTable(table => table.HasCheckConstraint("ck_body_weights_weight_kg", "weight_kg > 0"));

        builder.HasTimestamps();
    }
}
