using System.Numerics.Tensors;
using AiChatClient.Common.Models;
using Microsoft.Extensions.AI;
using UglyToad.PdfPig;

namespace AiChatClient.Common;

public class PdfIngestionService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
	const int _chunkSize = 500;
	const int _chunkOverlap = 100;
	const float _similarityThreshold = 0.7f;
	const int _maxResults = 3;

	readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
	readonly List<EmbeddingEntry> _entries = [];

	public bool HasDocuments => _entries.Count > 0;

	public async Task IngestPdfAsync(Stream pdfStream, string fileName, CancellationToken token = default)
	{
		var text = ExtractTextFromPdf(pdfStream);
		var chunks = ChunkText(text);

		if (chunks.Count is 0)
			return;

		var embeddings = await _embeddingGenerator.GenerateAsync(chunks, cancellationToken: token).ConfigureAwait(false);

		for (int i = 0; i < chunks.Count; i++)
		{
			_entries.Add(new EmbeddingEntry(chunks[i], embeddings[i].Vector.ToArray(), fileName));
		}
	}

	public async Task<string?> SearchAsync(string query, CancellationToken token = default)
	{
		if (_entries.Count is 0)
			return null;

		var queryEmbeddings = await _embeddingGenerator.GenerateAsync([query], cancellationToken: token).ConfigureAwait(false);
		var queryVector = queryEmbeddings[0].Vector.ToArray();

		var results = _entries
			.Select(e => (Entry: e, Similarity: TensorPrimitives.CosineSimilarity(new ReadOnlySpan<float>(queryVector), new ReadOnlySpan<float>(e.Vector))))
			.Where(static r => r.Similarity >= _similarityThreshold)
			.OrderByDescending(static r => r.Similarity)
			.Take(_maxResults)
			.ToList();

		if (results.Count is 0)
			return null;

		return string.Join("\n\n", results.Select(r => r.Entry.Text));
	}

	static string ExtractTextFromPdf(Stream pdfStream)
	{
		using var memoryStream = new MemoryStream();
		pdfStream.CopyTo(memoryStream);
		memoryStream.Position = 0;

		using var document = PdfDocument.Open(memoryStream);
		return string.Join("\n", document.GetPages().Select(p => p.Text));
	}

	static List<string> ChunkText(string text)
	{
		var chunks = new List<string>();

		if (string.IsNullOrWhiteSpace(text))
			return chunks;

		var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < words.Length; i += _chunkSize - _chunkOverlap)
		{
			var chunk = string.Join(' ', words.Skip(i).Take(_chunkSize));

			if (!string.IsNullOrWhiteSpace(chunk))
				chunks.Add(chunk);

			if (i + _chunkSize >= words.Length)
				break;
		}

		return chunks;
	}
}
