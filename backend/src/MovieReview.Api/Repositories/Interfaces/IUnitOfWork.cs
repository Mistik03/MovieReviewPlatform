namespace MovieReview.Api.Repositories.Interfaces;

/// <summary>
/// Allows a service to run several repository operations atomically
/// without taking a dependency on the DbContext itself.
/// </summary>
public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
}
