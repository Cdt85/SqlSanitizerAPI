using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using SqlSanitizerAPI.Controllers;
using SqlSanitizerAPI.Models.Responses;
using SqlSanitizerAPI.Models.Requests;
using SqlSanitizerAPI.Services.TokenSevice;
using Xunit;

namespace SqlSanitizerAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IOptions<AuthControllerOptions>> _mockOptions;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockTokenService = new Mock<ITokenService>();
            _mockOptions = new Mock<IOptions<AuthControllerOptions>>();

            // Setup token service to return a valid token response
            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<string>()))
                .Returns(new TokenResponse { Token = "test-token", ExpiresIn = 3600, TokenType = "Bearer" });

            // Setup auth options
            var authOptions = new AuthControllerOptions
            {
                Username = "admin",
                Password = "P@ssw0rd123"
            };
            _mockOptions.Setup(x => x.Value).Returns(authOptions);

            _controller = new AuthController(_mockTokenService.Object, _mockOptions.Object);
        }

        [Fact]
        public void Login_WithValidCredentials_ShouldReturnOkWithToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "P@ssw0rd123"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            _mockTokenService.Verify(x => x.GenerateToken("admin"), Times.Once);
        }

        [Fact]
        public void Login_WithInvalidUsername_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "wronguser",
                Password = "P@ssw0rd123"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "wrongpassword"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Login_WithEmptyCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "",
                Password = ""
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>()), Times.Never);
        }
    }
}
