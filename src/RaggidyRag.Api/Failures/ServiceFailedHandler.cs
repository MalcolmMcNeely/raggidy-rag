using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RaggidyRag.Api.Failures;

public sealed class ServiceFailedHandler(IProblemDetailsService problems) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellation)
    {
        if (exception is not ServiceFailed failed)
        {
            return ValueTask.FromResult(false);
        }

        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        return problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = $"{failed.Service} failed",
                Detail = failed.InnerException?.Message,
            },
        });
    }
}
