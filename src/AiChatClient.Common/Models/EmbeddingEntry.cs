namespace AiChatClient.Common.Models;

public record EmbeddingEntry(string Text, float[] Vector, string SourceFile);