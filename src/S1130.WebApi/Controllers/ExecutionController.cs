using Microsoft.AspNetCore.Mvc;
using S1130.WebApi.Models;
using S1130.WebApi.Services;

namespace S1130.WebApi.Controllers
{
    /// <summary>
    /// Controller for IBM 1130 CPU execution operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExecutionController : ControllerBase
    {
        private readonly EmulatorService _emulator;

        public ExecutionController(EmulatorService emulator)
        {
            _emulator = emulator;
        }

        /// <summary>
        /// Gets the current execution status and CPU state
        /// </summary>
        /// <returns>Execution status with CPU state</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ExecutionStatusDto), StatusCodes.Status200OK)]
        public ActionResult<ExecutionStatusDto> GetStatus()
        {
            var status = new ExecutionStatusDto
            {
                IsRunning = _emulator.IsRunning,
                CpuState = _emulator.GetState()
            };
            return Ok(status);
        }

        /// <summary>
        /// Executes a single instruction step
        /// </summary>
        /// <returns>CPU state after executing the instruction</returns>
        [HttpPost("step")]
        [ProducesResponseType(typeof(CpuStateDto), StatusCodes.Status200OK)]
        public ActionResult<CpuStateDto> Step()
        {
            var state = _emulator.ExecuteStep();
            return Ok(state);
        }

        /// <summary>
        /// Starts continuous execution at the specified speed
        /// </summary>
        /// <param name="instructionsPerSecond">Number of instructions to execute per second (default: 1000)</param>
        /// <returns>Success status</returns>
        [HttpPost("run")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Run([FromQuery] int instructionsPerSecond = 1000)
        {
            _emulator.StartExecution(instructionsPerSecond);
            return Ok();
        }

        /// <summary>
        /// Stops continuous execution
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Stop()
        {
            _emulator.StopExecution();
            return Ok();
        }
    }
}
