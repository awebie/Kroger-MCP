using System.Net;
using KrogerMcp.Domain.Common;

namespace KrogerMcp.Infrastructure.Kroger.Mapping;

public static class KrogerErrorMapper
{
    public static KrogerError FromStatus(HttpStatusCode statusCode, string? body)
    {
        var category = statusCode switch
        {
            HttpStatusCode.BadRequest => ErrorCategory.InvalidInput,
            HttpStatusCode.Unauthorized => ErrorCategory.Unauthorized,
            HttpStatusCode.Forbidden => ErrorCategory.Forbidden,
            HttpStatusCode.NotFound => ErrorCategory.NotFound,
            >= HttpStatusCode.InternalServerError => ErrorCategory.UpstreamUnavailable,
            _ => ErrorCategory.UnexpectedUpstreamResponse
        };

        var message = string.IsNullOrWhiteSpace(body)
            ? $"Kroger API returned {(int)statusCode}."
            : $"Kroger API returned {(int)statusCode}: {body}";
        return new KrogerError(category, message, (int)statusCode);
    }
}
