using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Api.Common;

public static class ResultHttpMapper
{
    public static IResult ToHttpResult<T>(this Result<T> result,
        int successStatusCode = StatusCodes.Status200OK)
    {
        return result.IsSuccess
            ? Results.Json(result.Value, statusCode: successStatusCode)
            : Problem(result.Error);
    }

    public static IResult Problem(AjaiaError error,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Problem(error.Code, error.Message, StatusCode(error.Type), errors);

    public static IResult Problem(string code, string detail, int statusCode,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Results.Json(new ProblemResponse(code, detail, errors),
            statusCode: statusCode, contentType: "application/problem+json");

    private static int StatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}
