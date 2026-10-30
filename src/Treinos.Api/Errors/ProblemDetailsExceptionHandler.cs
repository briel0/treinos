using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Treinos.Application.Common;
using Treinos.Domain;

namespace Treinos.Api.Errors;

/// <summary>
/// Converte os erros de regra em Problem Details (RFC 9457) com o <c>code</c> estável. Qualquer outra exceção
/// segue para o tratamento padrão: 500, sem detalhes internos para o cliente.
/// </summary>
internal sealed class ProblemDetailsExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct) {
        (int Status, string Code)? problem = exception switch {
            DomainException domain => (StatusCodes.Status400BadRequest, domain.Code),
            UseCaseException { Kind: UseCaseErrorKind.NotFound } useCase => (StatusCodes.Status404NotFound, useCase.Code),
            UseCaseException { Kind: UseCaseErrorKind.Conflict } useCase => (StatusCodes.Status409Conflict, useCase.Code),
            _ => null,
        };
        if(problem is not { } p) {
            return false;
        }

        context.Response.StatusCode = p.Status;
        return await problemDetails.TryWriteAsync(new() {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails {
                Status = p.Status,
                Title = exception.Message,
                Type = $"https://treinos.dev/errors/{p.Code}",
                Extensions = { ["code"] = p.Code },
            },
        });
    }
}
