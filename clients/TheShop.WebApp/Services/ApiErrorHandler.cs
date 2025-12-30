using System.Text.Json;

namespace TheShop.WebApp.Services;

public static class ApiErrorHandler
{
    public static string GetUserFriendlyMessage(ApiException ex, string defaultMessage = "An error occurred. Please try again.")
    {
        return ex.StatusCode switch
        {
            400 => ParseValidationError(ex.Response) ?? "Invalid request. Please check your input.",
            401 => "Your session has expired. Please log in again.",
            403 => "You don't have permission to perform this action.",
            404 => "The requested resource was not found.",
            409 => ParseValidationError(ex.Response) ?? "A conflict occurred. Please refresh and try again.",
            422 => ParseValidationError(ex.Response) ?? "The request could not be processed.",
            429 => "Too many requests. Please wait a moment and try again.",
            >= 500 => "A server error occurred. Please try again later.",
            _ => defaultMessage
        };
    }

    public static string? ParseValidationError(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        try
        {
            var errors = JsonSerializer.Deserialize<List<string>>(response);
            if (errors?.Count > 0)
            {
                return string.Join(" ", errors);
            }
        }
        catch
        {
            // Try parsing as ProblemDetails
            try
            {
                var problem = JsonSerializer.Deserialize<ProblemDetailsResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (!string.IsNullOrEmpty(problem?.Detail))
                {
                    return problem.Detail;
                }
                if (!string.IsNullOrEmpty(problem?.Title))
                {
                    return problem.Title;
                }
            }
            catch
            {
                // If all parsing fails, don't return raw response (might contain sensitive info)
            }
        }

        return null;
    }

    private class ProblemDetailsResponse
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
    }
}

