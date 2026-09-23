namespace ChronoKraft.Application.Services;

public enum AccessOutcome { Success, NotFound, Forbidden }

public record ServiceResult<T>(AccessOutcome Outcome, T? Value)
{
    public static ServiceResult<T> Success(T value) => new(AccessOutcome.Success, value);
    public static ServiceResult<T> NotFound() => new(AccessOutcome.NotFound, default);
    public static ServiceResult<T> Forbidden() => new(AccessOutcome.Forbidden, default);
}