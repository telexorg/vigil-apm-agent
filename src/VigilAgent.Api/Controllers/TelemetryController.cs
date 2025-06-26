using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Instrumentation;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private static int n = 1;
        private readonly ITelemetryHandler _telemetryHandler;

        public TelemetryController(ITelemetryHandler telemetryHandler)
        {
            _telemetryHandler = telemetryHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //var junk = new List<byte[]>();
            //for (int i = 0; i < 10000; i++)
            //{
            //    junk.Add(new byte[1024 * 50]); // 50 KB
            //}

            if (n % 2 == 0)
            {
                n++;
                throw new Exception();
            }
                n++;
            return Ok("Controller is working");
        }

       [HttpPost]
        public async Task<IActionResult> Batch([FromBody] JsonElement batch)
        {
            try
            {
                if (batch.ValueKind != JsonValueKind.Array || batch.GetArrayLength() == 0)
                    return BadRequest("Invalid or empty batch");

                if (!HttpContext.Items.TryGetValue("Project", out var projectObj) || projectObj is not Project project)
                    return Unauthorized("Project context missing or invalid");

                await _telemetryHandler.HandleBatchItemsAsync(batch, project);

                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing batch: {ex.Message}", ex);
            }
        }

       
    }
}
