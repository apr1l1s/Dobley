namespace Dobley.Endpoints.Auth.Dto;

public record HealthResponse(string Status, bool DatabaseAvailable, bool CacheAvailable);
