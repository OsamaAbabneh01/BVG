using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum StatusCodeEnum
    {
        Ok = 200,
        Created = 201,
        Accepted = 202,
        NoContent = 204,
        BadRequest = 400,
        UnAuthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        Locked = 423,
        TooManyRequests = 429,
        Conflict = 409,
        InternalServerError = 500,
        BadGateway = 502,
        ServiceUnavailable = 503
    }
}
