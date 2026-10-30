using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Treinos.Api.Authentication;
using Treinos.Api.Errors;
using Treinos.Application;
using Treinos.Application.Common;
using Treinos.Infrastructure;
using Treinos.Infrastructure.Development;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Autenticação: por enquanto só existe o esquema de desenvolvimento, e ele nunca pode valer em outro ambiente.
// Na VPS a API roda em Development escutando só em 127.0.0.1, como o Planner.
if(!builder.Environment.IsDevelopment()) {
    throw new InvalidOperationException(
        "No authentication is configured outside Development: the development scheme must never run there.");
}
builder.Services
    .AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
        DevelopmentAuthenticationHandler.SchemeName, configureOptions: null);
// Todo endpoint exige um usuário autenticado, sem precisar marcar um por um.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, ClaimsCurrentUser>();

// Erros: Problem Details com o campo "code".
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

// JSON estrito (números só como números) e listas de valores em snake_case ("weight_reps"), o mesmo formato
// do banco (decisão 9). O corpo dos pedidos é sempre application/json.
builder.Services.AddControllers(options => {
    var json = options.InputFormatters.OfType<SystemTextJsonInputFormatter>().Single();
    json.SupportedMediaTypes.Remove("text/json");
    json.SupportedMediaTypes.Remove("application/*+json");
})
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
    })
    .ConfigureApiBehaviorOptions(options => {
        var defaultFactory = options.InvalidModelStateResponseFactory;
        options.InvalidModelStateResponseFactory = context => {
            var result = defaultFactory(context);
            if(result is Microsoft.AspNetCore.Mvc.ObjectResult { Value: Microsoft.AspNetCore.Mvc.ProblemDetails problem }) {
                problem.Extensions["code"] = "validation";
            }
            return result;
        };
    });
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi().AllowAnonymous();
await DevelopmentUser.EnsureCreatedAsync(app.Services);

app.MapControllers();

await app.RunAsync();
