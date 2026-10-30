using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Treinos.Application.BodyWeights;
using Treinos.Application.Common;
using Treinos.Application.Exercises;
using Treinos.Application.Workouts;
using Treinos.Infrastructure.Persistence;
using Treinos.Infrastructure.Persistence.BodyWeights;
using Treinos.Infrastructure.Persistence.Exercises;
using Treinos.Infrastructure.Persistence.Workouts;

namespace Treinos.Infrastructure;

public static class DependencyInjection {
    /// <summary>Registra os serviços da camada de infraestrutura (banco de dados).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        services.AddDbContext<TreinosDbContext>(options => Configure(options,
            configuration.GetConnectionString("Treinos")
            ?? throw new InvalidOperationException(
                "Connection string 'Treinos' is not configured. See 'Primeira configuração' in AGENTS.md.")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IExerciseQueries, ExerciseQueries>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IWorkoutQueries, WorkoutQueries>();
        services.AddScoped<IBodyWeightRepository, BodyWeightRepository>();
        services.AddScoped<IBodyWeightQueries, BodyWeightQueries>();

        return services;
    }

    /// <summary>
    /// Npgsql com nomes em snake_case e o histórico de migrations no schema do produto. O <c>search_path</c> fica
    /// no schema <c>treinos</c>, para que nomes não qualificados (como a collation) sejam achados ali.
    /// </summary>
    internal static DbContextOptionsBuilder Configure(DbContextOptionsBuilder options, string connectionString) {
        var connection = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = TreinosDbContext.Schema };
        return options
            .UseNpgsql(connection.ConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", TreinosDbContext.Schema))
            .UseSnakeCaseNamingConvention();
    }
}
