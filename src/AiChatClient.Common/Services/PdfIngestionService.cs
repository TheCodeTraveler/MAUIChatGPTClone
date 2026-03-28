using AiChatClient.Common.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using UglyToad.PdfPig;

namespace AiChatClient.Common;

public class PdfIngestionService(
	IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
	[FromKeyedServices("PdfVectorStore")] VectorStoreCollection<string, PdfChunkRecord> vectorCollection)
{
	const int _chunkSize = 1000;
	const int _chunkOverlap = 200;
	const int _maxResults = 3;

	readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
	readonly VectorStoreCollection<string, PdfChunkRecord> _vectorCollection = vectorCollection;

	public async Task IngestPdfAsync(Stream pdfStream, string fileName, CancellationToken token = default)
	{
		await _vectorCollection.EnsureCollectionExistsAsync(token).ConfigureAwait(false);

		var text = ExtractTextFromPdf(pdfStream);
		IReadOnlyList<string> chunks = [.. ChunkText(text)];

		if (chunks.Count is 0)
			return;

		List<PdfChunkRecord> records = [];
		foreach (var chunk in chunks)
		{
			var embedding = await _embeddingGenerator.GenerateAsync(chunk, cancellationToken: token).ConfigureAwait(false);

			var record = new PdfChunkRecord
			{
				Key = Guid.NewGuid().ToString(),
				Text = chunk,
				SourceFile = fileName,
				Vector = embedding.Vector,
			};
			records.Add(record);
		}

		await _vectorCollection.UpsertAsync(records, cancellationToken: token).ConfigureAwait(false);
	}

	public async Task<string?> SearchAsync(string query, CancellationToken token = default)
	{
		var doesCollectionExist = await _vectorCollection.CollectionExistsAsync(token).ConfigureAwait(false);
		if (!doesCollectionExist)
			return null;

		var queryEmbedding = await _embeddingGenerator.GenerateAsync(query, cancellationToken: token).ConfigureAwait(false);

		var results = new List<string>();
		await foreach (var result in _vectorCollection.SearchAsync(queryEmbedding.Vector, _maxResults, cancellationToken: token).ConfigureAwait(false))
		{
			if (result.Score is <= 0.3f)
				results.Add(result.Record.Text);
		}

		return results.Count > 0 ? string.Join("\n\n", results) : null;
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

		for (var i = 0; i < words.Length; i += _chunkSize - _chunkOverlap)
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