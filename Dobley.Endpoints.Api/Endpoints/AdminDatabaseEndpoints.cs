using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Dobley.Data.Core.Context;
using Dobley.Data.Core.Repositories.Users;
using Dobley.Domain.Core.Entities;
using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Repositories;
using Dobley.Endpoints.Api.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Dobley.Endpoints.Api.Endpoints;

public static class AdminDatabaseEndpoints
{
    private const int DEFAULT_PAGE_SIZE = 50;
    private const int MAX_PAGE_SIZE = 500;

    public static IEndpointRouteBuilder MapAdminDatabaseEndpoints(this IEndpointRouteBuilder app)
    {
        var adminApi = app.MapGroup("/admin/db")
            .RequireAuthorization()
            .WithTags("Admin database");

        adminApi.MapGet("/tables", ([FromServices] DobleyContext db)
            => Results.Ok(db.Model.GetEntityTypes()
                .Where(x => x.GetTableName() != null)
                .OrderBy(x => x.GetTableName())
                .Select(AdminTableInfo.Create)));

        adminApi.MapGet("/{tableName}", async (string tableName, [FromQuery] int? pageIndex,
            [FromQuery] int? pageSize, [FromServices] DobleyContext db, CancellationToken cancellationToken) =>
        {
            var entityType = FindEntityType(db, tableName);
            if (entityType == null)
            {
                return Results.NotFound(new { error = "Таблица не найдена" });
            }

            var index = Math.Max(pageIndex ?? 1, 1);
            var size = Math.Clamp(pageSize ?? DEFAULT_PAGE_SIZE, 1, MAX_PAGE_SIZE);
            var rows = await ToListAsync(entityType.ClrType, CreateQueryable(db, entityType), cancellationToken);

            return Results.Ok(new AdminTableRowsResponse(
                entityType.GetTableName() ?? entityType.ClrType.Name,
                index,
                size,
                rows.Count,
                rows.Skip((index - 1) * size).Take(size)));
        });

        adminApi.MapGet("/{tableName}/{key}", async (string tableName, string key, [FromServices] DobleyContext db,
            CancellationToken cancellationToken) =>
        {
            var entityType = FindEntityType(db, tableName);
            if (entityType == null)
            {
                return Results.NotFound(new { error = "Таблица не найдена" });
            }

            var entity = await FindByKeyAsync(db, entityType, key, cancellationToken);
            return entity == null ? Results.NotFound() : Results.Ok(entity);
        });

        adminApi.MapPost("/{tableName}", async (string tableName, [FromBody] JsonElement body,
            [FromServices] DobleyContext db, [FromServices] ICommonRepository commonRepository,
            CancellationToken cancellationToken) =>
        {
            var entityType = FindEntityType(db, tableName);
            if (entityType == null)
            {
                return Results.NotFound(new { error = "Таблица не найдена" });
            }

            var entity = CreateEntity(entityType, body, allowKeyProperties: true);
            db.Add(entity);
            await commonRepository.SaveChangesAsync(cancellationToken);

            return Results.Created($"/admin/db/{tableName}/{GetKeyValue(entityType, entity)}", entity);
        });

        adminApi.MapPatch("/{tableName}/{key}", async (string tableName, string key, [FromBody] JsonElement body,
            [FromServices] DobleyContext db, [FromServices] ICommonRepository commonRepository,
            CancellationToken cancellationToken) =>
        {
            var entityType = FindEntityType(db, tableName);
            if (entityType == null)
            {
                return Results.NotFound(new { error = "Таблица не найдена" });
            }

            var entity = await FindByKeyAsync(db, entityType, key, cancellationToken);
            if (entity == null)
            {
                return Results.NotFound();
            }

            ApplyJson(entityType, entity, body, allowKeyProperties: false);
            await commonRepository.SaveChangesAsync(cancellationToken);

            return Results.Ok(entity);
        });

        adminApi.MapDelete("/{tableName}/{key}", async (string tableName, string key, [FromQuery] bool? hardDelete,
            [FromServices] DobleyContext db, [FromServices] ICommonRepository commonRepository,
            CancellationToken cancellationToken) =>
        {
            var entityType = FindEntityType(db, tableName);
            if (entityType == null)
            {
                return Results.NotFound(new { error = "Таблица не найдена" });
            }

            var entity = await FindByKeyAsync(db, entityType, key, cancellationToken);
            if (entity == null)
            {
                return Results.NotFound();
            }

            if (hardDelete == true || entity is not ISoftDeletedEntity)
            {
                db.Remove(entity);
            }
            else
            {
                ((ISoftDeletedEntity)entity).Delete(DateTime.UtcNow);
            }

            await commonRepository.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });

        var adminUsersApi = app.MapGroup("/admin/users")
            .RequireAuthorization()
            .WithTags("Admin users");

        adminUsersApi.MapPost("/", async ([FromBody] AdminCreateUserRequest request, [FromServices] DobleyContext db,
            [FromServices] ICommonRepository commonRepository, CancellationToken cancellationToken) =>
        {
            if (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Login == request.Login, cancellationToken))
            {
                return Results.Conflict(new { error = "Пользователь уже существует" });
            }

            var user = User.Create(request.Login, AuthService.HashPassword(request.Password));
            await db.Users.AddAsync(user, cancellationToken);
            await commonRepository.SaveChangesAsync(cancellationToken);

            return Results.Created($"/admin/db/users/{user.Login}", user);
        });

