using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VigilAgent.Api.Dtos;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Services;

namespace VigilAgent.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class TelexController : ControllerBase
    {
        private readonly ITelemetryService _commandHandler;
        private ILogger<TelexController> _logger;
        private readonly IVigilAgentService _agent;

        public TelexController(ITelemetryService blogService, ILogger<TelexController> logger, IVigilAgentService agent)
        {
            _commandHandler = blogService;
            _logger = logger;
            _agent = agent;
        }

        [HttpGet(".well-known/agent.json")]
        public IActionResult GetIntegrationConfig()
        {

            var integrationJson = AgentCardLoader.GetAgentCard();

            if (integrationJson == null)
            {
                return NotFound();
            }

            return Ok(integrationJson);
        }

        /// <summary>
        /// Recieves task from Telex
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReceiveTask([FromBody] TaskRequest taskRequest)
        {
            _logger.LogInformation($"Task request received: {JsonSerializer.Serialize(taskRequest.Id)}");

            //ValidationHelper.ValidateRequest(taskRequest);           

            var response = await _agent.HandleUserInput(taskRequest);

            _logger.LogInformation($"Returning task response: {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })}");   
            
            return Ok(response);
        }

        [HttpPost("task/get{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTask([FromBody] string id)
        {
            return Ok();
        }
    }
}
