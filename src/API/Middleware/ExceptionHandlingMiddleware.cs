using AutoTallerManager.API.Helpers;
using AutoTallerManager.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace AutoTallerManager.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            BusinessRuleException business => (HttpStatusCode.BadRequest, business.Message),
            UnauthorizedAccessException unauthorized => (HttpStatusCode.Forbidden, unauthorized.Message),
            InvalidOperationException invalid when RenderConnectionHelper.IsCloudHost => (HttpStatusCode.ServiceUnavailable, invalid.Message),
            _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(response);
    }
}
