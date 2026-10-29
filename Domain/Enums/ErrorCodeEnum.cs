using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum ErrorCodeEnum
    {
        RoleAlreadyExists,
        UserAlreadyExists,
        InvalidCredential,
        InternalServerError,
        InvalidConfiguration
    }
}
