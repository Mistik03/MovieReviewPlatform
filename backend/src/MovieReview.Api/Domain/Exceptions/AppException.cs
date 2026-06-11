namespace MovieReview.Api.Domain.Exceptions;

/// <summary>Base type for domain exceptions translated to HTTP status codes by the global exception middleware.</summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

/// <summary>Resource does not exist — maps to 404.</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Business rule conflict (duplicates, blocked deletes) — maps to 409.</summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Caller lacks permission for the resource — maps to 403.</summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>Invalid input that passed model binding but fails business validation — maps to 400.</summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
}

/// <summary>Missing or invalid credentials — maps to 401.</summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
}
