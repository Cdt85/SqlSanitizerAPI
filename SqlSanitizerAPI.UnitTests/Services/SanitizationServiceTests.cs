using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SqlSanitizerAPI.Models.Responses;
using SqlSanitizerAPI.Repositories;
using SqlSanitizerAPI.Services.SanitizationService;
using Xunit;

namespace SqlSanitizerAPI.Tests.Services
{
    public class SanitizationServiceTests
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<ILogger<SanitizationService>> _mockLogger;
        private readonly IMemoryCache _memoryCache;
        private readonly SanitizationServiceOptions _options;
        private readonly SanitizationService _service;

        public SanitizationServiceTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockLogger = new Mock<ILogger<SanitizationService>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _options = new SanitizationServiceOptions { CacheExpirationInMinutes = 10 };

            var mockOptions = new Mock<IOptions<SanitizationServiceOptions>>();
            mockOptions.Setup(x => x.Value).Returns(_options);

            _service = new SanitizationService(
                _mockLogger.Object,
                _mockRepository.Object,
                _memoryCache,
                mockOptions.Object
            );
        }

        [Fact]
        public async Task SanitizeSqlQueryAsync_WithSensitiveWords_ShouldReplaceThem()
        {
            // Arrange
            var sensitiveWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(1, "password", DateTime.UtcNow),
                new GetSensitiveWordsResponse(2, "secret", DateTime.UtcNow)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            var query = "SELECT password, secret FROM users";

            // Act
            var result = await _service.SanitizeSqlQueryAsync(query);

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().Be("SELECT ********, ****** FROM users");
        }

        [Fact]
        public async Task SanitizeSqlQueryAsync_WithEmptyString_ShouldReturnError()
        {
            // Arrange
            var query = "";

            // Act
            var result = await _service.SanitizeSqlQueryAsync(query);

            // Assert
            result.IsT1.Should().BeTrue();
            result.AsT1.ErrorCode.Should().Be(400);
        }

        [Fact]
        public async Task SanitizeSqlQueryAsync_CaseInsensitive_ShouldReplaceSensitiveWords()
        {
            // Arrange
            var sensitiveWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(1, "admin", DateTime.UtcNow)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            var query = "SELECT ADMIN, Admin, admin FROM users";

            // Act
            var result = await _service.SanitizeSqlQueryAsync(query);

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().Be("SELECT *****, *****, ***** FROM users");
        }

        [Fact]
        public async Task GetSensitiveWordsAsync_WithoutId_ShouldReturnAllWords()
        {
            // Arrange
            var expectedWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(1, "test1", DateTime.UtcNow),
                new GetSensitiveWordsResponse(2, "test2", DateTime.UtcNow)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWords);

            // Act
            var result = await _service.GetSensitiveWordsAsync();

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().HaveCount(2);
        }

        [Fact]
        public async Task InsertSensitiveWordAsync_WithValidWord_ShouldReturnId()
        {
            // Arrange
            var word = "newword";
            _mockRepository.Setup(x => x.InsertSensitiveWordAsync(word, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.InsertSensitiveWordAsync(word);

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().Be(1);
        }

        [Fact]
        public async Task UpdateSensitiveWordAsync_WithValidData_ShouldReturnRowsAffected()
        {
            // Arrange
            var id = 1;
            var word = "updatedword";
            _mockRepository.Setup(x => x.UpdateSensitiveWordAsync(id, word, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateSensitiveWordAsync(id, word);

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().Be(1);
        }

        [Fact]
        public async Task DeleteSensitiveWordAsync_WithValidId_ShouldReturnRowsAffected()
        {
            // Arrange
            var id = 1;
            _mockRepository.Setup(x => x.DeleteSensitiveWordAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteSensitiveWordAsync(id);

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().Be(1);
        }

        [Fact]
        public async Task SanitizeSqlQueryAsync_ShouldUseCache()
        {
            // Arrange
            var sensitiveWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(1, "cached", DateTime.UtcNow)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            var query = "SELECT cached FROM table";

            // Act
            await _service.SanitizeSqlQueryAsync(query);

            // Assert - Repository should only be called once due to caching
            _mockRepository.Verify(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ListSensitiveWordsDetailAsync_WithSensitiveWords_ShouldReturnDetailList()
        {
            // Arrange
            var sensitiveWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(2, "password", DateTime.UtcNow),
                new GetSensitiveWordsResponse(1, "secret", DateTime.UtcNow),
                new GetSensitiveWordsResponse(3, "admin", DateTime.UtcNow)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            // Act
            var result = await _service.ListSensitiveWordsDetailAsync();

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().HaveCount(3);
            result.AsT0[0].Id.Should().Be(1);
            result.AsT0[0].Word.Should().Be("secret");
            result.AsT0[1].Id.Should().Be(2);
            result.AsT0[1].Word.Should().Be("password");
            result.AsT0[2].Id.Should().Be(3);
            result.AsT0[2].Word.Should().Be("admin");
        }

        [Fact]
        public async Task ListSensitiveWordsDetailAsync_WithEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var sensitiveWords = new List<GetSensitiveWordsResponse>();

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            // Act
            var result = await _service.ListSensitiveWordsDetailAsync();

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().BeEmpty();
        }

        [Fact]
        public async Task ListSensitiveWordsDetailAsync_WithNullList_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GetSensitiveWordsResponse>)null!);

            // Act
            var result = await _service.ListSensitiveWordsDetailAsync();

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().BeEmpty();
        }

        [Fact]
        public async Task ListSensitiveWordsDetailAsync_WhenRepositoryThrowsException_ShouldReturnError()
        {
            // Arrange
            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.ListSensitiveWordsDetailAsync();

            // Assert
            result.IsT1.Should().BeTrue();
            result.AsT1.ErrorCode.Should().Be(500);
            result.AsT1.ErrorMessage.Should().Be("An error occurred while retrieving sensitive words.");
        }

        [Fact]
        public async Task ListSensitiveWordsDetailAsync_ShouldMapFieldsCorrectly()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var sensitiveWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(42, "testword", createdAt)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            // Act
            var result = await _service.ListSensitiveWordsDetailAsync();

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().HaveCount(1);
            result.AsT0[0].Id.Should().Be(42);
            result.AsT0[0].Word.Should().Be("testword");
        }

        [Fact]
        public async Task ListSensitiveWordsDetailAsync_ShouldOrderById()
        {
            // Arrange
            var sensitiveWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(5, "word5", DateTime.UtcNow),
                new GetSensitiveWordsResponse(2, "word2", DateTime.UtcNow),
                new GetSensitiveWordsResponse(8, "word8", DateTime.UtcNow),
                new GetSensitiveWordsResponse(1, "word1", DateTime.UtcNow)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            // Act
            var result = await _service.ListSensitiveWordsDetailAsync();

            // Assert
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().HaveCount(4);
            result.AsT0.Should().BeInAscendingOrder(x => x.Id);
            result.AsT0[0].Id.Should().Be(1);
            result.AsT0[1].Id.Should().Be(2);
            result.AsT0[2].Id.Should().Be(5);
            result.AsT0[3].Id.Should().Be(8);
        }

        [Fact]
        public async Task ListSensitiveWordsDetailAsync_ShouldCallRepositoryOnce()
        {
            // Arrange
            var sensitiveWords = new List<GetSensitiveWordsResponse>
            {
                new GetSensitiveWordsResponse(1, "test", DateTime.UtcNow)
            };

            _mockRepository.Setup(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sensitiveWords);

            // Act
            await _service.ListSensitiveWordsDetailAsync();

            // Assert
            _mockRepository.Verify(x => x.GetActiveSensitiveWordsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
