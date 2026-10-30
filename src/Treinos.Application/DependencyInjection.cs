using Microsoft.Extensions.DependencyInjection;
using Treinos.Application.BodyWeights;
using Treinos.Application.Exercises;
using Treinos.Application.Workouts;

namespace Treinos.Application;

public static class DependencyInjection {
    /// <summary>Registra os casos de uso. As interfaces que eles usam são implementadas pela Infrastructure.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddScoped<ExerciseUseCases>();
        services.AddScoped<WorkoutUseCases>();
        services.AddScoped<BodyWeightUseCases>();
        return services;
    }
}
