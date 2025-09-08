namespace AiChatClient.Common.Models;

public record Wine(string Brand, string Varietal, string Appellation, DateOnly Vintage, string? Name = null);