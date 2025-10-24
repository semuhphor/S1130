using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using S1130.WebApi.Models;
using Xunit;

namespace UnitTests.S1130.WebApi.Controllers
{
    public class ExecutionControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ExecutionControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<AssembleResponse> LoadTestProgram()
        {
            var request = new AssembleRequest
            {
                SourceCode = @"       ORG  /100
       LD   L DATA
       WAIT
DATA   DC   42"
            };
            var response = await _client.PostAsJsonAsync("/api/assembler/assemble", request);
            return await response.Content.ReadFromJsonAsync<AssembleResponse>() 
                ?? throw new Exception("Failed to load test program");
        }

        [Fact]
        public async Task GetStatus_ReturnsCurrentCpuState()
        {
            // Arrange
            await LoadTestProgram();

            // Act
            var response = await _client.GetAsync("/api/execution/status");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var status = await response.Content.ReadFromJsonAsync<ExecutionStatusDto>();
            Assert.NotNull(status);
            Assert.NotNull(status.CpuState);
            Assert.False(status.IsRunning);
            Assert.Equal((ushort)0x100, status.CpuState.Iar);
        }

        [Fact]
        public async Task Step_ExecutesOneInstruction()
        {
            // Arrange
            await LoadTestProgram();
            
            // Get initial state
            var initialResponse = await _client.GetAsync("/api/execution/status");
            var initialStatus = await initialResponse.Content.ReadFromJsonAsync<ExecutionStatusDto>();
            Assert.NotNull(initialStatus);
            var initialInstructionCount = initialStatus.CpuState.InstructionCount;

            // Act
            var response = await _client.PostAsync("/api/execution/step", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<CpuStateDto>();
            Assert.NotNull(result);
            Assert.Equal(initialInstructionCount + 1, result.InstructionCount);
            Assert.Equal(42, result.Acc);  // Should have loaded DATA value
        }

        [Fact]
        public async Task Step_WithoutLoadedProgram_ReturnsState()
        {
            // Arrange
            await _client.PostAsync("/api/assembler/reset", null);

            // Act
            var response = await _client.PostAsync("/api/execution/step", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<CpuStateDto>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Run_StartsExecution()
        {
            // Arrange
            await _client.PostAsync("/api/assembler/reset", null);  // Ensure clean state
            
            var loopProgram = new AssembleRequest
            {
                SourceCode = @"       ORG  /100
LOOP   LD   L DATA
       BSC  L ZPM LOOP
DATA   DC   1"
            };
            var assembleResponse = await _client.PostAsJsonAsync("/api/assembler/assemble", loopProgram);
            var assembleResult = await assembleResponse.Content.ReadFromJsonAsync<AssembleResponse>();
            Assert.True(assembleResult?.Success, $"Assembly failed: {string.Join(", ", assembleResult?.Errors ?? new List<string>())}");

            // Act
            var response = await _client.PostAsync("/api/execution/run?instructionsPerSecond=1000", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Give it time to start and execute a few iterations
            await Task.Delay(300);
            
            // Check status
            var statusResponse = await _client.GetAsync("/api/execution/status");
            var status = await statusResponse.Content.ReadFromJsonAsync<ExecutionStatusDto>();
            Assert.NotNull(status);
            
            // The test is valid as long as the program ran
            // In rare cases on slow systems, it might complete before we check, so check instruction count
            bool wasRunningOrExecuted = status.IsRunning || status.CpuState.InstructionCount > 0;
            Assert.True(wasRunningOrExecuted, $"Expected execution to have started. IsRunning={status.IsRunning}, InstructionCount={status.CpuState.InstructionCount}");
            
            // Cleanup
            await _client.PostAsync("/api/execution/stop", null);
        }

        [Fact]
        public async Task Stop_StopsExecution()
        {
            // Arrange
            var loopProgram = new AssembleRequest
            {
                SourceCode = @"       ORG  /100
LOOP   LD   L DATA
       BSC  L ZPM LOOP
DATA   DC   1"
            };
            await _client.PostAsJsonAsync("/api/assembler/assemble", loopProgram);
            await _client.PostAsync("/api/execution/run?instructionsPerSecond=1000", null);
            await Task.Delay(50);  // Let it start

            // Act
            var response = await _client.PostAsync("/api/execution/stop", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var statusResponse = await _client.GetAsync("/api/execution/status");
            var status = await statusResponse.Content.ReadFromJsonAsync<ExecutionStatusDto>();
            Assert.NotNull(status);
            Assert.False(status.IsRunning);
        }

        [Fact]
        public async Task Run_StopsOnWaitInstruction()
        {
            // Arrange
            await LoadTestProgram();

            // Act
            var response = await _client.PostAsync("/api/execution/run?instructionsPerSecond=1000", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Give it time to execute and stop
            await Task.Delay(200);

            // Assert
            var statusResponse = await _client.GetAsync("/api/execution/status");
            var status = await statusResponse.Content.ReadFromJsonAsync<ExecutionStatusDto>();
            Assert.NotNull(status);
            Assert.False(status.IsRunning);  // Should have stopped on WAIT
            Assert.True(status.CpuState.Wait);  // WAIT flag should be set
        }

        [Fact]
        public async Task MultipleClients_CanAccessStatus()
        {
            // Arrange
            await LoadTestProgram();

            // Act - Multiple concurrent requests
            var tasks = Enumerable.Range(0, 10).Select(_ => 
                _client.GetAsync("/api/execution/status")
            ).ToArray();
            
            await Task.WhenAll(tasks);

            // Assert - All should succeed
            foreach (var task in tasks)
            {
                Assert.Equal(HttpStatusCode.OK, task.Result.StatusCode);
                var status = await task.Result.Content.ReadFromJsonAsync<ExecutionStatusDto>();
                Assert.NotNull(status);
            }
        }
    }
}
