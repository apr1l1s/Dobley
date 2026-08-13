using System.Text.Json.Serialization;

namespace Dobley.Data.Core.Integrations.Telegram.Dto;

public record TelegramUpdate(
    [property: JsonPropertyName("update_id")] long UpdateId,
    [property: JsonPropertyName("message")] TelegramMessage? Message);
