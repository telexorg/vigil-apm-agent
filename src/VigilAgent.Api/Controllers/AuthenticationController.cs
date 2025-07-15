using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VigilAgent.Api.Dtos;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Models;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Controllers
{ 

    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly IConfiguration _config;

        private readonly IAuthenticationService _service;

        public AuthenticationController(IAuthenticationService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }

        [HttpPost("register-project")]
        public async Task<IActionResult> RegisterProject([FromBody] ProjectRegistrationRequest request)
        {
            var (isConflict, created, apiKey) = await _service.RegisterProjectAsync(request);
            if (isConflict)
                return Conflict("Project name already exists.");

            return Ok(new
            {
                projectId = created.Id,
                apiKey
            });
        }
             


        //[HttpGet("config")]
        //public IActionResult GetConfig()
        //{
        //    var result = new Dictionary<string, string?>
        //{
        //    { "ASPNETCORE_ENVIRONMENT", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") },
        //    { "MySettings:ApiKey", _config["MySettings:ApiKey"] }, // sanitize or remove if sensitive
        //    { "ConnectionStrings:DefaultConnection", _config.GetConnectionString("DefaultConnection") }
        //};

        //    return Ok(result);
        //}
    }

}

