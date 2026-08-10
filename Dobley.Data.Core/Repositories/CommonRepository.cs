using Dobley.Data.Core.Context;
using System.Diagnostics;
using Dobley.Domain.Core.Entities;
using Dobley.Domain.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dobley.Data.Core.Repositories;

public class CommonRepository(DobleyContext context)
    : ICommonRepository
{
    public void FreeContext()
    {
        Debug.Write($"{context.ChangeTracker.Entries().Count():0000} / ");

        context.ChangeTracker.Clear();

        Debug.Write($"{context.ChangeTracker.Entries().Count():0000} / ");
    }

    public void FreeContext(object entity) => FreeContext([entity]);

    public void FreeContext(IEnumerable<object> exceptEntities)
    {
        FreeContext();

        foreach (var entity in exceptEntities)
        {
            Attach(context, entity);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entityEntries = context.ChangeTracker.Entries().ToArray();

        foreach (var entityEntry in entityEntries)
        {
            if (entityEntry.State == EntityState.Added && entityEntry.Entity is IAuditableEntity addedEntity)
            {
                addedEntity.SetDateAdded(now);
            }

            if (entityEntry.State is EntityState.Added or EntityState.Modified &&
                entityEntry.Entity is IAuditableEntity updatedEntity)
            {
                updatedEntity.SetDateUpdated(now);
            }

            if (entityEntry.State == EntityState.Deleted && entityEntry.Entity is ISoftDeletedEntity deletedEntity)
            {
                deletedEntity.Delete(now);
                entityEntry.State = EntityState.Modified;

                if (entityEntry.Entity is IAuditableEntity deletedUpdatedEntity)
                {
                    deletedUpdatedEntity.SetDateUpdated(now);
                }
            }
        }

        return context.SaveChangesAsync(cancellationToken);
    }

    public static EntityEntry GetEntry(DbContext dbContext, object entity) => dbContext.Entry(entity);

    public static void Attach(DbContext dbContext, object entity)
    {
        var entry = GetEntry(dbContext, entity);
        if (entry.State == EntityState.Detached)
        {
            entry.State = EntityState.Unchanged;
        }
    }

    public static void Detach(DbContext dbContext, object entity)
    {
        GetEntry(dbContext, entity).State = EntityState.Detached;
    }

    private static void FreeContext(IEnumerable<EntityEntry> entries)
    {
        foreach (var entry in entries.ToArray())
        {
            entry.State = EntityState.Detached;
        }
    }
}
