using AiChatClient.Common;
using AiChatClient.Common.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace AiChatClient.UnitTests;

[TestFixture]
public class PdfIngestionServiceTests : BaseTest
{
	[Test]
	public async Task IngestPdfAsync_UpsertsExpectedRecords()
	{
		// Arrange
		const string fileName = "test.pdf";
		const string pdfText = "Hello World";

		var vectorCollection = CreateVectorCollection();
		var service = new PdfIngestionService(EmbeddingGenerator, vectorCollection);
		var pdfStream = CreatePdfStream(pdfText);

		// Act
		await service.IngestPdfAsync(pdfStream, fileName);

		// Assert
		var result = await service.SearchAsync(pdfText);
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.Contain(pdfText));
	}

	[Test]
	public async Task SearchAsync_ReturnsNull_WhenCollectionDoesNotExist()
	{
		// Arrange
		var vectorCollection = CreateVectorCollection();
		var service = new PdfIngestionService(EmbeddingGenerator, vectorCollection);

		// Act
		var result = await service.SearchAsync("some query");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public async Task SearchAsync_ReturnsNull_WhenNoResultsWithinThreshold()
	{
		// Arrange
		var vectorCollection = CreateVectorCollection();
		var service = new PdfIngestionService(EmbeddingGenerator, vectorCollection);
		var pdfStream = CreatePdfStream("Advanced quantum chromodynamics and theoretical particle physics research");
		await service.IngestPdfAsync(pdfStream, "science.pdf");

		// Act
		var result = await service.SearchAsync("chocolate cake baking recipe with flour sugar and eggs");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public async Task SearchAsync_ReturnsExpectedText_WhenScoresAreWithinThreshold()
	{
		// Arrange
		const string pdfText = "The quick brown fox jumps over the lazy dog near the riverbank";
		var vectorCollection = CreateVectorCollection();
		var service = new PdfIngestionService(EmbeddingGenerator, vectorCollection);
		var pdfStream = CreatePdfStream(pdfText);
		await service.IngestPdfAsync(pdfStream, "foxes.pdf");

		// Act
		var result = await service.SearchAsync(pdfText);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.Contain(pdfText));
	}

	[Test]
	public async Task SearchAsync_ReturnsNull_WhenCollectionIsEmpty()
	{
		// Arrange
		var vectorCollection = CreateVectorCollection();
		await vectorCollection.EnsureCollectionExistsAsync();
		var service = new PdfIngestionService(EmbeddingGenerator, vectorCollection);

		// Act
		var result = await service.SearchAsync("some query");

		// Assert
		Assert.That(result, Is.Null);
	}

	static VectorStoreCollection<string, PdfChunkRecord> CreateVectorCollection()
	{
		var vectorStore = new InMemoryVectorStore();
		return vectorStore.GetCollection<string, PdfChunkRecord>("test-pdf-chunks");
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
}