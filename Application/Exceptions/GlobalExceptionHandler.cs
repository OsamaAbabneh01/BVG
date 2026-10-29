using API.Resources;
using Microsoft.AspNetCore.Diagnostics;
using Domain.Enums;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace Application.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IStringLocalizer<ErrorMessages> localizer,
            ILogger<GlobalExceptionHandler> logger)
        {
            _localizer = localizer;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is BusinessException businessException)
            {
                httpContext.Response.StatusCode = (int)businessException.StatusCode;
                await httpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        errorCode = businessException.ErrorCode.ToString(),
                        message = _localizer[businessException.ErrorCode.ToString()].Value
                    }
                );
                return true;
            }

            httpContext.Response.StatusCode = (int)StatusCodeEnum.InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    errorCode = ErrorCodeEnum.InternalServerError.ToString(),
                    //message = _localizer[ErrorCodeEnum.InternalServerError.ToString()].Value
                    message = exception.Message
                }
            );
            _logger.LogError(exception.Message);
            return true;
        }
    }
}