        adminUsersApi.MapPatch("/{login}/password", async (string login, [FromBody] AdminUpdatePasswordRequest request,
            [FromServices] DobleyContext db, [FromServices] ICommonRepository commonRepository,
            CancellationToken cancellationToken) =>
        {
            var user = await db.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Login == login, cancellationToken);
            if (user == null)
            {
                return Results.NotFound();
            }

            user.Password = AuthService.HashPassword(request.Password);
            await commonRepository.SaveChangesAsync(cancellationToken);

            return Results.Ok(user);
        });

        return app;
    }

    private static object CreateEntity(IEntityType entityType, JsonElement body, bool allowKeyProperties)
    {
        var constructor = entityType.ClrType.GetConstructor(BindingFlags.Instance | BindingFlags.Public |
                                                            BindingFlags.NonPublic, Type.EmptyTypes);
        var entity = constructor?.Invoke(null)
                     ?? throw new InvalidOperationException($"У сущности {entityType.ClrType.Name} нет конструктора.");

        ApplyJson(entityType, entity, body, allowKeyProperties);

        return entity;
    }

    private static void ApplyJson(IEntityType entityType, object entity, JsonElement body, bool allowKeyProperties)
    {
        var keyNames = entityType.FindPrimaryKey()?.Properties.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase) ??
                       [];
        var properties = entityType.GetProperties()
            .Select(x => x.PropertyInfo)
            .Where(x => x != null)
            .ToDictionary(x => x!.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var jsonProperty in body.EnumerateObject())
        {
            if (!allowKeyProperties && keyNames.Contains(jsonProperty.Name))
            {
                continue;
            }

            if (!properties.TryGetValue(jsonProperty.Name, out var property))
            {
                continue;
            }

            property!.SetValue(entity, ConvertJsonValue(jsonProperty.Value, property.PropertyType));
        }
    }

    private static IQueryable CreateQueryable(DobleyContext db, IEntityType entityType)
    {
        var set = typeof(DbContext)
            .GetMethods()
            .Single(x => x.Name == nameof(DbContext.Set) && x.IsGenericMethod && x.GetParameters().Length == 0)
            .MakeGenericMethod(entityType.ClrType)
            .Invoke(db, null)!;

        return ApplyIgnoreQueryFilters(entityType.ClrType, (IQueryable)set);
    }

    private static IQueryable ApplyIgnoreQueryFilters(Type entityClrType, IQueryable query)
    {
        var method = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .Single(x => x.Name == nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters) &&
                         x.GetParameters().Length == 1)
            .MakeGenericMethod(entityClrType);

        return (IQueryable)method.Invoke(null, [query])!;
    }

    private static IQueryable ApplyKeyFilter(IEntityType entityType, IQueryable query, string key)
    {
        var keyProperty = GetSingleKeyProperty(entityType);
        var parameter = Expression.Parameter(entityType.ClrType, "x");
        var property = Expression.Property(parameter, keyProperty.Name);
        var value = Expression.Constant(ConvertStringValue(key, keyProperty.ClrType), keyProperty.ClrType);
        var predicate = Expression.Lambda(Expression.Equal(property, value), parameter);
        var method = typeof(Queryable)
            .GetMethods()
            .Single(x => x.Name == nameof(Queryable.Where) &&
                         x.GetParameters().Length == 2 &&
                         x.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
            .MakeGenericMethod(entityType.ClrType);

        return (IQueryable)method.Invoke(null, [query, predicate])!;
    }

    private static object? ConvertJsonValue(JsonElement value, Type targetType)
    {
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (value.ValueKind == JsonValueKind.Null)
        {
            return Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType ? null : Activator.CreateInstance(type);
        }

        if (type.IsEnum)
        {
            return value.ValueKind == JsonValueKind.String
                ? Enum.Parse(type, value.GetString()!, true)
                : Enum.ToObject(type, value.GetInt32());
        }

        return type switch
        {
            _ when type == typeof(string) => value.GetString(),
            _ when type == typeof(int) => value.GetInt32(),
            _ when type == typeof(long) => value.GetInt64(),
            _ when type == typeof(decimal) => value.GetDecimal(),
            _ when type == typeof(double) => value.GetDouble(),
            _ when type == typeof(bool) => value.GetBoolean(),
            _ when type == typeof(DateTime) => value.GetDateTime(),
            _ when type == typeof(Guid) => value.GetGuid(),
            _ => JsonSerializer.Deserialize(value.GetRawText(), type)
        };
    }

    private static object ConvertStringValue(string value, Type targetType)
    {
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
        return type.IsEnum
            ? Enum.Parse(type, value, true)
            : Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
    }

    private static async Task<object?> FindByKeyAsync(DobleyContext db, IEntityType entityType, string key,
        CancellationToken cancellationToken)
    {
        var query = ApplyKeyFilter(entityType, CreateQueryable(db, entityType), key);
        var rows = await ToListAsync(entityType.ClrType, query, cancellationToken);

        return rows.SingleOrDefault();
    }

    private static IEntityType? FindEntityType(DobleyContext db, string tableName)
        => db.Model.GetEntityTypes()
            .Where(x => x.GetTableName() != null)
            .FirstOrDefault(x =>
                string.Equals(x.GetTableName(), tableName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.ClrType.Name, tableName, StringComparison.OrdinalIgnoreCase));

    private static object? GetKeyValue(IEntityType entityType, object entity)
    {
        var keyProperty = GetSingleKeyProperty(entityType);
        return keyProperty.PropertyInfo?.GetValue(entity);
    }

    private static IProperty GetSingleKeyProperty(IEntityType entityType)
    {
        var key = entityType.FindPrimaryKey();
        if (key?.Properties.Count != 1)
        {
            throw new InvalidOperationException("Админский CRUD поддерживает только таблицы с одним первичным ключом.");
        }

        return key.Properties[0];
    }

    private static async Task<IReadOnlyList<object>> ToListAsync(Type entityClrType, IQueryable query,
        CancellationToken cancellationToken)
    {
        var method = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .Single(x => x.Name == nameof(EntityFrameworkQueryableExtensions.ToListAsync) &&
                         x.GetParameters().Length == 2)
            .MakeGenericMethod(entityClrType);
        var task = (Task)method.Invoke(null, [query, cancellationToken])!;

        await task;

        var result = task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task);
        return ((IEnumerable)result!).Cast<object>().ToArray();
    }

    private record AdminTableInfo(string Name, string Entity, IReadOnlyList<string> KeyProperties,
        IReadOnlyList<string> Properties)
    {
        public static AdminTableInfo Create(IEntityType entityType)
            => new(entityType.GetTableName() ?? entityType.ClrType.Name, entityType.ClrType.Name,
                entityType.FindPrimaryKey()?.Properties.Select(x => x.Name).ToArray() ?? [],
                entityType.GetProperties().Select(x => x.Name).OrderBy(x => x).ToArray());
    }

    private record AdminTableRowsResponse(string Table, int PageIndex, int PageSize, int TotalCount,
        IEnumerable<object> Rows);
}
