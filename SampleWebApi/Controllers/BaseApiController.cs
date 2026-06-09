using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult(Result result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => Ok(),

            ResultStatus.BadRequest =>
                BadRequest(result.ErrorMessage),

            ResultStatus.NotFound =>
                NotFound(result.ErrorMessage),

            ResultStatus.Conflict =>
                Conflict(result.ErrorMessage),

            _ => StatusCode(500, result.ErrorMessage)
        };
    }

    protected IActionResult HandleResult<T>(
        Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Ok =>
                Ok(result.Value),

            ResultStatus.BadRequest =>
                BadRequest(result.ErrorMessage),

            ResultStatus.NotFound =>
                NotFound(result.ErrorMessage),

            ResultStatus.Conflict =>
                Conflict(result.ErrorMessage),

            _ => StatusCode(500, result.ErrorMessage)
        };
    }
}
