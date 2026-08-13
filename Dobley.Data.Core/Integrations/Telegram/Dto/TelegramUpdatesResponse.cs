using System.Text.Json.Serialization;

namespace Dobley.Data.Core.Integrations.Telegram.Dto;

public record TelegramUpdatesResponse(
    [property: JsonPropertyName("result")] IReadOnlyList<TelegramUpdate> Result);
