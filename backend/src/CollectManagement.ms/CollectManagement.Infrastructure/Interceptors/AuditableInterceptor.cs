using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CollectManagement.Infrastructure.Interceptors;

public class AuditableInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILoggedInUserService _loggedInUserService;

    public AuditableInterceptor(
        IDateTimeProvider dateTimeProvider, 
        ILoggedInUserService loggedInUserService)
    {
        _dateTimeProvider = dateTimeProvider;
        _loggedInUserService = loggedInUserService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {

        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        IEnumerable<EntityEntry<AuditableEntity>> entries =
            eventData
                .Context
                .ChangeTracker
                .Entries<AuditableEntity>();

        string userId = "";
        if (Ulid.TryParse(_loggedInUserService.UserId, out var ulidUserId))
        {
            userId = ulidUserId.ToGuid().ToString();
        }
        
        foreach (var entityEntry in entries)
        {
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    entityEntry.Entity.InsererPar = userId;
                    entityEntry.Entity.DateInsertion = _dateTimeProvider.Now;
                    break;
                case EntityState.Modified:
                    entityEntry.Entity.ModifierPar = userId;
                    entityEntry.Entity.DateModification = _dateTimeProvider.Now;
                    break;
            }
        }
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}