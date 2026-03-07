using Microsoft.Extensions.VectorData;

namespace AiChatClient.Common.Models;

public class PdfChunkRecord
{
	[VectorStoreKey]
	public string Key { get; set; } = Guid.NewGuid().ToString();

	[VectorStoreData]
	public string Text { get; set; } = string.Empty;

	[VectorStoreData(IsFullTextIndexed = true)]
	public string SourceFile { get; set; } = string.Empty;

	[VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
	public ReadOnlyMemory<float> Vector { get; set; }
}
