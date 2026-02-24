using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf;
using SqlSanitizerAPI.Controllers;
using SqlSanitizerAPI.Models;
using SqlSanitizerAPI.Models.Requests;
using SqlSanitizerAPI.Models.Responses;
using SqlSanitizerAPI.Services.SanitizationService;
using Xunit;

namespace SqlSanitizerAPI.Tests.Controllers
{
    public class SanitizeControllerTests
    {
        private readonly Mock<ISanitizationService> _mockService;
        private readonly SanitizeController _controller;

        public SanitizeControllerTests()
        {
            _mockService = new Mock<ISanitizationService>();
            _controller = new SanitizeController(_mockService.Object);
        }

        [Fact]
        public async Task SanitizeSqlQuery_WithValidQuery_ShouldReturnOk()
        {
            // Arrange
            var query = "SELECT * FROM users";
            var sanitized = "SELECT * FROM users";
            _mockService.Setup(x => x.SanitizeSqlQueryAsync(query))
                .ReturnsAsync(OneOf<string, ErrorDetails>.FromT0(sanitized));

            // Act
            var result = await _controller.SanitizeSqlQuery(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(sanitized);
        }

        [Fact]
        public async Task SanitizeSqlQuery_WithError_ShouldReturnErrorStatusCode()
        {
            // Arrange
            var query = "";
            var error = new ErrorDetails(400, "Invalid query");
            _mockService.Setup(x => x.SanitizeSqlQueryAsync(query))
                .ReturnsAsync(OneOf<string, ErrorDetails>.FromT1(error));

            // Act
            var result = await _controller.SanitizeSqlQuery(query);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GetSensitiveWords_WithoutId_ShouldReturnAllWords()
        {
            // Arrange
                var words = new List<string> { "test1", "test2" };
            _mockService.Setup(x => x.GetSensitiveWordsAsync())
                .ReturnsAsync(OneOf<List<string>, ErrorDetails>.FromT0(words));

            // Act
            var result = await _controller.GetSensitiveWords();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedWords = okResult!.Value as List<string>;
            returnedWords.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateSensitiveWord_WithValidWord_ShouldReturnCreated()
        {
            // Arrange
            var word = "newword";
            var newId = 5;
            _mockService.Setup(x => x.InsertSensitiveWordAsync(word))
                .ReturnsAsync(OneOf<int, ErrorDetails>.FromT0(newId));

            // Act
            var result = await _controller.CreateSensitiveWord(word);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(new { rowsAffected = newId });
        }

        [Fact]
        public async Task UpdateSensitiveWord_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var request = new UpdateSanitizeStringRequest { Id = 1, SanitizeString = "updated" };
            _mockService.Setup(x => x.UpdateSensitiveWordAsync(request.Id, request.SanitizeString))
                .ReturnsAsync(OneOf<int, ErrorDetails>.FromT0(1));

            // Act
            var result = await _controller.UpdateSensitiveWord(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteSensitiveWord_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var id = 1;
            _mockService.Setup(x => x.DeleteSensitiveWordAsync(id))
                .ReturnsAsync(OneOf<int, ErrorDetails>.FromT0(1));

            // Act
            var result = await _controller.DeleteSensitiveWord(id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteSensitiveWord_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var id = 999;
            var error = new ErrorDetails(404, "Word not found");
            _mockService.Setup(x => x.DeleteSensitiveWordAsync(id))
                .ReturnsAsync(OneOf<int, ErrorDetails>.FromT1(error));

            // Act
            var result = await _controller.DeleteSensitiveWord(id);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task ListSensitiveWords_WithData_ShouldReturnOk()
        {
            // Arrange
            var words = new List<SensitiveWordsDetailResponse>
            {
                new SensitiveWordsDetailResponse(1, "password"),
                new SensitiveWordsDetailResponse(2, "secret"),
                new SensitiveWordsDetailResponse(3, "admin")
            };
            _mockService.Setup(x => x.ListSensitiveWordsDetailAsync())
                .ReturnsAsync(OneOf<List<SensitiveWordsDetailResponse>, ErrorDetails>.FromT0(words));

            // Act
            var result = await _controller.ListSensitiveWords();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedWords = okResult!.Value as List<SensitiveWordsDetailResponse>;
            returnedWords.Should().HaveCount(3);
            returnedWords![0].Id.Should().Be(1);
            returnedWords[0].Word.Should().Be("password");
        }

        [Fact]
        public async Task ListSensitiveWords_WithError_ShouldReturnErrorStatusCode()
        {
            // Arrange
            var error = new ErrorDetails(404, "No sensitive words found");
            _mockService.Setup(x => x.ListSensitiveWordsDetailAsync())
                .ReturnsAsync(OneOf<List<SensitiveWordsDetailResponse>, ErrorDetails>.FromT1(error));

            // Act
            var result = await _controller.ListSensitiveWords();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            objectResult.Value.Should().Be("No sensitive words found");
        }
    }
}
