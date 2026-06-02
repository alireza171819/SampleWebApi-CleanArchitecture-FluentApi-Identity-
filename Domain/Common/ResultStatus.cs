
namespace Domain.Common;

public enum ResultStatus
{
    Ok,
    Error,
    Forbidden,
    Unauthorized,
    Invalid,
    NotFound,
    NoContent,
    Conflict,
    CriticalError,
    Unavailable,
    BadRequest,
    InternalServerError,
    ClientClosedRequest
}
