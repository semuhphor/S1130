using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using S1130.WebApi.Models;
using Xunit;

namespace UnitTests.S1130.WebApi.Controllers
{
    public class AssemblerControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AssemblerControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Assemble_WithValidCode_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new AssembleRequest
            {
                SourceCode = @"       ORG  /100
       LD   |L|DATA
       WAIT
DATA:  DC   42"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assembler/assemble", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<AssembleResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal((ushort)0x100, result.LoadedAddress);
            Assert.True(result.WordsLoaded > 0);
        }

        [Fact]
        public async Task Assemble_WithInvalidCode_ReturnsErrorResponse()
        {
            // Arrange
            var request = new AssembleRequest
            {
                SourceCode = @"INVALID ASSEMBLER CODE HERE"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assembler/assemble", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<AssembleResponse>();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task Assemble_WithEmptySourceCode_ReturnsBadRequest()
        {
            // Arrange
            var request = new AssembleRequest
            {
                SourceCode = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assembler/assemble", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Assemble_WithStartAddress_UsesProvidedAddress()
        {
            // Arrange
            var request = new AssembleRequest
            {
                SourceCode = @"       ORG  /100
       DC   5",
                StartAddress = 0x200
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assembler/assemble", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<AssembleResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal((ushort)0x200, result.LoadedAddress);
        }

        [Fact]
        public async Task Reset_ReturnsOk()
        {
            // Arrange
            // First, assemble and load some code
            var assembleRequest = new AssembleRequest
            {
                SourceCode = @"       ORG  /100
       LD   L DATA
       WAIT
DATA   DC   99"
            };
            await _client.PostAsJsonAsync("/api/assembler/assemble", assembleRequest);

            // Act
            var response = await _client.PostAsync("/api/assembler/reset", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task MultipleAssemble_ReturnsSuccessForEach()
        {
            // Arrange
            var firstRequest = new AssembleRequest
            {
                SourceCode = @"       ORG  /100
       DC   10"
            };
            var secondRequest = new AssembleRequest
            {
                SourceCode = @"       ORG  /200
       DC   20"
            };

            // Act
            var response1 = await _client.PostAsJsonAsync("/api/assembler/assemble", firstRequest);
            var result1 = await response1.Content.ReadFromJsonAsync<AssembleResponse>();
            
            var response2 = await _client.PostAsJsonAsync("/api/assembler/assemble", secondRequest);
            var result2 = await response2.Content.ReadFromJsonAsync<AssembleResponse>();

            // Assert
            Assert.NotNull(result1);
            Assert.True(result1.Success);
            Assert.Equal((ushort)0x100, result1.LoadedAddress);
            
            Assert.NotNull(result2);
            Assert.True(result2.Success);
            Assert.Equal((ushort)0x200, result2.LoadedAddress);
        }
    }
}
