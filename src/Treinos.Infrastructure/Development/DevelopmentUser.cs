using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treinos.Domain.Users;
using Treinos.Infrastructure.Persistence;

namespace Treinos.Infrastructure.Development;

/// <summary>
/// O usuário fixo do ambiente de desenvolvimento, enquanto não existe login (o esquema de autenticação de
/// desenvolvimento da API autentica todo pedido como ele). O id é o mesmo do usuário dev do Planner: a mesma
/// pessoa nos dois produtos, como será com o login do Google.
/// </summary>
public static class DevelopmentUser {
    public static readonly Guid Id = Guid.Parse("01999999-0000-7000-8000-000000000001");
    public const string Name = "Dev";
    public const string Email = "dev@treinos.local";

    /// <summary>Cria o usuário no banco se ainda não existir. Só deve ser chamado em desenvolvimento.</summary>
    public static async Task EnsureCreatedAsync(IServiceProvider services, CancellationToken ct = default) {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TreinosDbContext>();
        if(!await db.Users.AnyAsync(u => u.Id == Id, ct)) {
            db.Users.Add(User.Create(Id, Name, new Email(Email)));
            await db.SaveChangesAsync(ct);
        }
    }
}
