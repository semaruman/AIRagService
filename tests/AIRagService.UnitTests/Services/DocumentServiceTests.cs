using AIRagService.Application.Common;
using AIRagService.Application.Common.Exceptions;
using AIRagService.Application.DTOs;
using AIRagService.Application.Services;
using AIRagService.Domain.Interfaces;
using Moq;

namespace AIRagService.UnitTests.Services;

public class DocumentServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPagedAsync_PageLessThanOne_ThrowsValidationException(int page)
    {
        var repository = new Mock<IDocumentRepository>(MockBehavior.Strict);
        var service = new DocumentService(repository.Object);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedAsync(page, pageSize: 10));

        Assert.Contains("Page must be greater than or equal to 1", exception.Message);
    }

    [Fact]
    public async Task GetPagedAsync_PageSizeLessThanOne_ThrowsValidationException()
    {
        var repository = new Mock<IDocumentRepository>(MockBehavior.Strict);
        var service = new DocumentService(repository.Object);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedAsync(page: 1, pageSize: 0));

        Assert.Contains("Page size must be greater than or equal to 1", exception.Message);
    }

    [Fact]
    public async Task GetPagedAsync_ValidRequest_ReturnsPagedResult()
    {
        var repository = new Mock<IDocumentRepository>();
        repository
            .Setup(r => r.GetPagedAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<AIRagService.Domain.Entities.Document>(), 0));

        var service = new DocumentService(repository.Object);

        var result = await service.GetPagedAsync(page: 1, pageSize: 20);

        Assert.IsType<PagedResult<DocumentDto>>(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
