using erp_backend.Messages;

namespace erp_backend.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(int statusCode, ResponseMessage message, params object[] args)
        : base(message.ToText(args))
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(ResponseMessage message, params object[] args)
        : base(StatusCodes.Status404NotFound, message, args)
    {
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(ResponseMessage message, params object[] args)
        : base(StatusCodes.Status400BadRequest, message, args)
    {
    }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(ResponseMessage message, params object[] args)
        : base(StatusCodes.Status401Unauthorized, message, args)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(ResponseMessage message, params object[] args)
        : base(StatusCodes.Status403Forbidden, message, args)
    {
    }
}
