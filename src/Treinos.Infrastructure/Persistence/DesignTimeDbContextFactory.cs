using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Treinos.Infrastructure.Persistence;

/// <summary>
/// Usado só pelas ferramentas do EF Core (<c>dotnet ef migrations add</c>), que não se conectam ao banco.
/// Para aplicar migrations, gere o script SQL (<c>dotnet ef migrations script --idempotent</c>).
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TreinosDbContext> {
    public TreinosDbContext CreateDbContext(string[] args) {
        var options = new DbContextOptionsBuilder<TreinosDbContext>();
        DependencyInjection.Configure(options, "Host=localhost");
        return new TreinosDbContext(options.Options);
    }
}
