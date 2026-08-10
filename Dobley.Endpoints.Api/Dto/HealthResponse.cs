namespace Dobley.Endpoints.Api.Dto;

public record HealthResponse(string Status, bool DatabaseAvailable, bool CacheAvailable);
