using System.Text.Json.Serialization;

namespace Dobley.Data.Core.Integrations.Telegram.Dto;

public record TelegramMessage(
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("chat")] TelegramChat Chat);
