using Ardalis.Result;
using FastEndpoints;

namespace BusStop.Web.Extensions;

public static class ResultExtensions
{
    public static async Task ToOkResultAsync<TResponse>(this IEndpoint ep, Result<TResponse> result, CancellationToken ct)
    {
        if (result.IsSuccess)
        {
            await ep.HttpContext.Response.SendAsync(result.Value, cancellation: ct);
            return;
        }
        await ep.HandleErrorResultAsync(result, ct);
    }

    public static async Task ToCreatedResultAsync<TResponse>(this IEndpoint ep, Result<TResponse> result, object routeValues, CancellationToken ct)
    {
        if (result.IsSuccess)
        {
            // FastEndpoints doesn't have a direct IEndpoint.SendCreatedAtAsync that takes route values easily without the endpoint type.
            // We can use SendAsync with 201 status code.
            ep.HttpContext.Response.StatusCode = 201;
            // Note: Ideally we'd set the Location header here using routeValues
            await ep.HttpContext.Response.SendAsync(result.Value, cancellation: ct);
            return;
        }
        await ep.HandleErrorResultAsync(result, ct);
    }

    public static async Task ToGetByIdResultAsync<TResponse>(this IEndpoint ep, Result<TResponse> result, CancellationToken ct)
    {
        if (result.IsSuccess)
        {
            await ep.HttpContext.Response.SendAsync(result.Value, cancellation: ct);
            return;
        }
        await ep.HandleErrorResultAsync(result, ct);
    }

    public static async Task ToNoContentResultAsync(this IEndpoint ep, Result result, CancellationToken ct)
    {
        if (result.IsSuccess)
        {
            await ep.HttpContext.Response.SendAsync<object?>(null, 204, cancellation: ct);
            return;
        }
        await ep.HandleErrorResultAsync(result, ct);
    }

    private static async Task HandleErrorResultAsync(this IEndpoint ep, Ardalis.Result.IResult result, CancellationToken ct)
    {
        if (result.Status == ResultStatus.NotFound)
        {
            await ep.HttpContext.Response.SendNotFoundAsync(cancellation: ct);
            return;
        }

        if (result.Status == ResultStatus.Unauthorized)
        {
            await ep.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
            return;
        }

        if (result.Status == ResultStatus.Forbidden)
        {
            await ep.HttpContext.Response.SendForbiddenAsync(cancellation: ct);
            return;
        }

        if (result.Status == ResultStatus.Invalid)
        {
            foreach (var error in result.ValidationErrors)
            {
                ep.ValidationFailures.Add(new FluentValidation.Results.ValidationFailure(error.Identifier, error.ErrorMessage));
            }
            await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, cancellation: ct);
            return;
        }

        await ep.HttpContext.Response.SendErrorsAsync([new FluentValidation.Results.ValidationFailure("Error", result.Errors.FirstOrDefault() ?? "An error occurred")], cancellation: ct);
    }
}
