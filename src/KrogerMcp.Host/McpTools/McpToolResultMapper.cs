using KrogerMcp.Domain.Common;

namespace KrogerMcp.Host.McpTools;

internal static class McpToolResultMapper
{
    public static T Unwrap<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value!;
        }

        var error = result.Error!;
        throw new InvalidOperationException($"{error.Category}: {error.Message}");
    }
}
