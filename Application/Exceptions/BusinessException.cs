using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Exceptions
{
    public class BusinessException : Exception
    {
        public ErrorCodeEnum ErrorCode { get; set; }
        public StatusCodeEnum StatusCode { get; set; }

        public BusinessException(ErrorCodeEnum errorCode, StatusCodeEnum statusCode)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}
