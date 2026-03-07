using System.Linq.Expressions;
using AiChatClient.Common;
using AiChatClient.Common.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace AiChatClient.UnitTests;

[TestFixture]
public class PdfIngestionServiceTests
{
	[Test]
	public async Task IngestPdfAsync_UpsertsExpectedRecords()
	{
		// Arrange
		const string fileName = "test.pdf";
		const string pdfText = "Hello World";

		var fakeEmbedding = new ReadOnlyMemory<float>([0.1f, 0.2f, 0.3f]);
		var embeddingGenerator = new FakeEmbeddingGenerator(fakeEmbedding);
		var vectorCollection = new FakeVectorStoreCollection(collectionExists: true);

		var service = new PdfIngestionService(embeddingGenerator, vectorCollection);
		var pdfStream = CreatePdfStream(pdfText);

		// Act
		await service.IngestPdfAsync(pdfStream, fileName);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(vectorCollection.UpsertedRecords, Is.Not.Empty);
			Assert.That(vectorCollection.UpsertedRecords.All(r => r.SourceFile == fileName), Is.True);
			Assert.That(vectorCollection.UpsertedRecords.All(r => !string.IsNullOrEmpty(r.Text)), Is.True);
			Assert.That(vectorCollection.EnsureCollectionExistsCallCount, Is.EqualTo(1));
		});
	}

	[Test]
	public async Task SearchAsync_ReturnsNull_WhenCollectionDoesNotExist()
	{
		// Arrange
		var embeddingGenerator = new FakeEmbeddingGenerator(new ReadOnlyMemory<float>([0.1f, 0.2f, 0.3f]));
		var vectorCollection = new FakeVectorStoreCollection(collectionExists: false);

		var service = new PdfIngestionService(embeddingGenerator, vectorCollection);

		// Act
		var result = await service.SearchAsync("some query");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public async Task SearchAsync_ReturnsNull_WhenNoResultsWithinThreshold()
	{
		// Arrange
		var embeddingGenerator = new FakeEmbeddingGenerator(new ReadOnlyMemory<float>([0.1f, 0.2f, 0.3f]));

		var searchResults = new[]
		{
			new VectorSearchResult<PdfChunkRecord>(new PdfChunkRecord { Key = "1", Text = "Result 1" }, score: 0.5),
			new VectorSearchResult<PdfChunkRecord>(new PdfChunkRecord { Key = "2", Text = "Result 2" }, score: 0.8),
		};

		var vectorCollection = new FakeVectorStoreCollection(collectionExists: true, searchResults: searchResults);
		var service = new PdfIngestionService(embeddingGenerator, vectorCollection);

		// Act
		var result = await service.SearchAsync("some query");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public async Task SearchAsync_ReturnsExpectedText_WhenScoresAreWithinThreshold()
	{
		// Arrange
		var embeddingGenerator = new FakeEmbeddingGenerator(new ReadOnlyMemory<float>([0.1f, 0.2f, 0.3f]));

		const string expectedText1 = "Relevant result 1";
		const string expectedText2 = "Relevant result 2";

		var searchResults = new[]
		{
			new VectorSearchResult<PdfChunkRecord>(new PdfChunkRecord { Key = "1", Text = expectedText1 }, score: 0.1),
			new VectorSearchResult<PdfChunkRecord>(new PdfChunkRecord { Key = "2", Text = expectedText2 }, score: 0.3),
			new VectorSearchResult<PdfChunkRecord>(new PdfChunkRecord { Key = "3", Text = "Too far away" }, score: 0.4),
		};

		var vectorCollection = new FakeVectorStoreCollection(collectionExists: true, searchResults: searchResults);
		var service = new PdfIngestionService(embeddingGenerator, vectorCollection);

		// Act
		var result = await service.SearchAsync("some query");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.Contain(expectedText1));
		Assert.That(result, Does.Contain(expectedText2));
		Assert.That(result, Does.Not.Contain("Too far away"));
	}

	[Test]
	public async Task SearchAsync_ReturnsNull_WhenCollectionIsEmpty()
	{
		// Arrange
		var embeddingGenerator = new FakeEmbeddingGenerator(new ReadOnlyMemory<float>([0.1f, 0.2f, 0.3f]));
		var vectorCollection = new FakeVectorStoreCollection(collectionExists: true, searchResults: []);

		var service = new PdfIngestionService(embeddingGenerator, vectorCollection);

		// Act
		var result = await service.SearchAsync("some query");

		// Assert
		Assert.That(result, Is.Null);
	}

	static Stream CreatePdfStream(string text)
	{
		var builder = new PdfDocumentBuilder();
		var page = builder.AddPage(PageSize.A4);
		var font = builder.AddStandard14Font(Standard14Font.Helvetica);
		page.AddText(text, 12, new PdfPoint(25, 700), font);
		var bytes = builder.Build();
		return new MemoryStream(bytes);
	}

	sealed class FakeEmbeddingGenerator(ReadOnlyMemory<float> embedding) : IEmbeddingGenerator<string, Embedding<float>>
	{
		public EmbeddingGeneratorMetadata Metadata { get; } = new("FakeEmbeddingGenerator");

		public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
			IEnumerable<string> values,
			EmbeddingGenerationOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			var embeddings = values
				.Select(_ => new Embedding<float>(embedding))
				.ToList();

			return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
		}

		public object? GetService(Type serviceType, object? key = null) => null;

		public void Dispose() { }
	}

	sealed class FakeVectorStoreCollection : VectorStoreCollection<string, PdfChunkRecord>
	{
		readonly bool _collectionExists;
		readonly IReadOnlyList<VectorSearchResult<PdfChunkRecord>> _searchResults;

		public List<PdfChunkRecord> UpsertedRecords { get; } = [];
		public int EnsureCollectionExistsCallCount { get; private set; }

		public FakeVectorStoreCollection(
			bool collectionExists,
			IReadOnlyList<VectorSearchResult<PdfChunkRecord>>? searchResults = null)
		{
			_collectionExists = collectionExists;
			_searchResults = searchResults ?? [];
		}

		public override string Name => "fake-collection";

		public override object? GetService(Type serviceType, object? key = null) => null;

		public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult(_collectionExists);

		public override Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
		{
			EnsureCollectionExistsCallCount++;
			return Task.CompletedTask;
		}

		public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		public override Task<string> UpsertAsync(PdfChunkRecord record, CancellationToken cancellationToken = default)
		{
			UpsertedRecords.Add(record);
			return Task.FromResult(record.Key);
		}

		public override Task UpsertAsync(IEnumerable<PdfChunkRecord> records, CancellationToken cancellationToken = default)
		{
			UpsertedRecords.AddRange(records);
			return Task.CompletedTask;
		}

		public override IAsyncEnumerable<VectorSearchResult<PdfChunkRecord>> SearchAsync<TVector>(
			TVector vector,
			int top = 3,
			VectorSearchOptions<PdfChunkRecord>? options = null,
			CancellationToken cancellationToken = default)
			=> _searchResults.ToAsyncEnumerable();

		public override Task<PdfChunkRecord?> GetAsync(string key, RecordRetrievalOptions? options = null, CancellationToken cancellationToken = default)
			=> Task.FromResult<PdfChunkRecord?>(null);

		public override IAsyncEnumerable<PdfChunkRecord> GetAsync(IEnumerable<string> keys, RecordRetrievalOptions? options = null, CancellationToken cancellationToken = default)
			=> AsyncEnumerable.Empty<PdfChunkRecord>();

		public override IAsyncEnumerable<PdfChunkRecord> GetAsync(Expression<Func<PdfChunkRecord, bool>> filter, int top = 3, FilteredRecordRetrievalOptions<PdfChunkRecord>? options = null, CancellationToken cancellationToken = default)
			=> AsyncEnumerable.Empty<PdfChunkRecord>();

		public override Task DeleteAsync(string key, CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		public override Task DeleteAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
			=> Task.CompletedTask;
	}
}
