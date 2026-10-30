using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treinos.Domain.Users;

namespace Treinos.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasDefaultValueSql("uuidv7()");

        // Email (objeto de valor) ↔ coluna de texto.
        builder.Property(u => u.Email).HasConversion(email => email.Value, value => new Email(value));
        builder.HasIndex(u => u.Email).IsUnique();

        builder.ToTable(table => {
            table.HasCheckConstraint("ck_users_name", Checks.RequiredText("name", User.NameMaxLength));
            table.HasCheckConstraint("ck_users_email", $"length(email) <= {Email.MaxLength} AND email = lower(email)");
        });

        builder.HasTimestamps();
    }
}
