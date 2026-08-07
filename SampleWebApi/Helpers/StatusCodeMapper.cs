using ApplicationService.Common;

namespace SampleWebApi.Helpers;

public class StatusCodeMapper
{
    public static int Map(ResultStatus status)
    {
        return status switch
        {
            ResultStatus.Ok => StatusCodes.Status200OK,

            ResultStatus.NoContent => StatusCodes.Status204NoContent,

            ResultStatus.BadRequest => StatusCodes.Status400BadRequest,
            ResultStatus.Invalid => StatusCodes.Status400BadRequest,

            ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,

            ResultStatus.Forbidden => StatusCodes.Status403Forbidden,

            ResultStatus.NotFound => StatusCodes.Status404NotFound,

            ResultStatus.Conflict => StatusCodes.Status409Conflict,

            ResultStatus.Unavailable => StatusCodes.Status503ServiceUnavailable,

            ResultStatus.ClientClosedRequest => 499,

            ResultStatus.Error => StatusCodes.Status500InternalServerError,
            ResultStatus.CriticalError => StatusCodes.Status500InternalServerError,
            ResultStatus.InternalServerError => StatusCodes.Status500InternalServerError,

            _ => StatusCodes.Status500InternalServerError
        };
    }
}
