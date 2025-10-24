using Microsoft.AspNetCore.Mvc;
using S1130.WebApi.Models;
using S1130.WebApi.Services;

namespace S1130.WebApi.Controllers
{
    /// <summary>
    /// Controller for IBM 1130 assembler operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AssemblerController : ControllerBase
    {
        private readonly EmulatorService _emulator;

        public AssemblerController(EmulatorService emulator)
        {
            _emulator = emulator;
        }

        /// <summary>
        /// Assembles IBM 1130 assembler source code and loads it into memory
        /// </summary>
        /// <param name="request">Assembly request containing source code and optional start address</param>
        /// <returns>Assembly result with success status, errors, and load information</returns>
        [HttpPost("assemble")]
        [ProducesResponseType(typeof(AssembleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AssembleResponse> Assemble([FromBody] AssembleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SourceCode))
            {
                return BadRequest("Source code cannot be empty");
            }

            var result = _emulator.Assemble(request.SourceCode, request.StartAddress);
            return Ok(result);
        }

        /// <summary>
        /// Resets the CPU and clears all memory
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Reset()
        {
            _emulator.Reset();
            return Ok();
        }
    }
}
