using DogPlatform.API.Exceptions;

namespace DogPlatform.API.Middlewares;

internal sealed class ExceptionHandlingMiddleware(
    RequestDelegate next, 
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            if (ex is DomainException domainException)
            {
                httpContext.Response.StatusCode = (int) domainException.ErrorCode;

                if (string.IsNullOrEmpty(domainException.Message))
                {
                    await httpContext.Response.WriteAsync(ex.Message);
                    return;
                }
                
                await httpContext.Response.WriteAsJsonAsync(new 
                {
                    message = domainException.Message
                });
            }
            else
            {
                logger.LogError(ex, "Unhandled exception occurred");
                
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(new 
                {
                    message = "Something went wrong! Please contact us"
                });
            }
        }
    }
}